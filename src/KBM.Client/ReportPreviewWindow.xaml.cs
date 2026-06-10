using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using KBM.Client.Services;

namespace KBM.Client;

/// <summary>
/// Anteprima di stampa interna (stile Business Cube, help pc10): visualizzazione paginata
/// con zoom/ricerca/navigazione (FlowDocumentReader) + toolbar Stampa / Esporta multi-formato.
/// </summary>
public partial class ReportPreviewWindow : Window
{
    private readonly ApiClient _api;
    private readonly string _reportKey;
    private readonly ReportDocumentDto _model;

    public ReportPreviewWindow(ApiClient api, string reportKey, ReportDocumentDto model)
    {
        InitializeComponent();
        _api = api;
        _reportKey = reportKey;
        _model = model;
        TitleText.Text = $"{model.Title}  ·  {model.Rows.Count} record";
        Reader.Document = ReportFlowBuilder.Build(model);
        InputBindings.Add(new KeyBinding(new RelayCommand2(() => Close()), Key.Escape, ModifierKeys.None));
        InputBindings.Add(new KeyBinding(new RelayCommand2(Print), Key.P, ModifierKeys.Control));
    }

    /// <summary>Apre l'anteprima non bloccante (come Business: non è una modale).</summary>
    public static void Show(ApiClient api, string reportKey, ReportDocumentDto model, Window? owner)
    {
        var w = new ReportPreviewWindow(api, reportKey, model) { Owner = owner };
        w.Show();
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void Print_Click(object sender, RoutedEventArgs e) => Print();

    private void Print()
    {
        var dlg = new System.Windows.Controls.PrintDialog();
        if (dlg.ShowDialog() != true) return;

        // FlowDocument fresco per la stampa (quello a video è già "parented" nel reader)
        var doc = ReportFlowBuilder.Build(_model);
        doc.PageHeight = dlg.PrintableAreaHeight;
        doc.PageWidth = dlg.PrintableAreaWidth;
        doc.ColumnWidth = dlg.PrintableAreaWidth;
        doc.PagePadding = new Thickness(40);
        IDocumentPaginatorSource idoc = doc;
        dlg.PrintDocument(idoc.DocumentPaginator, "KBM — " + _model.Title);
        ToastService.Success("Documento inviato alla stampante.", "Stampa");
    }

    private async void ExportPdf_Click(object sender, RoutedEventArgs e)
    {
        var outcome = await _api.RenderReportAsync(_reportKey, _model);
        if (!outcome.Ok || outcome.Content is null)
        {
            ToastService.Error(outcome.Error ?? "Esportazione PDF non riuscita.", "Esporta PDF");
            return;
        }
        try
        {
            var path = Path.Combine(Path.GetTempPath(), outcome.FileName ?? $"{_reportKey}.pdf");
            File.WriteAllBytes(path, outcome.Content);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
            ToastService.Success("PDF generato.", "Esporta PDF");
        }
        catch { ToastService.Error("Impossibile aprire il PDF generato.", "Esporta PDF"); }
    }

    private void ExportExcel_Click(object sender, RoutedEventArgs e) => ExportLocal(GridExportFormat.Excel);
    private void ExportCsv_Click(object sender, RoutedEventArgs e) => ExportLocal(GridExportFormat.Csv);
    private void ExportHtml_Click(object sender, RoutedEventArgs e) => ExportLocal(GridExportFormat.Html);

    private void ExportLocal(GridExportFormat format)
    {
        try
        {
            var path = GridExporter.ExportToDesktop(_model.Title, format, _model.Columns, _model.Rows);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
            ToastService.Success($"Esportati {_model.Rows.Count} record sul Desktop.", "Esportazione");
        }
        catch (Exception ex) { ToastService.Error("Esportazione non riuscita: " + ex.Message, "Esportazione"); }
    }
}

/// <summary>ICommand minimale per le scorciatoie dell'anteprima.</summary>
internal sealed class RelayCommand2(Action action) : ICommand
{
    public event EventHandler? CanExecuteChanged { add { } remove { } }
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => action();
}
