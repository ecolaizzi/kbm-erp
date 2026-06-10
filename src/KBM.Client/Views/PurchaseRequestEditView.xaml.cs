using System.Collections.ObjectModel;
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

public partial class PurchaseRequestEditView : UserControl
{
    private readonly ApiClient _api;
    private readonly string _companyName;
    private long? _id;
    private string? _number;
    private string _status = "Generato";
    private bool IsEdit => _id.HasValue;

    private readonly ObservableCollection<PrLineVm> _lines = new();
    private readonly FilterableGrid _grid = new();

    private static readonly string[] DocStatuses = { "Generato", "Confermato", "EmessaRDO", "Chiuso" };
    private static readonly string[] LineStatuses = { "Aperta", "Confermata", "Chiusa" };
    // Stati che bloccano la modifica del documento.
    private static readonly HashSet<string> LockedStatuses = new() { "Chiuso" };

    public event Action? Saved;
    public event Action? Cancelled;

    public PurchaseRequestEditView(ApiClient api, long? id, string companyName)
    {
        InitializeComponent();
        _api = api;
        _id = id;
        _companyName = companyName;

        BuildGrid();
        DateBox.SelectedDate = DateTime.Today;
        foreach (var s in DocStatuses) StatusBox.Items.Add(s);

        if (IsEdit)
        {
            FormTitle.Text = "Modifica richiesta di acquisto";
            StatusBox.Visibility = Visibility.Visible;
            ConfirmBtn.Visibility = Visibility.Visible;
            Loaded += async (_, _) => await LoadAsync();
        }
        else
        {
            PrintBtn.IsEnabled = false;
        }

        FormShortcuts.Apply(this,
            save: () => { if (SaveBtn.IsEnabled) Save_Click(this, new RoutedEventArgs()); },
            print: () => { if (PrintBtn.IsEnabled) Print_Click(this, new RoutedEventArgs()); },
            exit: () => Cancel_Click(this, new RoutedEventArgs()));
    }

    private void BuildGrid()
    {
        _grid.AddTextColumn("Articolo", nameof(PrLineVm.ItemCode), width: 110);
        _grid.AddTextColumn("Descrizione", nameof(PrLineVm.Description), star: true, editable: true, realtime: true);
        _grid.AddTextColumn("Q.t\u00e0", nameof(PrLineVm.Quantity), width: 80, editable: true);
        _grid.AddTextColumn("UM", nameof(PrLineVm.UnitOfMeasure), width: 60, editable: true);
        _grid.AddTextColumn("Data richiesta", nameof(PrLineVm.RequiredDateText), width: 120, editable: true);
        _grid.AddTextColumn("Prezzo prop.", nameof(PrLineVm.ProposedPrice), width: 100, editable: true);
        _grid.AddTextColumn("Fornitori", nameof(PrLineVm.SupplierCount), width: 80);
        _grid.AddComboColumn("Stato riga", nameof(PrLineVm.LineStatus), LineStatuses, 120);
        _grid.SetItems(_lines);
        _grid.EnablePersonalization("pr-lines");
        LinesHost.Content = _grid;
    }

    private void Personalize_Click(object sender, RoutedEventArgs e) =>
        _grid.OpenPersonalization(Window.GetWindow(this));

    private async Task LoadAsync()
    {
        HideError();
        var d = await _api.GetPurchaseRequestAsync(_id!.Value);
        if (d is null) { ShowError("Impossibile caricare la RDA."); return; }
        _number = d.Number;
        _status = d.Status;
        FormSubtitle.Text = $"{d.Number} \u00b7 {d.Date:dd/MM/yyyy} \u00b7 {d.Status}";
        DateBox.SelectedDate = d.Date;
        UnitBox.Text = d.RequestingUnit ?? "";
        NotesBox.Text = d.Notes ?? "";
        StatusBox.SelectedItem = DocStatuses.Contains(d.Status) ? d.Status : null;
        _lines.Clear();
        foreach (var l in d.Lines) _lines.Add(PrLineVm.From(l));
        ApplyLock(LockedStatuses.Contains(_status));
    }

    private void ApplyLock(bool locked)
    {
        _grid.ReadOnly = locked;
        SaveBtn.IsEnabled = !locked;
        AddLineBtn.IsEnabled = !locked;
        RemoveLineBtn.IsEnabled = !locked;
        FromItemBtn.IsEnabled = !locked;
        SuppliersBtn.IsEnabled = !locked;
        ConfirmBtn.IsEnabled = !locked;
        DateBox.IsEnabled = !locked;
        UnitBox.IsEnabled = !locked;
        NotesBox.IsEnabled = !locked;
        StatusBox.IsEnabled = !locked;
        if (locked) FormSubtitle.Text = $"{_number} \u00b7 documento CHIUSO (sola lettura)";
    }

    private async void FromItem_Click(object sender, RoutedEventArgs e)
    {
        var picker = new ItemPickerWindow(_api) { Owner = Window.GetWindow(this) };
        if (picker.ShowDialog() != true) return;
        foreach (var it in picker.SelectedItems)
            _lines.Add(new PrLineVm
            {
                ItemId = it.Id, ItemCode = it.Code, Description = it.Description,
                Quantity = 1m, UnitOfMeasure = it.UnitOfMeasure,
                ProposedPrice = it.BasePrice == 0 ? null : it.BasePrice, LineStatus = "Aperta"
            });
    }

    private void LineSuppliers_Click(object sender, RoutedEventArgs e)
    {
        if (_grid.SelectedItem is not PrLineVm vm)
        {
            MessageBox.Show("Seleziona una riga.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var picker = new SupplierMultiPickerWindow(_api, vm.SupplierIds, vm.Description) { Owner = Window.GetWindow(this) };
        if (picker.ShowDialog() != true) return;
        vm.SetSuppliers(picker.SelectedSupplierIds);
    }

    private void AddLine_Click(object sender, RoutedEventArgs e) =>
        _lines.Add(new PrLineVm { Description = "Nuova riga", Quantity = 1m, LineStatus = "Aperta" });

    private void RemoveLine_Click(object sender, RoutedEventArgs e)
    {
        if (_grid.SelectedItem is PrLineVm vm) _lines.Remove(vm);
    }

    private async void Confirm_Click(object sender, RoutedEventArgs e)
    {
        if (!IsEdit) return;
        if (MessageBox.Show("Confermare la RDA? Le righe restano modificabili finche non viene chiusa.",
                "KBM", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        StatusBox.SelectedItem = "Confermato";
        await SaveInternalAsync(reloadAfter: false);
    }

    private async void Save_Click(object sender, RoutedEventArgs e) => await SaveInternalAsync(reloadAfter: false);

    private async Task SaveInternalAsync(bool reloadAfter)
    {
        HideError();
        _grid.Grid.CommitEdit(DataGridEditingUnit.Row, true);
        var lines = _lines.Where(l => !string.IsNullOrWhiteSpace(l.Description)).Select(l => l.ToSaveDto()).ToList();
        if (lines.Count == 0) { ShowError("Inserire almeno una riga."); return; }
        if (lines.Any(l => l.Quantity <= 0)) { ShowError("Le quantita devono essere maggiori di zero."); return; }

        var date = DateBox.SelectedDate ?? DateTime.Today;
        var unit = string.IsNullOrWhiteSpace(UnitBox.Text) ? null : UnitBox.Text.Trim();
        var notes = string.IsNullOrWhiteSpace(NotesBox.Text) ? null : NotesBox.Text.Trim();

        ApiWriteResult res;
        if (IsEdit)
        {
            var status = StatusBox.SelectedItem as string ?? "Generato";
            res = await _api.SavePurchaseRequestAsync(_id!.Value, new SavePurchaseRequestDto(date, unit, status, notes, lines));
        }
        else
        {
            res = await _api.CreatePurchaseRequestAsync(new CreatePurchaseRequestDto(date, unit, notes, lines));
        }

        if (!res.Ok) { ShowError(res.Error ?? "Salvataggio non riuscito."); return; }
        Services.ToastService.Success("Richiesta di acquisto salvata.", "RDA");
        Saved?.Invoke();
    }

    private void Print_Click(object sender, RoutedEventArgs e)
    {
        HideError();
        if (!IsEdit || _number is null) { ShowError("Salva la RDA prima di stamparla."); return; }

        var header = new List<ReportFieldDto>
        {
            new("Numero", _number),
            new("Data", (DateBox.SelectedDate ?? DateTime.Today).ToString("dd/MM/yyyy")),
            new("Reparto", string.IsNullOrWhiteSpace(UnitBox.Text) ? "-" : UnitBox.Text.Trim()),
            new("Stato", StatusBox.SelectedItem as string ?? _status)
        };
        var columns = new[] { "Articolo", "Descrizione", "Q.ta", "UM", "Data richiesta", "Prezzo proposto", "Fornitori" };
        var rows = _lines.Where(l => !string.IsNullOrWhiteSpace(l.Description)).Select(l => (IReadOnlyList<string>)new[]
        {
            l.ItemCode ?? "", l.Description, l.Quantity.ToString("0.##", CultureInfo.CurrentCulture),
            l.UnitOfMeasure ?? "", l.RequiredDateText ?? "",
            l.ProposedPrice?.ToString("0.00", CultureInfo.CurrentCulture) ?? "",
            l.SupplierCount.ToString()
        }).ToList();

        var model = new ReportDocumentDto(
            $"Richiesta di Acquisto {_number}", "Documento RDA", header, columns, rows, null,
            "KBM ERP - Ciclo passivo", _companyName);

        KBM.Client.ReportPreviewWindow.Show(_api, "rda.print", model, Window.GetWindow(this));
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Cancelled?.Invoke();

    private void ShowError(string m) { ErrorText.Text = m; ErrorBox.Visibility = Visibility.Visible; }
    private void HideError() => ErrorBox.Visibility = Visibility.Collapsed;
}

public sealed class PrLineVm : INotifyPropertyChanged
{
    private string _description = string.Empty;
    private decimal _quantity = 1m;
    private string? _unitOfMeasure;
    private string? _requiredDateText;
    private decimal? _proposedPrice;
    private string _lineStatus = "Aperta";
    private List<long> _supplierIds = new();

    public long Id { get; set; }
    public long? ItemId { get; set; }
    public string? ItemCode { get; set; }

    public string Description { get => _description; set { _description = value; OnChanged(nameof(Description)); } }
    public decimal Quantity { get => _quantity; set { _quantity = value; OnChanged(nameof(Quantity)); } }
    public string? UnitOfMeasure { get => _unitOfMeasure; set { _unitOfMeasure = value; OnChanged(nameof(UnitOfMeasure)); } }
    public string? RequiredDateText { get => _requiredDateText; set { _requiredDateText = value; OnChanged(nameof(RequiredDateText)); } }
    public decimal? ProposedPrice { get => _proposedPrice; set { _proposedPrice = value; OnChanged(nameof(ProposedPrice)); } }
    public string LineStatus { get => _lineStatus; set { _lineStatus = value; OnChanged(nameof(LineStatus)); } }

    public IReadOnlyList<long> SupplierIds => _supplierIds;
    public int SupplierCount => _supplierIds.Count;

    public void SetSuppliers(IEnumerable<long> ids)
    {
        _supplierIds = ids.Distinct().ToList();
        OnChanged(nameof(SupplierCount));
    }

    public static PrLineVm From(PurchaseRequestLineDto l)
    {
        var vm = new PrLineVm
        {
            Id = l.Id, ItemId = l.ItemId, ItemCode = l.ItemCode, Description = l.Description,
            Quantity = l.Quantity, UnitOfMeasure = l.UnitOfMeasure,
            RequiredDateText = l.RequiredDate?.ToString("yyyy-MM-dd"),
            ProposedPrice = l.ProposedPrice, LineStatus = l.LineStatus
        };
        vm._supplierIds = l.SupplierIds?.ToList() ?? new List<long>();
        return vm;
    }

    public SavePurchaseRequestLineDto ToSaveDto()
    {
        DateTime? required = DateTime.TryParse(RequiredDateText, out var dt) ? dt : null;
        return new SavePurchaseRequestLineDto(Id, ItemId, Description.Trim(), Quantity <= 0 ? 1m : Quantity,
            string.IsNullOrWhiteSpace(UnitOfMeasure) ? null : UnitOfMeasure.Trim(), required, ProposedPrice, LineStatus,
            _supplierIds.ToList());
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnChanged(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
}
