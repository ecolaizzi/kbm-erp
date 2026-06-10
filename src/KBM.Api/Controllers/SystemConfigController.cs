using KBM.Api.Authorization;
using KBM.Application.Auth;
using KBM.Application.Configuration;
using KBM.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBM.Api.Controllers;

/// <summary>
/// Configurazioni azienda e tecniche + definizioni report. Riservato alla modalita
/// sviluppatore: tutti gli endpoint richiedono permessi system.* non assegnati ai ruoli standard.
/// </summary>
[ApiController]
[Route("api/system-config")]
[Authorize]
public class SystemConfigController(ISystemConfigService config) : ControllerBase
{
    /// <summary>Verifica accesso alla modalita sviluppatore (usato dalla gesture nascosta).</summary>
    [HttpGet("can-access")]
    [RequiresPermission(PermissionCodes.DeveloperAccess)]
    public IActionResult CanAccess() => Ok(new { ok = true });

    [HttpGet("settings")]
    [RequiresPermission(PermissionCodes.ConfigRead)]
    public Task<IReadOnlyList<SystemSettingDto>> ListSettings([FromQuery] long? companyId, [FromQuery] string? category, CancellationToken ct) =>
        config.ListSettingsAsync(companyId, category, ct);

    [HttpPut("settings")]
    [RequiresPermission(PermissionCodes.ConfigEdit)]
    public async Task<ActionResult<SystemSettingDto>> UpsertSetting([FromBody] UpsertSettingRequest request, CancellationToken ct)
    {
        try { return Ok(await config.UpsertSettingAsync(request, ct)); }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpDelete("settings/{id:long}")]
    [RequiresPermission(PermissionCodes.ConfigEdit)]
    public async Task<IActionResult> DeleteSetting(long id, CancellationToken ct) =>
        await config.DeleteSettingAsync(id, ct) ? NoContent() : NotFound();

    [HttpGet("reports")]
    [RequiresPermission(PermissionCodes.ConfigRead)]
    public Task<IReadOnlyList<ReportDefinitionDto>> ListReports([FromQuery] long? companyId, CancellationToken ct) =>
        config.ListReportDefinitionsAsync(companyId, ct);

    [HttpPut("reports")]
    [RequiresPermission(PermissionCodes.ConfigEdit)]
    public async Task<ActionResult<ReportDefinitionDto>> UpsertReport([FromBody] UpsertReportDefinitionRequest request, CancellationToken ct)
    {
        try { return Ok(await config.UpsertReportDefinitionAsync(request, ct)); }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpDelete("reports/{id:long}")]
    [RequiresPermission(PermissionCodes.ConfigEdit)]
    public async Task<IActionResult> DeleteReport(long id, CancellationToken ct) =>
        await config.DeleteReportDefinitionAsync(id, ct) ? NoContent() : NotFound();
}
