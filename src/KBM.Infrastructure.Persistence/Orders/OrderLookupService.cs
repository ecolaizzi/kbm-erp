using KBM.Application.Orders;
using KBM.Application.Security;
using KBM.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;

namespace KBM.Infrastructure.Persistence.Orders;

public sealed class OrderLookupService(KbmDbContext db, ICurrentUserContext currentUser) : IOrderLookupService
{
    public async Task<IReadOnlyList<LookupListItem>> ListUnitsAsync(string? search = null, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var q = db.UnitsOfMeasure.Where(u => !u.IsDeleted && u.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            q = q.Where(u => u.Code.Contains(term) || u.Description.Contains(term));
        }
        return await q.OrderBy(u => u.Code).Select(u => new LookupListItem(u.Id, u.Code, u.Description, u.Status)).ToListAsync(ct);
    }

    public async Task<LookupListItem> CreateUnitAsync(SaveLookupRequest request, CancellationToken ct = default)
    {
        var entity = await CreateCodeEntityAsync(db.UnitsOfMeasure, request, ct);
        entity.DecimalPlaces = 2;
        await db.SaveChangesAsync(ct);
        return new LookupListItem(entity.Id, entity.Code, entity.Description, entity.Status);
    }

    public async Task<IReadOnlyList<LookupListItem>> ListZonesAsync(string? search = null, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var q = db.Zones.Where(z => !z.IsDeleted && z.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            q = q.Where(z => z.Code.Contains(term) || z.Description.Contains(term));
        }
        return await q.OrderBy(z => z.Code).Select(z => new LookupListItem(z.Id, z.Code, z.Description, z.Status)).ToListAsync(ct);
    }

    public async Task<LookupListItem> CreateZoneAsync(SaveLookupRequest request, CancellationToken ct = default)
    {
        var entity = await CreateCodeEntityAsync(db.Zones, request, ct);
        await db.SaveChangesAsync(ct);
        return new LookupListItem(entity.Id, entity.Code, entity.Description, entity.Status);
    }

    public async Task<IReadOnlyList<LookupListItem>> ListCarriersAsync(string? search = null, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var q = db.Carriers.Where(c => !c.IsDeleted && c.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            q = q.Where(c => c.Code.Contains(term) || c.Description.Contains(term));
        }
        return await q.OrderBy(c => c.Code).Select(c => new LookupListItem(c.Id, c.Code, c.Description, c.Status)).ToListAsync(ct);
    }

    public async Task<LookupListItem> CreateCarrierAsync(SaveLookupRequest request, CancellationToken ct = default)
    {
        var entity = await CreateCodeEntityAsync(db.Carriers, request, ct);
        await db.SaveChangesAsync(ct);
        return new LookupListItem(entity.Id, entity.Code, entity.Description, entity.Status);
    }

    public async Task<IReadOnlyList<LookupListItem>> ListPortTypesAsync(string? search = null, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var q = db.PortTypes.Where(p => !p.IsDeleted && p.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            q = q.Where(p => p.Code.Contains(term) || p.Description.Contains(term));
        }
        return await q.OrderBy(p => p.Code).Select(p => new LookupListItem(p.Id, p.Code, p.Description, p.Status)).ToListAsync(ct);
    }

    public async Task<LookupListItem> CreatePortTypeAsync(SaveLookupRequest request, CancellationToken ct = default)
    {
        var entity = await CreateCodeEntityAsync(db.PortTypes, request, ct);
        await db.SaveChangesAsync(ct);
        return new LookupListItem(entity.Id, entity.Code, entity.Description, entity.Status);
    }

    public async Task<IReadOnlyList<LookupListItem>> ListDocumentTypesAsync(int? category = null, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var q = db.DocumentTypes.Where(d => !d.IsDeleted && d.CompanyId == companyId);
        if (category is not null) q = q.Where(d => (int)d.Category == category);
        return await q.OrderBy(d => d.Code)
            .Select(d => new LookupListItem(d.Id, d.Code, d.Description, d.Status))
            .ToListAsync(ct);
    }

    public async Task<LookupListItem> CreateDocumentTypeAsync(SaveLookupRequest request, int category, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var code = request.Code.Trim().ToUpperInvariant();
        if (await db.DocumentTypes.AnyAsync(d => d.CompanyId == companyId && d.Code == code && !d.IsDeleted, ct))
            throw new InvalidOperationException($"Tipo documento '{code}' gia in uso.");
        var entity = new DocumentType
        {
            CompanyId = companyId,
            Code = code,
            Description = request.Description.Trim(),
            Category = (DocumentCategory)category,
            NumberingPrefix = code,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUser.UserId ?? 0
        };
        db.DocumentTypes.Add(entity);
        await db.SaveChangesAsync(ct);
        return new LookupListItem(entity.Id, entity.Code, entity.Description, entity.Status);
    }

    public async Task<IReadOnlyList<LookupListItem>> ListCurrenciesAsync(CancellationToken ct = default)
    {
        var companyId = CompanyId();
        return await db.Currencies.Where(c => !c.IsDeleted && c.CompanyId == companyId)
            .OrderBy(c => c.Code)
            .Select(c => new LookupListItem(c.Id, c.Code, c.Description, c.Status))
            .ToListAsync(ct);
    }

    public async Task<LookupListItem> CreateCurrencyAsync(SaveLookupRequest request, string symbol, bool isDefault, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var code = request.Code.Trim().ToUpperInvariant();
        if (await db.Currencies.AnyAsync(c => c.CompanyId == companyId && c.Code == code && !c.IsDeleted, ct))
            throw new InvalidOperationException($"Valuta '{code}' gia in uso.");
        if (isDefault)
        {
            var existing = await db.Currencies.Where(c => c.CompanyId == companyId && c.IsDefault && !c.IsDeleted).ToListAsync(ct);
            foreach (var c in existing) c.IsDefault = false;
        }
        var entity = new Currency
        {
            CompanyId = companyId,
            Code = code,
            Description = request.Description.Trim(),
            Symbol = symbol,
            IsDefault = isDefault,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUser.UserId ?? 0
        };
        db.Currencies.Add(entity);
        await db.SaveChangesAsync(ct);
        return new LookupListItem(entity.Id, entity.Code, entity.Description, entity.Status);
    }

    public async Task<int> SeedLookupsAsync(CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var actor = currentUser.UserId ?? 0;
        var now = DateTime.UtcNow;
        var added = 0;

        if (!await db.UnitsOfMeasure.AnyAsync(u => u.CompanyId == companyId && !u.IsDeleted, ct))
        {
            db.UnitsOfMeasure.AddRange(
                new UnitOfMeasure { CompanyId = companyId, Code = "NR", Description = "Numero", CreatedAt = now, CreatedBy = actor },
                new UnitOfMeasure { CompanyId = companyId, Code = "KG", Description = "Chilogrammo", DecimalPlaces = 3, CreatedAt = now, CreatedBy = actor },
                new UnitOfMeasure { CompanyId = companyId, Code = "MT", Description = "Metro", DecimalPlaces = 2, CreatedAt = now, CreatedBy = actor });
            added += 3;
        }

        if (!await db.Currencies.AnyAsync(c => c.CompanyId == companyId && !c.IsDeleted, ct))
        {
            db.Currencies.Add(new Currency { CompanyId = companyId, Code = "EUR", Description = "Euro", Symbol = "€", IsDefault = true, CreatedAt = now, CreatedBy = actor });
            added++;
        }

        if (!await db.PortTypes.AnyAsync(p => p.CompanyId == companyId && !p.IsDeleted, ct))
        {
            db.PortTypes.AddRange(
                new PortType { CompanyId = companyId, Code = "FR", Description = "Franco", Charge = PortCharge.Franco, CreatedAt = now, CreatedBy = actor },
                new PortType { CompanyId = companyId, Code = "AS", Description = "Assegnato", Charge = PortCharge.Assegnato, CreatedAt = now, CreatedBy = actor });
            added += 2;
        }

        if (!await db.DocumentTypes.AnyAsync(d => d.CompanyId == companyId && !d.IsDeleted, ct))
        {
            db.DocumentTypes.AddRange(
                new DocumentType { CompanyId = companyId, Code = "OV", Description = "Ordine cliente", Category = DocumentCategory.SalesOrder, NumberingPrefix = "OV", CreatedAt = now, CreatedBy = actor },
                new DocumentType { CompanyId = companyId, Code = "ODA", Description = "Ordine fornitore", Category = DocumentCategory.PurchaseOrder, NumberingPrefix = "ODA", CreatedAt = now, CreatedBy = actor });
            added += 2;
        }

        if (added > 0) await db.SaveChangesAsync(ct);
        return added;
    }

    private async Task<UnitOfMeasure> CreateCodeEntityAsync(DbSet<UnitOfMeasure> set, SaveLookupRequest request, CancellationToken ct)
    {
        var companyId = CompanyId();
        var code = request.Code.Trim().ToUpperInvariant();
        if (await set.AnyAsync(x => x.CompanyId == companyId && x.Code == code && !x.IsDeleted, ct))
            throw new InvalidOperationException($"Codice '{code}' gia in uso.");
        var entity = new UnitOfMeasure
        {
            CompanyId = companyId, Code = code, Description = request.Description.Trim(),
            Status = "Active", CreatedAt = DateTime.UtcNow, CreatedBy = currentUser.UserId ?? 0
        };
        set.Add(entity);
        return entity;
    }

    private async Task<Zone> CreateCodeEntityAsync(DbSet<Zone> set, SaveLookupRequest request, CancellationToken ct)
    {
        var companyId = CompanyId();
        var code = request.Code.Trim().ToUpperInvariant();
        if (await set.AnyAsync(x => x.CompanyId == companyId && x.Code == code && !x.IsDeleted, ct))
            throw new InvalidOperationException($"Codice '{code}' gia in uso.");
        var entity = new Zone
        {
            CompanyId = companyId, Code = code, Description = request.Description.Trim(),
            Status = "Active", CreatedAt = DateTime.UtcNow, CreatedBy = currentUser.UserId ?? 0
        };
        set.Add(entity);
        return entity;
    }

    private async Task<Carrier> CreateCodeEntityAsync(DbSet<Carrier> set, SaveLookupRequest request, CancellationToken ct)
    {
        var companyId = CompanyId();
        var code = request.Code.Trim().ToUpperInvariant();
        if (await set.AnyAsync(x => x.CompanyId == companyId && x.Code == code && !x.IsDeleted, ct))
            throw new InvalidOperationException($"Codice '{code}' gia in uso.");
        var entity = new Carrier
        {
            CompanyId = companyId, Code = code, Description = request.Description.Trim(),
            Status = "Active", CreatedAt = DateTime.UtcNow, CreatedBy = currentUser.UserId ?? 0
        };
        set.Add(entity);
        return entity;
    }

    private async Task<PortType> CreateCodeEntityAsync(DbSet<PortType> set, SaveLookupRequest request, CancellationToken ct)
    {
        var companyId = CompanyId();
        var code = request.Code.Trim().ToUpperInvariant();
        if (await set.AnyAsync(x => x.CompanyId == companyId && x.Code == code && !x.IsDeleted, ct))
            throw new InvalidOperationException($"Codice '{code}' gia in uso.");
        var entity = new PortType
        {
            CompanyId = companyId, Code = code, Description = request.Description.Trim(),
            Status = "Active", CreatedAt = DateTime.UtcNow, CreatedBy = currentUser.UserId ?? 0
        };
        set.Add(entity);
        return entity;
    }

    private long CompanyId() => currentUser.CompanyId ?? throw new InvalidOperationException("Contesto azienda mancante.");
}
