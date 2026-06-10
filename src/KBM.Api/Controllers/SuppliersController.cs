using KBM.Api.Authorization;
using KBM.Application.Auth;
using KBM.Application.Security;
using KBM.Application.Suppliers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBM.Api.Controllers;

[ApiController]
[Route("api/suppliers")]
[Authorize]
public class SuppliersController(ISupplierService suppliers) : ControllerBase
{
    [HttpGet]
    [RequiresPermission(PermissionCodes.SuppliersRead)]
    public Task<IReadOnlyList<SupplierListItem>> List([FromQuery] string? search, CancellationToken ct) =>
        suppliers.ListAsync(search, ct);

    [HttpGet("{id:long}")]
    [RequiresPermission(PermissionCodes.SuppliersRead)]
    public async Task<ActionResult<SupplierDetail>> Get(long id, CancellationToken ct)
    {
        var s = await suppliers.GetAsync(id, ct);
        return s is null ? NotFound() : Ok(s);
    }

    [HttpPut("{id:long}")]
    [RequiresPermission(PermissionCodes.SuppliersEdit)]
    public async Task<ActionResult<SupplierDetail>> Update(long id, [FromBody] UpdateSupplierRequest request, CancellationToken ct)
    {
        try
        {
            var s = await suppliers.UpdateAsync(id, request, ct);
            return s is null ? NotFound() : Ok(s);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpGet("{id:long}/full")]
    [RequiresPermission(PermissionCodes.SuppliersRead)]
    public async Task<ActionResult<SupplierAggregate>> GetFull(long id, CancellationToken ct)
    {
        var agg = await suppliers.GetFullAsync(id, ct);
        return agg is null ? NotFound() : Ok(agg);
    }

    [HttpPost("full")]
    [RequiresPermission(PermissionCodes.SuppliersCreate)]
    public async Task<ActionResult<SupplierAggregate>> CreateFull([FromBody] CreateSupplierAggregateRequest request, CancellationToken ct)
    {
        try
        {
            var agg = await suppliers.CreateFullAsync(request, ct);
            return CreatedAtAction(nameof(Get), new { id = agg.Detail.Id }, agg);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpPut("{id:long}/full")]
    [RequiresPermission(PermissionCodes.SuppliersEdit)]
    public async Task<ActionResult<SupplierAggregate>> SaveFull(long id, [FromBody] SaveSupplierAggregateRequest request, CancellationToken ct)
    {
        try
        {
            var agg = await suppliers.SaveFullAsync(id, request, ct);
            return agg is null ? NotFound() : Ok(agg);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpPost("{id:long}/disable")]
    [RequiresPermission(PermissionCodes.SuppliersEdit)]
    public async Task<IActionResult> Disable(long id, CancellationToken ct) =>
        await suppliers.SetEnabledAsync(id, false, ct) ? NoContent() : NotFound();

    [HttpPost("{id:long}/enable")]
    [RequiresPermission(PermissionCodes.SuppliersEdit)]
    public async Task<IActionResult> Enable(long id, CancellationToken ct) =>
        await suppliers.SetEnabledAsync(id, true, ct) ? NoContent() : NotFound();
}
