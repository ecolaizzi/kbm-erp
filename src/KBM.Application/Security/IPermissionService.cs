namespace KBM.Application.Security;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(long userId, long companyId, string permissionCode, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetPermissionsAsync(long userId, long companyId, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetRoleCodesAsync(long userId, long companyId, CancellationToken ct = default);
}
