using KBM.Application.Orders;
using KBM.Application.Security;
using KBM.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;

namespace KBM.Infrastructure.Persistence.Orders;

public sealed class PriceListService(KbmDbContext db, ICurrentUserContext currentUser) : IPriceListService
{
    public async Task<IReadOnlyList<PriceListListItem>> ListAsync(string? search = null, int? kind = null, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var q = db.PriceLists.Where(p => !p.IsDeleted && p.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            q = q.Where(p => p.Code.Contains(term) || p.Description.Contains(term));
        }
        if (kind is not null) q = q.Where(p => (int)p.Kind == kind);
        return await q.OrderBy(p => p.Code)
            .Select(p => new PriceListListItem(p.Id, p.Code, p.Description, (int)p.Kind, p.Lines.Count, p.Status))
            .ToListAsync(ct);
    }

    public async Task<PriceListDetail?> GetAsync(long id, CancellationToken ct = default)
    {
        var p = await LoadAsync(id, ct);
        return p is null ? null : Map(p);
    }

    public async Task<PriceListDetail> CreateAsync(CreatePriceListRequest request, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var actorId = currentUser.UserId ?? 0;
        var now = DateTime.UtcNow;
        var code = request.Code.Trim().ToUpperInvariant();
        if (await db.PriceLists.AnyAsync(p => p.CompanyId == companyId && p.Code == code && !p.IsDeleted, ct))
            throw new InvalidOperationException($"Listino '{code}' gia in uso.");

        var entity = new PriceList
        {
            CompanyId = companyId,
            Code = code,
            Description = request.Description.Trim(),
            Kind = (PriceListKind)request.Kind,
            CurrencyId = request.CurrencyId,
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo,
            Status = "Active",
            CreatedAt = now,
            CreatedBy = actorId
        };
        foreach (var l in request.Lines.Where(x => x.ItemId > 0))
            entity.Lines.Add(NewLine(l, companyId, actorId, now));
        db.PriceLists.Add(entity);
        await db.SaveChangesAsync(ct);
        return (await GetAsync(entity.Id, ct))!;
    }

    public async Task<PriceListDetail?> SaveAsync(long id, SavePriceListRequest request, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var actorId = currentUser.UserId ?? 0;
        var now = DateTime.UtcNow;
        var entity = await db.PriceLists.Include(p => p.Lines).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId && !p.IsDeleted, ct);
        if (entity is null) return null;

        entity.Description = request.Description.Trim();
        entity.Kind = (PriceListKind)request.Kind;
        entity.CurrencyId = request.CurrencyId;
        entity.ValidFrom = request.ValidFrom;
        entity.ValidTo = request.ValidTo;
        entity.Status = request.Status;
        entity.UpdatedAt = now;
        entity.UpdatedBy = actorId;

        var keepIds = request.Lines.Where(l => l.Id > 0).Select(l => l.Id!.Value).ToHashSet();
        foreach (var rem in entity.Lines.Where(l => !keepIds.Contains(l.Id)).ToList())
            db.PriceListLines.Remove(rem);

        foreach (var l in request.Lines.Where(x => x.ItemId > 0))
        {
            if (l.Id > 0)
            {
                var line = entity.Lines.First(x => x.Id == l.Id);
                line.ItemId = l.ItemId;
                line.UnitPrice = l.UnitPrice;
                line.DiscountPercent = l.DiscountPercent;
                line.MinQuantity = l.MinQuantity;
                line.UpdatedAt = now;
                line.UpdatedBy = actorId;
            }
            else entity.Lines.Add(NewLine(l, companyId, actorId, now));
        }
        await db.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    private async Task<PriceList?> LoadAsync(long id, CancellationToken ct)
    {
        var companyId = CompanyId();
        return await db.PriceLists.Include(p => p.Lines).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId && !p.IsDeleted, ct);
    }

    private static PriceListLine NewLine(SavePriceListLineRequest l, long companyId, long actorId, DateTime now) => new()
    {
        CompanyId = companyId,
        ItemId = l.ItemId,
        UnitPrice = l.UnitPrice,
        DiscountPercent = l.DiscountPercent,
        MinQuantity = l.MinQuantity,
        CreatedAt = now,
        CreatedBy = actorId
    };

    private static PriceListDetail Map(PriceList p) => new(
        p.Id, p.Code, p.Description, (int)p.Kind, p.CurrencyId, p.ValidFrom, p.ValidTo, p.Status,
        p.Lines.OrderBy(l => l.Id).Select(l => new PriceListLineDto(
            l.Id, l.ItemId, l.Item?.Code, l.Item?.Description, l.UnitPrice, l.DiscountPercent, l.MinQuantity)).ToList());

    private long CompanyId() => currentUser.CompanyId ?? throw new InvalidOperationException("Contesto azienda mancante.");
}
