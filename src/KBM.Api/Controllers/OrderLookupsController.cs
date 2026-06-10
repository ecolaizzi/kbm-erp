using KBM.Api.Authorization;
using KBM.Application.Auth;
using KBM.Application.Orders;
using KBM.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBM.Api.Controllers;

[ApiController]
[Route("api/order-lookups")]
[Authorize]
public class OrderLookupsController(IOrderLookupService lookups) : ControllerBase
{
    [HttpGet("units")]
    [RequiresPermission(PermissionCodes.OrdersSetupRead)]
    public Task<IReadOnlyList<LookupListItem>> Units([FromQuery] string? search, CancellationToken ct) =>
        lookups.ListUnitsAsync(search, ct);

    [HttpPost("units")]
    [RequiresPermission(PermissionCodes.OrdersSetupManage)]
    public async Task<ActionResult<LookupListItem>> CreateUnit([FromBody] SaveLookupRequest request, CancellationToken ct)
    {
        try { return Ok(await lookups.CreateUnitAsync(request, ct)); }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpGet("zones")]
    [RequiresPermission(PermissionCodes.OrdersSetupRead)]
    public Task<IReadOnlyList<LookupListItem>> Zones([FromQuery] string? search, CancellationToken ct) =>
        lookups.ListZonesAsync(search, ct);

    [HttpPost("zones")]
    [RequiresPermission(PermissionCodes.OrdersSetupManage)]
    public async Task<ActionResult<LookupListItem>> CreateZone([FromBody] SaveLookupRequest request, CancellationToken ct)
    {
        try { return Ok(await lookups.CreateZoneAsync(request, ct)); }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpGet("carriers")]
    [RequiresPermission(PermissionCodes.OrdersSetupRead)]
    public Task<IReadOnlyList<LookupListItem>> Carriers([FromQuery] string? search, CancellationToken ct) =>
        lookups.ListCarriersAsync(search, ct);

    [HttpPost("carriers")]
    [RequiresPermission(PermissionCodes.OrdersSetupManage)]
    public async Task<ActionResult<LookupListItem>> CreateCarrier([FromBody] SaveLookupRequest request, CancellationToken ct)
    {
        try { return Ok(await lookups.CreateCarrierAsync(request, ct)); }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpGet("port-types")]
    [RequiresPermission(PermissionCodes.OrdersSetupRead)]
    public Task<IReadOnlyList<LookupListItem>> PortTypes([FromQuery] string? search, CancellationToken ct) =>
        lookups.ListPortTypesAsync(search, ct);

    [HttpGet("document-types")]
    [RequiresPermission(PermissionCodes.OrdersSetupRead)]
    public Task<IReadOnlyList<LookupListItem>> DocumentTypes([FromQuery] int? category, CancellationToken ct) =>
        lookups.ListDocumentTypesAsync(category, ct);

    [HttpGet("currencies")]
    [RequiresPermission(PermissionCodes.OrdersSetupRead)]
    public Task<IReadOnlyList<LookupListItem>> Currencies(CancellationToken ct) =>
        lookups.ListCurrenciesAsync(ct);

    [HttpPost("seed-standard")]
    [RequiresPermission(PermissionCodes.OrdersSetupManage)]
    public async Task<ActionResult<object>> Seed(CancellationToken ct) =>
        Ok(new { created = await lookups.SeedLookupsAsync(ct) });
}
