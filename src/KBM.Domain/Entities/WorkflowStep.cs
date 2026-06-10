using KBM.Domain.Common;

namespace KBM.Domain.Entities;

/// <summary>Step (attivit&#224;/approvazione/decisione) di un modello di workflow.</summary>
public class WorkflowStep : AuditableTenantEntity
{
    public long WorkflowDefinitionId { get; set; }
    public WorkflowDefinition? Definition { get; set; }

    public int StepOrder { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public WorkflowStepType StepType { get; set; } = WorkflowStepType.Approval;

    public WorkflowAssigneeType AssigneeType { get; set; } = WorkflowAssigneeType.Role;
    public long? AssigneeUserId { get; set; }
    public long? AssigneeRoleId { get; set; }

    /// <summary>Giorni di SLA dalla creazione del task (per scadenze/previsionale).</summary>
    public int? DueDays { get; set; }

    /// <summary>Per step Decisione: JSON lista di {Label, Outcome} (Outcome = Next|Reject|Complete).</summary>
    public string? DecisionOptionsJson { get; set; }

    /// <summary>Azioni eseguite al completamento (JSON: lista di {Type, Value}).</summary>
    public string? ActionsJson { get; set; }
}
