using KBM.Application.Auth;
using KBM.Application.Setup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBM.Api.Controllers;

[ApiController]
[Route("api/setup")]
public class SetupController(ISetupService setup) : ControllerBase
{
    [HttpGet("status")]
    [AllowAnonymous]
    public async Task<ActionResult<SetupStatusResponse>> Status(CancellationToken ct)
        => Ok(await setup.GetStatusAsync(ct));

    [HttpPost("complete")]
    [AllowAnonymous]
    public async Task<ActionResult<SetupCompleteResponse>> Complete([FromBody] SetupCompleteRequest request, CancellationToken ct)
    {
        try
        {
            return Ok(await setup.CompleteAsync(request, ct));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiError("SETUP_ALREADY_DONE", ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiError("SETUP_INVALID", ex.Message));
        }
    }
}
