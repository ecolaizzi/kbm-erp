using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class ItemPickerWindow : Window
{
    private readonly ApiClient _api;
    private List<ItemListItem> _all = new();
    private ICollectionView? _view;

    public IReadOnlyList<ItemListItem> SelectedItems { get; private set; } = new List<ItemListItem>();

    public ItemPickerWindow(ApiClient api)
    {
        InitializeComponent();
        _api = api;
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var items = await _api.GetItemsAsync();
        _all = items?.Where(i => i.Status == "Active").ToList() ?? new List<ItemListItem>();
        _view = CollectionViewSource.GetDefaultView(_all);
        _view.Filter = FilterPredicate;
        Grid.ItemsSource = _view;
    }

    private bool FilterPredicate(object obj)
    {
        if (obj is not ItemListItem i) return false;
        var q = SearchBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(q)) return true;
        return i.Code.Contains(q, StringComparison.OrdinalIgnoreCase)
               || i.Description.Contains(q, StringComparison.OrdinalIgnoreCase);
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => _view?.Refresh();

    private void Grid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (Grid.SelectedItem is ItemListItem) Confirm();
    }

    private void Ok_Click(object sender, RoutedEventArgs e) => Confirm();

    private void Confirm()
    {
        var selected = Grid.SelectedItems.Cast<ItemListItem>().ToList();
        if (selected.Count == 0) { HintText.Text = "Seleziona almeno un articolo."; return; }
        SelectedItems = selected;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
