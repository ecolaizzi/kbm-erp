namespace KBM.Domain.Entities;

public class RefreshToken
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long CompanyId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevocationReason { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public User User { get; set; } = null!;
}
