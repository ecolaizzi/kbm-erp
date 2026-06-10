namespace KBM.Domain.Entities;

public class UserRole
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long RoleId { get; set; }
    public long CompanyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public long CreatedBy { get; set; }

    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
    public Company Company { get; set; } = null!;
}
