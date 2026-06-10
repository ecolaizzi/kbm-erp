using KBM.Domain.Common;

namespace KBM.Domain.Entities;

/// <summary>
/// Modello di workflow (template). Definisce un processo aziendale guidato:
/// step ordinati con approvazioni, campi configurabili e azioni eseguibili.
/// </summary>
public class WorkflowDefinition : AuditableTenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Tipo di oggetto gestionale collegabile (es. "purchase-request", "rfq"); vuoto = generico.</summary>
    public string? TargetEntityType { get; set; }

    /// <summary>Draft | Active | Archived.</summary>
    public string Status { get; set; } = "Draft";

    /// <summary>Definizione dei campi configurabili (JSON: lista di {Key,Label,Type,Required,SetAtRuntime}).</summary>
    public string? FieldsJson { get; set; }

    public ICollection<WorkflowStep> Steps { get; set; } = [];
}
