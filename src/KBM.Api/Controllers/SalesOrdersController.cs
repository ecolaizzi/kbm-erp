using KBM.Api.Authorization;
using KBM.Application.Auth;
using KBM.Application.Orders;
using KBM.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBM.Api.Controllers;

[ApiController]
[Route("api/sales-orders")]
[Authorize]
public class SalesOrdersController(ISalesOrderService orders) : ControllerBase
{
    [HttpGet]
    [RequiresPermission(PermissionCodes.SalesOrdersRead)]
    public Task<IReadOnlyList<SalesOrderListItem>> List([FromQuery] string? search, CancellationToken ct) =>
        orders.ListAsync(search, ct);

    [HttpGet("{id:long}")]
    [RequiresPermission(PermissionCodes.SalesOrdersRead)]
    public async Task<ActionResult<SalesOrderDetail>> Get(long id, CancellationToken ct)
    {
        var o = await orders.GetAsync(id, ct);
        return o is null ? NotFound() : Ok(o);
    }

    [HttpPost]
    [RequiresPermission(PermissionCodes.SalesOrdersCreate)]
    public async Task<ActionResult<SalesOrderDetail>> Create([FromBody] CreateSalesOrderRequest request, CancellationToken ct)
    {
        try
        {
            var o = await orders.CreateAsync(request, ct);
            return CreatedAtAction(nameof(Get), new { id = o.Id }, o);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpPut("{id:long}")]
    [RequiresPermission(PermissionCodes.SalesOrdersEdit)]
    public async Task<ActionResult<SalesOrderDetail>> Save(long id, [FromBody] SaveSalesOrderRequest request, CancellationToken ct)
    {
        try
        {
            var o = await orders.SaveAsync(id, request, ct);
            return o is null ? NotFound() : Ok(o);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpDelete("{id:long}")]
    [RequiresPermission(PermissionCodes.SalesOrdersDelete)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        try { return await orders.DeleteAsync(id, ct) ? NoContent() : NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }
}
