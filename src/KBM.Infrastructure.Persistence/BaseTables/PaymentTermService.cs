using KBM.Application.BaseTables;
using KBM.Application.Security;
using KBM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KBM.Infrastructure.Persistence.BaseTables;

public sealed class PaymentTermService(KbmDbContext db, ICurrentUserContext currentUser) : IPaymentTermService
{
    public async Task<IReadOnlyList<PaymentTermListItem>> ListAsync(string? search = null, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var query = db.PaymentTerms.Where(p => !p.IsDeleted && p.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p => p.Code.Contains(term) || p.Description.Contains(term));
        }
        return await query.OrderBy(p => p.Code)
            .Select(p => new PaymentTermListItem(p.Id, p.Code, p.Description, p.InstallmentsCount,
                p.FirstDueDays, p.IntervalDays, p.EndOfMonth, p.PaymentMethod, p.Status))
            .ToListAsync(ct);
    }

    public async Task<PaymentTermDetail?> GetAsync(long id, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var p = await db.PaymentTerms.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        return p is null ? null : Map(p);
    }

    public async Task<PaymentTermDetail> CreateAsync(CreatePaymentTermRequest request, CancellationToken ct = default)
    {
        var actorId = currentUser.UserId ?? 0;
        var companyId = CompanyId();
        var code = request.Code.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code)) throw new InvalidOperationException("Il codice condizione di pagamento e obbligatorio.");
        if (await db.PaymentTerms.AnyAsync(p => p.Code == code && p.CompanyId == companyId && !p.IsDeleted, ct))
            throw new InvalidOperationException("Codice condizione di pagamento gia in uso.");

        var entity = new PaymentTerm
        {
            CompanyId = companyId,
            Code = code,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = actorId
        };
        Apply(entity, request.Description, request.InstallmentsCount, request.FirstDueDays,
            request.IntervalDays, request.EndOfMonth, request.PaymentMethod, request.Notes);
        db.PaymentTerms.Add(entity);
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<PaymentTermDetail?> UpdateAsync(long id, UpdatePaymentTermRequest request, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var entity = await db.PaymentTerms.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        if (entity is null) return null;
        Apply(entity, request.Description, request.InstallmentsCount, request.FirstDueDays,
            request.IntervalDays, request.EndOfMonth, request.PaymentMethod, request.Notes);
        entity.Status = request.Status;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.UserId;
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<bool> SetEnabledAsync(long id, bool enabled, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var entity = await db.PaymentTerms.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        if (entity is null) return false;
        entity.Status = enabled ? "Active" : "Inactive";
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.UserId;
        await db.SaveChangesAsync(ct);
        return true;
    }

    private long CompanyId() => currentUser.CompanyId ?? throw new InvalidOperationException("Contesto azienda mancante.");

    private static void Apply(PaymentTerm p, string description, int installments, int firstDueDays,
        int intervalDays, bool endOfMonth, string? paymentMethod, string? notes)
    {
        p.Description = description.Trim();
        p.InstallmentsCount = installments < 1 ? 1 : installments;
        p.FirstDueDays = firstDueDays < 0 ? 0 : firstDueDays;
        p.IntervalDays = intervalDays < 0 ? 0 : intervalDays;
        p.EndOfMonth = endOfMonth;
        p.PaymentMethod = Clean(paymentMethod);
        p.Notes = Clean(notes);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static PaymentTermDetail Map(PaymentTerm p) => new(
        p.Id, p.Code, p.Description, p.InstallmentsCount, p.FirstDueDays, p.IntervalDays,
        p.EndOfMonth, p.PaymentMethod, p.Notes, p.Status);
}
