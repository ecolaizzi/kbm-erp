using KBM.Application.Reporting;

namespace KBM.Reporting;

public sealed class ReportingService(IReportDefinitionProvider definitions, IEnumerable<IReportEngine> engines) : IReportingService
{
    public async Task<ReportResult?> RenderAsync(string key, long? companyId, ReportDocumentModel model, CancellationToken ct = default)
    {
        var def = await definitions.GetAsync(key, companyId, ct);
        if (def is null) return null;

        var engine = engines.FirstOrDefault(e => e.Engine == def.Engine)
            ?? throw new InvalidOperationException($"Nessun motore di stampa registrato per '{def.Engine}'.");

        return await engine.RenderAsync(def, model, ct);
    }
}
