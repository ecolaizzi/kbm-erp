using KBM.Api.Authorization;
using KBM.Application.Auth;
using KBM.Application.Roles;
using KBM.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBM.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize]
public class RolesController(IRoleService roles) : ControllerBase
{
    [HttpGet]
    [RequiresPermission(PermissionCodes.RolesRead)]
    public Task<IReadOnlyList<RoleListItem>> List(CancellationToken ct) =>
        roles.ListAsync(ct);

    [HttpGet("{id:long}")]
    [RequiresPermission(PermissionCodes.RolesRead)]
    public async Task<ActionResult<RoleDetail>> Get(long id, CancellationToken ct)
    {
        var role = await roles.GetAsync(id, ct);
        return role is null ? NotFound() : Ok(role);
    }

    [HttpPost]
    [RequiresPermission(PermissionCodes.RolesCreate)]
    public async Task<ActionResult<RoleDetail>> Create([FromBody] CreateRoleRequest request, CancellationToken ct)
    {
        try
        {
            var role = await roles.CreateAsync(request, ct);
            return CreatedAtAction(nameof(Get), new { id = role.Id }, role);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiError("CONFLICT", ex.Message));
        }
    }

    [HttpPut("{id:long}")]
    [RequiresPermission(PermissionCodes.RolesEdit)]
    public async Task<ActionResult<RoleDetail>> Update(long id, [FromBody] UpdateRoleRequest request, CancellationToken ct)
    {
        var role = await roles.UpdateAsync(id, request, ct);
        return role is null ? NotFound() : Ok(role);
    }
}
