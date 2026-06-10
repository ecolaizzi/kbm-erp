namespace KBM.Application.Reporting;

/// <summary>Campo etichetta/valore di testata report.</summary>
public record ReportField(string Label, string Value);

/// <summary>
/// Modello documento generico e tabellare, indipendente dal motore di stampa.
/// Permette di rendere lo stesso documento con QuestPDF o stampa unione Word.
/// </summary>
public record ReportDocumentModel(
    string Title,
    string? Subtitle,
    IReadOnlyList<ReportField> Header,
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyList<string>> Rows,
    IReadOnlyList<ReportField>? Totals = null,
    string? Footer = null,
    string? CompanyName = null);

/// <summary>Esito del rendering di un report.</summary>
public record ReportResult(byte[] Content, string MimeType, string FileName);
