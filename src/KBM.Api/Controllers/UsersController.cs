using KBM.Api.Authorization;
using KBM.Application.Auth;
using KBM.Application.Security;
using KBM.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBM.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(IUserService users) : ControllerBase
{
    [HttpGet]
    [RequiresPermission(PermissionCodes.UsersRead)]
    public Task<PagedResult<UserListItem>> List([FromQuery] UserListQuery query, CancellationToken ct) =>
        users.ListAsync(query, ct);

    [HttpGet("{id:long}")]
    [RequiresPermission(PermissionCodes.UsersRead)]
    public async Task<ActionResult<UserDetail>> Get(long id, CancellationToken ct)
    {
        var user = await users.GetAsync(id, ct);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    [RequiresPermission(PermissionCodes.UsersCreate)]
    public async Task<ActionResult<UserDetail>> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        try
        {
            var user = await users.CreateAsync(request, ct);
            return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiError("VALIDATION", ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiError("CONFLICT", ex.Message));
        }
    }

    [HttpPut("{id:long}")]
    [RequiresPermission(PermissionCodes.UsersEdit)]
    public async Task<ActionResult<UserDetail>> Update(long id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        var user = await users.UpdateAsync(id, request, ct);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost("{id:long}/disable")]
    [RequiresPermission(PermissionCodes.UsersDelete)]
    public async Task<IActionResult> Disable(long id, CancellationToken ct)
    {
        var ok = await users.DisableAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:long}/enable")]
    [RequiresPermission(PermissionCodes.UsersEdit)]
    public async Task<IActionResult> Enable(long id, CancellationToken ct)
    {
        var ok = await users.EnableAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }
}
