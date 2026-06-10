using KBM.Application.Reporting;
using KBM.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBM.Api.Controllers;

/// <summary>
/// Rendering report. Il client invia il modello documento; il motore di stampa
/// e il percorso/nome del template sono risolti dalla ReportDefinition (config tecnica).
/// </summary>
[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController(IReportingService reporting, ICurrentUserContext currentUser) : ControllerBase
{
    [HttpPost("{key}")]
    public async Task<IActionResult> Render(string key, [FromBody] ReportDocumentModel model, CancellationToken ct)
    {
        try
        {
            var result = await reporting.RenderAsync(key, currentUser.CompanyId, model, ct);
            if (result is null) return NotFound(new { message = $"Report '{key}' non configurato." });
            return File(result.Content, result.MimeType, result.FileName);
        }
        catch (NotSupportedException ex) { return StatusCode(501, new { message = ex.Message }); }
        catch (FileNotFoundException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
