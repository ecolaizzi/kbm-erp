using KBM.Application.Reporting;
using KBM.Domain.Entities;

namespace KBM.Reporting;

/// <summary>
/// Segnaposto per Crystal Reports. Crystal NON e supportato nativamente su .NET 8:
/// l'integrazione reale richiede un host out-of-process .NET Framework o un Report Server.
/// L'astrazione e gia predisposta per attivare questo provider senza modificare i chiamanti.
/// </summary>
public sealed class CrystalReportEngine : IReportEngine
{
    public ReportEngineType Engine => ReportEngineType.Crystal;

    public Task<ReportResult> RenderAsync(ReportDefinition definition, ReportDocumentModel model, CancellationToken ct = default) =>
        throw new NotSupportedException(
            "Crystal Reports non e ancora attivo: richiede un host out-of-process .NET Framework / Report Server. " +
            $"Selezionare un altro motore per il report '{definition.Key}' oppure completare l'integrazione Crystal.");
}
