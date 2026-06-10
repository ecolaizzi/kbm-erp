namespace KBM.Domain.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; }
    public long? UserId { get; set; }
    public long? CompanyId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public long? EntityId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? CorrelationId { get; set; }
}
