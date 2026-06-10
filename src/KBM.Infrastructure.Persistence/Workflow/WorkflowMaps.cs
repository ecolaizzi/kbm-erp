using KBM.Domain.Entities;

namespace KBM.Infrastructure.Persistence.Workflow;

/// <summary>Mappature stato/tipo enum ↔ stringhe usate nei DTO e nelle query consolle.</summary>
internal static class WorkflowMaps
{
    public static string Instance(WorkflowInstanceState s) => s switch
    {
        WorkflowInstanceState.Open => "Open",
        WorkflowInstanceState.Completed => "Completed",
        WorkflowInstanceState.Cancelled => "Cancelled",
        WorkflowInstanceState.Suspended => "Suspended",
        _ => "Open"
    };

    public static string Task(WorkflowTaskState s) => s switch
    {
        WorkflowTaskState.Open => "Open",
        WorkflowTaskState.Completed => "Completed",
        WorkflowTaskState.Rejected => "Rejected",
        WorkflowTaskState.Cancelled => "Cancelled",
        _ => "Open"
    };

    public static string StepType(WorkflowStepType t) => t switch
    {
        WorkflowStepType.Task => "Task",
        WorkflowStepType.Approval => "Approval",
        WorkflowStepType.Decision => "Decision",
        _ => "Approval"
    };

    public static string Assignee(WorkflowAssigneeType a) => a switch
    {
        WorkflowAssigneeType.User => "User",
        WorkflowAssigneeType.Role => "Role",
        WorkflowAssigneeType.Initiator => "Initiator",
        _ => "Role"
    };
}
