namespace KBM.Domain.Entities;

/// <summary>Motore di stampa usato per rendere un report.</summary>
public enum ReportEngineType
{
    /// <summary>Motore moderno code-based (PDF).</summary>
    QuestPdf = 0,
    /// <summary>Stampa unione su template Word (.docx) via OpenXML.</summary>
    WordMailMerge = 1,
    /// <summary>Crystal Reports (segnaposto: non supportato nativamente su .NET 8).</summary>
    Crystal = 2
}

/// <summary>Formato di output del report.</summary>
public enum ReportOutputFormat
{
    Pdf = 0,
    Docx = 1
}

/// <summary>
/// Definizione tecnica di un report: motore di stampa, formato e percorso/nome
/// del template. Configurabile solo dalla modalita sviluppatore.
/// CompanyId null = definizione globale predefinita.
/// </summary>
public class ReportDefinition
{
    public long Id { get; set; }
    public long? CompanyId { get; set; }

    /// <summary>Chiave logica del report (es. "rda.print", "rdo.print").</summary>
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    public ReportEngineType Engine { get; set; } = ReportEngineType.QuestPdf;
    public ReportOutputFormat OutputFormat { get; set; } = ReportOutputFormat.Pdf;

    /// <summary>Percorso o nome del template/report (per WordMailMerge/Crystal).</summary>
    public string? TemplatePathOrName { get; set; }

    public bool Enabled { get; set; } = true;
    public DateTime UpdatedAt { get; set; }
}
