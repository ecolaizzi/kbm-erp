using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class CompaniesView : UserControl
{
    private readonly ApiClient _api;
    private readonly DataGrid _grid;
    private List<CompanyListItem> _rows = new();

    public event Action? RequestNewCompany;
    public event Action<long>? RequestEditCompany;

    public CompaniesView(ApiClient api)
    {
        InitializeComponent();
        _api = api;
        _grid = CreateGrid();
        Page.GridContent = _grid;
        Page.PageKey = "companies";
        Page.PageTitle = "Aziende";
        Page.Subtitle = "Anagrafica aziende (multi-ditta)";
        Page.EnableWings();
        Page.AddToolbarButton(IconCatalog.New, "Nuova", true, (_, _) => RequestNewCompany?.Invoke(), Key.F2);
        Page.AddToolbarButton(IconCatalog.Edit, "Modifica", false, (_, _) => EditSelected(), Key.F3);
        Page.AddToolbarSeparator();
        Page.AddToolbarButton(IconCatalog.Refresh, "Aggiorna", false, async (_, _) => await LoadAsync(), Key.F5);
        Loaded += async (_, _) => await LoadAsync();
    }

    private void EditSelected()
    {
        if (_grid.SelectedItem is CompanyListItem c)
            RequestEditCompany?.Invoke(c.Id);
        else
            MessageBox.Show("Seleziona un'azienda da modificare.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public Task ReloadAsync() => LoadAsync();

    private DataGrid CreateGrid()
    {
        var grid = new DataGrid
        {
            Style = (Style)Application.Current.FindResource("KbmDataGrid"),
            AutoGenerateColumns = false,
            IsReadOnly = true
        };
        grid.Columns.Add(new DataGridTextColumn { Header = "Codice", Binding = new System.Windows.Data.Binding("Code"), Width = 110 });
        grid.Columns.Add(new DataGridTextColumn { Header = "Ragione sociale", Binding = new System.Windows.Data.Binding("BusinessName"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
        grid.Columns.Add(new DataGridTextColumn { Header = "Stato", Binding = new System.Windows.Data.Binding("Status"), Width = 100 });
        grid.SelectionChanged += Grid_SelectionChanged;
        grid.MouseDoubleClick += (_, _) => EditSelected();
        return grid;
    }

    private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_grid.SelectedItem is not CompanyListItem c) return;
        Page.SetWingDetail(
            c.BusinessName,
            new (string, string)[]
            {
                ("Codice", c.Code),
                ("Ragione sociale", c.BusinessName),
                ("Stato", c.Status)
            });
    }

    public void TrySelect(long id)
    {
        var row = _rows.FirstOrDefault(r => r.Id == id);
        if (row is null) return;
        _grid.SelectedItem = row;
        _grid.ScrollIntoView(row);
    }

    private async Task LoadAsync()
    {
        Page.HideError();
        try
        {
            var items = await _api.GetCompaniesAsync();
            if (items is null) { Page.ShowError("Impossibile caricare aziende."); return; }
            _rows = items.ToList();
            _grid.ItemsSource = _rows;
            Page.RecordCountLabel = $"{_rows.Count} totali";
            Page.PaginationLabel = $"Tot: {_rows.Count} record";
        }
        catch { Page.ShowError("Errore di connessione API."); }
    }
}
