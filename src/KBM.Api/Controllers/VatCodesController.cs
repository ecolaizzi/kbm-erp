using KBM.Api.Authorization;
using KBM.Application.Auth;
using KBM.Application.Orders;
using KBM.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBM.Api.Controllers;

[ApiController]
[Route("api/vat-codes")]
[Authorize]
public class VatCodesController(IVatCodeService service) : ControllerBase
{
    [HttpGet]
    [RequiresPermission(PermissionCodes.OrdersSetupRead)]
    public Task<IReadOnlyList<VatCodeListItem>> List([FromQuery] string? search, CancellationToken ct) =>
        service.ListAsync(search, ct);

    [HttpGet("{id:long}")]
    [RequiresPermission(PermissionCodes.OrdersSetupRead)]
    public async Task<ActionResult<VatCodeDetail>> Get(long id, CancellationToken ct)
    {
        var v = await service.GetAsync(id, ct);
        return v is null ? NotFound() : Ok(v);
    }

    [HttpPost]
    [RequiresPermission(PermissionCodes.OrdersSetupManage)]
    public async Task<ActionResult<VatCodeDetail>> Create([FromBody] SaveVatCodeRequest request, CancellationToken ct)
    {
        try
        {
            var v = await service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(Get), new { id = v.Id }, v);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpPut("{id:long}")]
    [RequiresPermission(PermissionCodes.OrdersSetupManage)]
    public async Task<ActionResult<VatCodeDetail>> Update(long id, [FromBody] SaveVatCodeRequest request, CancellationToken ct)
    {
        var v = await service.UpdateAsync(id, request, ct);
        return v is null ? NotFound() : Ok(v);
    }

    [HttpPost("seed-standard")]
    [RequiresPermission(PermissionCodes.OrdersSetupManage)]
    public async Task<ActionResult<object>> Seed(CancellationToken ct) =>
        Ok(new { created = await service.SeedStandardAsync(ct) });
}
