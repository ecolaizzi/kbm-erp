using KBM.Api.Authorization;
using KBM.Application.Accounting;
using KBM.Application.Auth;
using KBM.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBM.Api.Controllers;

[ApiController]
[Route("api/chart-accounts")]
[Authorize]
public class ChartAccountsController(IChartAccountService accounts) : ControllerBase
{
    [HttpGet]
    [RequiresPermission(PermissionCodes.ChartAccountsRead)]
    public Task<IReadOnlyList<ChartAccountListItem>> List([FromQuery] string? search, [FromQuery] bool postableOnly, CancellationToken ct) =>
        accounts.ListAsync(search, postableOnly, ct);

    [HttpGet("{id:long}")]
    [RequiresPermission(PermissionCodes.ChartAccountsRead)]
    public async Task<ActionResult<ChartAccountDetail>> Get(long id, CancellationToken ct)
    {
        var a = await accounts.GetAsync(id, ct);
        return a is null ? NotFound() : Ok(a);
    }

    [HttpPost]
    [RequiresPermission(PermissionCodes.ChartAccountsManage)]
    public async Task<ActionResult<ChartAccountDetail>> Create([FromBody] CreateChartAccountRequest request, CancellationToken ct)
    {
        try
        {
            var a = await accounts.CreateAsync(request, ct);
            return CreatedAtAction(nameof(Get), new { id = a.Id }, a);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpPut("{id:long}")]
    [RequiresPermission(PermissionCodes.ChartAccountsManage)]
    public async Task<ActionResult<ChartAccountDetail>> Update(long id, [FromBody] UpdateChartAccountRequest request, CancellationToken ct)
    {
        try
        {
            var a = await accounts.UpdateAsync(id, request, ct);
            return a is null ? NotFound() : Ok(a);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpPost("{id:long}/disable")]
    [RequiresPermission(PermissionCodes.ChartAccountsManage)]
    public async Task<IActionResult> Disable(long id, CancellationToken ct) =>
        await accounts.SetEnabledAsync(id, false, ct) ? NoContent() : NotFound();

    [HttpPost("{id:long}/enable")]
    [RequiresPermission(PermissionCodes.ChartAccountsManage)]
    public async Task<IActionResult> Enable(long id, CancellationToken ct) =>
        await accounts.SetEnabledAsync(id, true, ct) ? NoContent() : NotFound();

    [HttpPost("seed-standard")]
    [RequiresPermission(PermissionCodes.ChartAccountsManage)]
    public async Task<ActionResult<object>> SeedStandard(CancellationToken ct)
    {
        var created = await accounts.SeedStandardAsync(ct);
        return Ok(new { created });
    }
}
