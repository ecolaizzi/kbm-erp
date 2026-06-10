using KBM.Application.Security;
using KBM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KBM.Infrastructure.Persistence.Seeding;

public sealed class RbacSeedService(KbmDbContext db)
{
    private static readonly (string Code, string Name, string[] Permissions)[] SystemRoles =
    [
        ("Admin", "Amministratore", PermissionCodes.All),
        ("Manager", "Manager", [
            PermissionCodes.UsersRead, PermissionCodes.UsersCreate, PermissionCodes.UsersEdit,
            PermissionCodes.RolesRead,
            PermissionCodes.CompaniesRead, PermissionCodes.CompaniesEdit,
            PermissionCodes.AuditRead,
            PermissionCodes.CustomersRead, PermissionCodes.CustomersCreate,
            PermissionCodes.CustomersEdit, PermissionCodes.CustomersDelete,
            PermissionCodes.SuppliersRead, PermissionCodes.SuppliersCreate,
            PermissionCodes.SuppliersEdit, PermissionCodes.SuppliersDelete,
            PermissionCodes.ItemsRead, PermissionCodes.ItemsCreate,
            PermissionCodes.ItemsEdit, PermissionCodes.ItemsDelete,
            PermissionCodes.PaymentTermsRead, PermissionCodes.PaymentTermsCreate,
            PermissionCodes.PaymentTermsEdit, PermissionCodes.PaymentTermsDelete,
            PermissionCodes.ChartAccountsRead, PermissionCodes.ChartAccountsManage,
            PermissionCodes.OrdersSetupRead, PermissionCodes.OrdersSetupManage,
            PermissionCodes.SalesOrdersRead, PermissionCodes.SalesOrdersCreate,
            PermissionCodes.SalesOrdersEdit, PermissionCodes.SalesOrdersDelete,
            PermissionCodes.PurchaseOrdersRead, PermissionCodes.PurchaseOrdersCreate,
            PermissionCodes.PurchaseOrdersEdit, PermissionCodes.PurchaseOrdersDelete,
            PermissionCodes.PriceListsRead, PermissionCodes.PriceListsManage,
            PermissionCodes.PurchaseRequestsRead, PermissionCodes.PurchaseRequestsCreate,
            PermissionCodes.PurchaseRequestsEdit, PermissionCodes.PurchaseRequestsDelete,
            PermissionCodes.RfqRead, PermissionCodes.RfqCreate,
            PermissionCodes.RfqEdit, PermissionCodes.RfqDelete,
            PermissionCodes.WorkflowRead, PermissionCodes.WorkflowParticipate, PermissionCodes.WorkflowStart,
            PermissionCodes.WorkflowManage, PermissionCodes.WorkflowAdmin
        ]),
        ("Operatore", "Operatore", [
            PermissionCodes.UsersRead,
            PermissionCodes.CompaniesRead,
            PermissionCodes.CustomersRead, PermissionCodes.CustomersCreate, PermissionCodes.CustomersEdit,
            PermissionCodes.SuppliersRead, PermissionCodes.SuppliersCreate, PermissionCodes.SuppliersEdit,
            PermissionCodes.ItemsRead, PermissionCodes.ItemsCreate, PermissionCodes.ItemsEdit,
            PermissionCodes.PaymentTermsRead, PermissionCodes.PaymentTermsCreate, PermissionCodes.PaymentTermsEdit,
            PermissionCodes.ChartAccountsRead, PermissionCodes.ChartAccountsManage,
            PermissionCodes.OrdersSetupRead, PermissionCodes.OrdersSetupManage,
            PermissionCodes.SalesOrdersRead, PermissionCodes.SalesOrdersCreate, PermissionCodes.SalesOrdersEdit,
            PermissionCodes.PurchaseOrdersRead, PermissionCodes.PurchaseOrdersCreate, PermissionCodes.PurchaseOrdersEdit,
            PermissionCodes.PriceListsRead,
            PermissionCodes.PurchaseRequestsRead, PermissionCodes.PurchaseRequestsCreate, PermissionCodes.PurchaseRequestsEdit,
            PermissionCodes.RfqRead, PermissionCodes.RfqCreate, PermissionCodes.RfqEdit,
            PermissionCodes.WorkflowRead, PermissionCodes.WorkflowParticipate, PermissionCodes.WorkflowStart
        ]),
        ("ReadOnly", "Sola lettura", [
            PermissionCodes.UsersRead,
            PermissionCodes.RolesRead,
            PermissionCodes.CompaniesRead,
            PermissionCodes.AuditRead,
            PermissionCodes.CustomersRead,
            PermissionCodes.SuppliersRead,
            PermissionCodes.ItemsRead,
            PermissionCodes.PaymentTermsRead,
            PermissionCodes.ChartAccountsRead,
            PermissionCodes.OrdersSetupRead,
            PermissionCodes.SalesOrdersRead,
            PermissionCodes.PurchaseOrdersRead,
            PermissionCodes.PriceListsRead,
            PermissionCodes.PurchaseRequestsRead,
            PermissionCodes.RfqRead,
            PermissionCodes.WorkflowRead
        ])
    ];

    public async Task EnsureSeededAsync(CancellationToken ct = default)
    {
        await SeedPermissionsAsync(ct);
        await SeedSystemRolesAsync(ct);
        await SyncSystemRolePermissionsAsync(ct);
        await AssignAdminToExistingUsersAsync(ct);
    }

    private async Task SeedPermissionsAsync(CancellationToken ct)
    {
        var existing = await db.Permissions.Select(p => p.Code).ToListAsync(ct);
        var missing = PermissionCodes.All.Except(existing).ToList();
        if (missing.Count == 0) return;

        var permissions = missing.Select(code => new Permission
        {
            Code = code,
            Module = code.Split('.')[0],
            Description = code
        });

        db.Permissions.AddRange(permissions);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Allinea i permessi dei ruoli di sistema (gestisce nuovi permessi su DB gia seedati).</summary>
    private async Task SyncSystemRolePermissionsAsync(CancellationToken ct)
    {
        var permissionMap = await db.Permissions.ToDictionaryAsync(p => p.Code, ct);
        var now = DateTime.UtcNow;
        var changed = false;

        foreach (var (code, _, perms) in SystemRoles)
        {
            var role = await db.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.IsSystem && r.Code == code, ct);
            if (role is null) continue;

            var assigned = role.RolePermissions.Select(rp => rp.PermissionId).ToHashSet();
            foreach (var perm in perms)
            {
                if (!permissionMap.TryGetValue(perm, out var permission)) continue;
                if (assigned.Contains(permission.Id)) continue;

                db.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id,
                    CreatedAt = now,
                    CreatedBy = 0
                });
                changed = true;
            }
        }

        if (changed) await db.SaveChangesAsync(ct);
    }

    private async Task SeedSystemRolesAsync(CancellationToken ct)
    {
        if (await db.Roles.AnyAsync(r => r.IsSystem, ct)) return;

        var permissionMap = await db.Permissions.ToDictionaryAsync(p => p.Code, ct);
        var now = DateTime.UtcNow;

        foreach (var (code, name, perms) in SystemRoles)
        {
            var role = new Role
            {
                Code = code,
                Name = name,
                IsSystem = true,
                CompanyId = null,
                CreatedAt = now,
                CreatedBy = 0
            };
            db.Roles.Add(role);
            await db.SaveChangesAsync(ct);

            foreach (var perm in perms)
            {
                if (!permissionMap.TryGetValue(perm, out var permission)) continue;
                db.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id,
                    CreatedAt = now,
                    CreatedBy = 0
                });
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task AssignAdminToExistingUsersAsync(CancellationToken ct)
    {
        var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.IsSystem && r.Code == "Admin", ct);
        if (adminRole is null) return;

        var links = await db.UserCompanies
            .Where(uc => !db.UserRoles.Any(ur => ur.UserId == uc.UserId && ur.CompanyId == uc.CompanyId))
            .ToListAsync(ct);

        if (links.Count == 0) return;

        var now = DateTime.UtcNow;
        foreach (var link in links)
        {
            db.UserRoles.Add(new UserRole
            {
                UserId = link.UserId,
                RoleId = adminRole.Id,
                CompanyId = link.CompanyId,
                CreatedAt = now,
                CreatedBy = link.UserId
            });
        }

        await db.SaveChangesAsync(ct);
    }
}
