using KBM.Application.Auth;
using KBM.Application.Setup;
using KBM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KBM.Infrastructure.Persistence.Setup;

public sealed class SetupService(KbmDbContext db, IPasswordHasher passwordHasher) : ISetupService
{
    public const string SetupCompletedKey = "Setup.Completed";

    public async Task<SetupStatusResponse> GetStatusAsync(CancellationToken ct = default)
    {
        var completed = await IsSetupCompletedAsync(ct);
        var hasUsers = await db.Users.AnyAsync(ct);
        var required = !completed && !hasUsers;
        return new SetupStatusResponse(
            required,
            completed || hasUsers,
            required ? "Configurazione iniziale richiesta." : null);
    }

    public async Task<SetupCompleteResponse> CompleteAsync(SetupCompleteRequest request, CancellationToken ct = default)
    {
        if (await db.Users.AnyAsync(ct))
            throw new InvalidOperationException("Setup gia completato.");

        if (string.IsNullOrWhiteSpace(request.CompanyCode) || string.IsNullOrWhiteSpace(request.BusinessName))
            throw new ArgumentException("Dati azienda obbligatori.");

        if (request.AdminPassword.Length < 8)
            throw new ArgumentException("Password admin minimo 8 caratteri.");

        var company = new Company
        {
            Code = request.CompanyCode.Trim().ToUpperInvariant(),
            BusinessName = request.BusinessName.Trim(),
            VatNumber = request.VatNumber?.Trim(),
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 0
        };
        db.Companies.Add(company);

        var user = new User
        {
            Username = request.AdminUsername.Trim(),
            Email = request.AdminEmail.Trim(),
            PasswordHash = passwordHasher.Hash(request.AdminPassword),
            FirstName = request.AdminFirstName.Trim(),
            LastName = request.AdminLastName.Trim(),
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 0,
            PasswordChangedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        db.UserCompanies.Add(new UserCompany
        {
            UserId = user.Id,
            CompanyId = company.Id,
            IsDefault = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user.Id
        });
        await db.SaveChangesAsync(ct);

        var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.IsSystem && r.Code == "Admin", ct);
        if (adminRole is not null)
        {
            db.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = adminRole.Id,
                CompanyId = company.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user.Id
            });
        }

        await SetSettingAsync(SetupCompletedKey, "true", ct);
        await db.SaveChangesAsync(ct);

        return new SetupCompleteResponse(company.Id, user.Id, "Setup completato con successo.");
    }

    public async Task MarkCompletedIfLegacyDataAsync(CancellationToken ct = default)
    {
        if (await IsSetupCompletedAsync(ct)) return;
        if (!await db.Users.AnyAsync(ct)) return;
        await SetSettingAsync(SetupCompletedKey, "true", ct);
        await db.SaveChangesAsync(ct);
    }

    private async Task<bool> IsSetupCompletedAsync(CancellationToken ct)
    {
        var val = await db.SystemSettings
            .Where(s => s.Key == SetupCompletedKey)
            .Select(s => s.Value)
            .FirstOrDefaultAsync(ct);
        return val == "true";
    }

    private async Task SetSettingAsync(string key, string value, CancellationToken ct)
    {
        var existing = await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key, ct);
        if (existing is null)
        {
            db.SystemSettings.Add(new SystemSetting
            {
                Key = key,
                Value = value,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.Value = value;
            existing.UpdatedAt = DateTime.UtcNow;
        }
    }
}
