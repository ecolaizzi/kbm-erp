using KBM.Application.Security;
using Microsoft.EntityFrameworkCore;

namespace KBM.Infrastructure.Persistence.Security;

public sealed class PermissionService(KbmDbContext db) : IPermissionService
{
    public async Task<bool> HasPermissionAsync(long userId, long companyId, string permissionCode, CancellationToken ct = default)
    {
        var permissions = await GetPermissionsAsync(userId, companyId, ct);
        return permissions.Contains(permissionCode);
    }

    public async Task<IReadOnlyList<string>> GetPermissionsAsync(long userId, long companyId, CancellationToken ct = default)
    {
        return await db.UserRoles
            .Where(ur => ur.UserId == userId && ur.CompanyId == companyId)
            .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.Code))
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetRoleCodesAsync(long userId, long companyId, CancellationToken ct = default)
    {
        return await db.UserRoles
            .Where(ur => ur.UserId == userId && ur.CompanyId == companyId)
            .Select(ur => ur.Role.Code)
            .Distinct()
            .ToListAsync(ct);
    }
}
