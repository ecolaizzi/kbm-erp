using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class SuppliersView : UserControl
{
    private readonly ApiClient _api;
    private readonly FilterableGrid _grid;
    private List<SupplierRow> _rows = new();
    private bool _saving;

    public event Action? RequestNewSupplier;
    public event Action<long>? RequestEditSupplier;

    public SuppliersView(ApiClient api)
    {
        InitializeComponent();
        _api = api;

        _grid = new FilterableGrid();
        _grid.AddTextColumn("Codice", nameof(SupplierRow.Code), width: 110);
        _grid.AddTextColumn("Ragione sociale", nameof(SupplierRow.BusinessName), star: true, editable: true, realtime: true);
        _grid.AddTextColumn("Partita IVA", nameof(SupplierRow.VatNumber), width: 150, editable: true, realtime: true);
        _grid.AddTextColumn("Citt\u00e0", nameof(SupplierRow.City), width: 170, editable: true);
        _grid.AddComboColumn("Stato", nameof(SupplierRow.Status), new[] { "Active", "Inactive" }, width: 110);
        _grid.SelectionChanged += OnSelectionChanged;
        _grid.RowCommitted += OnRowCommitted;

        Page.GridContent = _grid;
        Page.PageKey = "suppliers";
        Page.PageTitle = "Fornitori";
        Page.Subtitle = "Anagrafica fornitori \u00b7 doppio click sulla cella per modifica in linea";
        Page.EnableWings();
        Page.AddToolbarButton(IconCatalog.New, "Nuovo", true, (_, _) => RequestNewSupplier?.Invoke(), Key.F2);
        Page.AddToolbarButton(IconCatalog.Edit, "Modifica", false, (_, _) => EditSelected(), Key.F3);
        Page.AddToolbarSeparator();
        Page.AddToolbarButton(IconCatalog.Enable, "Abilita", false, async (_, _) => await SetEnabled(true));
        Page.AddToolbarButton(IconCatalog.Disable, "Disabilita", false, async (_, _) => await SetEnabled(false), Key.F4);
        Page.AddToolbarSeparator();
        Page.AddToolbarButton(IconCatalog.Print, "Stampa", false, (_, _) => PrintList(), Key.F6);
        Page.AddToolbarButton(IconCatalog.Refresh, "Aggiorna", false, async (_, _) => await LoadAsync(), Key.F5);
        Page.AttachRecordNavigator(_grid);
        Page.AttachPersonalization(_grid, "suppliers");
        _grid.ExportName = "Fornitori";
        _grid.RowActivated += _ => EditSelected();

        Loaded += async (_, _) => await LoadAsync();
    }

    private void PrintList()
    {
        var model = _grid.BuildReportModel("Elenco fornitori", "Anagrafica fornitori",
            _api.CompanyName, "KBM ERP — Anagrafiche");
        ReportPreviewWindow.Show(_api, "suppliers.list", model, Window.GetWindow(this));
    }

    private void OnSelectionChanged(object? item)
    {
        if (item is not SupplierRow s) return;
        Page.SetWingDetail(
            s.BusinessName,
            new (string, string)[]
            {
                ("Codice", s.Code),
                ("Ragione sociale", s.BusinessName),
                ("Partita IVA", s.VatNumber ?? "-"),
                ("Citt\u00e0", s.City ?? "-"),
                ("Stato", s.Status == "Active" ? "Attivo" : "Disattivato")
            });
    }

    private void EditSelected()
    {
        if (_grid.SelectedItem is SupplierRow s)
            RequestEditSupplier?.Invoke(s.Id);
        else
            MessageBox.Show("Seleziona un fornitore da modificare.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void OnRowCommitted(object item)
    {
        if (_saving || item is not SupplierRow r || !r.IsValid) return;
        _saving = true;
        try
        {
            var agg = await _api.GetSupplierFullAsync(r.Id);
            if (agg is null) { Page.ShowError("Fornitore non trovato."); return; }
            var d = agg.Detail;

            var dto = new UpdateSupplierDto(
                r.BusinessName.Trim(),
                string.IsNullOrWhiteSpace(r.VatNumber) ? null : r.VatNumber.Trim(),
                d.FiscalCode, d.SdiCode, d.PecEmail, d.Email, d.Phone,
                d.Address, string.IsNullOrWhiteSpace(r.City) ? null : r.City.Trim(),
                d.Province, d.PostalCode, d.Country,
                d.Iban, d.PaymentTerms, d.Notes, r.Status);

            var res = await _api.UpdateSupplierAsync(r.Id, dto);
            if (!res.Ok) Page.ShowError(res.Error ?? "Salvataggio non riuscito.");
            else Page.HideError();
        }
        catch { Page.ShowError("Errore di connessione API durante il salvataggio."); }
        finally { _saving = false; }
    }

    private async Task SetEnabled(bool enabled)
    {
        if (_grid.SelectedItem is not SupplierRow s)
        {
            MessageBox.Show("Seleziona un fornitore.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        try
        {
            if (await _api.SetSupplierEnabledAsync(s.Id, enabled)) await LoadAsync();
            else Page.ShowError("Operazione non riuscita.");
        }
        catch { Page.ShowError("Errore di connessione API."); }
    }

    public Task ReloadAsync() => LoadAsync();

    private async Task LoadAsync()
    {
        Page.HideError();
        try
        {
            var items = await _api.GetSuppliersAsync();
            if (items is null) { Page.ShowError("Impossibile caricare i fornitori."); return; }
            _rows = items.Select(i => new SupplierRow(i)).ToList();
            _grid.SetItems(_rows);
            Page.RecordCountLabel = $"{_rows.Count} totali";
            Page.PaginationLabel = $"Tot: {_rows.Count} record";
            Page.UpdateRecordNavigator();
        }
        catch { Page.ShowError("Errore di connessione API."); }
    }
}

public sealed class SupplierRow : INotifyPropertyChanged, IDataErrorInfo
{
    private string _businessName;
    private string? _vatNumber;
    private string? _city;
    private string _status;

    public SupplierRow(SupplierListItem i)
    {
        Id = i.Id;
        Code = i.Code;
        _businessName = i.BusinessName;
        _vatNumber = i.VatNumber;
        _city = i.City;
        _status = i.Status;
    }

    public long Id { get; }
    public string Code { get; }

    public string BusinessName { get => _businessName; set { _businessName = value; OnChanged(nameof(BusinessName)); } }
    public string? VatNumber { get => _vatNumber; set { _vatNumber = value; OnChanged(nameof(VatNumber)); } }
    public string? City { get => _city; set { _city = value; OnChanged(nameof(City)); } }
    public string Status { get => _status; set { _status = value; OnChanged(nameof(Status)); } }

    public bool IsValid =>
        string.IsNullOrEmpty(this[nameof(BusinessName)]) &&
        string.IsNullOrEmpty(this[nameof(VatNumber)]);

    public string Error => string.Empty;

    public string this[string columnName] => columnName switch
    {
        nameof(BusinessName) when string.IsNullOrWhiteSpace(BusinessName) => "Ragione sociale obbligatoria.",
        nameof(VatNumber) when !ValidVat(VatNumber) => "Partita IVA: 11 cifre con check digit valido.",
        _ => string.Empty
    };

    private static bool ValidVat(string? vat)
    {
        if (string.IsNullOrWhiteSpace(vat)) return true;
        vat = vat.Trim();
        if (vat.Length != 11 || !vat.All(char.IsDigit)) return false;
        var sum = 0;
        for (var i = 0; i < 11; i++)
        {
            var d = vat[i] - '0';
            if (i % 2 == 1) { d *= 2; if (d > 9) d -= 9; }
            sum += d;
        }
        return sum % 10 == 0;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnChanged(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
}
