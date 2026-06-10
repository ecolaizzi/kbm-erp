using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace KBM.Client.Services;

/// <summary>
/// Costruisce un FlowDocument stampabile da un ReportDocumentDto (anteprima interna
/// stile Business Cube, vedi help pc10): testata, tabella dati zebrata, totali, piè di pagina.
/// </summary>
public static class ReportFlowBuilder
{
    private static readonly Brush HeaderBrush = new SolidColorBrush(Color.FromRgb(0x1F, 0x3A, 0x5F));
    private static readonly Brush ZebraBrush = new SolidColorBrush(Color.FromRgb(0xF5, 0xF7, 0xFA));
    private static readonly Brush LineBrush = new SolidColorBrush(Color.FromRgb(0xD0, 0xD7, 0xE2));
    private static readonly Brush MutedBrush = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80));

    public static FlowDocument Build(ReportDocumentDto m)
    {
        var doc = new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 11,
            PagePadding = new Thickness(48, 40, 48, 40),
            // ColumnWidth lasciato auto: il lettore adatta la colonna alla pagina;
            // in stampa viene impostato sulla larghezza stampabile.
            Background = Brushes.White,
            Foreground = new SolidColorBrush(Color.FromRgb(0x1F, 0x29, 0x37))
        };

        if (!string.IsNullOrWhiteSpace(m.CompanyName))
            doc.Blocks.Add(new Paragraph(new Run(m.CompanyName))
            {
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = HeaderBrush,
                Margin = new Thickness(0, 0, 0, 2)
            });

        doc.Blocks.Add(new Paragraph(new Run(m.Title))
        {
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 2)
        });

        if (!string.IsNullOrWhiteSpace(m.Subtitle))
            doc.Blocks.Add(new Paragraph(new Run(m.Subtitle))
            {
                FontSize = 12,
                Foreground = MutedBrush,
                Margin = new Thickness(0, 0, 0, 8)
            });

        if (m.Header is { Count: > 0 })
        {
            var hp = new Paragraph { Margin = new Thickness(0, 0, 0, 10), FontSize = 11 };
            for (var i = 0; i < m.Header.Count; i++)
            {
                var f = m.Header[i];
                hp.Inlines.Add(new Run($"{f.Label}: ") { Foreground = MutedBrush });
                hp.Inlines.Add(new Run(f.Value) { FontWeight = FontWeights.SemiBold });
                if (i < m.Header.Count - 1) hp.Inlines.Add(new Run("      "));
            }
            doc.Blocks.Add(hp);
        }

        doc.Blocks.Add(BuildTable(m));

        if (m.Totals is { Count: > 0 })
        {
            var tp = new Paragraph { Margin = new Thickness(0, 12, 0, 0), TextAlignment = TextAlignment.Right };
            foreach (var t in m.Totals)
            {
                tp.Inlines.Add(new Run($"{t.Label}: ") { Foreground = MutedBrush });
                tp.Inlines.Add(new Run(t.Value + "    ") { FontWeight = FontWeights.Bold, FontSize = 12 });
            }
            doc.Blocks.Add(tp);
        }

        var footerText = string.IsNullOrWhiteSpace(m.Footer) ? "KBM ERP" : m.Footer;
        doc.Blocks.Add(new Paragraph(new Run($"{footerText}  ·  generato il {DateTime.Now:dd/MM/yyyy HH:mm}"))
        {
            FontSize = 9.5,
            Foreground = MutedBrush,
            Margin = new Thickness(0, 16, 0, 0)
        });

        return doc;
    }

    private static Table BuildTable(ReportDocumentDto m)
    {
        var table = new Table { CellSpacing = 0, Margin = new Thickness(0) };
        foreach (var _ in m.Columns)
            table.Columns.Add(new TableColumn { Width = GridLength.Auto });

        var headerGroup = new TableRowGroup();
        var headerRow = new TableRow { Background = HeaderBrush };
        foreach (var c in m.Columns)
            headerRow.Cells.Add(Cell(c, bold: true, foreground: Brushes.White));
        headerGroup.Rows.Add(headerRow);
        table.RowGroups.Add(headerGroup);

        var body = new TableRowGroup();
        for (var r = 0; r < m.Rows.Count; r++)
        {
            var row = new TableRow();
            if (r % 2 == 1) row.Background = ZebraBrush;
            foreach (var cell in m.Rows[r])
                row.Cells.Add(Cell(cell, bold: false, foreground: null));
            body.Rows.Add(row);
        }
        table.RowGroups.Add(body);
        return table;
    }

    private static TableCell Cell(string text, bool bold, Brush? foreground)
    {
        var p = new Paragraph(new Run(text ?? "")) { Margin = new Thickness(0), FontSize = 10 };
        if (bold) p.FontWeight = FontWeights.SemiBold;
        if (foreground is not null) p.Foreground = foreground;
        return new TableCell(p)
        {
            Padding = new Thickness(6, 3, 6, 3),
            BorderBrush = LineBrush,
            BorderThickness = new Thickness(0, 0, 1, 1)
        };
    }
}
