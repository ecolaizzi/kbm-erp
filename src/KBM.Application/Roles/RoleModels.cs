namespace KBM.Application.Roles;

public record RoleListItem(long Id, string Code, string Name, bool IsSystem, IReadOnlyList<string> Permissions);

public record RoleDetail(
    long Id,
    string Code,
    string Name,
    string? Description,
    bool IsSystem,
    IReadOnlyList<string> Permissions);

public record CreateRoleRequest(string Code, string Name, string? Description, IReadOnlyList<string> PermissionCodes);

public record UpdateRoleRequest(string Name, string? Description, IReadOnlyList<string> PermissionCodes);
