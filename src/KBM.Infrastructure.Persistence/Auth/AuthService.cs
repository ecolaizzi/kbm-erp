using KBM.Application.Auth;

using KBM.Application.Security;

using KBM.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Configuration;



namespace KBM.Infrastructure.Persistence.Auth;



public sealed class AuthService(

    KbmDbContext db,

    IPasswordHasher passwordHasher,

    IJwtTokenService jwt,

    IPermissionService permissions,

    IConfiguration configuration) : IAuthService

{

    private const int MaxFailedAttempts = 5;

    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);



    public async Task<LoginResult?> LoginAsync(LoginRequest request, string? ip, string? userAgent, CancellationToken ct = default)

    {

        var user = await db.Users

            .Include(u => u.UserCompanies)

            .ThenInclude(uc => uc.Company)

            .FirstOrDefaultAsync(u => u.Username == request.Username && !u.IsDeleted, ct);



        if (user is null) return null;



        if (user.Status != "Active")

            throw new InvalidOperationException("Account disabilitato, contattare amministratore.");



        if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)

            return null;



        if (!passwordHasher.Verify(request.Password, user.PasswordHash))

        {

            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= MaxFailedAttempts)

                user.LockedUntil = DateTime.UtcNow.Add(LockoutDuration);

            await db.SaveChangesAsync(ct);

            return null;

        }



        user.FailedLoginAttempts = 0;

        user.LockedUntil = null;

        user.LastLoginAt = DateTime.UtcNow;



        var activeCompanies = user.UserCompanies

            .Where(uc => uc.Company.Status == "Active")

            .ToList();



        if (activeCompanies.Count == 0) return null;



        // La scelta dell'azienda avviene sempre in fase di login (seconda modale),

        // anche con una sola azienda: evita di operare per sbaglio sull'azienda errata.

        if (!request.CompanyId.HasValue)

        {

            await db.SaveChangesAsync(ct);

            var options = activeCompanies

                .Select(uc => new CompanyOption(uc.CompanyId, uc.Company.Code, uc.Company.BusinessName))

                .ToList();

            return new LoginResult(true, null, options);

        }



        var companyLink = ResolveCompany(activeCompanies, request.CompanyId);

        if (companyLink is null) return null;



        var session = await IssueTokensAsync(user, companyLink.Company, ip, userAgent, ct);

        return new LoginResult(false, session, null);

    }



    public async Task<LoginResponse?> RefreshAsync(RefreshRequest request, string? ip, string? userAgent, CancellationToken ct = default)

    {

        var hash = jwt.HashRefreshToken(request.RefreshToken);

        var stored = await db.RefreshTokens

            .Include(r => r.User)

            .ThenInclude(u => u.UserCompanies)

            .ThenInclude(uc => uc.Company)

            .FirstOrDefaultAsync(r => r.TokenHash == hash && r.RevokedAt == null && r.ExpiresAt > DateTime.UtcNow, ct);



        if (stored is null) return null;



        var company = stored.User.UserCompanies.FirstOrDefault(uc => uc.CompanyId == request.CompanyId)?.Company;

        if (company is null || company.Status != "Active") return null;



        stored.RevokedAt = DateTime.UtcNow;

        stored.RevocationReason = "Rotated";

        return await IssueTokensAsync(stored.User, company, ip, userAgent, ct);

    }



    public async Task LogoutAsync(LogoutRequest request, CancellationToken ct = default)

    {

        var hash = jwt.HashRefreshToken(request.RefreshToken);

        var stored = await db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash && r.RevokedAt == null, ct);

        if (stored is null) return;

        stored.RevokedAt = DateTime.UtcNow;

        stored.RevocationReason = "Logout";

        await db.SaveChangesAsync(ct);

    }



    private static UserCompany? ResolveCompany(IReadOnlyList<UserCompany> links, long? companyId)

    {

        if (companyId.HasValue)

            return links.FirstOrDefault(uc => uc.CompanyId == companyId.Value);

        return links.FirstOrDefault(uc => uc.IsDefault) ?? links.FirstOrDefault();

    }



    private async Task<LoginResponse> IssueTokensAsync(User user, Company company, string? ip, string? userAgent, CancellationToken ct)

    {

        var refreshPlain = jwt.CreateRefreshToken();

        var refreshDays = int.Parse(configuration["Jwt:RefreshTokenDays"] ?? "7");

        var expiresAt = DateTime.UtcNow.AddMinutes(int.Parse(configuration["Jwt:AccessTokenMinutes"] ?? "15"));



        db.RefreshTokens.Add(new RefreshToken

        {

            UserId = user.Id,

            CompanyId = company.Id,

            TokenHash = jwt.HashRefreshToken(refreshPlain),

            IssuedAt = DateTime.UtcNow,

            ExpiresAt = DateTime.UtcNow.AddDays(refreshDays),

            IpAddress = ip,

            UserAgent = userAgent

        });



        await db.SaveChangesAsync(ct);



        var roles = await permissions.GetRoleCodesAsync(user.Id, company.Id, ct);

        var access = jwt.CreateAccessToken(user, company.Id, roles);

        return new LoginResponse(

            access,

            refreshPlain,

            expiresAt,

            user.Id,

            user.Username,

            $"{user.FirstName} {user.LastName}".Trim(),

            company.Id,

            company.BusinessName,

            roles);

    }

}


