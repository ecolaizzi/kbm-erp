using KBM.Domain.Common;

namespace KBM.Domain.Entities;

public class User : AuditableEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public bool MfaEnabled { get; set; }
    public string? MfaSecret { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }
    public DateTime PasswordChangedAt { get; set; } = DateTime.UtcNow;
    public string PreferredLanguage { get; set; } = "it-IT";

    public ICollection<UserCompany> UserCompanies { get; set; } = [];
    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
