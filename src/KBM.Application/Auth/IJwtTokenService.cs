using KBM.Domain.Entities;

namespace KBM.Application.Auth;

public interface IJwtTokenService
{
    string CreateAccessToken(User user, long companyId, IEnumerable<string> roles);
    string CreateRefreshToken();
    string HashRefreshToken(string token);
}
