using KBM.Application.Reporting;
using KBM.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KBM.Reporting;

/// <summary>Motore moderno code-based: rende il modello documento in PDF A4.</summary>
public sealed class QuestPdfReportEngine : IReportEngine
{
    public ReportEngineType Engine => ReportEngineType.QuestPdf;

    public Task<ReportResult> RenderAsync(ReportDefinition definition, ReportDocumentModel model, CancellationToken ct = default)
    {
        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);
                page.DefaultTextStyle(t => t.FontSize(9).FontColor(Colors.Grey.Darken4));

                page.Header().Column(col =>
                {
                    if (!string.IsNullOrWhiteSpace(model.CompanyName))
                        col.Item().Text(model.CompanyName!).FontSize(11).SemiBold().FontColor(Colors.Blue.Darken3);
                    col.Item().PaddingTop(2).Text(model.Title).FontSize(15).Bold();
                    if (!string.IsNullOrWhiteSpace(model.Subtitle))
                        col.Item().Text(model.Subtitle!).FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().PaddingVertical(8).Column(col =>
                {
                    if (model.Header.Count > 0)
                    {
                        col.Item().PaddingBottom(8).Column(h =>
                        {
                            foreach (var f in model.Header)
                                h.Item().Text(txt =>
                                {
                                    txt.Span($"{f.Label}: ").SemiBold();
                                    txt.Span(f.Value);
                                });
                        });
                    }

                    if (model.Columns.Count > 0)
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                foreach (var _ in model.Columns) columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                foreach (var c in model.Columns)
                                    header.Cell().Background(Colors.Blue.Darken3).Padding(4)
                                        .Text(c).FontColor(Colors.White).SemiBold().FontSize(8.5f);
                            });

                            var alt = false;
                            foreach (var row in model.Rows)
                            {
                                var background = alt ? Colors.Grey.Lighten4 : Colors.White;
                                alt = !alt;
                                for (var i = 0; i < model.Columns.Count; i++)
                                {
                                    var value = i < row.Count ? row[i] : string.Empty;
                                    table.Cell().Background(background).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                        .Padding(4).Text(value).FontSize(8.5f);
                                }
                            }
                        });
                    }

                    if (model.Totals is { Count: > 0 })
                    {
                        col.Item().PaddingTop(10).AlignRight().Column(t =>
                        {
                            foreach (var f in model.Totals)
                                t.Item().Text(txt =>
                                {
                                    txt.Span($"{f.Label}: ").SemiBold();
                                    txt.Span(f.Value);
                                });
                        });
                    }
                });

                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                    col.Item().PaddingTop(3).Row(row =>
                    {
                        row.RelativeItem().Text(model.Footer ?? "KBM ERP").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                        row.ConstantItem(120).AlignRight().Text(txt =>
                        {
                            txt.DefaultTextStyle(s => s.FontSize(7.5f).FontColor(Colors.Grey.Darken1));
                            txt.Span("Pag. ");
                            txt.CurrentPageNumber();
                            txt.Span(" / ");
                            txt.TotalPages();
                        });
                    });
                });
            });
        }).GeneratePdf();

        var fileName = $"{definition.Key}-{DateTime.Now:yyyyMMddHHmmss}.pdf";
        return Task.FromResult(new ReportResult(bytes, "application/pdf", fileName));
    }
}
