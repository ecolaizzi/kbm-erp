namespace KBM.Application.Audit;

public record AuditLogItem(
    long Id,
    DateTime Timestamp,
    long? UserId,
    long? CompanyId,
    string Action,
    string? EntityType,
    long? EntityId,
    string? IpAddress);

public record AuditLogDetail(
    long Id,
    DateTime Timestamp,
    long? UserId,
    long? CompanyId,
    string Action,
    string? EntityType,
    long? EntityId,
    string? OldValue,
    string? NewValue,
    string? IpAddress,
    string? UserAgent,
    string? CorrelationId);

public record AuditListQuery(
    DateTime? From = null,
    DateTime? To = null,
    long? UserId = null,
    string? Action = null,
    string? EntityType = null,
    int Page = 1,
    int PageSize = 50);
