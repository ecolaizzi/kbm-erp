namespace KBM.Application.Auth;

public interface IAuthService
{
    Task<LoginResult?> LoginAsync(LoginRequest request, string? ip, string? userAgent, CancellationToken ct = default);
    Task<LoginResponse?> RefreshAsync(RefreshRequest request, string? ip, string? userAgent, CancellationToken ct = default);
    Task LogoutAsync(LogoutRequest request, CancellationToken ct = default);
}
