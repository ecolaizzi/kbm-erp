using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class ChartAccountsView : UserControl
{
    private readonly ApiClient _api;
    private readonly FilterableGrid _grid;
    private List<ChartAccountRow> _rows = new();
    private bool _saving;

    public event Action? RequestNew;
    public event Action<long>? RequestEdit;

    public ChartAccountsView(ApiClient api)
    {
        InitializeComponent();
        _api = api;

        _grid = new FilterableGrid();
        _grid.AddTextColumn("Codice", nameof(ChartAccountRow.FullCode), width: 150);
        _grid.AddTextColumn("Descrizione", nameof(ChartAccountRow.Name), star: true, editable: true, realtime: true);
        _grid.AddTextColumn("Livello", nameof(ChartAccountRow.LevelLabel), width: 100);
        _grid.AddTextColumn("Natura", nameof(ChartAccountRow.NatureLabel), width: 120);
        _grid.AddTextColumn("Segno", nameof(ChartAccountRow.SignLabel), width: 70);
        _grid.AddTextColumn("Tipo", nameof(ChartAccountRow.SubKindLabel), width: 110);
        _grid.AddTextColumn("Reg.", nameof(ChartAccountRow.PostingLabel), width: 60);
        _grid.AddComboColumn("Stato", nameof(ChartAccountRow.Status), new[] { "Active", "Inactive" }, width: 100);
        _grid.SelectionChanged += OnSelectionChanged;
        _grid.RowCommitted += OnRowCommitted;

        Page.GridContent = _grid;
        Page.PageKey = "chart-accounts";
        Page.PageTitle = "Piano dei conti (mastri)";
        Page.Subtitle = "Mastro \u2192 Conto \u2192 Sottoconto \u00b7 le registrazioni avvengono sui sottoconti";
        Page.EnableWings();
        Page.AddToolbarButton(IconCatalog.New, "Nuovo", true, (_, _) => RequestNew?.Invoke(), Key.F2);
        Page.AddToolbarButton(IconCatalog.Edit, "Modifica", false, (_, _) => EditSelected(), Key.F3);
        Page.AddToolbarSeparator();
        Page.AddToolbarButton(IconCatalog.Enable, "Abilita", false, async (_, _) => await SetEnabled(true));
        Page.AddToolbarButton(IconCatalog.Disable, "Disabilita", false, async (_, _) => await SetEnabled(false), Key.F4);
        Page.AddToolbarSeparator();
        Page.AddToolbarButton(IconCatalog.Import, "Piano standard", false, async (_, _) => await SeedStandard());
        Page.AddToolbarButton(IconCatalog.Print, "Stampa", false, (_, _) => PrintList(), Key.F6);
        Page.AddToolbarButton(IconCatalog.Refresh, "Aggiorna", false, async (_, _) => await LoadAsync(), Key.F5);
        Page.AttachRecordNavigator(_grid);
        Page.AttachPersonalization(_grid, "chart-accounts");
        _grid.ExportName = "PianoDeiConti";
        _grid.RowActivated += _ => EditSelected();

        Loaded += async (_, _) => await LoadAsync();
    }

    private void OnSelectionChanged(object? item)
    {
        if (item is not ChartAccountRow a) return;
        Page.SetWingDetail(a.Name, new[]
        {
            ("Codice", a.FullCode),
            ("Livello", a.LevelLabel),
            ("Natura", a.NatureLabel),
            ("Segno", a.SignLabel),
            ("Tipo conto", a.SubKindLabel),
            ("Registrabile", a.AllowsPosting ? "S\u00ec (sottoconto)" : "No (raggruppamento)"),
            ("Stato", a.Status == "Active" ? "Attivo" : "Disattivato"),
        });
    }

    private void EditSelected()
    {
        if (_grid.SelectedItem is ChartAccountRow a)
            RequestEdit?.Invoke(a.Id);
        else
            MessageBox.Show("Seleziona un conto da modificare.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void OnRowCommitted(object item)
    {
        if (_saving || item is not ChartAccountRow r || !r.IsValid) return;
        _saving = true;
        try
        {
            var d = await _api.GetChartAccountAsync(r.Id);
            if (d is null) { Page.ShowError("Conto non trovato."); return; }

            var dto = new UpdateChartAccountDto(r.Name.Trim(), d.Sign, d.SubKind, d.BilCeeDare, d.BilCeeAvere, r.Status);
            var res = await _api.UpdateChartAccountAsync(r.Id, dto);
            if (!res.Ok) Page.ShowError(res.Error ?? "Salvataggio non riuscito.");
            else { Page.HideError(); ToastService.Success("Conto aggiornato."); }
        }
        catch { Page.ShowError("Errore di connessione API durante il salvataggio."); }
        finally { _saving = false; }
    }

    private async Task SetEnabled(bool enabled)
    {
        if (_grid.SelectedItem is not ChartAccountRow a)
        {
            MessageBox.Show("Seleziona un conto.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        try
        {
            if (await _api.SetChartAccountEnabledAsync(a.Id, enabled)) await LoadAsync();
            else Page.ShowError("Operazione non riuscita.");
        }
        catch { Page.ShowError("Errore di connessione API."); }
    }

    private async Task SeedStandard()
    {
        if (_rows.Count > 0)
        {
            MessageBox.Show("Il piano dei conti contiene gi\u00e0 delle voci: il caricamento standard \u00e8 disponibile solo su un piano vuoto.",
                "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (MessageBox.Show("Caricare un piano dei conti standard italiano (mastri, conti e sottoconti base)?",
                "KBM", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        try
        {
            var created = await _api.SeedStandardChartAsync();
            if (created < 0) { Page.ShowError("Caricamento non riuscito."); return; }
            ToastService.Success($"Piano standard caricato: {created} voci.");
            await LoadAsync();
        }
        catch { Page.ShowError("Errore di connessione API."); }
    }

    private void PrintList()
    {
        var model = _grid.BuildReportModel(
            "Piano dei conti",
            "Sistema dei mastri \u00b7 mastro / conto / sottoconto",
            _api.CompanyName,
            "KBM ERP \u2014 Contabilit\u00e0");
        ReportPreviewWindow.Show(_api, "list.print", model, Window.GetWindow(this));
    }

    public Task ReloadAsync() => LoadAsync();

    private async Task LoadAsync()
    {
        Page.HideError();
        try
        {
            var accounts = await _api.GetChartAccountsAsync();
            if (accounts is null) { Page.ShowError("Impossibile caricare il piano dei conti."); return; }
            _rows = accounts.Select(a => new ChartAccountRow(a)).ToList();
            _grid.SetItems(_rows);
            Page.RecordCountLabel = $"{_rows.Count} conti";
            Page.PaginationLabel = $"Tot: {_rows.Count} record";
            Page.UpdateRecordNavigator();
        }
        catch { Page.ShowError("Errore di connessione API."); }
    }
}

public sealed class ChartAccountRow : INotifyPropertyChanged, IDataErrorInfo
{
    private string _name;
    private string _status;

    public ChartAccountRow(ChartAccountListItem a)
    {
        Id = a.Id;
        FullCode = a.FullCode;
        Level = a.Level;
        Nature = a.Nature;
        Sign = a.Sign;
        SubKind = a.SubKind;
        AllowsPosting = a.AllowsPosting;
        _name = a.Name;
        _status = a.Status;
    }

    public long Id { get; }
    public string FullCode { get; }
    public int Level { get; }
    public int Nature { get; }
    public int Sign { get; }
    public int SubKind { get; }
    public bool AllowsPosting { get; }

    public string Name { get => _name; set { _name = value; OnChanged(nameof(Name)); } }
    public string Status { get => _status; set { _status = value; OnChanged(nameof(Status)); } }

    public string LevelLabel => ChartAccountLabels.Level(Level);
    public string NatureLabel => ChartAccountLabels.Nature(Nature);
    public string SignLabel => ChartAccountLabels.Sign(Sign);
    public string SubKindLabel => ChartAccountLabels.SubKind(SubKind);
    public string PostingLabel => AllowsPosting ? "S\u00ec" : "-";

    public bool IsValid => string.IsNullOrEmpty(this[nameof(Name)]);
    public string Error => string.Empty;
    public string this[string columnName] => columnName switch
    {
        nameof(Name) when string.IsNullOrWhiteSpace(Name) => "Descrizione obbligatoria.",
        _ => string.Empty
    };

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnChanged(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
}

public static class ChartAccountLabels
{
    public static string Level(int v) => v switch { 1 => "Mastro", 2 => "Conto", 3 => "Sottoconto", _ => "-" };
    public static string Nature(int v) => v switch { 1 => "Patrimoniale", 2 => "Economico", 3 => "Ordine", _ => "-" };
    public static string Sign(int v) => v switch { 1 => "Dare", 2 => "Avere", _ => "-" };
    public static string SubKind(int v) => v switch
    {
        1 => "Cliente",
        2 => "Fornitore",
        3 => "Banca",
        4 => "Cassa",
        5 => "IVA",
        6 => "Ritenuta",
        _ => "Standard"
    };
}
