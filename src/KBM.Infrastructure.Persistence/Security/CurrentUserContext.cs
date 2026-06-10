using System.Security.Claims;
using KBM.Application.Security;
using Microsoft.AspNetCore.Http;

namespace KBM.Infrastructure.Persistence.Security;

public sealed class CurrentUserContext(IHttpContextAccessor http) : ICurrentUserContext
{
    public long? UserId => ParseLong(
        http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? http.HttpContext?.User.FindFirst("sub")?.Value);

    public long? CompanyId => ParseLong(http.HttpContext?.User.FindFirstValue("company_id"));

    public string? IpAddress => http.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent => http.HttpContext?.Request.Headers.UserAgent.ToString();

    public string? CorrelationId => http.HttpContext?.TraceIdentifier;

    public bool IsAuthenticated => http.HttpContext?.User.Identity?.IsAuthenticated == true;

    private static long? ParseLong(string? value) =>
        long.TryParse(value, out var id) ? id : null;
}
