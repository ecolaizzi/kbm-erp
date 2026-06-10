using KBM.Api.Authorization;
using KBM.Application.Auth;
using KBM.Application.Purchasing;
using KBM.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBM.Api.Controllers;

/// <summary>Richieste di Offerta (RDO).</summary>
[ApiController]
[Route("api/rfqs")]
[Authorize]
public class RfqsController(IRfqService service) : ControllerBase
{
    [HttpGet]
    [RequiresPermission(PermissionCodes.RfqRead)]
    public Task<IReadOnlyList<RfqListItem>> List([FromQuery] string? search, CancellationToken ct) => service.ListAsync(search, ct);

    [HttpGet("{id:long}")]
    [RequiresPermission(PermissionCodes.RfqRead)]
    public async Task<ActionResult<RfqDetail>> Get(long id, CancellationToken ct)
    {
        var r = await service.GetAsync(id, ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpPost]
    [RequiresPermission(PermissionCodes.RfqCreate)]
    public async Task<ActionResult<RfqDetail>> Create([FromBody] CreateRfqRequest request, CancellationToken ct)
    {
        try
        {
            var created = await service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpPost("from-purchase-request")]
    [RequiresPermission(PermissionCodes.RfqCreate)]
    public async Task<ActionResult<RfqDetail>> CreateFromPurchaseRequest([FromBody] CreateRfqFromRequest request, CancellationToken ct)
    {
        try
        {
            var created = await service.CreateFromPurchaseRequestAsync(request, ct);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpPut("{id:long}")]
    [RequiresPermission(PermissionCodes.RfqEdit)]
    public async Task<ActionResult<RfqDetail>> Save(long id, [FromBody] SaveRfqRequest request, CancellationToken ct)
    {
        try
        {
            var saved = await service.SaveAsync(id, request, ct);
            return saved is null ? NotFound() : Ok(saved);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpDelete("{id:long}")]
    [RequiresPermission(PermissionCodes.RfqDelete)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct) =>
        await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
