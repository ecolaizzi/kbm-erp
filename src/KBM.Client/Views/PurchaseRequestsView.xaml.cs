using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class PurchaseRequestsView : UserControl
{
    private readonly ApiClient _api;
    private readonly FilterableGrid _grid;
    private List<PrListRow> _rows = new();

    public event Action? RequestNew;
    public event Action<long>? RequestEdit;
    public event Action<long>? RequestGenerateRfq;
    public event Action<long>? RequestGenerateOda;

    public PurchaseRequestsView(ApiClient api)
    {
        InitializeComponent();
        _api = api;

        _grid = new FilterableGrid();
        _grid.AddTextColumn("Numero", nameof(PrListRow.Number), width: 140);
        _grid.AddTextColumn("Data", nameof(PrListRow.DateText), width: 110);
        _grid.AddTextColumn("Reparto", nameof(PrListRow.RequestingUnit), star: true);
        _grid.AddTextColumn("Righe", nameof(PrListRow.LineCount), width: 80);
        _grid.AddTextColumn("Stato", nameof(PrListRow.Status), width: 130);
        _grid.SelectionChanged += OnSelectionChanged;
        _grid.RowActivated += _ => EditSelected();

        Page.GridContent = _grid;
        Page.PageKey = "purchase-requests";
        Page.PageTitle = "Richieste di acquisto";
        Page.Subtitle = "RDA \u00b7 ciclo passivo";
        Page.EnableWings();
        Page.AddToolbarButton(IconCatalog.New, "Nuova", true, (_, _) => RequestNew?.Invoke(), Key.F2);
        Page.AddToolbarButton(IconCatalog.Edit, "Modifica", false, (_, _) => EditSelected(), Key.F3);
        Page.AddToolbarSeparator();
        Page.AddToolbarButton(IconCatalog.Document, "Genera RDO", false, (_, _) => GenerateSelected());
        Page.AddToolbarButton(IconCatalog.Document, "Genera ODA", false, (_, _) => GenerateOdaSelected());
        Page.AddToolbarButton(IconCatalog.Delete, "Elimina", false, async (_, _) => await DeleteSelected(), Key.F4);
        Page.AddToolbarSeparator();
        Page.AddToolbarButton(IconCatalog.Refresh, "Aggiorna", false, async (_, _) => await LoadAsync(), Key.F5);
        Page.AttachRecordNavigator(_grid);
        Page.AttachPersonalization(_grid, "purchase-requests");
        _grid.ExportName = "RDA";

        Loaded += async (_, _) => await LoadAsync();
    }

    private void OnSelectionChanged(object? item)
    {
        if (item is not PrListRow r) return;
        Page.SetWingDetail(r.Number, new (string, string)[]
        {
            ("Numero", r.Number),
            ("Data", r.DateText),
            ("Reparto", r.RequestingUnit ?? "-"),
            ("Righe", r.LineCount.ToString()),
            ("Stato", r.Status)
        });
    }

    private void EditSelected()
    {
        if (_grid.SelectedItem is PrListRow r) RequestEdit?.Invoke(r.Id);
        else MessageBox.Show("Seleziona una RDA da modificare.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void GenerateSelected()
    {
        if (_grid.SelectedItem is PrListRow r) RequestGenerateRfq?.Invoke(r.Id);
        else MessageBox.Show("Seleziona una RDA.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void GenerateOdaSelected()
    {
        if (_grid.SelectedItem is PrListRow r) RequestGenerateOda?.Invoke(r.Id);
        else MessageBox.Show("Seleziona una RDA.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task DeleteSelected()
    {
        if (_grid.SelectedItem is not PrListRow r)
        {
            MessageBox.Show("Seleziona una RDA.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (MessageBox.Show($"Eliminare la RDA {r.Number}?", "KBM", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        if (await _api.DeletePurchaseRequestAsync(r.Id)) await LoadAsync();
        else Page.ShowError("Eliminazione non riuscita.");
    }

    public Task ReloadAsync() => LoadAsync();

    private async Task LoadAsync()
    {
        Page.HideError();
        try
        {
            var items = await _api.GetPurchaseRequestsAsync();
            if (items is null) { Page.ShowError("Impossibile caricare le RDA."); return; }
            _rows = items.Select(i => new PrListRow(i)).ToList();
            _grid.SetItems(_rows);
            Page.RecordCountLabel = $"{_rows.Count} totali";
            Page.PaginationLabel = $"Tot: {_rows.Count} record";
            Page.UpdateRecordNavigator();
        }
        catch { Page.ShowError("Errore di connessione API."); }
    }
}

public sealed class PrListRow
{
    public PrListRow(PurchaseRequestListItem i)
    {
        Id = i.Id;
        Number = i.Number;
        DateText = i.Date.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture);
        RequestingUnit = i.RequestingUnit;
        LineCount = i.LineCount;
        Status = i.Status;
    }

    public long Id { get; }
    public string Number { get; }
    public string DateText { get; }
    public string? RequestingUnit { get; }
    public int LineCount { get; }
    public string Status { get; }
}
