using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class SupplierMultiPickerWindow : Window
{
    private readonly ApiClient _api;
    private readonly HashSet<long> _preselected;
    private readonly ObservableCollection<SupplierPick> _rows = new();

    public IReadOnlyList<long> SelectedSupplierIds { get; private set; } = new List<long>();

    public SupplierMultiPickerWindow(ApiClient api, IEnumerable<long> preselected, string? lineDescription = null)
    {
        InitializeComponent();
        _api = api;
        _preselected = preselected.ToHashSet();
        if (!string.IsNullOrWhiteSpace(lineDescription))
            HeaderText.Text = $"Fornitori candidati \u00b7 {lineDescription}";
        List.ItemsSource = _rows;
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var suppliers = await _api.GetSuppliersAsync();
        if (suppliers is null) return;
        _rows.Clear();
        foreach (var s in suppliers.Where(s => s.Status == "Active"))
            _rows.Add(new SupplierPick { Id = s.Id, Code = s.Code, BusinessName = s.BusinessName, Selected = _preselected.Contains(s.Id) });
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        SelectedSupplierIds = _rows.Where(r => r.Selected).Select(r => r.Id).ToList();
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}

public sealed class SupplierPick : INotifyPropertyChanged
{
    private bool _selected;
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public bool Selected { get => _selected; set { _selected = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selected))); } }
    public event PropertyChangedEventHandler? PropertyChanged;
}
