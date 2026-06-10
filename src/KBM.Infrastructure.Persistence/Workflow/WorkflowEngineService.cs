using System.Text.Json;
using KBM.Application.Security;
using KBM.Application.Workflow;
using KBM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KBM.Infrastructure.Persistence.Workflow;

public sealed class WorkflowEngineService(KbmDbContext db, ICurrentUserContext currentUser) : IWorkflowEngineService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    // ===================== Avvio =====================
    public async Task<WorkflowInstanceDetail> StartAsync(StartWorkflowRequest request, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var actorId = currentUser.UserId ?? 0;

        var def = await db.WorkflowDefinitions
            .Include(d => d.Steps.Where(s => !s.IsDeleted))
            .FirstOrDefaultAsync(d => d.Id == request.WorkflowDefinitionId && !d.IsDeleted && d.CompanyId == companyId, ct)
            ?? throw new InvalidOperationException("Modello di workflow non trovato.");
        if (def.Status != "Active")
            throw new InvalidOperationException("Il modello non e attivo: attivalo prima di avviare un processo.");

        var steps = def.Steps.Where(s => !s.IsDeleted).OrderBy(s => s.StepOrder).ToList();
        if (steps.Count == 0)
            throw new InvalidOperationException("Il modello non ha step definiti.");

        var now = DateTime.UtcNow;
        var instance = new WorkflowInstance
        {
            CompanyId = companyId,
            WorkflowDefinitionId = def.Id,
            Number = await NextNumberAsync(companyId, now, ct),
            Title = string.IsNullOrWhiteSpace(request.Title) ? def.Name : request.Title!.Trim(),
            State = WorkflowInstanceState.Open,
            LinkedEntityType = Clean(request.LinkedEntityType),
            LinkedEntityId = request.LinkedEntityId is 0 ? null : request.LinkedEntityId,
            FieldValuesJson = Clean(request.FieldValuesJson),
            CurrentStepOrder = steps[0].StepOrder,
            StartedBy = actorId,
            StartedAt = now,
            CreatedAt = now,
            CreatedBy = actorId
        };

        instance.Tasks.Add(BuildTask(steps[0], instance, companyId, actorId, now));
        instance.Events.Add(Event(companyId, actorId, "Started", $"Processo avviato dal modello {def.Code}."));
        db.WorkflowInstances.Add(instance);
        await db.SaveChangesAsync(ct);

        return (await GetInstanceAsync(instance.Id, ct))!;
    }

    // ===================== Liste / dettaglio =====================
    public async Task<IReadOnlyList<WorkflowInstanceListItem>> ListInstancesAsync(string? search = null, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var query = db.WorkflowInstances.Where(i => !i.IsDeleted && i.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(i => i.Number.Contains(term) || i.Title.Contains(term));
        }

        var rows = await query.OrderByDescending(i => i.StartedAt)
            .Select(i => new
            {
                i.Id, i.Number, i.Title,
                DefinitionName = i.Definition!.Name,
                i.State, i.StartedAt, i.LinkedEntityType, i.LinkedEntityId,
                CurrentStepName = i.Tasks.Where(t => t.State == WorkflowTaskState.Open)
                    .OrderBy(t => t.StepOrder).Select(t => t.StepName).FirstOrDefault()
            })
            .ToListAsync(ct);

        return rows.Select(r => new WorkflowInstanceListItem(
            r.Id, r.Number, r.Title, r.DefinitionName, WorkflowMaps.Instance(r.State),
            r.CurrentStepName, r.StartedAt, r.LinkedEntityType, r.LinkedEntityId)).ToList();
    }

    public async Task<WorkflowInstanceDetail?> GetInstanceAsync(long id, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var i = await db.WorkflowInstances
            .Include(x => x.Definition)
            .Include(x => x.Tasks)
            .Include(x => x.Events)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        if (i is null) return null;

        return new WorkflowInstanceDetail(
            i.Id, i.Number, i.Title, i.Definition?.Name ?? "", WorkflowMaps.Instance(i.State),
            i.LinkedEntityType, i.LinkedEntityId, i.CurrentStepOrder, i.FieldValuesJson,
            i.StartedBy, i.StartedAt, i.CompletedAt,
            i.Tasks.OrderBy(t => t.StepOrder).ThenBy(t => t.Id).Select(t => new WorkflowTaskDto(
                t.Id, t.StepOrder, t.StepName, WorkflowMaps.StepType(t.StepType), WorkflowMaps.Task(t.State),
                WorkflowMaps.Assignee(t.AssigneeType), t.AssigneeUserId, t.AssigneeRoleId, t.TakenByUserId,
                t.DueDate, t.Decision, t.CompletedAt)).ToList(),
            i.Events.OrderBy(e => e.Timestamp).Select(e => new WorkflowEventDto(
                e.Id, e.Action, e.ActorUserId, e.Notes, e.Timestamp)).ToList());
    }

    // ===================== Operazioni sui task =====================
    public async Task<bool> CompleteTaskAsync(long taskId, CompleteTaskRequest request, CancellationToken ct = default)
    {
        var (instance, task) = await LoadActiveTaskAsync(taskId, ct);
        if (instance is null || task is null) return false;
        await EnsureCanActAsync(task, ct);

        var step = await db.WorkflowSteps.FirstOrDefaultAsync(s => s.Id == task.WorkflowStepId, ct);
        var outcome = "Next";
        if (task.StepType == WorkflowStepType.Decision)
        {
            var options = ParseDecisionOptions(step?.DecisionOptionsJson);
            var chosen = options.FirstOrDefault(o => string.Equals(o.Label, request.Decision, StringComparison.OrdinalIgnoreCase));
            if (chosen is null) throw new InvalidOperationException("Esito decisione non valido.");
            task.Decision = chosen.Label;
            outcome = string.IsNullOrWhiteSpace(chosen.Outcome) ? "Next" : chosen.Outcome;
        }

        var now = DateTime.UtcNow;
        var actorId = currentUser.UserId ?? 0;
        task.State = WorkflowTaskState.Completed;
        task.TakenByUserId ??= actorId;
        task.CompletedByUserId = actorId;
        task.CompletedAt = now;
        task.UpdatedAt = now;
        task.UpdatedBy = actorId;

        var noteText = task.StepType == WorkflowStepType.Decision
            ? $"Task '{task.StepName}' completato (esito: {task.Decision})."
            : $"Task '{task.StepName}' completato.";
        instance.Events.Add(Event(instance.CompanyId, actorId, "Completed", Combine(noteText, request.Notes), task.Id, now));

        await ExecuteActionsAsync(instance, step?.ActionsJson, actorId, ct);

        switch (outcome)
        {
            case "Complete":
                CompleteInstance(instance, actorId, now);
                break;
            case "Reject":
                ReopenPrevious(instance, task, actorId, now, ct);
                break;
            default:
                await AdvanceAsync(instance, task.StepOrder, actorId, now, ct);
                break;
        }

        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RejectTaskAsync(long taskId, string? notes, CancellationToken ct = default)
    {
        var (instance, task) = await LoadActiveTaskAsync(taskId, ct);
        if (instance is null || task is null) return false;
        await EnsureCanActAsync(task, ct);

        var now = DateTime.UtcNow;
        var actorId = currentUser.UserId ?? 0;
        task.State = WorkflowTaskState.Rejected;
        task.TakenByUserId ??= actorId;
        task.CompletedByUserId = actorId;
        task.CompletedAt = now;
        task.UpdatedAt = now;
        task.UpdatedBy = actorId;
        instance.Events.Add(Event(instance.CompanyId, actorId, "Rejected", Combine($"Task '{task.StepName}' respinto.", notes), task.Id, now));

        ReopenPrevious(instance, task, actorId, now, ct);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> TakeChargeAsync(long taskId, CancellationToken ct = default)
    {
        var (instance, task) = await LoadActiveTaskAsync(taskId, ct);
        if (instance is null || task is null) return false;
        await EnsureCanActAsync(task, ct);

        var actorId = currentUser.UserId ?? 0;
        task.TakenByUserId = actorId;
        task.UpdatedAt = DateTime.UtcNow;
        task.UpdatedBy = actorId;
        instance.Events.Add(Event(instance.CompanyId, actorId, "TakenCharge", $"Task '{task.StepName}' preso in carico.", task.Id));
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ReleaseAsync(long taskId, CancellationToken ct = default)
    {
        var (instance, task) = await LoadActiveTaskAsync(taskId, ct);
        if (instance is null || task is null) return false;
        var actorId = currentUser.UserId ?? 0;
        task.TakenByUserId = null;
        task.UpdatedAt = DateTime.UtcNow;
        task.UpdatedBy = actorId;
        instance.Events.Add(Event(instance.CompanyId, actorId, "Released", $"Task '{task.StepName}' rilasciato.", task.Id));
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> AddNoteAsync(long taskId, string notes, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(notes)) return false;
        var (instance, task) = await LoadActiveTaskAsync(taskId, ct, requireOpen: false);
        if (instance is null || task is null) return false;
        var actorId = currentUser.UserId ?? 0;
        instance.Events.Add(Event(instance.CompanyId, actorId, "Note", notes.Trim(), task.Id));
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> SetInstanceStateAsync(long instanceId, string action, string? notes, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var instance = await db.WorkflowInstances
            .Include(i => i.Tasks)
            .Include(i => i.Events)
            .FirstOrDefaultAsync(i => i.Id == instanceId && !i.IsDeleted && i.CompanyId == companyId, ct);
        if (instance is null) return false;

        var now = DateTime.UtcNow;
        var actorId = currentUser.UserId ?? 0;
        switch (action.ToLowerInvariant())
        {
            case "cancel":
            case "force-close":
                instance.State = action.ToLowerInvariant() == "cancel" ? WorkflowInstanceState.Cancelled : WorkflowInstanceState.Completed;
                instance.CompletedAt = now;
                foreach (var t in instance.Tasks.Where(t => t.State == WorkflowTaskState.Open))
                    t.State = WorkflowTaskState.Cancelled;
                instance.Events.Add(Event(companyId, actorId, action == "cancel" ? "Cancelled" : "ForceClosed", notes, now: now));
                break;
            case "suspend":
                if (instance.State != WorkflowInstanceState.Open) return false;
                instance.State = WorkflowInstanceState.Suspended;
                instance.Events.Add(Event(companyId, actorId, "Suspended", notes, now: now));
                break;
            case "reactivate":
                if (instance.State != WorkflowInstanceState.Suspended) return false;
                instance.State = WorkflowInstanceState.Open;
                instance.Events.Add(Event(companyId, actorId, "Reactivated", notes, now: now));
                break;
            default:
                return false;
        }
        instance.UpdatedAt = now;
        instance.UpdatedBy = actorId;
        await db.SaveChangesAsync(ct);
        return true;
    }

    // ===================== Helpers motore =====================
    private async Task AdvanceAsync(WorkflowInstance instance, int fromStepOrder, long actorId, DateTime now, CancellationToken ct)
    {
        var next = await db.WorkflowSteps
            .Where(s => s.WorkflowDefinitionId == instance.WorkflowDefinitionId && !s.IsDeleted && s.StepOrder > fromStepOrder)
            .OrderBy(s => s.StepOrder)
            .FirstOrDefaultAsync(ct);

        if (next is null) { CompleteInstance(instance, actorId, now); return; }

        instance.CurrentStepOrder = next.StepOrder;
        instance.Tasks.Add(BuildTask(next, instance, instance.CompanyId, actorId, now));
        instance.Events.Add(Event(instance.CompanyId, actorId, "Advanced", $"Avanzamento allo step '{next.Name}'.", now: now));
    }

    private void ReopenPrevious(WorkflowInstance instance, WorkflowTask current, long actorId, DateTime now, CancellationToken ct)
    {
        var prev = db.WorkflowSteps
            .Where(s => s.WorkflowDefinitionId == instance.WorkflowDefinitionId && !s.IsDeleted && s.StepOrder < current.StepOrder)
            .OrderByDescending(s => s.StepOrder)
            .FirstOrDefault();

        // Se non c'e uno step precedente, riapre lo step corrente.
        var target = prev ?? db.WorkflowSteps.FirstOrDefault(s => s.Id == current.WorkflowStepId);
        if (target is null) { CompleteInstance(instance, actorId, now); return; }

        instance.CurrentStepOrder = target.StepOrder;
        instance.Tasks.Add(BuildTask(target, instance, instance.CompanyId, actorId, now));
        instance.Events.Add(Event(instance.CompanyId, actorId, "Reopened", $"Riapertura step '{target.Name}'.", now: now));
    }

    private static void CompleteInstance(WorkflowInstance instance, long actorId, DateTime now)
    {
        instance.State = WorkflowInstanceState.Completed;
        instance.CompletedAt = now;
        instance.Events.Add(Event(instance.CompanyId, actorId, "ProcessCompleted", "Processo completato.", now: now));
    }

    private static WorkflowTask BuildTask(WorkflowStep step, WorkflowInstance instance, long companyId, long actorId, DateTime now)
    {
        long? userId = step.AssigneeType switch
        {
            WorkflowAssigneeType.User => step.AssigneeUserId,
            WorkflowAssigneeType.Initiator => instance.StartedBy,
            _ => null
        };
        return new WorkflowTask
        {
            CompanyId = companyId,
            WorkflowStepId = step.Id,
            StepOrder = step.StepOrder,
            StepName = step.Name,
            StepType = step.StepType,
            State = WorkflowTaskState.Open,
            AssigneeType = step.AssigneeType,
            AssigneeUserId = userId,
            AssigneeRoleId = step.AssigneeType == WorkflowAssigneeType.Role ? step.AssigneeRoleId : null,
            DueDate = step.DueDays is > 0 ? now.AddDays(step.DueDays.Value) : null,
            CreatedAt = now,
            CreatedBy = actorId
        };
    }

    private async Task ExecuteActionsAsync(WorkflowInstance instance, string? actionsJson, long actorId, CancellationToken ct)
    {
        var actions = ParseActions(actionsJson);
        foreach (var a in actions)
        {
            switch (a.Type?.ToLowerInvariant())
            {
                case "setentitystatus":
                    await SetLinkedEntityStatusAsync(instance, a.Value, ct);
                    instance.Events.Add(Event(instance.CompanyId, actorId, "Action",
                        $"Stato oggetto collegato impostato a '{a.Value}'."));
                    break;
                case "note":
                    instance.Events.Add(Event(instance.CompanyId, actorId, "Action", a.Value));
                    break;
            }
        }
    }

    /// <summary>Integrazione con i moduli: aggiorna lo stato dell'oggetto gestionale collegato.</summary>
    private async Task SetLinkedEntityStatusAsync(WorkflowInstance instance, string? status, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(status) || instance.LinkedEntityId is null) return;
        switch (instance.LinkedEntityType?.ToLowerInvariant())
        {
            case "purchase-request":
                var pr = await db.PurchaseRequests.FirstOrDefaultAsync(x => x.Id == instance.LinkedEntityId && x.CompanyId == instance.CompanyId, ct);
                if (pr is not null) { pr.Status = status!; pr.UpdatedAt = DateTime.UtcNow; pr.UpdatedBy = currentUser.UserId; }
                break;
            case "rfq":
                var rfq = await db.Rfqs.FirstOrDefaultAsync(x => x.Id == instance.LinkedEntityId && x.CompanyId == instance.CompanyId, ct);
                if (rfq is not null) { rfq.Status = status!; rfq.UpdatedAt = DateTime.UtcNow; rfq.UpdatedBy = currentUser.UserId; }
                break;
        }
    }

    private async Task<(WorkflowInstance? instance, WorkflowTask? task)> LoadActiveTaskAsync(long taskId, CancellationToken ct, bool requireOpen = true)
    {
        var companyId = CompanyId();
        var task = await db.WorkflowTasks
            .Include(t => t.Instance!).ThenInclude(i => i.Events)
            .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted && t.CompanyId == companyId, ct);
        if (task?.Instance is null) return (null, null);
        if (task.Instance.State != WorkflowInstanceState.Open) throw new InvalidOperationException("Il processo non e aperto.");
        if (requireOpen && task.State != WorkflowTaskState.Open) throw new InvalidOperationException("Il task non e aperto.");
        return (task.Instance, task);
    }

    private async Task EnsureCanActAsync(WorkflowTask task, CancellationToken ct)
    {
        var actorId = currentUser.UserId ?? 0;
        if (task.AssigneeType is WorkflowAssigneeType.User or WorkflowAssigneeType.Initiator)
        {
            if (task.AssigneeUserId == actorId) return;
        }
        else if (task.AssigneeType == WorkflowAssigneeType.Role && task.AssigneeRoleId is not null)
        {
            var companyId = CompanyId();
            var hasRole = await db.UserRoles.AnyAsync(ur => ur.UserId == actorId && ur.RoleId == task.AssigneeRoleId && ur.CompanyId == companyId, ct);
            if (hasRole) return;
        }
        throw new InvalidOperationException("Non sei autorizzato ad agire su questo task.");
    }

    private async Task<string> NextNumberAsync(long companyId, DateTime date, CancellationToken ct)
    {
        var year = date.Year;
        var count = await db.WorkflowInstances.CountAsync(i => i.CompanyId == companyId && i.StartedAt.Year == year, ct);
        return $"WF/{year}/{count + 1:0000}";
    }

    private static WorkflowEvent Event(long companyId, long actorId, string action, string? notes, long? taskId = null, DateTime? now = null) => new()
    {
        CompanyId = companyId,
        WorkflowTaskId = taskId,
        Action = action,
        ActorUserId = actorId,
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
        Timestamp = now ?? DateTime.UtcNow,
        CreatedAt = now ?? DateTime.UtcNow,
        CreatedBy = actorId
    };

    private static string Combine(string baseText, string? extra) =>
        string.IsNullOrWhiteSpace(extra) ? baseText : $"{baseText} {extra.Trim()}";

    private static List<DecisionOption> ParseDecisionOptions(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<DecisionOption>>(json, JsonOpts) ?? []; }
        catch { return []; }
    }

    private static List<WorkflowActionDef> ParseActions(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<WorkflowActionDef>>(json, JsonOpts) ?? []; }
        catch { return []; }
    }

    private long CompanyId() => currentUser.CompanyId ?? throw new InvalidOperationException("Contesto azienda mancante.");
    private static string? Clean(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

    private sealed class DecisionOption { public string Label { get; set; } = ""; public string Outcome { get; set; } = "Next"; }
    private sealed class WorkflowActionDef { public string Type { get; set; } = ""; public string? Value { get; set; } }
}
