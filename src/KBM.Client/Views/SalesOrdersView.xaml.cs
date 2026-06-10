using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class SalesOrdersView : UserControl
{
    private readonly ApiClient _api;
    private readonly FilterableGrid _grid;

    public SalesOrdersView(ApiClient api)
    {
        InitializeComponent();
        _api = api;
        _grid = new FilterableGrid();
        _grid.AddTextColumn("Numero", nameof(SoRow.Number), width: 140);
        _grid.AddTextColumn("Data", nameof(SoRow.DateText), width: 100);
        _grid.AddTextColumn("Cliente", nameof(SoRow.CustomerName), star: true);
        _grid.AddTextColumn("Righe", nameof(SoRow.LineCount), width: 70);
        _grid.AddTextColumn("Totale", nameof(SoRow.TotalText), width: 110);
        _grid.AddTextColumn("Stato", nameof(SoRow.StatusLabel), width: 120);
        _grid.SelectionChanged += OnSelectionChanged;

        Page.GridContent = _grid;
        Page.PageKey = "sales-orders";
        Page.PageTitle = "Ordini cliente";
        Page.Subtitle = "OV \u00b7 struttura dati modulo ordini (ambsor02)";
        Page.EnableWings();
        Page.AddToolbarButton(IconCatalog.Refresh, "Aggiorna", true, async (_, _) => await LoadAsync(), Key.F5);
        Page.AddToolbarButton(IconCatalog.Print, "Stampa", false, (_, _) => PrintList(), Key.F6);
        Page.AttachRecordNavigator(_grid);
        Page.AttachPersonalization(_grid, "sales-orders");
        _grid.ExportName = "OrdiniCliente";

        Loaded += async (_, _) => await LoadAsync();
    }

    private void OnSelectionChanged(object? item)
    {
        if (item is not SoRow r) return;
        Page.SetWingDetail(r.Number, new[]
        {
            ("Numero", r.Number),
            ("Data", r.DateText),
            ("Cliente", r.CustomerName),
            ("Righe", r.LineCount.ToString(CultureInfo.CurrentCulture)),
            ("Totale", r.TotalText),
            ("Stato", r.StatusLabel),
        });
    }

    private void PrintList()
    {
        var model = _grid.BuildReportModel("Ordini cliente", "OV", _api.CompanyName, "KBM ERP \u2014 Ordini");
        ReportPreviewWindow.Show(_api, "list.print", model, System.Windows.Window.GetWindow(this));
    }

    private async Task LoadAsync()
    {
        Page.HideError();
        try
        {
            var rows = await _api.GetSalesOrdersAsync();
            if (rows is null) { Page.ShowError("Impossibile caricare gli ordini cliente."); return; }
            _grid.SetItems(rows.Select(r => new SoRow(r)).ToList());
            Page.RecordCountLabel = $"{rows.Count} ordini";
            Page.UpdateRecordNavigator();
        }
        catch { Page.ShowError("Errore di connessione API."); }
    }

    private sealed class SoRow(SalesOrderListItem o)
    {
        public string Number { get; } = o.Number;
        public string DateText { get; } = o.OrderDate.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture);
        public string CustomerName { get; } = o.CustomerName;
        public int LineCount { get; } = o.LineCount;
        public string TotalText { get; } = o.TotalAmount.ToString("N2", CultureInfo.CurrentCulture);
        public string StatusLabel { get; } = OrderStatusLabel(o.Status);
    }

    private static string OrderStatusLabel(int s) => s switch
    {
        0 => "Bozza", 1 => "Confermato", 2 => "Parz. evaso", 3 => "Evaso", 4 => "Fatturato", 5 => "Annullato", _ => "-"
    };
}
