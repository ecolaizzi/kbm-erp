using KBM.Application.Companies;
using KBM.Application.Security;
using KBM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KBM.Infrastructure.Persistence.Companies;

public sealed class CompanyService(KbmDbContext db, ICurrentUserContext currentUser) : ICompanyService
{
    public async Task<IReadOnlyList<CompanyListItem>> ListAsync(CancellationToken ct = default)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("Utente non autenticato.");

        return await db.Companies
            .Where(c => !c.IsDeleted)
            .Where(c => c.UserCompanies.Any(uc => uc.UserId == userId))
            .OrderBy(c => c.BusinessName)
            .Select(c => new CompanyListItem(c.Id, c.Code, c.BusinessName, c.Status))
            .ToListAsync(ct);
    }

    public async Task<CompanyDetail?> GetAsync(long id, CancellationToken ct = default)
    {
        var company = await db.Companies.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);
        return company is null ? null : Map(company);
    }

    public async Task<CompanyDetail> CreateAsync(CreateCompanyRequest request, CancellationToken ct = default)
    {
        var actorId = currentUser.UserId ?? 0;
        var code = request.Code.Trim().ToUpperInvariant();

        if (await db.Companies.AnyAsync(c => c.Code == code && !c.IsDeleted, ct))
            throw new InvalidOperationException("Codice azienda gia in uso.");

        var company = new Company
        {
            Code = code,
            BusinessName = request.BusinessName.Trim(),
            LegalName = request.LegalName?.Trim(),
            VatNumber = request.VatNumber?.Trim(),
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = actorId
        };
        db.Companies.Add(company);
        await db.SaveChangesAsync(ct);

        if (actorId > 0)
        {
            db.UserCompanies.Add(new UserCompany
            {
                UserId = actorId,
                CompanyId = company.Id,
                IsDefault = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = actorId
            });

            var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.IsSystem && r.Code == "Admin", ct);
            if (adminRole is not null)
            {
                db.UserRoles.Add(new UserRole
                {
                    UserId = actorId,
                    RoleId = adminRole.Id,
                    CompanyId = company.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = actorId
                });
            }

            await db.SaveChangesAsync(ct);
        }

        return Map(company);
    }

    public async Task<CompanyDetail?> UpdateAsync(long id, UpdateCompanyRequest request, CancellationToken ct = default)
    {
        var company = await db.Companies.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);
        if (company is null) return null;

        company.BusinessName = request.BusinessName.Trim();
        company.LegalName = request.LegalName?.Trim();
        company.VatNumber = request.VatNumber?.Trim();
        company.Status = request.Status;
        company.UpdatedAt = DateTime.UtcNow;
        company.UpdatedBy = currentUser.UserId;
        await db.SaveChangesAsync(ct);
        return Map(company);
    }

    private static CompanyDetail Map(Company c) => new(
        c.Id, c.Code, c.BusinessName, c.LegalName, c.VatNumber, c.Status);
}
