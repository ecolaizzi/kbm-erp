using KBM.Application.Security;
using KBM.Application.Workflow;
using KBM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KBM.Infrastructure.Persistence.Workflow;

public sealed class WorkflowConsoleService(KbmDbContext db, ICurrentUserContext currentUser) : IWorkflowConsoleService
{
    public async Task<IReadOnlyList<WorkflowConsoleItem>> ListTasksAsync(WorkflowConsoleQuery query, CancellationToken ct = default)
    {
        var companyId = currentUser.CompanyId ?? throw new InvalidOperationException("Contesto azienda mancante.");
        var actorId = currentUser.UserId ?? 0;
        var myRoles = await db.UserRoles
            .Where(ur => ur.UserId == actorId && ur.CompanyId == companyId)
            .Select(ur => ur.RoleId)
            .ToListAsync(ct);

        var q = db.WorkflowTasks.Where(t => !t.IsDeleted && t.CompanyId == companyId && !t.Instance!.IsDeleted);

        // Stato processo
        var processState = query.ProcessState ?? "Open";
        if (TryInstanceState(processState, out var ps))
            q = q.Where(t => t.Instance!.State == ps);

        // Stato task
        var taskState = query.TaskState ?? "Open";
        if (TryTaskState(taskState, out var tsk))
            q = q.Where(t => t.State == tsk);

        // Visibilita
        var visibility = (query.Visibility ?? "All").ToLowerInvariant();
        q = visibility switch
        {
            "mine" => q.Where(t => t.AssigneeUserId == actorId || t.TakenByUserId == actorId),
            "unassigned" => q.Where(t => t.AssigneeType == WorkflowAssigneeType.Role
                                         && t.TakenByUserId == null
                                         && t.AssigneeRoleId != null && myRoles.Contains(t.AssigneeRoleId.Value)),
            _ => q.Where(t => t.AssigneeUserId == actorId
                              || t.TakenByUserId == actorId
                              || (t.AssigneeRoleId != null && myRoles.Contains(t.AssigneeRoleId.Value)))
        };

        var rows = await q.OrderByDescending(t => t.CreatedAt)
            .Select(t => new
            {
                t.Id, t.WorkflowInstanceId,
                t.Instance!.Number, t.Instance.Title,
                InstanceState = t.Instance.State,
                t.Instance.LinkedEntityType, t.Instance.LinkedEntityId,
                t.StepName, t.StepType,
                TaskState = t.State,
                t.AssigneeType, t.AssigneeRoleId, t.AssigneeUserId, t.TakenByUserId, t.DueDate,
                DecisionOptionsJson = db.WorkflowSteps.Where(s => s.Id == t.WorkflowStepId).Select(s => s.DecisionOptionsJson).FirstOrDefault()
            })
            .ToListAsync(ct);

        return rows.Select(r => new WorkflowConsoleItem(
            r.Id, r.WorkflowInstanceId, r.Number, r.Title, r.StepName,
            WorkflowMaps.StepType(r.StepType), WorkflowMaps.Task(r.TaskState), WorkflowMaps.Instance(r.InstanceState),
            WorkflowMaps.Assignee(r.AssigneeType), r.AssigneeRoleId, r.TakenByUserId, r.AssigneeUserId,
            r.DueDate, r.DecisionOptionsJson, r.LinkedEntityType, r.LinkedEntityId)).ToList();
    }

    private static bool TryInstanceState(string value, out WorkflowInstanceState state)
    {
        state = WorkflowInstanceState.Open;
        if (string.IsNullOrWhiteSpace(value) || value.Equals("All", StringComparison.OrdinalIgnoreCase)) return false;
        return Enum.TryParse(value, true, out state);
    }

    private static bool TryTaskState(string value, out WorkflowTaskState state)
    {
        state = WorkflowTaskState.Open;
        if (string.IsNullOrWhiteSpace(value) || value.Equals("All", StringComparison.OrdinalIgnoreCase)) return false;
        return Enum.TryParse(value, true, out state);
    }
}
