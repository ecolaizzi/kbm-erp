namespace KBM.Application.Auth;

public record LoginRequest(string Username, string Password, long? CompanyId = null);

public record CompanyOption(long Id, string Code, string BusinessName);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    long UserId,
    string Username,
    string DisplayName,
    long CompanyId,
    string CompanyName,
    IReadOnlyList<string> Roles);

public record LoginResult(
    bool RequiresCompanySelection,
    LoginResponse? Session,
    IReadOnlyList<CompanyOption>? Companies);

public record RefreshRequest(string RefreshToken, long CompanyId);

public record LogoutRequest(string RefreshToken);

public record ApiError(string Code, string Message);
