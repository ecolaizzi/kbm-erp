namespace KBM.Application.Users;

public record UserListItem(
    long Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string Status,
    IReadOnlyList<string> Roles);

public record UserDetail(
    long Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string Status,
    DateTime? LastLoginAt,
    IReadOnlyList<long> CompanyIds,
    IReadOnlyList<string> RoleCodes);

public record CreateUserRequest(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    IReadOnlyList<long> CompanyIds,
    IReadOnlyList<string> RoleCodes);

public record UpdateUserRequest(
    string Email,
    string FirstName,
    string LastName,
    string Status,
    IReadOnlyList<long> CompanyIds,
    IReadOnlyList<string> RoleCodes);

public record UserListQuery(
    string? Search = null,
    string? Status = null,
    int Page = 1,
    int PageSize = 20);

public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
