using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class RolesView : UserControl
{
    private readonly ApiClient _api;
    private readonly DataGrid _grid;
    private List<RoleRow> _rows = new();

    public event Action? RequestNewRole;
    public event Action<long>? RequestEditRole;

    public RolesView(ApiClient api)
    {
        InitializeComponent();
        _api = api;
        _grid = CreateGrid();
        Page.GridContent = _grid;
        Page.PageKey = "roles";
        Page.PageTitle = "Ruoli e permessi";
        Page.Subtitle = "Controllo accessi basato sui ruoli (RBAC)";
        Page.EnableWings();
        Page.AddToolbarButton(IconCatalog.New, "Nuovo", true, (_, _) => RequestNewRole?.Invoke(), Key.F2);
        Page.AddToolbarButton(IconCatalog.Edit, "Modifica", false, (_, _) => EditSelected(), Key.F3);
        Page.AddToolbarSeparator();
        Page.AddToolbarButton(IconCatalog.Refresh, "Aggiorna", false, async (_, _) => await LoadAsync(), Key.F5);
        Loaded += async (_, _) => await LoadAsync();
    }

    private void EditSelected()
    {
        if (_grid.SelectedItem is RoleRow r)
            RequestEditRole?.Invoke(r.Id);
        else
            MessageBox.Show("Seleziona un ruolo da modificare.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
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
        grid.Columns.Add(new DataGridTextColumn { Header = "Codice", Binding = new System.Windows.Data.Binding("Code"), Width = 130 });
        grid.Columns.Add(new DataGridTextColumn { Header = "Nome", Binding = new System.Windows.Data.Binding("Name"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
        grid.Columns.Add(new DataGridCheckBoxColumn { Header = "Sistema", Binding = new System.Windows.Data.Binding("IsSystem"), Width = 70 });
        grid.Columns.Add(new DataGridTextColumn { Header = "N. permessi", Binding = new System.Windows.Data.Binding("PermissionCount"), Width = 90 });
        grid.SelectionChanged += Grid_SelectionChanged;
        grid.MouseDoubleClick += (_, _) => EditSelected();
        return grid;
    }

    private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_grid.SelectedItem is not RoleRow r) return;
        Page.SetWingDetail(
            r.Name,
            new (string, string)[]
            {
                ("Codice", r.Code),
                ("Tipo", r.IsSystem ? "Ruolo di sistema" : "Personalizzato"),
                ("Permessi", r.PermissionsDisplay)
            });
    }

    private async Task LoadAsync()
    {
        Page.HideError();
        try
        {
            var items = await _api.GetRolesAsync();
            if (items is null) { Page.ShowError("Impossibile caricare ruoli."); return; }
            _rows = items.Select(r => new RoleRow(
                r.Id, r.Code, r.Name, r.IsSystem,
                r.Permissions?.Count ?? 0,
                r.Permissions is { Count: > 0 } ? string.Join(", ", r.Permissions) : "-")).ToList();
            _grid.ItemsSource = _rows;
            Page.RecordCountLabel = $"{_rows.Count} totali";
            Page.PaginationLabel = $"Tot: {_rows.Count} record";
        }
        catch { Page.ShowError("Errore di connessione API."); }
    }
}

public sealed record RoleRow(long Id, string Code, string Name, bool IsSystem,
    int PermissionCount, string PermissionsDisplay);
