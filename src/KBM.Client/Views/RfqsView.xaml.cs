using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class RfqsView : UserControl
{
    private readonly ApiClient _api;
    private readonly FilterableGrid _grid;
    private List<RfqListRow> _rows = new();

    public event Action? RequestNew;
    public event Action<long>? RequestEdit;

    public RfqsView(ApiClient api)
    {
        InitializeComponent();
        _api = api;

        _grid = new FilterableGrid();
        _grid.AddTextColumn("Numero", nameof(RfqListRow.Number), width: 140);
        _grid.AddTextColumn("Data", nameof(RfqListRow.DateText), width: 110);
        _grid.AddTextColumn("Fornitore", nameof(RfqListRow.SupplierName), star: true);
        _grid.AddTextColumn("Righe", nameof(RfqListRow.LineCount), width: 80);
        _grid.AddTextColumn("Stato", nameof(RfqListRow.Status), width: 130);
        _grid.SelectionChanged += OnSelectionChanged;
        _grid.RowActivated += _ => EditSelected();

        Page.GridContent = _grid;
        Page.PageKey = "rfqs";
        Page.PageTitle = "Richieste di offerta";
        Page.Subtitle = "RDO \u00b7 quotazioni fornitori";
        Page.EnableWings();
        Page.AddToolbarButton(IconCatalog.New, "Nuova", true, (_, _) => RequestNew?.Invoke(), Key.F2);
        Page.AddToolbarButton(IconCatalog.Edit, "Modifica", false, (_, _) => EditSelected(), Key.F3);
        Page.AddToolbarSeparator();
        Page.AddToolbarButton(IconCatalog.Delete, "Elimina", false, async (_, _) => await DeleteSelected(), Key.F4);
        Page.AddToolbarSeparator();
        Page.AddToolbarButton(IconCatalog.Refresh, "Aggiorna", false, async (_, _) => await LoadAsync(), Key.F5);
        Page.AttachRecordNavigator(_grid);
        Page.AttachPersonalization(_grid, "rfqs");
        _grid.ExportName = "RDO";

        Loaded += async (_, _) => await LoadAsync();
    }

    private void OnSelectionChanged(object? item)
    {
        if (item is not RfqListRow r) return;
        Page.SetWingDetail(r.Number, new (string, string)[]
        {
            ("Numero", r.Number),
            ("Data", r.DateText),
            ("Fornitore", r.SupplierName),
            ("Righe", r.LineCount.ToString()),
            ("Stato", r.Status)
        });
    }

    private void EditSelected()
    {
        if (_grid.SelectedItem is RfqListRow r) RequestEdit?.Invoke(r.Id);
        else MessageBox.Show("Seleziona una RDO da modificare.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task DeleteSelected()
    {
        if (_grid.SelectedItem is not RfqListRow r)
        {
            MessageBox.Show("Seleziona una RDO.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (MessageBox.Show($"Eliminare la RDO {r.Number}?", "KBM", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        if (await _api.DeleteRfqAsync(r.Id)) await LoadAsync();
        else Page.ShowError("Eliminazione non riuscita.");
    }

    public Task ReloadAsync() => LoadAsync();

    private async Task LoadAsync()
    {
        Page.HideError();
        try
        {
            var items = await _api.GetRfqsAsync();
            if (items is null) { Page.ShowError("Impossibile caricare le RDO."); return; }
            _rows = items.Select(i => new RfqListRow(i)).ToList();
            _grid.SetItems(_rows);
            Page.RecordCountLabel = $"{_rows.Count} totali";
            Page.PaginationLabel = $"Tot: {_rows.Count} record";
            Page.UpdateRecordNavigator();
        }
        catch { Page.ShowError("Errore di connessione API."); }
    }
}

public sealed class RfqListRow
{
    public RfqListRow(RfqListItem i)
    {
        Id = i.Id;
        Number = i.Number;
        DateText = i.Date.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture);
        SupplierName = i.SupplierName;
        LineCount = i.LineCount;
        Status = i.Status;
    }

    public long Id { get; }
    public string Number { get; }
    public string DateText { get; }
    public string SupplierName { get; }
    public int LineCount { get; }
    public string Status { get; }
}
