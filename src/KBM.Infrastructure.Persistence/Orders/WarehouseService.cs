using KBM.Application.Orders;
using KBM.Application.Security;
using KBM.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;

namespace KBM.Infrastructure.Persistence.Orders;

public sealed class WarehouseService(KbmDbContext db, ICurrentUserContext currentUser) : IWarehouseService
{
    public async Task<IReadOnlyList<WarehouseListItem>> ListAsync(string? search = null, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var q = db.Warehouses.Where(w => !w.IsDeleted && w.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            q = q.Where(w => w.Code.Contains(term) || w.Description.Contains(term));
        }
        return await q.OrderBy(w => w.Code)
            .Select(w => new WarehouseListItem(w.Id, w.Code, w.Description, (int)w.Kind, w.IsDefault, w.Status))
            .ToListAsync(ct);
    }

    public async Task<WarehouseDetail?> GetAsync(long id, CancellationToken ct = default)
    {
        var w = await FindAsync(id, ct);
        return w is null ? null : new WarehouseDetail(w.Id, w.Code, w.Description, (int)w.Kind, w.Address, w.IsDefault, w.Status);
    }

    public async Task<WarehouseDetail> CreateAsync(SaveWarehouseRequest request, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var code = request.Code.Trim().ToUpperInvariant();
        if (await db.Warehouses.AnyAsync(w => w.CompanyId == companyId && w.Code == code && !w.IsDeleted, ct))
            throw new InvalidOperationException($"Magazzino '{code}' gia in uso.");
        if (request.IsDefault)
            await ClearDefaultAsync(companyId, ct);
        var entity = new Warehouse
        {
            CompanyId = companyId,
            Code = code,
            Description = request.Description.Trim(),
            Kind = (WarehouseKind)request.Kind,
            Address = Clean(request.Address),
            IsDefault = request.IsDefault,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUser.UserId ?? 0
        };
        db.Warehouses.Add(entity);
        await db.SaveChangesAsync(ct);
        return new WarehouseDetail(entity.Id, entity.Code, entity.Description, (int)entity.Kind, entity.Address, entity.IsDefault, entity.Status);
    }

    public async Task<WarehouseDetail?> UpdateAsync(long id, SaveWarehouseRequest request, CancellationToken ct = default)
    {
        var w = await FindAsync(id, ct);
        if (w is null) return null;
        if (request.IsDefault && !w.IsDefault)
            await ClearDefaultAsync(w.CompanyId, ct);
        w.Description = request.Description.Trim();
        w.Kind = (WarehouseKind)request.Kind;
        w.Address = Clean(request.Address);
        w.IsDefault = request.IsDefault;
        w.UpdatedAt = DateTime.UtcNow;
        w.UpdatedBy = currentUser.UserId;
        await db.SaveChangesAsync(ct);
        return new WarehouseDetail(w.Id, w.Code, w.Description, (int)w.Kind, w.Address, w.IsDefault, w.Status);
    }

    public async Task<IReadOnlyList<WarehouseReasonListItem>> ListReasonsAsync(CancellationToken ct = default)
    {
        var companyId = CompanyId();
        return await db.WarehouseReasons.Where(r => !r.IsDeleted && r.CompanyId == companyId)
            .OrderBy(r => r.Code)
            .Select(r => new WarehouseReasonListItem(r.Id, r.Code, r.Description, (int)r.MovementSign, (int)r.Category, r.Status))
            .ToListAsync(ct);
    }

    public async Task<WarehouseReasonListItem> CreateReasonAsync(SaveWarehouseReasonRequest request, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var code = request.Code.Trim().ToUpperInvariant();
        if (await db.WarehouseReasons.AnyAsync(r => r.CompanyId == companyId && r.Code == code && !r.IsDeleted, ct))
            throw new InvalidOperationException($"Causale '{code}' gia in uso.");
        var entity = new WarehouseReason
        {
            CompanyId = companyId,
            Code = code,
            Description = request.Description.Trim(),
            MovementSign = (StockMovementSign)request.MovementSign,
            AffectsStock = request.AffectsStock,
            Category = (DocumentCategory)request.Category,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUser.UserId ?? 0
        };
        db.WarehouseReasons.Add(entity);
        await db.SaveChangesAsync(ct);
        return new WarehouseReasonListItem(entity.Id, entity.Code, entity.Description, (int)entity.MovementSign, (int)entity.Category, entity.Status);
    }

    public async Task<int> SeedStandardAsync(CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var actor = currentUser.UserId ?? 0;
        var now = DateTime.UtcNow;
        var added = 0;
        if (!await db.Warehouses.AnyAsync(w => w.CompanyId == companyId && !w.IsDeleted, ct))
        {
            db.Warehouses.Add(new Warehouse
            {
                CompanyId = companyId, Code = "01", Description = "Magazzino principale",
                Kind = WarehouseKind.Own, IsDefault = true, CreatedAt = now, CreatedBy = actor
            });
            added++;
        }
        if (!await db.WarehouseReasons.AnyAsync(r => r.CompanyId == companyId && !r.IsDeleted, ct))
        {
            db.WarehouseReasons.AddRange(
                new WarehouseReason { CompanyId = companyId, Code = "VEN", Description = "Vendita", MovementSign = StockMovementSign.Out, Category = DocumentCategory.DeliveryNote, CreatedAt = now, CreatedBy = actor },
                new WarehouseReason { CompanyId = companyId, Code = "ACQ", Description = "Acquisto", MovementSign = StockMovementSign.In, Category = DocumentCategory.PurchaseOrder, CreatedAt = now, CreatedBy = actor });
            added += 2;
        }
        if (added > 0) await db.SaveChangesAsync(ct);
        return added;
    }

    private async Task ClearDefaultAsync(long companyId, CancellationToken ct)
    {
        var defaults = await db.Warehouses.Where(w => w.CompanyId == companyId && w.IsDefault && !w.IsDeleted).ToListAsync(ct);
        foreach (var w in defaults) w.IsDefault = false;
    }

    private async Task<Warehouse?> FindAsync(long id, CancellationToken ct)
    {
        var companyId = CompanyId();
        return await db.Warehouses.FirstOrDefaultAsync(w => w.Id == id && w.CompanyId == companyId && !w.IsDeleted, ct);
    }

    private long CompanyId() => currentUser.CompanyId ?? throw new InvalidOperationException("Contesto azienda mancante.");
    private static string? Clean(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();
}
