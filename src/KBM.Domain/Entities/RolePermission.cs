namespace KBM.Domain.Entities;

public class RolePermission
{
    public long Id { get; set; }
    public long RoleId { get; set; }
    public long PermissionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public long CreatedBy { get; set; }

    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
