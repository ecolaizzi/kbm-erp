using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class PaymentTermsView : UserControl
{
    private readonly ApiClient _api;
    private readonly FilterableGrid _grid;
    private List<PaymentTermRow> _rows = new();
    private bool _saving;

    public event Action? RequestNew;
    public event Action<long>? RequestEdit;

    public PaymentTermsView(ApiClient api)
    {
        InitializeComponent();
        _api = api;

        _grid = new FilterableGrid();
        _grid.AddTextColumn("Codice", nameof(PaymentTermRow.Code), width: 110);
        _grid.AddTextColumn("Descrizione", nameof(PaymentTermRow.Description), star: true, editable: true, realtime: true);
        _grid.AddTextColumn("Rate", nameof(PaymentTermRow.InstallmentsCount), width: 70, editable: true, realtime: true);
        _grid.AddTextColumn("1\u00aa scad. (gg)", nameof(PaymentTermRow.FirstDueDays), width: 100, editable: true, realtime: true);
        _grid.AddTextColumn("Intervallo (gg)", nameof(PaymentTermRow.IntervalDays), width: 110, editable: true, realtime: true);
        _grid.AddTextColumn("Fine mese", nameof(PaymentTermRow.EndOfMonthLabel), width: 90);
        _grid.AddTextColumn("Pagamento", nameof(PaymentTermRow.PaymentMethod), width: 130);
        _grid.AddComboColumn("Stato", nameof(PaymentTermRow.Status), new[] { "Active", "Inactive" }, width: 110);
        _grid.SelectionChanged += OnSelectionChanged;
        _grid.RowCommitted += OnRowCommitted;

        Page.GridContent = _grid;
        Page.PageKey = "payment-terms";
        Page.PageTitle = "Condizioni di pagamento";
        Page.Subtitle = "Tabella base \u00b7 doppio click sulla cella per modifica in linea";
        Page.EnableWings();
        Page.AddToolbarButton(IconCatalog.New, "Nuovo", true, (_, _) => RequestNew?.Invoke(), Key.F2);
        Page.AddToolbarButton(IconCatalog.Edit, "Modifica", false, (_, _) => EditSelected(), Key.F3);
        Page.AddToolbarSeparator();
        Page.AddToolbarButton(IconCatalog.Enable, "Abilita", false, async (_, _) => await SetEnabled(true));
        Page.AddToolbarButton(IconCatalog.Disable, "Disabilita", false, async (_, _) => await SetEnabled(false), Key.F4);
        Page.AddToolbarSeparator();
        Page.AddToolbarButton(IconCatalog.Print, "Stampa", false, (_, _) => PrintList(), Key.F6);
        Page.AddToolbarButton(IconCatalog.Refresh, "Aggiorna", false, async (_, _) => await LoadAsync(), Key.F5);
        Page.AttachRecordNavigator(_grid);
        Page.AttachPersonalization(_grid, "payment-terms");
        _grid.ExportName = "CondizioniPagamento";
        _grid.RowActivated += _ => EditSelected();

        Loaded += async (_, _) => await LoadAsync();
    }

    private void OnSelectionChanged(object? item)
    {
        if (item is not PaymentTermRow p) return;

        // Anteprima scadenziere su importo campione (1.000,00) — logica gestionale riutilizzabile.
        var schedule = BuildSchedulePreview(p, 1000m);
        var details = new List<(string, string)>
        {
            ("Codice", p.Code),
            ("Descrizione", p.Description),
            ("N. rate", p.InstallmentsCount.ToString(CultureInfo.CurrentCulture)),
            ("1\u00aa scadenza", $"{p.FirstDueDays} gg"),
            ("Intervallo", $"{p.IntervalDays} gg"),
            ("Fine mese", p.EndOfMonth ? "S\u00ec" : "No"),
            ("Pagamento", p.PaymentMethod ?? "-"),
            ("Stato", p.Status == "Active" ? "Attivo" : "Disattivato"),
        };
        details.AddRange(schedule);
        Page.SetWingDetail(p.Description, details.ToArray());
    }

    private static IEnumerable<(string, string)> BuildSchedulePreview(PaymentTermRow p, decimal amount)
    {
        var count = p.InstallmentsCount < 1 ? 1 : p.InstallmentsCount;
        var per = Math.Round(amount / count, 2, MidpointRounding.AwayFromZero);
        var today = DateTime.Today;
        decimal allocated = 0m;
        var list = new List<(string, string)>();
        for (var i = 0; i < count && i < 6; i++)
        {
            var due = today.AddDays(p.FirstDueDays + (p.IntervalDays * i));
            if (p.EndOfMonth) due = new DateTime(due.Year, due.Month, DateTime.DaysInMonth(due.Year, due.Month));
            var value = i == count - 1 ? amount - allocated : per;
            allocated += value;
            list.Add(($"Rata {i + 1} (su 1.000)", $"{due:dd/MM/yyyy} \u00b7 {value:N2}"));
        }
        return list;
    }

    private void EditSelected()
    {
        if (_grid.SelectedItem is PaymentTermRow p)
            RequestEdit?.Invoke(p.Id);
        else
            MessageBox.Show("Seleziona una condizione di pagamento da modificare.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void OnRowCommitted(object item)
    {
        if (_saving || item is not PaymentTermRow r || !r.IsValid) return;
        _saving = true;
        try
        {
            var d = await _api.GetPaymentTermAsync(r.Id);
            if (d is null) { Page.ShowError("Condizione di pagamento non trovata."); return; }

            var dto = new UpdatePaymentTermDto(
                r.Description.Trim(), r.InstallmentsCount, r.FirstDueDays, r.IntervalDays,
                d.EndOfMonth, d.PaymentMethod, d.Notes, r.Status);

            var res = await _api.UpdatePaymentTermAsync(r.Id, dto);
            if (!res.Ok) Page.ShowError(res.Error ?? "Salvataggio non riuscito.");
            else { Page.HideError(); ToastService.Success("Condizione di pagamento aggiornata."); }
        }
        catch { Page.ShowError("Errore di connessione API durante il salvataggio."); }
        finally { _saving = false; }
    }

    private async Task SetEnabled(bool enabled)
    {
        if (_grid.SelectedItem is not PaymentTermRow p)
        {
            MessageBox.Show("Seleziona una condizione di pagamento.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        try
        {
            if (await _api.SetPaymentTermEnabledAsync(p.Id, enabled)) await LoadAsync();
            else Page.ShowError("Operazione non riuscita.");
        }
        catch { Page.ShowError("Errore di connessione API."); }
    }

    private void PrintList()
    {
        var model = _grid.BuildReportModel(
            "Condizioni di pagamento",
            "Tabella base \u00b7 condizioni di pagamento",
            _api.CompanyName,
            "KBM ERP \u2014 Tabelle e archivi");
        ReportPreviewWindow.Show(_api, "list.print", model, Window.GetWindow(this));
    }

    public Task ReloadAsync() => LoadAsync();

    private async Task LoadAsync()
    {
        Page.HideError();
        try
        {
            var terms = await _api.GetPaymentTermsAsync();
            if (terms is null) { Page.ShowError("Impossibile caricare le condizioni di pagamento."); return; }
            _rows = terms.Select(t => new PaymentTermRow(t)).ToList();
            _grid.SetItems(_rows);
            Page.RecordCountLabel = $"{_rows.Count} totali";
            Page.PaginationLabel = $"Tot: {_rows.Count} record";
            Page.UpdateRecordNavigator();
        }
        catch { Page.ShowError("Errore di connessione API."); }
    }
}

public sealed class PaymentTermRow : INotifyPropertyChanged, IDataErrorInfo
{
    private string _description;
    private int _installmentsCount;
    private int _firstDueDays;
    private int _intervalDays;
    private string _status;

    public PaymentTermRow(PaymentTermListItem t)
    {
        Id = t.Id;
        Code = t.Code;
        _description = t.Description;
        _installmentsCount = t.InstallmentsCount;
        _firstDueDays = t.FirstDueDays;
        _intervalDays = t.IntervalDays;
        EndOfMonth = t.EndOfMonth;
        PaymentMethod = t.PaymentMethod;
        _status = t.Status;
    }

    public long Id { get; }
    public string Code { get; }
    public bool EndOfMonth { get; }
    public string EndOfMonthLabel => EndOfMonth ? "S\u00ec" : "No";
    public string? PaymentMethod { get; }

    public string Description { get => _description; set { _description = value; OnChanged(nameof(Description)); } }
    public int InstallmentsCount { get => _installmentsCount; set { _installmentsCount = value; OnChanged(nameof(InstallmentsCount)); } }
    public int FirstDueDays { get => _firstDueDays; set { _firstDueDays = value; OnChanged(nameof(FirstDueDays)); } }
    public int IntervalDays { get => _intervalDays; set { _intervalDays = value; OnChanged(nameof(IntervalDays)); } }
    public string Status { get => _status; set { _status = value; OnChanged(nameof(Status)); } }

    public bool IsValid =>
        string.IsNullOrEmpty(this[nameof(Description)]) &&
        string.IsNullOrEmpty(this[nameof(InstallmentsCount)]) &&
        string.IsNullOrEmpty(this[nameof(FirstDueDays)]) &&
        string.IsNullOrEmpty(this[nameof(IntervalDays)]);

    public string Error => string.Empty;

    public string this[string columnName] => columnName switch
    {
        nameof(Description) when string.IsNullOrWhiteSpace(Description) => "Descrizione obbligatoria.",
        nameof(InstallmentsCount) when InstallmentsCount < 1 => "Almeno 1 rata.",
        nameof(FirstDueDays) when FirstDueDays < 0 => "Giorni non validi.",
        nameof(IntervalDays) when IntervalDays < 0 => "Giorni non validi.",
        _ => string.Empty
    };

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnChanged(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
}
