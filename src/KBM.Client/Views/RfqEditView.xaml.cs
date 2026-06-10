using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class RfqEditView : UserControl
{
    private readonly ApiClient _api;
    private readonly string _companyName;
    private long? _id;
    private string? _number;
    private string _status = "Generato";
    private bool IsEdit => _id.HasValue;

    private readonly ObservableCollection<RfqLineVm> _lines = new();
    private readonly FilterableGrid _grid = new();

    private static readonly string[] DocStatuses = { "Generato", "Inviata", "OffertaRicevuta", "Confermato", "Annullato" };
    // Stati che bloccano la modifica del documento.
    private static readonly HashSet<string> LockedStatuses = new() { "Confermato", "Annullato" };

    public event Action? Saved;
    public event Action? Cancelled;

    public RfqEditView(ApiClient api, long? id, string companyName)
    {
        InitializeComponent();
        _api = api;
        _id = id;
        _companyName = companyName;

        BuildGrid();
        DateBox.SelectedDate = DateTime.Today;
        foreach (var s in DocStatuses) StatusBox.Items.Add(s);
        StatusBox.SelectedIndex = 0;

        _lines.CollectionChanged += OnLinesChanged;

        if (!IsEdit) PrintBtn.IsEnabled = false;
        else FormTitle.Text = "Modifica richiesta di offerta";

        FormShortcuts.Apply(this,
            save: () => { if (SaveBtn.IsEnabled) Save_Click(this, new RoutedEventArgs()); },
            print: () => { if (PrintBtn.IsEnabled) Print_Click(this, new RoutedEventArgs()); },
            exit: () => Cancel_Click(this, new RoutedEventArgs()));

        Loaded += async (_, _) => await InitAsync();
    }

    private void BuildGrid()
    {
        _grid.AddTextColumn("Articolo", nameof(RfqLineVm.ItemCode), width: 110);
        _grid.AddTextColumn("Descrizione", nameof(RfqLineVm.Description), star: true, editable: true, realtime: true);
        _grid.AddTextColumn("Q.t\u00e0", nameof(RfqLineVm.Quantity), width: 80, editable: true, realtime: true);
        _grid.AddTextColumn("UM", nameof(RfqLineVm.UnitOfMeasure), width: 60, editable: true);
        _grid.AddTextColumn("Prezzo unit.", nameof(RfqLineVm.UnitPrice), width: 100, editable: true, realtime: true);
        _grid.AddTextColumn("Sconto %", nameof(RfqLineVm.DiscountPercent), width: 80, editable: true, realtime: true);
        _grid.AddTextColumn("Importo", nameof(RfqLineVm.LineNet), width: 100);
        _grid.AddCheckColumn("Disp.", nameof(RfqLineVm.Available), 60);
        _grid.AddTextColumn("Note", nameof(RfqLineVm.Notes), width: 180, editable: true);
        _grid.SetItems(_lines);
        _grid.EnablePersonalization("rfq-lines");
        LinesHost.Content = _grid;
    }

    private void Personalize_Click(object sender, RoutedEventArgs e) =>
        _grid.OpenPersonalization(Window.GetWindow(this));

    private async Task InitAsync()
    {
        var suppliers = await _api.GetSuppliersAsync();
        if (suppliers is not null)
            SupplierCombo.ItemsSource = suppliers.Where(s => s.Status == "Active").ToList();

        if (IsEdit) await LoadAsync();
        RecalcTotals();
    }

    private async Task LoadAsync()
    {
        HideError();
        var d = await _api.GetRfqAsync(_id!.Value);
        if (d is null) { ShowError("Impossibile caricare la RDO."); return; }
        _number = d.Number;
        _status = d.Status;
        FormSubtitle.Text = $"{d.Number} \u00b7 {d.SupplierName} \u00b7 {d.Status}";
        DateBox.SelectedDate = d.Date;
        DueDateBox.SelectedDate = d.ResponseDueDate;
        SupplierCombo.SelectedValue = d.SupplierId;
        StatusBox.SelectedItem = DocStatuses.Contains(d.Status) ? d.Status : "Generato";
        _lines.Clear();
        foreach (var l in d.Lines) _lines.Add(RfqLineVm.From(l));
        ApplyLock(LockedStatuses.Contains(_status));
    }

    private void ApplyLock(bool locked)
    {
        _grid.ReadOnly = locked;
        SaveBtn.IsEnabled = !locked;
        AddLineBtn.IsEnabled = !locked;
        RemoveLineBtn.IsEnabled = !locked;
        FromItemBtn.IsEnabled = !locked;
        DateBox.IsEnabled = !locked;
        DueDateBox.IsEnabled = !locked;
        SupplierCombo.IsEnabled = !locked;
        StatusBox.IsEnabled = !locked;
        if (locked) FormSubtitle.Text = $"{_number} \u00b7 {_status} (sola lettura)";
    }

    private void OnLinesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
            foreach (RfqLineVm vm in e.NewItems) vm.PropertyChanged += OnLinePropertyChanged;
        if (e.OldItems is not null)
            foreach (RfqLineVm vm in e.OldItems) vm.PropertyChanged -= OnLinePropertyChanged;
        RecalcTotals();
    }

    private void OnLinePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(RfqLineVm.Quantity) or nameof(RfqLineVm.UnitPrice) or nameof(RfqLineVm.DiscountPercent))
            RecalcTotals();
    }

    private void RecalcTotals()
    {
        var total = _lines.Where(l => l.Available).Sum(l => l.LineNet);
        TotalText.Text = total.ToString("N2", CultureInfo.CurrentCulture);
        var count = _lines.Count(l => !string.IsNullOrWhiteSpace(l.Description));
        LineCountText.Text = count == 1 ? "1 riga" : $"{count} righe";
    }

    private void FromItem_Click(object sender, RoutedEventArgs e)
    {
        var picker = new ItemPickerWindow(_api) { Owner = Window.GetWindow(this) };
        if (picker.ShowDialog() != true) return;
        foreach (var it in picker.SelectedItems)
            _lines.Add(new RfqLineVm
            {
                ItemId = it.Id, ItemCode = it.Code, Description = it.Description,
                Quantity = 1m, UnitOfMeasure = it.UnitOfMeasure,
                UnitPrice = it.BasePrice == 0 ? null : it.BasePrice, Available = true
            });
    }

    private void AddLine_Click(object sender, RoutedEventArgs e) =>
        _lines.Add(new RfqLineVm { Description = "Nuova riga", Quantity = 1m, Available = true });

    private void RemoveLine_Click(object sender, RoutedEventArgs e)
    {
        if (_grid.SelectedItem is RfqLineVm vm) _lines.Remove(vm);
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        HideError();
        _grid.Grid.CommitEdit(DataGridEditingUnit.Row, true);
        if (SupplierCombo.SelectedValue is not long supplierId) { ShowError("Seleziona un fornitore."); return; }

        var lines = _lines.Where(l => !string.IsNullOrWhiteSpace(l.Description)).Select(l => l.ToSaveDto()).ToList();
        if (lines.Count == 0) { ShowError("Inserire almeno una riga."); return; }
        if (lines.Any(l => l.Quantity <= 0)) { ShowError("Le quantita devono essere maggiori di zero."); return; }

        var status = StatusBox.SelectedItem as string ?? "Generato";
        if (status is "OffertaRicevuta" or "Confermato" && lines.Any(l => l.UnitPrice is null or <= 0))
        {
            ShowError("Per registrare l'offerta inserire il prezzo unitario su tutte le righe.");
            return;
        }

        var date = DateBox.SelectedDate ?? DateTime.Today;
        var due = DueDateBox.SelectedDate;

        ApiWriteResult res = IsEdit
            ? await _api.SaveRfqAsync(_id!.Value, new SaveRfqDto(date, supplierId, due, status, null, lines))
            : await _api.CreateRfqAsync(new CreateRfqDto(date, supplierId, null, due, null, lines));

        if (!res.Ok) { ShowError(res.Error ?? "Salvataggio non riuscito."); return; }
        Saved?.Invoke();
    }

    private void Print_Click(object sender, RoutedEventArgs e)
    {
        HideError();
        if (!IsEdit || _number is null) { ShowError("Salva la RDO prima di stamparla."); return; }

        var supplierName = (SupplierCombo.SelectedItem as SupplierListItem)?.BusinessName ?? "";
        var header = new List<ReportFieldDto>
        {
            new("Numero", _number),
            new("Data", (DateBox.SelectedDate ?? DateTime.Today).ToString("dd/MM/yyyy")),
            new("Fornitore", supplierName),
            new("Scadenza risposta", DueDateBox.SelectedDate?.ToString("dd/MM/yyyy") ?? "-"),
            new("Stato", StatusBox.SelectedItem as string ?? _status)
        };
        var columns = new[] { "Articolo", "Descrizione", "Q.ta", "UM", "Prezzo unit.", "Sconto %", "Importo" };
        var printable = _lines.Where(l => !string.IsNullOrWhiteSpace(l.Description)).ToList();
        var rows = printable.Select(l => (IReadOnlyList<string>)new[]
        {
            l.ItemCode ?? "", l.Description, l.Quantity.ToString("0.##", CultureInfo.CurrentCulture),
            l.UnitOfMeasure ?? "", l.UnitPrice?.ToString("0.00", CultureInfo.CurrentCulture) ?? "",
            l.DiscountPercent?.ToString("0.##", CultureInfo.CurrentCulture) ?? "",
            l.LineNet.ToString("0.00", CultureInfo.CurrentCulture)
        }).ToList();

        var total = printable.Where(l => l.Available).Sum(l => l.LineNet);
        var totals = new List<ReportFieldDto> { new("Totale imponibile", total.ToString("N2", CultureInfo.CurrentCulture)) };

        var model = new ReportDocumentDto(
            $"Richiesta di Offerta {_number}", supplierName, header, columns, rows, totals,
            "KBM ERP - Ciclo passivo", _companyName);

        KBM.Client.ReportPreviewWindow.Show(_api, "rdo.print", model, Window.GetWindow(this));
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Cancelled?.Invoke();

    private void ShowError(string m) { ErrorText.Text = m; ErrorBox.Visibility = Visibility.Visible; }
    private void HideError() => ErrorBox.Visibility = Visibility.Collapsed;
}

public sealed class RfqLineVm : INotifyPropertyChanged
{
    private string _description = string.Empty;
    private decimal _quantity = 1m;
    private string? _unitOfMeasure;
    private decimal? _unitPrice;
    private decimal? _discountPercent;
    private bool _available = true;
    private string? _notes;

    public long Id { get; set; }
    public long? ItemId { get; set; }
    public string? ItemCode { get; set; }

    public string Description { get => _description; set { _description = value; OnChanged(nameof(Description)); } }
    public decimal Quantity { get => _quantity; set { _quantity = value; OnChanged(nameof(Quantity)); OnChanged(nameof(LineNet)); } }
    public string? UnitOfMeasure { get => _unitOfMeasure; set { _unitOfMeasure = value; OnChanged(nameof(UnitOfMeasure)); } }
    public decimal? UnitPrice { get => _unitPrice; set { _unitPrice = value; OnChanged(nameof(UnitPrice)); OnChanged(nameof(LineNet)); } }
    public decimal? DiscountPercent { get => _discountPercent; set { _discountPercent = value; OnChanged(nameof(DiscountPercent)); OnChanged(nameof(LineNet)); } }
    public bool Available { get => _available; set { _available = value; OnChanged(nameof(Available)); } }
    public string? Notes { get => _notes; set { _notes = value; OnChanged(nameof(Notes)); } }

    /// <summary>Importo netto di riga: Q.ta x Prezzo x (1 - Sconto%).</summary>
    public decimal LineNet
    {
        get
        {
            var price = _unitPrice ?? 0m;
            var disc = _discountPercent ?? 0m;
            var net = _quantity * price * (1 - disc / 100m);
            return Math.Round(net < 0 ? 0 : net, 2, MidpointRounding.AwayFromZero);
        }
    }

    public static RfqLineVm From(RfqLineDto l) => new()
    {
        Id = l.Id, ItemId = l.ItemId, ItemCode = l.ItemCode, Description = l.Description,
        Quantity = l.Quantity, UnitOfMeasure = l.UnitOfMeasure, UnitPrice = l.UnitPrice,
        DiscountPercent = l.DiscountPercent, Available = l.Available, Notes = l.Notes
    };

    public SaveRfqLineDto ToSaveDto() => new(
        Id, ItemId, Description.Trim(), Quantity <= 0 ? 1m : Quantity,
        string.IsNullOrWhiteSpace(UnitOfMeasure) ? null : UnitOfMeasure.Trim(),
        UnitPrice, DiscountPercent, Available,
        string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim());

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnChanged(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
}
