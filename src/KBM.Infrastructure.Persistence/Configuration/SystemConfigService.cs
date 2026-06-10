using KBM.Application.Configuration;
using KBM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KBM.Infrastructure.Persistence.Configuration;

public sealed class SystemConfigService(KbmDbContext db) : ISystemConfigService
{
    public async Task<IReadOnlyList<SystemSettingDto>> ListSettingsAsync(long? companyId, string? category, CancellationToken ct = default)
    {
        var query = db.SystemSettings.AsQueryable();
        query = companyId is null ? query.Where(s => s.CompanyId == null) : query.Where(s => s.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(s => s.Category == category);
        return await query.OrderBy(s => s.Category).ThenBy(s => s.Key)
            .Select(s => new SystemSettingDto(s.Id, s.CompanyId, s.Key, s.Value, s.Category, s.Description, s.UpdatedAt))
            .ToListAsync(ct);
    }

    public async Task<SystemSettingDto> UpsertSettingAsync(UpsertSettingRequest request, CancellationToken ct = default)
    {
        var key = request.Key.Trim();
        var existing = await db.SystemSettings
            .FirstOrDefaultAsync(s => s.CompanyId == request.CompanyId && s.Key == key, ct);
        if (existing is null)
        {
            existing = new SystemSetting { CompanyId = request.CompanyId, Key = key };
            db.SystemSettings.Add(existing);
        }
        existing.Value = request.Value;
        existing.Category = string.IsNullOrWhiteSpace(request.Category) ? "General" : request.Category.Trim();
        existing.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        existing.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return new SystemSettingDto(existing.Id, existing.CompanyId, existing.Key, existing.Value, existing.Category, existing.Description, existing.UpdatedAt);
    }

    public async Task<bool> DeleteSettingAsync(long id, CancellationToken ct = default)
    {
        var s = await db.SystemSettings.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (s is null) return false;
        db.SystemSettings.Remove(s);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<ReportDefinitionDto>> ListReportDefinitionsAsync(long? companyId, CancellationToken ct = default)
    {
        var query = companyId is null
            ? db.ReportDefinitions.Where(r => r.CompanyId == null)
            : db.ReportDefinitions.Where(r => r.CompanyId == companyId || r.CompanyId == null);
        return await query.OrderBy(r => r.Key)
            .Select(r => new ReportDefinitionDto(r.Id, r.CompanyId, r.Key, r.Title, r.Engine, r.OutputFormat, r.TemplatePathOrName, r.Enabled, r.UpdatedAt))
            .ToListAsync(ct);
    }

    public async Task<ReportDefinitionDto> UpsertReportDefinitionAsync(UpsertReportDefinitionRequest request, CancellationToken ct = default)
    {
        ReportDefinition? def = request.Id > 0
            ? await db.ReportDefinitions.FirstOrDefaultAsync(r => r.Id == request.Id, ct)
            : await db.ReportDefinitions.FirstOrDefaultAsync(r => r.CompanyId == request.CompanyId && r.Key == request.Key, ct);

        if (def is null)
        {
            def = new ReportDefinition { CompanyId = request.CompanyId, Key = request.Key.Trim() };
            db.ReportDefinitions.Add(def);
        }
        def.Title = request.Title.Trim();
        def.Engine = request.Engine;
        def.OutputFormat = request.OutputFormat;
        def.TemplatePathOrName = string.IsNullOrWhiteSpace(request.TemplatePathOrName) ? null : request.TemplatePathOrName.Trim();
        def.Enabled = request.Enabled;
        def.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return new ReportDefinitionDto(def.Id, def.CompanyId, def.Key, def.Title, def.Engine, def.OutputFormat, def.TemplatePathOrName, def.Enabled, def.UpdatedAt);
    }

    public async Task<bool> DeleteReportDefinitionAsync(long id, CancellationToken ct = default)
    {
        var r = await db.ReportDefinitions.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (r is null) return false;
        db.ReportDefinitions.Remove(r);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task EnsureDefaultReportsAsync(CancellationToken ct = default)
    {
        var defaults = new (string Key, string Title)[]
        {
            ("rda.print", "Stampa Richiesta di Acquisto (RDA)"),
            ("rdo.print", "Stampa Richiesta di Offerta (RDO)"),
            // Reportistica interna moduli (anagrafiche / magazzino) — stampa elenchi
            ("items.catalog", "Listino / Catalogo articoli"),
            ("items.inventory", "Situazione articoli di magazzino"),
            ("customers.list", "Elenco clienti"),
            ("suppliers.list", "Elenco fornitori"),
            ("list.print", "Stampa elenco (generico)")
        };
        var existing = await db.ReportDefinitions.Where(r => r.CompanyId == null).Select(r => r.Key).ToListAsync(ct);
        var added = false;
        foreach (var (key, title) in defaults)
        {
            if (existing.Contains(key)) continue;
            db.ReportDefinitions.Add(new ReportDefinition
            {
                CompanyId = null, Key = key, Title = title,
                Engine = ReportEngineType.QuestPdf, OutputFormat = ReportOutputFormat.Pdf,
                Enabled = true, UpdatedAt = DateTime.UtcNow
            });
            added = true;
        }
        if (added) await db.SaveChangesAsync(ct);
    }
}
