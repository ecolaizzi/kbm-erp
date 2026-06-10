namespace KBM.Application.Security;

public interface ICurrentUserContext
{
    long? UserId { get; }
    long? CompanyId { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
    string? CorrelationId { get; }
    bool IsAuthenticated { get; }
}
