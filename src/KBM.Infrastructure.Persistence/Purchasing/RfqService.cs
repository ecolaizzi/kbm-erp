using KBM.Application.Purchasing;
using KBM.Application.Security;
using KBM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KBM.Infrastructure.Persistence.Purchasing;

public sealed class RfqService(KbmDbContext db, ICurrentUserContext currentUser) : IRfqService
{
    public async Task<IReadOnlyList<RfqListItem>> ListAsync(string? search = null, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var q = db.Rfqs.Where(r => !r.IsDeleted && r.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(r => r.Number.Contains(s) || r.Supplier.BusinessName.Contains(s));
        }
        return await q.OrderByDescending(r => r.Date).ThenByDescending(r => r.Id)
            .Select(r => new RfqListItem(r.Id, r.Number, r.Date, r.SupplierId, r.Supplier.BusinessName, r.Lines.Count, r.Status))
            .ToListAsync(ct);
    }

    public async Task<RfqDetail?> GetAsync(long id, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var r = await db.Rfqs
            .Include(x => x.Supplier)
            .Include(x => x.PurchaseRequest)
            .Include(x => x.Lines).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        return r is null ? null : Map(r);
    }

    public async Task<RfqDetail> CreateAsync(CreateRfqRequest request, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var actorId = currentUser.UserId ?? 0;
        var now = DateTime.UtcNow;
        await EnsureSupplierAsync(request.SupplierId, companyId, ct);

        var rfq = new Rfq
        {
            CompanyId = companyId,
            Number = await NextNumberAsync(companyId, request.Date, ct),
            Date = request.Date == default ? now : request.Date,
            SupplierId = request.SupplierId,
            PurchaseRequestId = request.PurchaseRequestId,
            ResponseDueDate = request.ResponseDueDate,
            Notes = Clean(request.Notes),
            Status = "Generato",
            CreatedAt = now, CreatedBy = actorId
        };
        var createLines = request.Lines.Where(l => !string.IsNullOrWhiteSpace(l.Description)).ToList();
        if (createLines.Count == 0) throw new InvalidOperationException("La RDO deve contenere almeno una riga.");
        foreach (var l in createLines) rfq.Lines.Add(NewLine(l, companyId, actorId, now));
        db.Rfqs.Add(rfq);
        await db.SaveChangesAsync(ct);
        return (await GetAsync(rfq.Id, ct))!;
    }

    public async Task<RfqDetail> CreateFromPurchaseRequestAsync(CreateRfqFromRequest request, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        await EnsureSupplierAsync(request.SupplierId, companyId, ct);

        var pr = await db.PurchaseRequests
            .Include(p => p.Lines).ThenInclude(l => l.Suppliers)
            .FirstOrDefaultAsync(p => p.Id == request.PurchaseRequestId && !p.IsDeleted && p.CompanyId == companyId, ct)
            ?? throw new InvalidOperationException("RDA non trovata.");

        // Includi le righe in cui il fornitore e tra quelli proposti; se nessuna riga ha fornitori, includile tutte.
        var anySupplierAssigned = pr.Lines.Any(l => l.Suppliers.Count > 0);
        var lines = pr.Lines
            .Where(l => !anySupplierAssigned || l.Suppliers.Any(s => s.SupplierId == request.SupplierId))
            .Select(l => new SaveRfqLine(0, l.ItemId, l.Description, l.Quantity, l.UnitOfMeasure, l.ProposedPrice, null, true, null))
            .ToList();
        if (lines.Count == 0)
            throw new InvalidOperationException("Nessuna riga della RDA e associata a questo fornitore.");

        var created = await CreateAsync(new CreateRfqRequest(
            DateTime.UtcNow, request.SupplierId, request.PurchaseRequestId, request.ResponseDueDate, $"Generata da {pr.Number}", lines), ct);

        if (pr.Status is "Generato") { pr.Status = "EmessaRDO"; pr.UpdatedAt = DateTime.UtcNow; pr.UpdatedBy = currentUser.UserId; await db.SaveChangesAsync(ct); }
        return created;
    }

    public async Task<RfqDetail?> SaveAsync(long id, SaveRfqRequest request, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var actorId = currentUser.UserId ?? 0;
        var now = DateTime.UtcNow;

        var rfq = await db.Rfqs.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        if (rfq is null) return null;
        if (rfq.Status is "Confermato" or "Annullato")
            throw new InvalidOperationException("La RDO non e modificabile nello stato corrente.");
        await EnsureSupplierAsync(request.SupplierId, companyId, ct);

        var saveLines = request.Lines.Where(l => !string.IsNullOrWhiteSpace(l.Description)).ToList();
        if (saveLines.Count == 0) throw new InvalidOperationException("La RDO deve contenere almeno una riga.");
        if (request.Status is "OffertaRicevuta" or "Confermato" && saveLines.Any(l => l.UnitPrice is null or <= 0))
            throw new InvalidOperationException("Per registrare l'offerta inserire il prezzo unitario su tutte le righe.");

        rfq.Date = request.Date == default ? rfq.Date : request.Date;
        rfq.SupplierId = request.SupplierId;
        rfq.ResponseDueDate = request.ResponseDueDate;
        rfq.Status = string.IsNullOrWhiteSpace(request.Status) ? rfq.Status : request.Status;
        rfq.Notes = Clean(request.Notes);
        rfq.UpdatedAt = now; rfq.UpdatedBy = actorId;

        var keepIds = saveLines.Where(l => l.Id > 0).Select(l => l.Id).ToHashSet();
        foreach (var ex in rfq.Lines.Where(l => !keepIds.Contains(l.Id)).ToList())
            db.RfqLines.Remove(ex);

        foreach (var dto in saveLines)
        {
            var line = dto.Id > 0 ? rfq.Lines.FirstOrDefault(l => l.Id == dto.Id) : null;
            if (line is null) { rfq.Lines.Add(NewLine(dto, companyId, actorId, now)); continue; }
            ApplyLine(line, dto);
            line.UpdatedAt = now; line.UpdatedBy = actorId;
        }

        await db.SaveChangesAsync(ct);
        return await GetAsync(rfq.Id, ct);
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var rfq = await db.Rfqs.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        if (rfq is null) return false;
        rfq.IsDeleted = true;
        rfq.UpdatedAt = DateTime.UtcNow;
        rfq.UpdatedBy = currentUser.UserId;
        await db.SaveChangesAsync(ct);
        return true;
    }

    private RfqLine NewLine(SaveRfqLine dto, long companyId, long actorId, DateTime now)
    {
        var line = new RfqLine { CompanyId = companyId, CreatedAt = now, CreatedBy = actorId };
        ApplyLine(line, dto);
        return line;
    }

    private static void ApplyLine(RfqLine line, SaveRfqLine dto)
    {
        line.ItemId = dto.ItemId is 0 ? null : dto.ItemId;
        line.Description = dto.Description.Trim();
        line.Quantity = dto.Quantity <= 0 ? 1m : dto.Quantity;
        line.UnitOfMeasure = Clean(dto.UnitOfMeasure);
        line.UnitPrice = dto.UnitPrice;
        line.DiscountPercent = dto.DiscountPercent;
        line.Available = dto.Available;
        line.Notes = Clean(dto.Notes);
    }

    private async Task EnsureSupplierAsync(long supplierId, long companyId, CancellationToken ct)
    {
        if (!await db.Suppliers.AnyAsync(s => s.Id == supplierId && s.CompanyId == companyId && !s.IsDeleted, ct))
            throw new InvalidOperationException("Fornitore non valido.");
    }

    private async Task<string> NextNumberAsync(long companyId, DateTime date, CancellationToken ct)
    {
        var year = (date == default ? DateTime.UtcNow : date).Year;
        var count = await db.Rfqs.CountAsync(r => r.CompanyId == companyId && r.Date.Year == year, ct);
        return $"RDO/{year}/{count + 1:0000}";
    }

    private long CompanyId() => currentUser.CompanyId ?? throw new InvalidOperationException("Contesto azienda mancante.");
    private static string? Clean(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

    private static RfqDetail Map(Rfq r) => new(
        r.Id, r.Number, r.Date, r.SupplierId, r.Supplier?.BusinessName ?? "",
        r.PurchaseRequestId, r.PurchaseRequest?.Number, r.ResponseDueDate, r.Status, r.Notes,
        r.Lines.OrderBy(l => l.Id).Select(l => new RfqLineDto(
            l.Id, l.ItemId, l.Item?.Code, l.Description, l.Quantity, l.UnitOfMeasure,
            l.UnitPrice, l.DiscountPercent, l.Available, l.Notes)).ToList());
}
