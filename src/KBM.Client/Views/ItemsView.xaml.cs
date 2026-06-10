using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class ItemsView : UserControl
{
    private readonly ApiClient _api;
    private readonly FilterableGrid _grid;
    private List<ItemRow> _rows = new();
    private bool _saving;

    public event Action? RequestNewItem;
    public event Action<long>? RequestEditItem;

    public ItemsView(ApiClient api)
    {
        InitializeComponent();
        _api = api;

        _grid = new FilterableGrid();
        _grid.AddTextColumn("Codice", nameof(ItemRow.Code), width: 110);
        _grid.AddTextColumn("Descrizione", nameof(ItemRow.Description), star: true, editable: true, realtime: true);
        _grid.AddTextColumn("Categoria", nameof(ItemRow.CategoryName), width: 160);
        _grid.AddTextColumn("UM", nameof(ItemRow.UnitOfMeasure), width: 60);
        _grid.AddTextColumn("Prezzo", nameof(ItemRow.BasePrice), width: 100, editable: true, realtime: true);
        _grid.AddTextColumn("IVA %", nameof(ItemRow.VatRate), width: 70);
        _grid.AddComboColumn("Stato", nameof(ItemRow.Status), new[] { "Active", "Inactive" }, width: 110);
        _grid.SelectionChanged += OnSelectionChanged;
        _grid.RowCommitted += OnRowCommitted;

        Page.GridContent = _grid;
        Page.PageKey = "items";
        Page.PageTitle = "Articoli";
        Page.Subtitle = "Anagrafica articoli \u00b7 doppio click sulla cella per modifica in linea";
        Page.EnableWings();
        Page.AddToolbarButton(IconCatalog.New, "Nuovo", true, (_, _) => RequestNewItem?.Invoke(), Key.F2);
        Page.AddToolbarButton(IconCatalog.Edit, "Modifica", false, (_, _) => EditSelected(), Key.F3);
        Page.AddToolbarSeparator();
        Page.AddToolbarButton(IconCatalog.Enable, "Abilita", false, async (_, _) => await SetEnabled(true));
        Page.AddToolbarButton(IconCatalog.Disable, "Disabilita", false, async (_, _) => await SetEnabled(false), Key.F4);
        Page.AddToolbarSeparator();
        Page.AddToolbarButton(IconCatalog.Print, "Stampa", false, (_, _) => PrintList(), Key.F6);
        Page.AddToolbarButton(IconCatalog.Refresh, "Aggiorna", false, async (_, _) => await LoadAsync(), Key.F5);
        Page.AttachRecordNavigator(_grid);
        Page.AttachPersonalization(_grid, "items");
        _grid.ExportName = "Articoli";
        _grid.RowActivated += _ => EditSelected();

        Loaded += async (_, _) => await LoadAsync();
    }

    private void OnSelectionChanged(object? item)
    {
        if (item is not ItemRow i) return;
        Page.SetWingDetail(
            i.Description,
            new (string, string)[]
            {
                ("Codice", i.Code),
                ("Descrizione", i.Description),
                ("Categoria", i.CategoryName ?? "-"),
                ("UM", i.UnitOfMeasure),
                ("Prezzo", i.BasePrice.ToString("N4", CultureInfo.CurrentCulture)),
                ("IVA %", i.VatRate.ToString("N2", CultureInfo.CurrentCulture)),
                ("Stato", i.Status == "Active" ? "Attivo" : "Disattivato")
            });
    }

    private void EditSelected()
    {
        if (_grid.SelectedItem is ItemRow i)
            RequestEditItem?.Invoke(i.Id);
        else
            MessageBox.Show("Seleziona un articolo da modificare.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void OnRowCommitted(object item)
    {
        if (_saving || item is not ItemRow r || !r.IsValid) return;
        _saving = true;
        try
        {
            var d = await _api.GetItemAsync(r.Id);
            if (d is null) { Page.ShowError("Articolo non trovato."); return; }

            var dto = new UpdateItemDto(
                r.Description.Trim(), d.CategoryId, d.UnitOfMeasure, d.Barcode, d.SupplierItemCode,
                r.BasePrice, d.VatRate, d.RevenueAccount, d.CostAccount, d.Notes, r.Status);

            var res = await _api.UpdateItemAsync(r.Id, dto);
            if (!res.Ok) Page.ShowError(res.Error ?? "Salvataggio non riuscito.");
            else Page.HideError();
        }
        catch { Page.ShowError("Errore di connessione API durante il salvataggio."); }
        finally { _saving = false; }
    }

    private async Task SetEnabled(bool enabled)
    {
        if (_grid.SelectedItem is not ItemRow i)
        {
            MessageBox.Show("Seleziona un articolo.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        try
        {
            if (await _api.SetItemEnabledAsync(i.Id, enabled)) await LoadAsync();
            else Page.ShowError("Operazione non riuscita.");
        }
        catch { Page.ShowError("Errore di connessione API."); }
    }

    private void PrintList()
    {
        var model = _grid.BuildReportModel(
            "Listino / Catalogo articoli",
            "Anagrafica articoli di magazzino",
            _api.CompanyName,
            "KBM ERP — Magazzino");
        ReportPreviewWindow.Show(_api, "items.catalog", model, Window.GetWindow(this));
    }

    public Task ReloadAsync() => LoadAsync();

    private async Task LoadAsync()
    {
        Page.HideError();
        try
        {
            var items = await _api.GetItemsAsync();
            if (items is null) { Page.ShowError("Impossibile caricare gli articoli."); return; }
            _rows = items.Select(i => new ItemRow(i)).ToList();
            _grid.SetItems(_rows);
            Page.RecordCountLabel = $"{_rows.Count} totali";
            Page.PaginationLabel = $"Tot: {_rows.Count} record";
            Page.UpdateRecordNavigator();
        }
        catch { Page.ShowError("Errore di connessione API."); }
    }
}

public sealed class ItemRow : INotifyPropertyChanged, IDataErrorInfo
{
    private string _description;
    private decimal _basePrice;
    private string _status;

    public ItemRow(ItemListItem i)
    {
        Id = i.Id;
        Code = i.Code;
        _description = i.Description;
        CategoryName = i.CategoryName;
        UnitOfMeasure = i.UnitOfMeasure;
        _basePrice = i.BasePrice;
        VatRate = i.VatRate;
        _status = i.Status;
    }

    public long Id { get; }
    public string Code { get; }
    public string? CategoryName { get; }
    public string UnitOfMeasure { get; }
    public decimal VatRate { get; }

    public string Description { get => _description; set { _description = value; OnChanged(nameof(Description)); } }
    public decimal BasePrice { get => _basePrice; set { _basePrice = value; OnChanged(nameof(BasePrice)); } }
    public string Status { get => _status; set { _status = value; OnChanged(nameof(Status)); } }

    public bool IsValid => string.IsNullOrEmpty(this[nameof(Description)]) && string.IsNullOrEmpty(this[nameof(BasePrice)]);

    public string Error => string.Empty;

    public string this[string columnName] => columnName switch
    {
        nameof(Description) when string.IsNullOrWhiteSpace(Description) => "Descrizione obbligatoria.",
        nameof(BasePrice) when BasePrice < 0 => "Il prezzo non pu\u00f2 essere negativo.",
        _ => string.Empty
    };

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnChanged(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
}
