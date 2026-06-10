using KBM.Application.Reporting;
using KBM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KBM.Infrastructure.Persistence.Configuration;

/// <summary>Risolve la ReportDefinition: prima specifica dell'azienda, poi globale.</summary>
public sealed class ReportDefinitionProvider(KbmDbContext db) : IReportDefinitionProvider
{
    public async Task<ReportDefinition?> GetAsync(string key, long? companyId, CancellationToken ct = default)
    {
        if (companyId is not null)
        {
            var scoped = await db.ReportDefinitions
                .FirstOrDefaultAsync(r => r.Key == key && r.CompanyId == companyId && r.Enabled, ct);
            if (scoped is not null) return scoped;
        }
        return await db.ReportDefinitions.FirstOrDefaultAsync(r => r.Key == key && r.CompanyId == null && r.Enabled, ct);
    }
}
