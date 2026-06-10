using KBM.Api.Authorization;
using KBM.Application.Auth;
using KBM.Application.BaseTables;
using KBM.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBM.Api.Controllers;

[ApiController]
[Route("api/payment-terms")]
[Authorize]
public class PaymentTermsController(IPaymentTermService terms) : ControllerBase
{
    [HttpGet]
    [RequiresPermission(PermissionCodes.PaymentTermsRead)]
    public Task<IReadOnlyList<PaymentTermListItem>> List([FromQuery] string? search, CancellationToken ct) =>
        terms.ListAsync(search, ct);

    [HttpGet("{id:long}")]
    [RequiresPermission(PermissionCodes.PaymentTermsRead)]
    public async Task<ActionResult<PaymentTermDetail>> Get(long id, CancellationToken ct)
    {
        var t = await terms.GetAsync(id, ct);
        return t is null ? NotFound() : Ok(t);
    }

    [HttpPost]
    [RequiresPermission(PermissionCodes.PaymentTermsCreate)]
    public async Task<ActionResult<PaymentTermDetail>> Create([FromBody] CreatePaymentTermRequest request, CancellationToken ct)
    {
        try
        {
            var t = await terms.CreateAsync(request, ct);
            return CreatedAtAction(nameof(Get), new { id = t.Id }, t);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpPut("{id:long}")]
    [RequiresPermission(PermissionCodes.PaymentTermsEdit)]
    public async Task<ActionResult<PaymentTermDetail>> Update(long id, [FromBody] UpdatePaymentTermRequest request, CancellationToken ct)
    {
        try
        {
            var t = await terms.UpdateAsync(id, request, ct);
            return t is null ? NotFound() : Ok(t);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpPost("{id:long}/disable")]
    [RequiresPermission(PermissionCodes.PaymentTermsEdit)]
    public async Task<IActionResult> Disable(long id, CancellationToken ct) =>
        await terms.SetEnabledAsync(id, false, ct) ? NoContent() : NotFound();

    [HttpPost("{id:long}/enable")]
    [RequiresPermission(PermissionCodes.PaymentTermsEdit)]
    public async Task<IActionResult> Enable(long id, CancellationToken ct) =>
        await terms.SetEnabledAsync(id, true, ct) ? NoContent() : NotFound();
}
