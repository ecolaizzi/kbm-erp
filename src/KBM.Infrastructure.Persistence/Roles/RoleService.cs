using KBM.Application.Roles;
using KBM.Application.Security;
using KBM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KBM.Infrastructure.Persistence.Roles;

public sealed class RoleService(KbmDbContext db, ICurrentUserContext currentUser) : IRoleService
{
    public async Task<IReadOnlyList<RoleListItem>> ListAsync(CancellationToken ct = default)
    {
        var companyId = currentUser.CompanyId
            ?? throw new InvalidOperationException("Contesto azienda mancante.");

        return await db.Roles
            .Where(r => !r.IsDeleted && (r.IsSystem || r.CompanyId == companyId))
            .OrderBy(r => r.Name)
            .Select(r => new RoleListItem(
                r.Id,
                r.Code,
                r.Name,
                r.IsSystem,
                r.RolePermissions.Select(rp => rp.Permission.Code).ToList()))
            .ToListAsync(ct);
    }

    public async Task<RoleDetail?> GetAsync(long id, CancellationToken ct = default)
    {
        var companyId = currentUser.CompanyId
            ?? throw new InvalidOperationException("Contesto azienda mancante.");

        var role = await db.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted && (r.IsSystem || r.CompanyId == companyId), ct);

        return role is null ? null : Map(role);
    }

    public async Task<RoleDetail> CreateAsync(CreateRoleRequest request, CancellationToken ct = default)
    {
        var actorId = currentUser.UserId ?? 0;
        var companyId = currentUser.CompanyId
            ?? throw new InvalidOperationException("Contesto azienda mancante.");

        var code = request.Code.Trim().ToUpperInvariant();
        if (await db.Roles.AnyAsync(r => r.Code == code && r.CompanyId == companyId && !r.IsDeleted, ct))
            throw new InvalidOperationException("Codice ruolo gia in uso.");

        var role = new Role
        {
            CompanyId = companyId,
            Code = code,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            IsSystem = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = actorId
        };
        db.Roles.Add(role);
        await db.SaveChangesAsync(ct);
        await SyncPermissionsAsync(role, request.PermissionCodes, actorId, ct);
        await db.SaveChangesAsync(ct);

        return (await GetAsync(role.Id, ct))!;
    }

    public async Task<RoleDetail?> UpdateAsync(long id, UpdateRoleRequest request, CancellationToken ct = default)
    {
        var actorId = currentUser.UserId ?? 0;
        var companyId = currentUser.CompanyId
            ?? throw new InvalidOperationException("Contesto azienda mancante.");

        var role = await db.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted && !r.IsSystem && r.CompanyId == companyId, ct);

        if (role is null) return null;

        role.Name = request.Name.Trim();
        role.Description = request.Description?.Trim();
        role.UpdatedAt = DateTime.UtcNow;
        role.UpdatedBy = actorId;

        db.RolePermissions.RemoveRange(role.RolePermissions);
        await SyncPermissionsAsync(role, request.PermissionCodes, actorId, ct);
        await db.SaveChangesAsync(ct);

        return await GetAsync(id, ct);
    }

    private async Task SyncPermissionsAsync(Role role, IReadOnlyList<string> codes, long actorId, CancellationToken ct)
    {
        var permissions = await db.Permissions
            .Where(p => codes.Contains(p.Code))
            .ToListAsync(ct);

        foreach (var p in permissions)
        {
            db.RolePermissions.Add(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = p.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = actorId
            });
        }
    }

    private static RoleDetail Map(Role r) => new(
        r.Id, r.Code, r.Name, r.Description, r.IsSystem,
        r.RolePermissions.Select(rp => rp.Permission.Code).ToList());
}
