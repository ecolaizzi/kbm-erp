namespace KBM.Application.Roles;

public interface IRoleService
{
    Task<IReadOnlyList<RoleListItem>> ListAsync(CancellationToken ct = default);
    Task<RoleDetail?> GetAsync(long id, CancellationToken ct = default);
    Task<RoleDetail> CreateAsync(CreateRoleRequest request, CancellationToken ct = default);
    Task<RoleDetail?> UpdateAsync(long id, UpdateRoleRequest request, CancellationToken ct = default);
}
