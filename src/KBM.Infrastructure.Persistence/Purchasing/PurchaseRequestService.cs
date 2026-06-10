using KBM.Application.Purchasing;
using KBM.Application.Security;
using KBM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KBM.Infrastructure.Persistence.Purchasing;

public sealed class PurchaseRequestService(KbmDbContext db, ICurrentUserContext currentUser) : IPurchaseRequestService
{
    public async Task<IReadOnlyList<PurchaseRequestListItem>> ListAsync(string? search = null, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var q = db.PurchaseRequests.Where(p => !p.IsDeleted && p.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(p => p.Number.Contains(s) || (p.RequestingUnit != null && p.RequestingUnit.Contains(s)));
        }
        return await q.OrderByDescending(p => p.Date).ThenByDescending(p => p.Id)
            .Select(p => new PurchaseRequestListItem(p.Id, p.Number, p.Date, p.RequestingUnit, p.Lines.Count, p.Status))
            .ToListAsync(ct);
    }

    public async Task<PurchaseRequestDetail?> GetAsync(long id, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var p = await db.PurchaseRequests
            .Include(x => x.Lines).ThenInclude(l => l.Item)
            .Include(x => x.Lines).ThenInclude(l => l.Suppliers)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        return p is null ? null : Map(p);
    }

    public async Task<PurchaseRequestDetail> CreateAsync(CreatePurchaseRequestRequest request, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var actorId = currentUser.UserId ?? 0;
        var now = DateTime.UtcNow;

        var pr = new PurchaseRequest
        {
            CompanyId = companyId,
            Number = await NextNumberAsync("RDA", companyId, request.Date, ct),
            Date = request.Date == default ? now : request.Date,
            RequestingUnit = Clean(request.RequestingUnit),
            Notes = Clean(request.Notes),
            Status = "Generato",
            CreatedAt = now, CreatedBy = actorId
        };
        var createLines = request.Lines.Where(l => !string.IsNullOrWhiteSpace(l.Description)).ToList();
        if (createLines.Count == 0) throw new InvalidOperationException("La RDA deve contenere almeno una riga.");
        foreach (var l in createLines) pr.Lines.Add(NewLine(l, companyId, actorId, now));
        db.PurchaseRequests.Add(pr);
        await db.SaveChangesAsync(ct);
        return (await GetAsync(pr.Id, ct))!;
    }

    public async Task<PurchaseRequestDetail?> SaveAsync(long id, SavePurchaseRequestRequest request, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var actorId = currentUser.UserId ?? 0;
        var now = DateTime.UtcNow;

        var pr = await db.PurchaseRequests
            .Include(x => x.Lines).ThenInclude(l => l.Suppliers)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        if (pr is null) return null;

        if (pr.Status == "Chiuso")
            throw new InvalidOperationException("La RDA chiusa non e modificabile.");
        if (request.Lines.Count == 0 || request.Lines.All(l => string.IsNullOrWhiteSpace(l.Description)))
            throw new InvalidOperationException("La RDA deve contenere almeno una riga.");

        pr.Date = request.Date == default ? pr.Date : request.Date;
        pr.RequestingUnit = Clean(request.RequestingUnit);
        pr.Status = string.IsNullOrWhiteSpace(request.Status) ? pr.Status : request.Status;
        pr.Notes = Clean(request.Notes);
        pr.UpdatedAt = now; pr.UpdatedBy = actorId;

        var keepIds = request.Lines.Where(l => l.Id > 0).Select(l => l.Id).ToHashSet();
        foreach (var existing in pr.Lines.Where(l => !keepIds.Contains(l.Id)).ToList())
            db.PurchaseRequestLines.Remove(existing);

        foreach (var dto in request.Lines)
        {
            var line = dto.Id > 0 ? pr.Lines.FirstOrDefault(l => l.Id == dto.Id) : null;
            if (line is null)
            {
                pr.Lines.Add(NewLine(dto, companyId, actorId, now));
                continue;
            }
            ApplyLine(line, dto);
            line.UpdatedAt = now; line.UpdatedBy = actorId;
            SyncLineSuppliers(line, dto.SupplierIds, companyId, actorId, now);
        }

        await db.SaveChangesAsync(ct);
        return await GetAsync(pr.Id, ct);
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var pr = await db.PurchaseRequests.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        if (pr is null) return false;
        pr.IsDeleted = true;
        pr.UpdatedAt = DateTime.UtcNow;
        pr.UpdatedBy = currentUser.UserId;
        await db.SaveChangesAsync(ct);
        return true;
    }

    private PurchaseRequestLine NewLine(SavePurchaseRequestLine dto, long companyId, long actorId, DateTime now)
    {
        var line = new PurchaseRequestLine { CompanyId = companyId, CreatedAt = now, CreatedBy = actorId };
        ApplyLine(line, dto);
        foreach (var sid in dto.SupplierIds.Distinct())
            line.Suppliers.Add(new PurchaseRequestLineSupplier { CompanyId = companyId, SupplierId = sid, CreatedAt = now, CreatedBy = actorId });
        return line;
    }

    private static void ApplyLine(PurchaseRequestLine line, SavePurchaseRequestLine dto)
    {
        line.ItemId = dto.ItemId is 0 ? null : dto.ItemId;
        line.Description = dto.Description.Trim();
        line.Quantity = dto.Quantity <= 0 ? 1m : dto.Quantity;
        line.UnitOfMeasure = Clean(dto.UnitOfMeasure);
        line.RequiredDate = dto.RequiredDate;
        line.ProposedPrice = dto.ProposedPrice;
        line.LineStatus = string.IsNullOrWhiteSpace(dto.LineStatus) ? "Aperta" : dto.LineStatus;
    }

    private void SyncLineSuppliers(PurchaseRequestLine line, IReadOnlyList<long> incoming, long companyId, long actorId, DateTime now)
    {
        var wanted = incoming.Distinct().ToHashSet();
        foreach (var ex in line.Suppliers.Where(s => !wanted.Contains(s.SupplierId)).ToList())
            db.PurchaseRequestLineSuppliers.Remove(ex);
        var current = line.Suppliers.Select(s => s.SupplierId).ToHashSet();
        foreach (var sid in wanted.Where(w => !current.Contains(w)))
            line.Suppliers.Add(new PurchaseRequestLineSupplier { CompanyId = companyId, SupplierId = sid, CreatedAt = now, CreatedBy = actorId });
    }

    private async Task<string> NextNumberAsync(string prefix, long companyId, DateTime date, CancellationToken ct)
    {
        var year = (date == default ? DateTime.UtcNow : date).Year;
        var count = await db.PurchaseRequests.CountAsync(p => p.CompanyId == companyId && p.Date.Year == year, ct);
        return $"{prefix}/{year}/{count + 1:0000}";
    }

    private long CompanyId() => currentUser.CompanyId ?? throw new InvalidOperationException("Contesto azienda mancante.");
    private static string? Clean(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

    private static PurchaseRequestDetail Map(PurchaseRequest p) => new(
        p.Id, p.Number, p.Date, p.RequestingUnit, p.Status, p.Notes,
        p.Lines.OrderBy(l => l.Id).Select(l => new PurchaseRequestLineDto(
            l.Id, l.ItemId, l.Item?.Code, l.Description, l.Quantity, l.UnitOfMeasure,
            l.RequiredDate, l.ProposedPrice, l.LineStatus,
            l.Suppliers.Select(s => s.SupplierId).ToList())).ToList());
}
