using KBM.Api.Authorization;
using KBM.Application.Auth;
using KBM.Application.Orders;
using KBM.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBM.Api.Controllers;

[ApiController]
[Route("api/purchase-orders")]
[Authorize]
public class PurchaseOrdersController(IPurchaseOrderService orders) : ControllerBase
{
    [HttpGet]
    [RequiresPermission(PermissionCodes.PurchaseOrdersRead)]
    public Task<IReadOnlyList<PurchaseOrderListItem>> List([FromQuery] string? search, CancellationToken ct) =>
        orders.ListAsync(search, ct);

    [HttpGet("{id:long}")]
    [RequiresPermission(PermissionCodes.PurchaseOrdersRead)]
    public async Task<ActionResult<PurchaseOrderDetail>> Get(long id, CancellationToken ct)
    {
        var o = await orders.GetAsync(id, ct);
        return o is null ? NotFound() : Ok(o);
    }

    [HttpPost]
    [RequiresPermission(PermissionCodes.PurchaseOrdersCreate)]
    public async Task<ActionResult<PurchaseOrderDetail>> Create([FromBody] CreatePurchaseOrderRequest request, CancellationToken ct)
    {
        try
        {
            var o = await orders.CreateAsync(request, ct);
            return CreatedAtAction(nameof(Get), new { id = o.Id }, o);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpPost("from-purchase-request/{purchaseRequestId:long}")]
    [RequiresPermission(PermissionCodes.PurchaseOrdersCreate)]
    public async Task<ActionResult<PurchaseOrderDetail>> FromRda(long purchaseRequestId, CancellationToken ct)
    {
        try
        {
            var o = await orders.CreateFromPurchaseRequestAsync(purchaseRequestId, ct);
            return o is null ? NotFound() : CreatedAtAction(nameof(Get), new { id = o.Id }, o);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpPut("{id:long}")]
    [RequiresPermission(PermissionCodes.PurchaseOrdersEdit)]
    public async Task<ActionResult<PurchaseOrderDetail>> Save(long id, [FromBody] SavePurchaseOrderRequest request, CancellationToken ct)
    {
        try
        {
            var o = await orders.SaveAsync(id, request, ct);
            return o is null ? NotFound() : Ok(o);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpDelete("{id:long}")]
    [RequiresPermission(PermissionCodes.PurchaseOrdersDelete)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        try { return await orders.DeleteAsync(id, ct) ? NoContent() : NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }
}
