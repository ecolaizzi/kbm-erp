using KBM.Domain.Entities;

namespace KBM.Application.Configuration;

public record SystemSettingDto(long Id, long? CompanyId, string Key, string Value, string Category, string? Description, DateTime UpdatedAt);
public record UpsertSettingRequest(long? CompanyId, string Key, string Value, string Category, string? Description);

public record ReportDefinitionDto(
    long Id, long? CompanyId, string Key, string Title,
    ReportEngineType Engine, ReportOutputFormat OutputFormat,
    string? TemplatePathOrName, bool Enabled, DateTime UpdatedAt);

public record UpsertReportDefinitionRequest(
    long Id, long? CompanyId, string Key, string Title,
    ReportEngineType Engine, ReportOutputFormat OutputFormat,
    string? TemplatePathOrName, bool Enabled);
