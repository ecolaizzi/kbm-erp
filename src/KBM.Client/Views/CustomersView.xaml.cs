using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class CustomersView : UserControl
{
    private readonly ApiClient _api;
    private readonly FilterableGrid _grid;
    private List<CustomerRow> _rows = new();
    private bool _saving;

    public event Action? RequestNewCustomer;
    public event Action<long>? RequestEditCustomer;

    public CustomersView(ApiClient api)
    {
        InitializeComponent();
        _api = api;

        _grid = new FilterableGrid();
        _grid.AddTextColumn("Codice", nameof(CustomerRow.Code), width: 110);
        _grid.AddTextColumn("Ragione sociale", nameof(CustomerRow.BusinessName), star: true, editable: true, realtime: true);
        _grid.AddTextColumn("Partita IVA", nameof(CustomerRow.VatNumber), width: 150, editable: true, realtime: true);
        _grid.AddTextColumn("Citt\u00e0", nameof(CustomerRow.City), width: 170, editable: true);
        _grid.AddComboColumn("Stato", nameof(CustomerRow.Status), new[] { "Active", "Inactive" }, width: 110);
        _grid.SelectionChanged += OnSelectionChanged;
        _grid.RowCommitted += OnRowCommitted;

        Page.GridContent = _grid;
        Page.PageKey = "customers";
        Page.PageTitle = "Clienti";
        Page.Subtitle = "Anagrafica clienti \u00b7 doppio click sulla cella per modifica in linea";
        Page.EnableWings();
        Page.AddToolbarButton(IconCatalog.New, "Nuovo", true, (_, _) => RequestNewCustomer?.Invoke(), Key.F2);
        Page.AddToolbarButton(IconCatalog.Edit, "Modifica", false, (_, _) => EditSelected(), Key.F3);
        Page.AddToolbarSeparator();
        Page.AddToolbarButton(IconCatalog.Enable, "Abilita", false, async (_, _) => await SetEnabled(true));
        Page.AddToolbarButton(IconCatalog.Disable, "Disabilita", false, async (_, _) => await SetEnabled(false), Key.F4);
        Page.AddToolbarSeparator();
        Page.AddToolbarButton(IconCatalog.Print, "Stampa", false, (_, _) => PrintList(), Key.F6);
        Page.AddToolbarButton(IconCatalog.Refresh, "Aggiorna", false, async (_, _) => await LoadAsync(), Key.F5);
        Page.AttachRecordNavigator(_grid);
        Page.AttachPersonalization(_grid, "customers");
        _grid.ExportName = "Clienti";
        _grid.RowActivated += _ => EditSelected();

        Loaded += async (_, _) => await LoadAsync();
    }

    private void PrintList()
    {
        var model = _grid.BuildReportModel("Elenco clienti", "Anagrafica clienti",
            _api.CompanyName, "KBM ERP — Anagrafiche");
        ReportPreviewWindow.Show(_api, "customers.list", model, Window.GetWindow(this));
    }

    private void OnSelectionChanged(object? item)
    {
        if (item is not CustomerRow c) return;
        Page.SetWingDetail(
            c.BusinessName,
            new (string, string)[]
            {
                ("Codice", c.Code),
                ("Ragione sociale", c.BusinessName),
                ("Partita IVA", c.VatNumber ?? "-"),
                ("Citt\u00e0", c.City ?? "-"),
                ("Stato", c.Status == "Active" ? "Attivo" : "Disattivato")
            });
    }

    private void EditSelected()
    {
        if (_grid.SelectedItem is CustomerRow c)
            RequestEditCustomer?.Invoke(c.Id);
        else
            MessageBox.Show("Seleziona un cliente da modificare.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void OnRowCommitted(object item)
    {
        if (_saving || item is not CustomerRow r || !r.IsValid) return;
        _saving = true;
        try
        {
            var detail = await _api.GetCustomerAsync(r.Id);
            if (detail is null) { Page.ShowError("Cliente non trovato."); return; }

            var dto = new UpdateCustomerDto(
                r.BusinessName.Trim(),
                string.IsNullOrWhiteSpace(r.VatNumber) ? null : r.VatNumber.Trim(),
                detail.FiscalCode, detail.SdiCode, detail.PecEmail, detail.Email, detail.Phone,
                detail.Address, string.IsNullOrWhiteSpace(r.City) ? null : r.City.Trim(),
                detail.Province, detail.PostalCode, detail.Country,
                detail.Iban, detail.PaymentTerms, detail.Notes, r.Status);

            var res = await _api.UpdateCustomerAsync(r.Id, dto);
            if (!res.Ok) Page.ShowError(res.Error ?? "Salvataggio non riuscito.");
            else Page.HideError();
        }
        catch { Page.ShowError("Errore di connessione API durante il salvataggio."); }
        finally { _saving = false; }
    }

    private async Task SetEnabled(bool enabled)
    {
        if (_grid.SelectedItem is not CustomerRow c)
        {
            MessageBox.Show("Seleziona un cliente.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        try
        {
            if (await _api.SetCustomerEnabledAsync(c.Id, enabled)) await LoadAsync();
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
            var items = await _api.GetCustomersAsync();
            if (items is null) { Page.ShowError("Impossibile caricare i clienti."); return; }
            _rows = items.Select(i => new CustomerRow(i)).ToList();
            _grid.SetItems(_rows);
            Page.RecordCountLabel = $"{_rows.Count} totali";
            Page.PaginationLabel = $"Tot: {_rows.Count} record";
            Page.UpdateRecordNavigator();
        }
        catch { Page.ShowError("Errore di connessione API."); }
    }
}

/// <summary>Riga editabile in-line con validazione in tempo reale (IDataErrorInfo).</summary>
public sealed class CustomerRow : INotifyPropertyChanged, IDataErrorInfo
{
    private string _businessName;
    private string? _vatNumber;
    private string? _city;
    private string _status;

    public CustomerRow(CustomerListItem i)
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

    public string BusinessName
    {
        get => _businessName;
        set { _businessName = value; OnChanged(nameof(BusinessName)); }
    }

    public string? VatNumber
    {
        get => _vatNumber;
        set { _vatNumber = value; OnChanged(nameof(VatNumber)); }
    }

    public string? City
    {
        get => _city;
        set { _city = value; OnChanged(nameof(City)); }
    }

    public string Status
    {
        get => _status;
        set { _status = value; OnChanged(nameof(Status)); }
    }

    public bool IsValid =>
        string.IsNullOrEmpty(this[nameof(BusinessName)]) &&
        string.IsNullOrEmpty(this[nameof(VatNumber)]);

    public string Error => string.Empty;

    public string this[string columnName] => columnName switch
    {
        nameof(BusinessName) when string.IsNullOrWhiteSpace(BusinessName)
            => "Ragione sociale obbligatoria.",
        nameof(VatNumber) when !ValidVat(VatNumber)
            => "Partita IVA: 11 cifre con check digit valido.",
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
