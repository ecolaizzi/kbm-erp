using KBM.Domain.Common;

namespace KBM.Domain.Entities;

/// <summary>Task generato da uno step durante l'esecuzione di un processo.</summary>
public class WorkflowTask : AuditableTenantEntity
{
    public long WorkflowInstanceId { get; set; }
    public WorkflowInstance? Instance { get; set; }

    public long WorkflowStepId { get; set; }
    public int StepOrder { get; set; }
    public string StepName { get; set; } = string.Empty;
    public WorkflowStepType StepType { get; set; }

    public WorkflowTaskState State { get; set; } = WorkflowTaskState.Open;

    public WorkflowAssigneeType AssigneeType { get; set; }
    public long? AssigneeUserId { get; set; }
    public long? AssigneeRoleId { get; set; }

    /// <summary>Utente che ha preso in carico il task (per assegnazioni a ruolo).</summary>
    public long? TakenByUserId { get; set; }

    public DateTime? DueDate { get; set; }

    /// <summary>Esito scelto (per step Decisione).</summary>
    public string? Decision { get; set; }

    public long? CompletedByUserId { get; set; }
    public DateTime? CompletedAt { get; set; }
}
