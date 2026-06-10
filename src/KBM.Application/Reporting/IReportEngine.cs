using KBM.Domain.Entities;

namespace KBM.Application.Reporting;

/// <summary>Motore di stampa pluggable. Ogni implementazione gestisce un ReportEngineType.</summary>
public interface IReportEngine
{
    ReportEngineType Engine { get; }
    Task<ReportResult> RenderAsync(ReportDefinition definition, ReportDocumentModel model, CancellationToken ct = default);
}

/// <summary>Fornisce la definizione tecnica di un report (motore, formato, template).</summary>
public interface IReportDefinitionProvider
{
    Task<ReportDefinition?> GetAsync(string key, long? companyId, CancellationToken ct = default);
}

/// <summary>Risolve il motore in base alla definizione e produce il file.</summary>
public interface IReportingService
{
    Task<ReportResult?> RenderAsync(string key, long? companyId, ReportDocumentModel model, CancellationToken ct = default);
}
