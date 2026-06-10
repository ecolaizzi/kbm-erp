using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using KBM.Application.Reporting;
using KBM.Domain.Entities;

namespace KBM.Reporting;

/// <summary>
/// Stampa unione su template Word (.docx): sostituisce i segnaposto
/// {{Title}}, {{Subtitle}}, {{CompanyName}}, {{Footer}}, {{Campo:Etichetta}} e {{Lines}}.
/// </summary>
public sealed class WordMailMergeEngine : IReportEngine
{
    public ReportEngineType Engine => ReportEngineType.WordMailMerge;

    public Task<ReportResult> RenderAsync(ReportDefinition definition, ReportDocumentModel model, CancellationToken ct = default)
    {
        var templatePath = definition.TemplatePathOrName;
        if (string.IsNullOrWhiteSpace(templatePath) || !File.Exists(templatePath))
            throw new FileNotFoundException(
                $"Template di stampa unione non trovato per il report '{definition.Key}'. " +
                "Configurare il percorso del modello .docx dalla modalita sviluppatore.", templatePath ?? "(vuoto)");

        var tokens = BuildTokens(model);

        using var ms = new MemoryStream();
        var templateBytes = File.ReadAllBytes(templatePath);
        ms.Write(templateBytes, 0, templateBytes.Length);
        ms.Position = 0;

        using (var doc = WordprocessingDocument.Open(ms, true))
        {
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body is not null)
            {
                foreach (var text in body.Descendants<Text>())
                    foreach (var (token, value) in tokens)
                        if (text.Text.Contains(token))
                            text.Text = text.Text.Replace(token, value);
                // L'apertura in modalita edit (autosave) persiste le modifiche alla chiusura.
            }
        }

        var fileName = $"{definition.Key}-{DateTime.Now:yyyyMMddHHmmss}.docx";
        return Task.FromResult(new ReportResult(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName));
    }

    private static Dictionary<string, string> BuildTokens(ReportDocumentModel model)
    {
        var tokens = new Dictionary<string, string>
        {
            ["{{Title}}"] = model.Title,
            ["{{Subtitle}}"] = model.Subtitle ?? string.Empty,
            ["{{CompanyName}}"] = model.CompanyName ?? string.Empty,
            ["{{Footer}}"] = model.Footer ?? string.Empty
        };
        foreach (var f in model.Header) tokens[$"{{{{Campo:{f.Label}}}}}"] = f.Value;

        var lines = new StringBuilder();
        if (model.Columns.Count > 0) lines.AppendLine(string.Join("\t", model.Columns));
        foreach (var row in model.Rows) lines.AppendLine(string.Join("\t", row));
        tokens["{{Lines}}"] = lines.ToString();
        return tokens;
    }
}
