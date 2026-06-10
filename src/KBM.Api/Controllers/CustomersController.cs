using KBM.Api.Authorization;
using KBM.Application.Auth;
using KBM.Application.Customers;
using KBM.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBM.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController(ICustomerService customers) : ControllerBase
{
    [HttpGet]
    [RequiresPermission(PermissionCodes.CustomersRead)]
    public Task<IReadOnlyList<CustomerListItem>> List([FromQuery] string? search, CancellationToken ct) =>
        customers.ListAsync(search, ct);

    [HttpGet("{id:long}")]
    [RequiresPermission(PermissionCodes.CustomersRead)]
    public async Task<ActionResult<CustomerDetail>> Get(long id, CancellationToken ct)
    {
        var customer = await customers.GetAsync(id, ct);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpPost]
    [RequiresPermission(PermissionCodes.CustomersCreate)]
    public async Task<ActionResult<CustomerDetail>> Create([FromBody] CreateCustomerRequest request, CancellationToken ct)
    {
        try
        {
            var customer = await customers.CreateAsync(request, ct);
            return CreatedAtAction(nameof(Get), new { id = customer.Id }, customer);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiError("CONFLICT", ex.Message));
        }
    }

    [HttpPut("{id:long}")]
    [RequiresPermission(PermissionCodes.CustomersEdit)]
    public async Task<ActionResult<CustomerDetail>> Update(long id, [FromBody] UpdateCustomerRequest request, CancellationToken ct)
    {
        try
        {
            var customer = await customers.UpdateAsync(id, request, ct);
            return customer is null ? NotFound() : Ok(customer);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiError("CONFLICT", ex.Message));
        }
    }

    [HttpGet("{id:long}/full")]
    [RequiresPermission(PermissionCodes.CustomersRead)]
    public async Task<ActionResult<CustomerAggregate>> GetFull(long id, CancellationToken ct)
    {
        var agg = await customers.GetFullAsync(id, ct);
        return agg is null ? NotFound() : Ok(agg);
    }

    [HttpPost("full")]
    [RequiresPermission(PermissionCodes.CustomersCreate)]
    public async Task<ActionResult<CustomerAggregate>> CreateFull([FromBody] CreateCustomerAggregateRequest request, CancellationToken ct)
    {
        try
        {
            var agg = await customers.CreateFullAsync(request, ct);
            return CreatedAtAction(nameof(Get), new { id = agg.Detail.Id }, agg);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiError("CONFLICT", ex.Message));
        }
    }

    [HttpPut("{id:long}/full")]
    [RequiresPermission(PermissionCodes.CustomersEdit)]
    public async Task<ActionResult<CustomerAggregate>> SaveFull(long id, [FromBody] SaveCustomerAggregateRequest request, CancellationToken ct)
    {
        try
        {
            var agg = await customers.SaveFullAsync(id, request, ct);
            return agg is null ? NotFound() : Ok(agg);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiError("CONFLICT", ex.Message));
        }
    }

    [HttpPost("{id:long}/disable")]
    [RequiresPermission(PermissionCodes.CustomersEdit)]
    public async Task<IActionResult> Disable(long id, CancellationToken ct)
    {
        var ok = await customers.SetEnabledAsync(id, false, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:long}/enable")]
    [RequiresPermission(PermissionCodes.CustomersEdit)]
    public async Task<IActionResult> Enable(long id, CancellationToken ct)
    {
        var ok = await customers.SetEnabledAsync(id, true, ct);
        return ok ? NoContent() : NotFound();
    }
}
