namespace KBM.Application.Workflow;

// ===================== Modelli (definizioni) =====================
public record WorkflowStepDto(
    long Id,
    int StepOrder,
    string Code,
    string Name,
    int StepType,
    int AssigneeType,
    long? AssigneeUserId,
    long? AssigneeRoleId,
    int? DueDays,
    string? DecisionOptionsJson,
    string? ActionsJson);

public record WorkflowStepInput(
    long? Id,
    int StepOrder,
    string Code,
    string Name,
    int StepType,
    int AssigneeType,
    long? AssigneeUserId,
    long? AssigneeRoleId,
    int? DueDays,
    string? DecisionOptionsJson,
    string? ActionsJson);

public record WorkflowDefinitionListItem(
    long Id,
    string Code,
    string Name,
    string? TargetEntityType,
    string Status,
    int StepCount);

public record WorkflowDefinitionDetail(
    long Id,
    string Code,
    string Name,
    string? Description,
    string? TargetEntityType,
    string Status,
    string? FieldsJson,
    IReadOnlyList<WorkflowStepDto> Steps);

public record CreateWorkflowDefinitionRequest(
    string Code,
    string Name,
    string? Description,
    string? TargetEntityType,
    string? FieldsJson,
    IReadOnlyList<WorkflowStepInput> Steps);

public record UpdateWorkflowDefinitionRequest(
    string Name,
    string? Description,
    string? TargetEntityType,
    string Status,
    string? FieldsJson,
    IReadOnlyList<WorkflowStepInput> Steps);

// ===================== Istanze / processi =====================
public record StartWorkflowRequest(
    long WorkflowDefinitionId,
    string? Title,
    string? LinkedEntityType,
    long? LinkedEntityId,
    string? FieldValuesJson);

public record WorkflowInstanceListItem(
    long Id,
    string Number,
    string Title,
    string DefinitionName,
    string State,
    string? CurrentStepName,
    DateTime StartedAt,
    string? LinkedEntityType,
    long? LinkedEntityId);

public record WorkflowTaskDto(
    long Id,
    int StepOrder,
    string StepName,
    string StepType,
    string State,
    string AssigneeType,
    long? AssigneeUserId,
    long? AssigneeRoleId,
    long? TakenByUserId,
    DateTime? DueDate,
    string? Decision,
    DateTime? CompletedAt);

public record WorkflowEventDto(
    long Id,
    string Action,
    long? ActorUserId,
    string? Notes,
    DateTime Timestamp);

public record WorkflowInstanceDetail(
    long Id,
    string Number,
    string Title,
    string DefinitionName,
    string State,
    string? LinkedEntityType,
    long? LinkedEntityId,
    int CurrentStepOrder,
    string? FieldValuesJson,
    long StartedBy,
    DateTime StartedAt,
    DateTime? CompletedAt,
    IReadOnlyList<WorkflowTaskDto> Tasks,
    IReadOnlyList<WorkflowEventDto> Events);

public record CompleteTaskRequest(string? Decision, string? Notes);
public record TaskNoteRequest(string Notes);

// ===================== Consolle =====================
public record WorkflowConsoleQuery
{
    /// <summary>All | Open | Completed | Cancelled | Suspended</summary>
    public string? ProcessState { get; init; }
    /// <summary>All | Open | Completed | Rejected</summary>
    public string? TaskState { get; init; }
    /// <summary>All | Mine | Unassigned</summary>
    public string? Visibility { get; init; }
}

public record WorkflowConsoleItem(
    long TaskId,
    long InstanceId,
    string Number,
    string Title,
    string StepName,
    string StepType,
    string TaskState,
    string ProcessState,
    string AssigneeType,
    long? AssigneeRoleId,
    long? TakenByUserId,
    long? AssigneeUserId,
    DateTime? DueDate,
    string? DecisionOptionsJson,
    string? LinkedEntityType,
    long? LinkedEntityId);
