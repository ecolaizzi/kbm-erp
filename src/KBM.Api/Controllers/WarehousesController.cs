using KBM.Api.Authorization;
using KBM.Application.Auth;
using KBM.Application.Orders;
using KBM.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBM.Api.Controllers;

[ApiController]
[Route("api/warehouses")]
[Authorize]
public class WarehousesController(IWarehouseService service) : ControllerBase
{
    [HttpGet]
    [RequiresPermission(PermissionCodes.OrdersSetupRead)]
    public Task<IReadOnlyList<WarehouseListItem>> List([FromQuery] string? search, CancellationToken ct) =>
        service.ListAsync(search, ct);

    [HttpGet("{id:long}")]
    [RequiresPermission(PermissionCodes.OrdersSetupRead)]
    public async Task<ActionResult<WarehouseDetail>> Get(long id, CancellationToken ct)
    {
        var w = await service.GetAsync(id, ct);
        return w is null ? NotFound() : Ok(w);
    }

    [HttpPost]
    [RequiresPermission(PermissionCodes.OrdersSetupManage)]
    public async Task<ActionResult<WarehouseDetail>> Create([FromBody] SaveWarehouseRequest request, CancellationToken ct)
    {
        try
        {
            var w = await service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(Get), new { id = w.Id }, w);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpGet("reasons")]
    [RequiresPermission(PermissionCodes.OrdersSetupRead)]
    public Task<IReadOnlyList<WarehouseReasonListItem>> Reasons(CancellationToken ct) =>
        service.ListReasonsAsync(ct);

    [HttpPost("seed-standard")]
    [RequiresPermission(PermissionCodes.OrdersSetupManage)]
    public async Task<ActionResult<object>> Seed(CancellationToken ct) =>
        Ok(new { created = await service.SeedStandardAsync(ct) });
}
