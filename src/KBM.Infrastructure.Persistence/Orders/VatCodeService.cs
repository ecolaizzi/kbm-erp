using KBM.Application.Orders;
using KBM.Application.Security;
using KBM.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;

namespace KBM.Infrastructure.Persistence.Orders;

public sealed class VatCodeService(KbmDbContext db, ICurrentUserContext currentUser) : IVatCodeService
{
    public async Task<IReadOnlyList<VatCodeListItem>> ListAsync(string? search = null, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var q = db.VatCodes.Where(v => !v.IsDeleted && v.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            q = q.Where(v => v.Code.Contains(term) || v.Description.Contains(term));
        }
        return await q.OrderBy(v => v.Code)
            .Select(v => new VatCodeListItem(v.Id, v.Code, v.Description, v.Rate, v.NatureCode, v.Status))
            .ToListAsync(ct);
    }

    public async Task<VatCodeDetail?> GetAsync(long id, CancellationToken ct = default)
    {
        var v = await FindAsync(id, ct);
        return v is null ? null : Map(v);
    }

    public async Task<VatCodeDetail> CreateAsync(SaveVatCodeRequest request, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var code = request.Code.Trim().ToUpperInvariant();
        if (await db.VatCodes.AnyAsync(v => v.CompanyId == companyId && v.Code == code && !v.IsDeleted, ct))
            throw new InvalidOperationException($"Codice IVA '{code}' gia in uso.");
        var entity = new VatCode
        {
            CompanyId = companyId,
            Code = code,
            Description = request.Description.Trim(),
            Rate = request.Rate,
            NatureCode = Clean(request.NatureCode),
            DeductibilityPercent = request.DeductibilityPercent,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUser.UserId ?? 0
        };
        db.VatCodes.Add(entity);
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<VatCodeDetail?> UpdateAsync(long id, SaveVatCodeRequest request, CancellationToken ct = default)
    {
        var v = await FindAsync(id, ct);
        if (v is null) return null;
        v.Description = request.Description.Trim();
        v.Rate = request.Rate;
        v.NatureCode = Clean(request.NatureCode);
        v.DeductibilityPercent = request.DeductibilityPercent;
        v.UpdatedAt = DateTime.UtcNow;
        v.UpdatedBy = currentUser.UserId;
        await db.SaveChangesAsync(ct);
        return Map(v);
    }

    public async Task<bool> SetEnabledAsync(long id, bool enabled, CancellationToken ct = default)
    {
        var v = await FindAsync(id, ct);
        if (v is null) return false;
        v.Status = enabled ? "Active" : "Inactive";
        v.UpdatedAt = DateTime.UtcNow;
        v.UpdatedBy = currentUser.UserId;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<int> SeedStandardAsync(CancellationToken ct = default)
    {
        var companyId = CompanyId();
        if (await db.VatCodes.AnyAsync(v => v.CompanyId == companyId && !v.IsDeleted, ct)) return 0;
        var actor = currentUser.UserId ?? 0;
        var now = DateTime.UtcNow;
        var items = new[]
        {
            new VatCode { CompanyId = companyId, Code = "22", Description = "IVA ordinaria 22%", Rate = 22m, CreatedAt = now, CreatedBy = actor },
            new VatCode { CompanyId = companyId, Code = "10", Description = "IVA agevolata 10%", Rate = 10m, CreatedAt = now, CreatedBy = actor },
            new VatCode { CompanyId = companyId, Code = "04", Description = "IVA super-ridotta 4%", Rate = 4m, CreatedAt = now, CreatedBy = actor },
            new VatCode { CompanyId = companyId, Code = "00", Description = "Esente / fuori campo", Rate = 0m, NatureCode = "N4", CreatedAt = now, CreatedBy = actor },
        };
        db.VatCodes.AddRange(items);
        await db.SaveChangesAsync(ct);
        return items.Length;
    }

    private async Task<VatCode?> FindAsync(long id, CancellationToken ct)
    {
        var companyId = CompanyId();
        return await db.VatCodes.FirstOrDefaultAsync(v => v.Id == id && v.CompanyId == companyId && !v.IsDeleted, ct);
    }

    private long CompanyId() => currentUser.CompanyId ?? throw new InvalidOperationException("Contesto azienda mancante.");
    private static string? Clean(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();
    private static VatCodeDetail Map(VatCode v) => new(v.Id, v.Code, v.Description, v.Rate, v.NatureCode, v.DeductibilityPercent, v.Status);
}
