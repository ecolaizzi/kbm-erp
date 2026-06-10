using KBM.Api.Authorization;
using KBM.Application.Auth;
using KBM.Application.Purchasing;
using KBM.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBM.Api.Controllers;

/// <summary>Richieste di Acquisto (RDA).</summary>
[ApiController]
[Route("api/purchase-requests")]
[Authorize]
public class PurchaseRequestsController(IPurchaseRequestService service) : ControllerBase
{
    [HttpGet]
    [RequiresPermission(PermissionCodes.PurchaseRequestsRead)]
    public Task<IReadOnlyList<PurchaseRequestListItem>> List([FromQuery] string? search, CancellationToken ct) => service.ListAsync(search, ct);

    [HttpGet("{id:long}")]
    [RequiresPermission(PermissionCodes.PurchaseRequestsRead)]
    public async Task<ActionResult<PurchaseRequestDetail>> Get(long id, CancellationToken ct)
    {
        var pr = await service.GetAsync(id, ct);
        return pr is null ? NotFound() : Ok(pr);
    }

    [HttpPost]
    [RequiresPermission(PermissionCodes.PurchaseRequestsCreate)]
    public async Task<ActionResult<PurchaseRequestDetail>> Create([FromBody] CreatePurchaseRequestRequest request, CancellationToken ct)
    {
        try
        {
            var created = await service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpPut("{id:long}")]
    [RequiresPermission(PermissionCodes.PurchaseRequestsEdit)]
    public async Task<ActionResult<PurchaseRequestDetail>> Save(long id, [FromBody] SavePurchaseRequestRequest request, CancellationToken ct)
    {
        try
        {
            var saved = await service.SaveAsync(id, request, ct);
            return saved is null ? NotFound() : Ok(saved);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpDelete("{id:long}")]
    [RequiresPermission(PermissionCodes.PurchaseRequestsDelete)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct) =>
        await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
