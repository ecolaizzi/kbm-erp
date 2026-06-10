using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class UsersView : UserControl
{
    private readonly ApiClient _api;
    private readonly DataGrid _grid;
    private string? _statusFilter;
    private List<UserRow> _rows = new();

    public event Action? RequestNewUser;
    public event Action<long>? RequestEditUser;

    public UsersView(ApiClient api)
    {
        InitializeComponent();
        _api = api;
        _grid = CreateGrid();
        Page.GridContent = _grid;

        Page.PageKey = "users";
        Page.PageTitle = "Utenti";
        Page.Subtitle = "Gestione utenti e accessi";
        Page.EnableWings();
        Page.AddToolbarButton(IconCatalog.New, "Nuovo", true, (_, _) => RequestNewUser?.Invoke(), Key.F2);
        Page.AddToolbarButton(IconCatalog.Edit, "Modifica", false, (_, _) => EditSelected(), Key.F3);
        Page.AddToolbarSeparator();
        Page.AddToolbarButton(IconCatalog.Enable, "Abilita", false, async (_, _) => await SetEnabledSelectedAsync(true));
        Page.AddToolbarButton(IconCatalog.Disable, "Disabilita", false, async (_, _) => await SetEnabledSelectedAsync(false), Key.F4);
        Page.AddToolbarSeparator();
        Page.AddToolbarButton(IconCatalog.Refresh, "Aggiorna", false, async (_, _) => await LoadAsync(), Key.F5);
        Page.AddToolbarButton(IconCatalog.Export, "Esporta", false, (_, _) => { });
        Page.AddQuickFilter("Tutti", (_, _) => { _statusFilter = null; _ = LoadAsync(); }, active: true);
        Page.AddQuickFilter("Attivi", (_, _) => { _statusFilter = "Active"; _ = LoadAsync(); });
        Page.AddQuickFilter("Disabilitati", (_, _) => { _statusFilter = "Disabled"; _ = LoadAsync(); });
        Loaded += async (_, _) => await LoadAsync();
    }

    private DataGrid CreateGrid()
    {
        var grid = new DataGrid
        {
            Style = (Style)FindResource("KbmDataGrid"),
            AutoGenerateColumns = false,
            IsReadOnly = true
        };
        grid.Columns.Add(new DataGridTextColumn { Header = "Username", Binding = new System.Windows.Data.Binding("Username"), Width = 120 });
        grid.Columns.Add(new DataGridTextColumn { Header = "Nome", Binding = new System.Windows.Data.Binding("FirstName"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
        grid.Columns.Add(new DataGridTextColumn { Header = "Cognome", Binding = new System.Windows.Data.Binding("LastName"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
        grid.Columns.Add(new DataGridTextColumn { Header = "Email", Binding = new System.Windows.Data.Binding("Email"), Width = new DataGridLength(1.4, DataGridLengthUnitType.Star) });
        grid.Columns.Add(new DataGridTextColumn { Header = "Ruolo", Binding = new System.Windows.Data.Binding("RolesDisplay"), Width = 120 });
        grid.Columns.Add(new DataGridTextColumn { Header = "Stato", Binding = new System.Windows.Data.Binding("StatusLabel"), Width = 90 });
        grid.SelectionChanged += Grid_SelectionChanged;
        grid.MouseDoubleClick += (_, _) => EditSelected();
        return grid;
    }

    private void EditSelected()
    {
        if (_grid.SelectedItem is UserRow r)
            RequestEditUser?.Invoke(r.Id);
        else
            MessageBox.Show("Seleziona un utente da modificare.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task SetEnabledSelectedAsync(bool enabled)
    {
        if (_grid.SelectedItem is not UserRow r)
        {
            MessageBox.Show("Seleziona un utente.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = await _api.SetUserEnabledAsync(r.Id, enabled);
        if (!result.Ok)
        {
            MessageBox.Show(result.Error ?? "Operazione non riuscita.", "KBM", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        await LoadAsync();
    }

    public Task ReloadAsync() => LoadAsync();

    private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_grid.SelectedItem is not UserRow r) return;
        var disabled = r.Status == "Disabled";
        Page.SetWingDetail(
            $"{r.FirstName} {r.LastName}".Trim(),
            new (string, string)[]
            {
                ("Username", r.Username),
                ("Email", r.Email),
                ("Ruoli", r.RolesDisplay),
                ("Stato", r.StatusLabel)
            },
            badge: disabled
                ? ("Disabilitato", new SolidColorBrush(Color.FromRgb(0xFE, 0xE2, 0xE2)), new SolidColorBrush(Color.FromRgb(0x99, 0x1B, 0x1B)))
                : ("Attivo", new SolidColorBrush(Color.FromRgb(0xD1, 0xFA, 0xE5)), new SolidColorBrush(Color.FromRgb(0x06, 0x5F, 0x46))));
    }

    public void TrySelect(long id)
    {
        var row = _rows.FirstOrDefault(r => r.Id == id);
        if (row is null) return;
        _grid.SelectedItem = row;
        _grid.ScrollIntoView(row);
    }

    public void SelectFirstRow()
    {
        if (_rows.Count == 0) return;
        _grid.SelectedItem = _rows[0];
        _grid.ScrollIntoView(_rows[0]);
    }

    private async Task LoadAsync()
    {
        Page.HideError();
        try
        {
            var users = await _api.GetUsersAsync();
            if (users is null) { Page.ShowError("Impossibile caricare utenti."); return; }

            var filtered = string.IsNullOrEmpty(_statusFilter)
                ? users
                : users.Where(u => u.Status == _statusFilter).ToList();

            _rows = filtered.Select(u => new UserRow(
                u.Id, u.Username, u.FirstName, u.LastName, u.Email, u.Status,
                u.Roles is { Count: > 0 } ? string.Join(", ", u.Roles) : "-")).ToList();
            _grid.ItemsSource = _rows;

            Page.RecordCountLabel = $"{_rows.Count} totali";
            Page.PaginationLabel = $"Pag 1/1 · Mostra: {_rows.Count} · Tot: {_rows.Count} record";
        }
        catch { Page.ShowError("Errore di connessione API."); }
    }
}

public sealed record UserRow(long Id, string Username, string FirstName, string LastName,
    string Email, string Status, string RolesDisplay)
{
    public string StatusLabel => Status == "Disabled" ? "Disabilitato" : "Attivo";
}
