using System.Windows;
using System.Windows.Controls;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class UserEditView : UserControl
{
    private readonly ApiClient _api;
    private readonly long? _userId;
    private bool IsEdit => _userId.HasValue;

    private List<SelectableItem> _roles = new();
    private List<SelectableItem> _companies = new();

    public event Action? Saved;
    public event Action? Cancelled;

    public UserEditView(ApiClient api, long? userId)
    {
        InitializeComponent();
        _api = api;
        _userId = userId;

        if (IsEdit)
        {
            FormTitle.Text = "Modifica utente";
            UsernameBox.IsEnabled = false;
            PasswordPanel.Visibility = Visibility.Collapsed;
            StatusPanel.Visibility = Visibility.Visible;
        }

        FormShortcuts.Apply(this,
            save: () => Save_Click(this, new RoutedEventArgs()),
            exit: () => Cancel_Click(this, new RoutedEventArgs()));

        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        HideError();
        try
        {
            var roles = await _api.GetRolesAsync();
            _roles = (roles ?? new List<RoleListItem>())
                .Select(r => new SelectableItem(r.Code, r.Name)).ToList();
            RolesList.ItemsSource = _roles;

            var companies = await _api.GetCompaniesAsync();
            _companies = (companies ?? new List<CompanyListItem>())
                .Select(c => new SelectableItem(c.Id.ToString(), c.BusinessName)).ToList();
            CompaniesList.ItemsSource = _companies;

            if (IsEdit)
            {
                var detail = await _api.GetUserAsync(_userId!.Value);
                if (detail is null) { ShowError("Impossibile caricare l'utente."); return; }

                UsernameBox.Text = detail.Username;
                EmailBox.Text = detail.Email;
                FirstNameBox.Text = detail.FirstName;
                LastNameBox.Text = detail.LastName;
                FormSubtitle.Text = detail.Username;
                SelectStatus(detail.Status);

                var roleCodes = detail.RoleCodes ?? new List<string>();
                foreach (var r in _roles) r.IsSelected = roleCodes.Contains(r.Value);
                var companyIds = (detail.CompanyIds ?? new List<long>()).Select(i => i.ToString()).ToHashSet();
                foreach (var c in _companies) c.IsSelected = companyIds.Contains(c.Value);
                RolesList.Items.Refresh();
                CompaniesList.Items.Refresh();
            }
        }
        catch
        {
            ShowError("Errore di connessione API.");
        }
    }

    private void SelectStatus(string status)
    {
        foreach (var item in StatusCombo.Items)
            if (item is ComboBoxItem cbi && (string)cbi.Tag == status)
            {
                StatusCombo.SelectedItem = cbi;
                return;
            }
        StatusCombo.SelectedIndex = 0;
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        HideError();

        var email = EmailBox.Text.Trim();
        var first = FirstNameBox.Text.Trim();
        var last = LastNameBox.Text.Trim();
        var roleCodes = _roles.Where(r => r.IsSelected).Select(r => r.Value).ToList();
        var companyIds = _companies.Where(c => c.IsSelected).Select(c => long.Parse(c.Value)).ToList();

        if (string.IsNullOrWhiteSpace(email)) { ShowError("L'email è obbligatoria."); return; }

        SaveBtn.IsEnabled = false;
        try
        {
            ApiWriteResult result;
            if (IsEdit)
            {
                var status = (StatusCombo.SelectedItem as ComboBoxItem)?.Tag as string ?? "Active";
                result = await _api.UpdateUserAsync(_userId!.Value,
                    new UpdateUserDto(email, first, last, status, companyIds, roleCodes));
            }
            else
            {
                var username = UsernameBox.Text.Trim();
                var password = PasswordBox.Password;
                if (string.IsNullOrWhiteSpace(username)) { ShowError("Lo username è obbligatorio."); return; }
                if (string.IsNullOrWhiteSpace(password)) { ShowError("La password è obbligatoria."); return; }
                result = await _api.CreateUserAsync(
                    new CreateUserDto(username, email, password, first, last, companyIds, roleCodes));
            }

            if (result.Ok) Saved?.Invoke();
            else ShowError(result.Error ?? "Salvataggio non riuscito.");
        }
        catch
        {
            ShowError("Errore di connessione API durante il salvataggio.");
        }
        finally
        {
            SaveBtn.IsEnabled = true;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Cancelled?.Invoke();

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorBox.Visibility = Visibility.Visible;
    }

    private void HideError() => ErrorBox.Visibility = Visibility.Collapsed;
}

public sealed class SelectableItem
{
    public SelectableItem(string value, string label)
    {
        Value = value;
        Label = label;
    }

    public string Value { get; }
    public string Label { get; }
    public bool IsSelected { get; set; }
}
