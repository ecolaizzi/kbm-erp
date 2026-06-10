using KBM.Application.Security;
using KBM.Application.Workflow;
using KBM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KBM.Infrastructure.Persistence.Workflow;

public sealed class WorkflowDefinitionService(KbmDbContext db, ICurrentUserContext currentUser) : IWorkflowDefinitionService
{
    public async Task<IReadOnlyList<WorkflowDefinitionListItem>> ListAsync(string? search = null, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var query = db.WorkflowDefinitions.Where(d => !d.IsDeleted && d.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(d => d.Code.Contains(term) || d.Name.Contains(term));
        }
        return await query.OrderBy(d => d.Code)
            .Select(d => new WorkflowDefinitionListItem(d.Id, d.Code, d.Name, d.TargetEntityType, d.Status,
                d.Steps.Count(s => !s.IsDeleted)))
            .ToListAsync(ct);
    }

    public async Task<WorkflowDefinitionDetail?> GetAsync(long id, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var d = await db.WorkflowDefinitions
            .Include(x => x.Steps.Where(s => !s.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        return d is null ? null : Map(d);
    }

    public async Task<WorkflowDefinitionDetail> CreateAsync(CreateWorkflowDefinitionRequest request, CancellationToken ct = default)
    {
        var actorId = currentUser.UserId ?? 0;
        var companyId = CompanyId();
        var code = request.Code.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code)) throw new InvalidOperationException("Il codice modello e obbligatorio.");
        if (await db.WorkflowDefinitions.AnyAsync(d => d.Code == code && d.CompanyId == companyId && !d.IsDeleted, ct))
            throw new InvalidOperationException("Codice modello gia in uso.");

        var def = new WorkflowDefinition
        {
            CompanyId = companyId,
            Code = code,
            Name = request.Name.Trim(),
            Description = Clean(request.Description),
            TargetEntityType = Clean(request.TargetEntityType),
            FieldsJson = Clean(request.FieldsJson),
            Status = "Draft",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = actorId
        };
        SyncSteps(def, request.Steps, companyId, actorId);
        db.WorkflowDefinitions.Add(def);
        await db.SaveChangesAsync(ct);
        return Map(def);
    }

    public async Task<WorkflowDefinitionDetail?> UpdateAsync(long id, UpdateWorkflowDefinitionRequest request, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var actorId = currentUser.UserId ?? 0;
        var def = await db.WorkflowDefinitions
            .Include(x => x.Steps)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        if (def is null) return null;

        def.Name = request.Name.Trim();
        def.Description = Clean(request.Description);
        def.TargetEntityType = Clean(request.TargetEntityType);
        def.FieldsJson = Clean(request.FieldsJson);
        def.Status = string.IsNullOrWhiteSpace(request.Status) ? def.Status : request.Status;
        def.UpdatedAt = DateTime.UtcNow;
        def.UpdatedBy = currentUser.UserId;
        SyncSteps(def, request.Steps, companyId, actorId);
        await db.SaveChangesAsync(ct);

        var reloaded = await db.WorkflowDefinitions
            .Include(x => x.Steps.Where(s => !s.IsDeleted))
            .FirstAsync(x => x.Id == id, ct);
        return Map(reloaded);
    }

    public async Task<bool> SetStatusAsync(long id, string status, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var def = await db.WorkflowDefinitions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        if (def is null) return false;
        def.Status = status;
        def.UpdatedAt = DateTime.UtcNow;
        def.UpdatedBy = currentUser.UserId;
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static void SyncSteps(WorkflowDefinition def, IReadOnlyList<WorkflowStepInput> steps, long companyId, long actorId)
    {
        var incoming = (steps ?? []).OrderBy(s => s.StepOrder).ToList();
        var keepIds = incoming.Where(s => s.Id is > 0).Select(s => s.Id!.Value).ToHashSet();

        foreach (var existing in def.Steps.Where(s => !s.IsDeleted && !keepIds.Contains(s.Id)))
            existing.IsDeleted = true;

        var order = 1;
        foreach (var input in incoming)
        {
            var step = input.Id is > 0 ? def.Steps.FirstOrDefault(s => s.Id == input.Id) : null;
            if (step is null)
            {
                step = new WorkflowStep { CompanyId = companyId, CreatedAt = DateTime.UtcNow, CreatedBy = actorId };
                def.Steps.Add(step);
            }
            step.IsDeleted = false;
            step.StepOrder = order++;
            step.Code = string.IsNullOrWhiteSpace(input.Code) ? $"S{step.StepOrder}" : input.Code.Trim();
            step.Name = input.Name?.Trim() ?? string.Empty;
            step.StepType = (WorkflowStepType)input.StepType;
            step.AssigneeType = (WorkflowAssigneeType)input.AssigneeType;
            step.AssigneeUserId = input.AssigneeUserId is 0 ? null : input.AssigneeUserId;
            step.AssigneeRoleId = input.AssigneeRoleId is 0 ? null : input.AssigneeRoleId;
            step.DueDays = input.DueDays;
            step.DecisionOptionsJson = Clean(input.DecisionOptionsJson);
            step.ActionsJson = Clean(input.ActionsJson);
            step.UpdatedAt = DateTime.UtcNow;
            step.UpdatedBy = actorId;
        }
    }

    private long CompanyId() => currentUser.CompanyId ?? throw new InvalidOperationException("Contesto azienda mancante.");
    private static string? Clean(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

    private static WorkflowDefinitionDetail Map(WorkflowDefinition d) => new(
        d.Id, d.Code, d.Name, d.Description, d.TargetEntityType, d.Status, d.FieldsJson,
        d.Steps.Where(s => !s.IsDeleted).OrderBy(s => s.StepOrder).Select(s => new WorkflowStepDto(
            s.Id, s.StepOrder, s.Code, s.Name, (int)s.StepType, (int)s.AssigneeType,
            s.AssigneeUserId, s.AssigneeRoleId, s.DueDays, s.DecisionOptionsJson, s.ActionsJson)).ToList());
}
