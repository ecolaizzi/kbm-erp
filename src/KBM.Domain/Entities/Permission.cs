namespace KBM.Domain.Entities;

public class Permission
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
