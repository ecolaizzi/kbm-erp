using KBM.Api.Authorization;
using KBM.Application.Audit;
using KBM.Application.Security;
using KBM.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBM.Api.Controllers;

[ApiController]
[Route("api/audit")]
[Authorize]
public class AuditController(IAuditQueryService audit) : ControllerBase
{
    [HttpGet]
    [RequiresPermission(PermissionCodes.AuditRead)]
    public Task<PagedResult<AuditLogItem>> List([FromQuery] AuditListQuery query, CancellationToken ct) =>
        audit.ListAsync(query, ct);

    [HttpGet("{id:long}")]
    [RequiresPermission(PermissionCodes.AuditRead)]
    public async Task<ActionResult<AuditLogDetail>> Get(long id, CancellationToken ct)
    {
        var log = await audit.GetAsync(id, ct);
        return log is null ? NotFound() : Ok(log);
    }
}
