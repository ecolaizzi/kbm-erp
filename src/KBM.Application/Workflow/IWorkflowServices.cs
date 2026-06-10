namespace KBM.Application.Workflow;

/// <summary>Gestione dei modelli di workflow (template di processo).</summary>
public interface IWorkflowDefinitionService
{
    Task<IReadOnlyList<WorkflowDefinitionListItem>> ListAsync(string? search = null, CancellationToken ct = default);
    Task<WorkflowDefinitionDetail?> GetAsync(long id, CancellationToken ct = default);
    Task<WorkflowDefinitionDetail> CreateAsync(CreateWorkflowDefinitionRequest request, CancellationToken ct = default);
    Task<WorkflowDefinitionDetail?> UpdateAsync(long id, UpdateWorkflowDefinitionRequest request, CancellationToken ct = default);
    Task<bool> SetStatusAsync(long id, string status, CancellationToken ct = default);
}

/// <summary>Motore di esecuzione dei processi (istanze, task, transizioni, azioni).</summary>
public interface IWorkflowEngineService
{
    Task<WorkflowInstanceDetail> StartAsync(StartWorkflowRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<WorkflowInstanceListItem>> ListInstancesAsync(string? search = null, CancellationToken ct = default);
    Task<WorkflowInstanceDetail?> GetInstanceAsync(long id, CancellationToken ct = default);

    Task<bool> CompleteTaskAsync(long taskId, CompleteTaskRequest request, CancellationToken ct = default);
    Task<bool> RejectTaskAsync(long taskId, string? notes, CancellationToken ct = default);
    Task<bool> TakeChargeAsync(long taskId, CancellationToken ct = default);
    Task<bool> ReleaseAsync(long taskId, CancellationToken ct = default);
    Task<bool> AddNoteAsync(long taskId, string notes, CancellationToken ct = default);

    /// <summary>action = cancel | suspend | reactivate | force-close</summary>
    Task<bool> SetInstanceStateAsync(long instanceId, string action, string? notes, CancellationToken ct = default);
}

/// <summary>Consolle: inbox dei task con filtri e livelli di visibilit&#224;.</summary>
public interface IWorkflowConsoleService
{
    Task<IReadOnlyList<WorkflowConsoleItem>> ListTasksAsync(WorkflowConsoleQuery query, CancellationToken ct = default);
}
