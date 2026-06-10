using System.IO;
using System.IO.Packaging;
using System.Text;
using System.Xml;

namespace KBM.Client.Services;

public enum GridExportFormat { Excel, Csv, Html }

/// <summary>
/// Esportazione griglie stile Business Cube: genera Excel (.xlsx), CSV o HTML sul Desktop.
/// XLSX scritto via System.IO.Packaging (SpreadsheetML), senza dipendenze esterne.
/// </summary>
public static class GridExporter
{
    /// <summary>Esporta su Desktop e apre il file. Ritorna il percorso generato.</summary>
    public static string ExportToDesktop(string baseName, GridExportFormat format,
        IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
    {
        var ext = format switch
        {
            GridExportFormat.Excel => "xlsx",
            GridExportFormat.Csv => "csv",
            _ => "html"
        };
        var safe = string.Join("_", baseName.Split(Path.GetInvalidFileNameChars()));
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            $"{safe}_{DateTime.Now:yyyyMMdd_HHmmss}.{ext}");

        switch (format)
        {
            case GridExportFormat.Csv: WriteCsv(path, headers, rows); break;
            case GridExportFormat.Html: WriteHtml(path, baseName, headers, rows); break;
            default: WriteXlsx(path, baseName, headers, rows); break;
        }

        return path;
    }

    private static void WriteCsv(string path, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(";", headers.Select(CsvCell)));
        foreach (var r in rows)
            sb.AppendLine(string.Join(";", r.Select(CsvCell)));
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
    }

    private static string CsvCell(string v)
    {
        v ??= "";
        if (v.Contains('"') || v.Contains(';') || v.Contains('\n') || v.Contains('\r'))
            return "\"" + v.Replace("\"", "\"\"") + "\"";
        return v;
    }

    private static void WriteHtml(string path, string title, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html lang=\"it\"><head><meta charset=\"utf-8\"/>");
        sb.AppendLine($"<title>{Esc(title)}</title>");
        sb.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;font-size:12px;color:#1f2937;margin:24px}" +
                      "h1{font-size:16px}table{border-collapse:collapse;width:100%}" +
                      "th,td{border:1px solid #e5e7eb;padding:5px 8px;text-align:left}" +
                      "th{background:#1f3a5f;color:#fff;font-weight:600}tr:nth-child(even) td{background:#f8fafc}</style>");
        sb.AppendLine("</head><body>");
        sb.AppendLine($"<h1>{Esc(title)}</h1>");
        sb.AppendLine($"<p>{rows.Count} record &middot; {DateTime.Now:dd/MM/yyyy HH:mm}</p>");
        sb.AppendLine("<table><thead><tr>");
        foreach (var h in headers) sb.Append("<th>").Append(Esc(h)).Append("</th>");
        sb.AppendLine("</tr></thead><tbody>");
        foreach (var r in rows)
        {
            sb.Append("<tr>");
            foreach (var c in r) sb.Append("<td>").Append(Esc(c)).Append("</td>");
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("</tbody></table></body></html>");
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
    }

    private static string Esc(string? v) => System.Net.WebUtility.HtmlEncode(v ?? "");

    // ===================== XLSX (SpreadsheetML minimale via Packaging) =====================
    private const string NsSpreadsheet = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private const string NsRelDoc = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private const string RelOfficeDocument = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument";
    private const string RelWorksheet = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet";

    private static void WriteXlsx(string path, string sheetName, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
    {
        if (File.Exists(path)) File.Delete(path);
        using var pkg = Package.Open(path, FileMode.Create);

        var wbUri = new Uri("/xl/workbook.xml", UriKind.Relative);
        var wbPart = pkg.CreatePart(wbUri,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml", CompressionOption.Normal);
        pkg.CreateRelationship(wbUri, TargetMode.Internal, RelOfficeDocument, "rId1");

        var wsUri = new Uri("/xl/worksheets/sheet1.xml", UriKind.Relative);
        var wsPart = pkg.CreatePart(wsUri,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml", CompressionOption.Normal);
        wbPart.CreateRelationship(new Uri("worksheets/sheet1.xml", UriKind.Relative), TargetMode.Internal, RelWorksheet, "rId1");

        using (var w = XmlWriter.Create(wbPart.GetStream(FileMode.Create), new XmlWriterSettings { Encoding = new UTF8Encoding(false) }))
        {
            w.WriteStartDocument(true);
            w.WriteStartElement("workbook", NsSpreadsheet);
            w.WriteAttributeString("xmlns", "r", null, NsRelDoc);
            w.WriteStartElement("sheets", NsSpreadsheet);
            w.WriteStartElement("sheet", NsSpreadsheet);
            w.WriteAttributeString("name", Clip(sheetName));
            w.WriteAttributeString("sheetId", "1");
            w.WriteAttributeString("r", "id", NsRelDoc, "rId1");
            w.WriteEndElement(); // sheet
            w.WriteEndElement(); // sheets
            w.WriteEndElement(); // workbook
            w.WriteEndDocument();
        }

        using (var w = XmlWriter.Create(wsPart.GetStream(FileMode.Create), new XmlWriterSettings { Encoding = new UTF8Encoding(false) }))
        {
            w.WriteStartDocument(true);
            w.WriteStartElement("worksheet", NsSpreadsheet);
            w.WriteStartElement("sheetData", NsSpreadsheet);

            WriteRow(w, 1, headers);
            for (var i = 0; i < rows.Count; i++)
                WriteRow(w, i + 2, rows[i]);

            w.WriteEndElement(); // sheetData
            w.WriteEndElement(); // worksheet
            w.WriteEndDocument();
        }
    }

    private static void WriteRow(XmlWriter w, int rowIndex, IReadOnlyList<string> cells)
    {
        w.WriteStartElement("row", NsSpreadsheet);
        w.WriteAttributeString("r", rowIndex.ToString());
        for (var c = 0; c < cells.Count; c++)
        {
            w.WriteStartElement("c", NsSpreadsheet);
            w.WriteAttributeString("r", $"{ColumnName(c)}{rowIndex}");
            w.WriteAttributeString("t", "inlineStr");
            w.WriteStartElement("is", NsSpreadsheet);
            w.WriteStartElement("t", NsSpreadsheet);
            w.WriteAttributeString("xml", "space", null, "preserve");
            w.WriteString(cells[c] ?? "");
            w.WriteEndElement(); // t
            w.WriteEndElement(); // is
            w.WriteEndElement(); // c
        }
        w.WriteEndElement(); // row
    }

    private static string ColumnName(int index)
    {
        var name = "";
        index++;
        while (index > 0)
        {
            var rem = (index - 1) % 26;
            name = (char)('A' + rem) + name;
            index = (index - 1) / 26;
        }
        return name;
    }

    private static string Clip(string name)
    {
        name = string.Concat(name.Where(ch => ch is not ('\\' or '/' or '?' or '*' or '[' or ']' or ':')));
        return name.Length > 31 ? name[..31] : name;
    }
}
