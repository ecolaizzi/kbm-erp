using KBM.Domain.Common;

namespace KBM.Domain.Entities;

/// <summary>Evento del flusso di un processo (storico attivit&#224;, note, transizioni).</summary>
public class WorkflowEvent : AuditableTenantEntity
{
    public long WorkflowInstanceId { get; set; }
    public WorkflowInstance? Instance { get; set; }

    public long? WorkflowTaskId { get; set; }

    /// <summary>Started | Completed | Rejected | TakenCharge | Released | Note | Action | Cancelled | Suspended | Reactivated.</summary>
    public string Action { get; set; } = string.Empty;

    public long? ActorUserId { get; set; }
    public string? Notes { get; set; }
    public DateTime Timestamp { get; set; }
}
