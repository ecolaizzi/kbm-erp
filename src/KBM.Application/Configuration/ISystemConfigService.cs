namespace KBM.Application.Configuration;

public interface ISystemConfigService
{
    Task<IReadOnlyList<SystemSettingDto>> ListSettingsAsync(long? companyId, string? category, CancellationToken ct = default);
    Task<SystemSettingDto> UpsertSettingAsync(UpsertSettingRequest request, CancellationToken ct = default);
    Task<bool> DeleteSettingAsync(long id, CancellationToken ct = default);

    Task<IReadOnlyList<ReportDefinitionDto>> ListReportDefinitionsAsync(long? companyId, CancellationToken ct = default);
    Task<ReportDefinitionDto> UpsertReportDefinitionAsync(UpsertReportDefinitionRequest request, CancellationToken ct = default);
    Task<bool> DeleteReportDefinitionAsync(long id, CancellationToken ct = default);

    /// <summary>Crea le definizioni report predefinite (rda.print, rdo.print) se mancanti.</summary>
    Task EnsureDefaultReportsAsync(CancellationToken ct = default);
}
