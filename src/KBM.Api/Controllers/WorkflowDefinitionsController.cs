using KBM.Api.Authorization;
using KBM.Application.Auth;
using KBM.Application.Security;
using KBM.Application.Workflow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBM.Api.Controllers;

[ApiController]
[Route("api/workflow-definitions")]
[Authorize]
public class WorkflowDefinitionsController(IWorkflowDefinitionService defs) : ControllerBase
{
    [HttpGet]
    [RequiresPermission(PermissionCodes.WorkflowRead)]
    public Task<IReadOnlyList<WorkflowDefinitionListItem>> List([FromQuery] string? search, CancellationToken ct) =>
        defs.ListAsync(search, ct);

    [HttpGet("{id:long}")]
    [RequiresPermission(PermissionCodes.WorkflowRead)]
    public async Task<ActionResult<WorkflowDefinitionDetail>> Get(long id, CancellationToken ct)
    {
        var d = await defs.GetAsync(id, ct);
        return d is null ? NotFound() : Ok(d);
    }

    [HttpPost]
    [RequiresPermission(PermissionCodes.WorkflowManage)]
    public async Task<ActionResult<WorkflowDefinitionDetail>> Create([FromBody] CreateWorkflowDefinitionRequest request, CancellationToken ct)
    {
        try
        {
            var d = await defs.CreateAsync(request, ct);
            return CreatedAtAction(nameof(Get), new { id = d.Id }, d);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpPut("{id:long}")]
    [RequiresPermission(PermissionCodes.WorkflowManage)]
    public async Task<ActionResult<WorkflowDefinitionDetail>> Update(long id, [FromBody] UpdateWorkflowDefinitionRequest request, CancellationToken ct)
    {
        try
        {
            var d = await defs.UpdateAsync(id, request, ct);
            return d is null ? NotFound() : Ok(d);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpPost("{id:long}/status/{status}")]
    [RequiresPermission(PermissionCodes.WorkflowManage)]
    public async Task<IActionResult> SetStatus(long id, string status, CancellationToken ct) =>
        await defs.SetStatusAsync(id, status, ct) ? NoContent() : NotFound();
}
