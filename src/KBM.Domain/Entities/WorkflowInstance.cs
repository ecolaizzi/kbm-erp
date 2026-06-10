using KBM.Domain.Common;

namespace KBM.Domain.Entities;

/// <summary>Istanza di processo: esecuzione di un modello di workflow.</summary>
public class WorkflowInstance : AuditableTenantEntity
{
    public long WorkflowDefinitionId { get; set; }
    public WorkflowDefinition? Definition { get; set; }

    public string Number { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    public WorkflowInstanceState State { get; set; } = WorkflowInstanceState.Open;

    /// <summary>Oggetto gestionale collegato (es. "purchase-request" + id) per integrazione moduli.</summary>
    public string? LinkedEntityType { get; set; }
    public long? LinkedEntityId { get; set; }

    public int CurrentStepOrder { get; set; }

    /// <summary>Valori dei campi configurabili (JSON dizionario Key→Value).</summary>
    public string? FieldValuesJson { get; set; }

    public long StartedBy { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<WorkflowTask> Tasks { get; set; } = [];
    public ICollection<WorkflowEvent> Events { get; set; } = [];
}
