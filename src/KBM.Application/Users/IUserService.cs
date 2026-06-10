namespace KBM.Application.Users;

public interface IUserService
{
    Task<PagedResult<UserListItem>> ListAsync(UserListQuery query, CancellationToken ct = default);
    Task<UserDetail?> GetAsync(long id, CancellationToken ct = default);
    Task<UserDetail> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<UserDetail?> UpdateAsync(long id, UpdateUserRequest request, CancellationToken ct = default);
    Task<bool> DisableAsync(long id, CancellationToken ct = default);
    Task<bool> EnableAsync(long id, CancellationToken ct = default);
}
