using System.Linq;
using System.Windows;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class SupplierPickerWindow : Window
{
    private readonly ApiClient _api;
    public long? SelectedSupplierId { get; private set; }

    public SupplierPickerWindow(ApiClient api)
    {
        InitializeComponent();
        _api = api;
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var suppliers = await _api.GetSuppliersAsync();
        if (suppliers is not null)
        {
            SupplierCombo.ItemsSource = suppliers.Where(s => s.Status == "Active").ToList();
            if (SupplierCombo.Items.Count > 0) SupplierCombo.SelectedIndex = 0;
        }
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (SupplierCombo.SelectedValue is long id)
        {
            SelectedSupplierId = id;
            DialogResult = true;
        }
        else
        {
            MessageBox.Show("Seleziona un fornitore.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
