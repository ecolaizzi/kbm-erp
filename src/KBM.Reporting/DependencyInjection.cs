using KBM.Application.Reporting;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;

namespace KBM.Reporting;

public static class DependencyInjection
{
    public static IServiceCollection AddReporting(this IServiceCollection services)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        services.AddScoped<IReportEngine, QuestPdfReportEngine>();
        services.AddScoped<IReportEngine, WordMailMergeEngine>();
        services.AddScoped<IReportEngine, CrystalReportEngine>();
        services.AddScoped<IReportingService, ReportingService>();
        return services;
    }
}
