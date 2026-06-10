using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class PurchaseOrdersView : UserControl
{
    private readonly ApiClient _api;
    private readonly FilterableGrid _grid;

    public PurchaseOrdersView(ApiClient api)
    {
        InitializeComponent();
        _api = api;
        _grid = new FilterableGrid();
        _grid.AddTextColumn("Numero", nameof(PoRow.Number), width: 140);
        _grid.AddTextColumn("Data", nameof(PoRow.DateText), width: 100);
        _grid.AddTextColumn("Fornitore", nameof(PoRow.SupplierName), star: true);
        _grid.AddTextColumn("Righe", nameof(PoRow.LineCount), width: 70);
        _grid.AddTextColumn("Totale", nameof(PoRow.TotalText), width: 110);
        _grid.AddTextColumn("Stato", nameof(PoRow.StatusLabel), width: 120);
        _grid.SelectionChanged += OnSelectionChanged;

        Page.GridContent = _grid;
        Page.PageKey = "purchase-orders";
        Page.PageTitle = "Ordini fornitore";
        Page.Subtitle = "ODA \u00b7 evoluzione naturale da RDA/RDO";
        Page.EnableWings();
        Page.AddToolbarButton(IconCatalog.Refresh, "Aggiorna", true, async (_, _) => await LoadAsync(), Key.F5);
        Page.AddToolbarButton(IconCatalog.Print, "Stampa", false, (_, _) => PrintList(), Key.F6);
        Page.AttachRecordNavigator(_grid);
        Page.AttachPersonalization(_grid, "purchase-orders");
        _grid.ExportName = "OrdiniFornitore";

        Loaded += async (_, _) => await LoadAsync();
    }

    private void OnSelectionChanged(object? item)
    {
        if (item is not PoRow r) return;
        Page.SetWingDetail(r.Number, new[]
        {
            ("Numero", r.Number),
            ("Data", r.DateText),
            ("Fornitore", r.SupplierName),
            ("Righe", r.LineCount.ToString(CultureInfo.CurrentCulture)),
            ("Totale", r.TotalText),
            ("Stato", r.StatusLabel),
        });
    }

    private void PrintList()
    {
        var model = _grid.BuildReportModel("Ordini fornitore", "ODA", _api.CompanyName, "KBM ERP \u2014 Ordini");
        ReportPreviewWindow.Show(_api, "list.print", model, Window.GetWindow(this));
    }

    public Task ReloadAsync() => LoadAsync();

    private async Task LoadAsync()
    {
        Page.HideError();
        try
        {
            var rows = await _api.GetPurchaseOrdersAsync();
            if (rows is null) { Page.ShowError("Impossibile caricare gli ordini fornitore."); return; }
            _grid.SetItems(rows.Select(r => new PoRow(r)).ToList());
            Page.RecordCountLabel = $"{rows.Count} ordini";
            Page.UpdateRecordNavigator();
        }
        catch { Page.ShowError("Errore di connessione API."); }
    }

    private sealed class PoRow(PurchaseOrderListItem o)
    {
        public string Number { get; } = o.Number;
        public string DateText { get; } = o.OrderDate.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture);
        public string SupplierName { get; } = o.SupplierName;
        public int LineCount { get; } = o.LineCount;
        public string TotalText { get; } = o.TotalAmount.ToString("N2", CultureInfo.CurrentCulture);
        public string StatusLabel { get; } = o.Status switch
        {
            0 => "Bozza", 1 => "Confermato", 2 => "Parz. ricevuto", 3 => "Ricevuto", 4 => "Fatturato", 5 => "Annullato", _ => "-"
        };
    }
}
