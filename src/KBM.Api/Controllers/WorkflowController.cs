using KBM.Api.Authorization;
using KBM.Application.Auth;
using KBM.Application.Security;
using KBM.Application.Workflow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBM.Api.Controllers;

[ApiController]
[Route("api/workflow")]
[Authorize]
public class WorkflowController(IWorkflowEngineService engine, IWorkflowConsoleService console) : ControllerBase
{
    // ===================== Consolle =====================
    [HttpGet("console")]
    [RequiresPermission(PermissionCodes.WorkflowRead)]
    public Task<IReadOnlyList<WorkflowConsoleItem>> Console(
        [FromQuery] string? processState, [FromQuery] string? taskState, [FromQuery] string? visibility, CancellationToken ct) =>
        console.ListTasksAsync(new WorkflowConsoleQuery { ProcessState = processState, TaskState = taskState, Visibility = visibility }, ct);

    // ===================== Istanze =====================
    [HttpGet("instances")]
    [RequiresPermission(PermissionCodes.WorkflowRead)]
    public Task<IReadOnlyList<WorkflowInstanceListItem>> Instances([FromQuery] string? search, CancellationToken ct) =>
        engine.ListInstancesAsync(search, ct);

    [HttpGet("instances/{id:long}")]
    [RequiresPermission(PermissionCodes.WorkflowRead)]
    public async Task<ActionResult<WorkflowInstanceDetail>> Instance(long id, CancellationToken ct)
    {
        var i = await engine.GetInstanceAsync(id, ct);
        return i is null ? NotFound() : Ok(i);
    }

    [HttpPost("instances")]
    [RequiresPermission(PermissionCodes.WorkflowStart)]
    public async Task<ActionResult<WorkflowInstanceDetail>> Start([FromBody] StartWorkflowRequest request, CancellationToken ct)
    {
        try
        {
            var i = await engine.StartAsync(request, ct);
            return CreatedAtAction(nameof(Instance), new { id = i.Id }, i);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpPost("instances/{id:long}/state/{action}")]
    [RequiresPermission(PermissionCodes.WorkflowAdmin)]
    public async Task<IActionResult> SetState(long id, string action, [FromBody] TaskNoteRequest? note, CancellationToken ct)
    {
        try
        {
            return await engine.SetInstanceStateAsync(id, action, note?.Notes, ct) ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    // ===================== Task =====================
    [HttpPost("tasks/{id:long}/complete")]
    [RequiresPermission(PermissionCodes.WorkflowParticipate)]
    public async Task<IActionResult> Complete(long id, [FromBody] CompleteTaskRequest request, CancellationToken ct)
    {
        try
        {
            return await engine.CompleteTaskAsync(id, request, ct) ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpPost("tasks/{id:long}/reject")]
    [RequiresPermission(PermissionCodes.WorkflowParticipate)]
    public async Task<IActionResult> Reject(long id, [FromBody] TaskNoteRequest? note, CancellationToken ct)
    {
        try
        {
            return await engine.RejectTaskAsync(id, note?.Notes, ct) ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpPost("tasks/{id:long}/take")]
    [RequiresPermission(PermissionCodes.WorkflowParticipate)]
    public async Task<IActionResult> Take(long id, CancellationToken ct)
    {
        try { return await engine.TakeChargeAsync(id, ct) ? NoContent() : NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpPost("tasks/{id:long}/release")]
    [RequiresPermission(PermissionCodes.WorkflowParticipate)]
    public async Task<IActionResult> Release(long id, CancellationToken ct) =>
        await engine.ReleaseAsync(id, ct) ? NoContent() : NotFound();

    [HttpPost("tasks/{id:long}/note")]
    [RequiresPermission(PermissionCodes.WorkflowParticipate)]
    public async Task<IActionResult> Note(long id, [FromBody] TaskNoteRequest note, CancellationToken ct) =>
        await engine.AddNoteAsync(id, note.Notes, ct) ? NoContent() : NotFound();
}
