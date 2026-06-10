using KBM.Application.Auth;
using KBM.Application.Security;
using KBM.Application.Users;
using KBM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KBM.Infrastructure.Persistence.Users;

public sealed class UserService(
    KbmDbContext db,
    IPasswordHasher passwordHasher,
    ICurrentUserContext currentUser) : IUserService
{
    public async Task<PagedResult<UserListItem>> ListAsync(UserListQuery query, CancellationToken ct = default)
    {
        var companyId = currentUser.CompanyId
            ?? throw new InvalidOperationException("Contesto azienda mancante.");

        var q = db.Users
            .Where(u => !u.IsDeleted)
            .Where(u => u.UserCompanies.Any(uc => uc.CompanyId == companyId));

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(u =>
                u.Username.Contains(s) ||
                u.Email.Contains(s) ||
                u.FirstName.Contains(s) ||
                u.LastName.Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(u => u.Status == query.Status);

        var total = await q.CountAsync(ct);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var users = await q
            .OrderBy(u => u.Username)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Status,
                Roles = u.UserRoles
                    .Where(ur => ur.CompanyId == companyId)
                    .Select(ur => ur.Role.Code)
                    .ToList()
            })
            .ToListAsync(ct);

        var items = users.Select(u => new UserListItem(
            u.Id, u.Username, u.Email, u.FirstName, u.LastName, u.Status, u.Roles)).ToList();

        return new PagedResult<UserListItem>(items, total, page, pageSize);
    }

    public async Task<UserDetail?> GetAsync(long id, CancellationToken ct = default)
    {
        var companyId = currentUser.CompanyId
            ?? throw new InvalidOperationException("Contesto azienda mancante.");

        var user = await db.Users
            .Include(u => u.UserCompanies)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, ct);

        if (user is null || !user.UserCompanies.Any(uc => uc.CompanyId == companyId))
            return null;

        return MapDetail(user, companyId);
    }

    public async Task<UserDetail> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        ValidatePassword(request.Password);
        var actorId = currentUser.UserId ?? 0;
        var companyId = currentUser.CompanyId
            ?? throw new InvalidOperationException("Contesto azienda mancante.");

        if (await db.Users.AnyAsync(u => u.Username == request.Username && !u.IsDeleted, ct))
            throw new InvalidOperationException("Username gia in uso.");

        if (await db.Users.AnyAsync(u => u.Email == request.Email && !u.IsDeleted, ct))
            throw new InvalidOperationException("Email gia in uso.");

        var companyIds = request.CompanyIds.Count > 0
            ? request.CompanyIds.Distinct().ToList()
            : [companyId];

        var user = new User
        {
            Username = request.Username.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = passwordHasher.Hash(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = actorId,
            PasswordChangedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        foreach (var cid in companyIds)
        {
            db.UserCompanies.Add(new UserCompany
            {
                UserId = user.Id,
                CompanyId = cid,
                IsDefault = cid == companyId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = actorId
            });
        }

        await AssignRolesAsync(user.Id, companyId, request.RoleCodes, actorId, ct);
        await db.SaveChangesAsync(ct);

        return (await GetAsync(user.Id, ct))!;
    }

    public async Task<UserDetail?> UpdateAsync(long id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var actorId = currentUser.UserId ?? 0;
        var companyId = currentUser.CompanyId
            ?? throw new InvalidOperationException("Contesto azienda mancante.");

        var user = await db.Users
            .Include(u => u.UserCompanies)
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, ct);

        if (user is null || !user.UserCompanies.Any(uc => uc.CompanyId == companyId))
            return null;

        user.Email = request.Email.Trim();
        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Status = request.Status;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = actorId;

        if (request.Status != "Active")
            await RevokeTokensAsync(user.Id, ct);

        await SyncCompaniesAsync(user, request.CompanyIds, companyId, actorId, ct);
        await SyncRolesAsync(user, companyId, request.RoleCodes, actorId, ct);
        await db.SaveChangesAsync(ct);

        return await GetAsync(id, ct);
    }

    public async Task<bool> DisableAsync(long id, CancellationToken ct = default)
    {
        var companyId = currentUser.CompanyId
            ?? throw new InvalidOperationException("Contesto azienda mancante.");

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, ct);
        if (user is null || !await db.UserCompanies.AnyAsync(uc => uc.UserId == id && uc.CompanyId == companyId, ct))
            return false;

        user.Status = "Disabled";
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = currentUser.UserId;
        await RevokeTokensAsync(user.Id, ct);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> EnableAsync(long id, CancellationToken ct = default)
    {
        var companyId = currentUser.CompanyId
            ?? throw new InvalidOperationException("Contesto azienda mancante.");

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, ct);
        if (user is null || !await db.UserCompanies.AnyAsync(uc => uc.UserId == id && uc.CompanyId == companyId, ct))
            return false;

        user.Status = "Active";
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = currentUser.UserId;
        await db.SaveChangesAsync(ct);
        return true;
    }

    private async Task RevokeTokensAsync(long userId, CancellationToken ct)
    {
        var tokens = await db.RefreshTokens
            .Where(r => r.UserId == userId && r.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var t in tokens)
        {
            t.RevokedAt = DateTime.UtcNow;
            t.RevocationReason = "UserDisabled";
        }
    }

    private async Task AssignRolesAsync(long userId, long companyId, IReadOnlyList<string> roleCodes, long actorId, CancellationToken ct)
    {
        var codes = roleCodes.Count > 0 ? roleCodes : ["Operatore"];
        var roles = await db.Roles
            .Where(r => codes.Contains(r.Code) && !r.IsDeleted && (r.IsSystem || r.CompanyId == companyId))
            .ToListAsync(ct);

        foreach (var role in roles)
        {
            db.UserRoles.Add(new UserRole
            {
                UserId = userId,
                RoleId = role.Id,
                CompanyId = companyId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = actorId
            });
        }
    }

    private async Task SyncRolesAsync(User user, long companyId, IReadOnlyList<string> roleCodes, long actorId, CancellationToken ct)
    {
        var existing = user.UserRoles.Where(ur => ur.CompanyId == companyId).ToList();
        db.UserRoles.RemoveRange(existing);
        await AssignRolesAsync(user.Id, companyId, roleCodes, actorId, ct);
    }

    private async Task SyncCompaniesAsync(User user, IReadOnlyList<long> companyIds, long currentCompanyId, long actorId, CancellationToken ct)
    {
        if (companyIds.Count == 0) return;

        var desired = companyIds.Distinct().ToHashSet();
        var toRemove = user.UserCompanies.Where(uc => !desired.Contains(uc.CompanyId)).ToList();
        db.UserCompanies.RemoveRange(toRemove);

        foreach (var cid in desired)
        {
            if (user.UserCompanies.All(uc => uc.CompanyId != cid))
            {
                db.UserCompanies.Add(new UserCompany
                {
                    UserId = user.Id,
                    CompanyId = cid,
                    IsDefault = cid == currentCompanyId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = actorId
                });
            }
        }
    }

    private static UserDetail MapDetail(User user, long companyId) => new(
        user.Id,
        user.Username,
        user.Email,
        user.FirstName,
        user.LastName,
        user.Status,
        user.LastLoginAt,
        user.UserCompanies.Select(uc => uc.CompanyId).ToList(),
        user.UserRoles.Where(ur => ur.CompanyId == companyId).Select(ur => ur.Role.Code).ToList());

    private static void ValidatePassword(string password)
    {
        if (password.Length < 8)
            throw new ArgumentException("Password minimo 8 caratteri.");
        if (!password.Any(char.IsUpper))
            throw new ArgumentException("Password richiede almeno una maiuscola.");
        if (!password.Any(char.IsDigit))
            throw new ArgumentException("Password richiede almeno un numero.");
    }
}
