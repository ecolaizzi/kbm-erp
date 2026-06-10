using System.Windows;
using System.Windows.Controls;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class RoleEditView : UserControl
{
    private readonly ApiClient _api;
    private readonly long? _roleId;
    private bool IsEdit => _roleId.HasValue;
    private readonly List<SelectableItem> _allPerms = new();

    public event Action? Saved;
    public event Action? Cancelled;

    public RoleEditView(ApiClient api, long? roleId)
    {
        InitializeComponent();
        _api = api;
        _roleId = roleId;

        if (IsEdit)
        {
            FormTitle.Text = "Modifica ruolo";
            CodeBox.IsEnabled = false;
        }

        BuildPermissionSections();

        FormShortcuts.Apply(this,
            save: () => Save_Click(this, new RoutedEventArgs()),
            exit: () => Cancel_Click(this, new RoutedEventArgs()));

        Loaded += async (_, _) => await LoadAsync();
    }

    private void BuildPermissionSections()
    {
        foreach (var group in PermissionCatalog.Groups)
        {
            PermSections.Children.Add(new TextBlock
            {
                Text = group.Title.ToUpperInvariant(),
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = (System.Windows.Media.Brush)FindResource("Brush.TextSecondary"),
                Margin = new Thickness(0, 6, 0, 6)
            });

            var wrap = new WrapPanel { Margin = new Thickness(0, 0, 0, 10) };
            foreach (var (code, label) in group.Items)
            {
                var item = new SelectableItem(code, label);
                _allPerms.Add(item);
                var cb = new CheckBox
                {
                    Content = label,
                    Width = 250,
                    Margin = new Thickness(0, 3, 0, 3),
                    DataContext = item
                };
                cb.SetBinding(System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty,
                    new System.Windows.Data.Binding(nameof(SelectableItem.IsSelected)) { Mode = System.Windows.Data.BindingMode.TwoWay });
                cb.Checked += (_, _) => UpdateCount();
                cb.Unchecked += (_, _) => UpdateCount();
                wrap.Children.Add(cb);
            }
            PermSections.Children.Add(wrap);
        }
        UpdateCount();
    }

    private void UpdateCount() =>
        PermCount.Text = $"{_allPerms.Count(p => p.IsSelected)} / {_allPerms.Count} selezionati";

    private async Task LoadAsync()
    {
        if (!IsEdit) return;
        HideError();
        try
        {
            var detail = await _api.GetRoleAsync(_roleId!.Value);
            if (detail is null) { ShowError("Impossibile caricare il ruolo."); return; }

            CodeBox.Text = detail.Code;
            NameBox.Text = detail.Name;
            DescriptionBox.Text = detail.Description ?? "";
            FormSubtitle.Text = detail.IsSystem ? $"{detail.Code} · ruolo di sistema" : detail.Code;

            var perms = (detail.Permissions ?? new List<string>()).ToHashSet();
            foreach (var p in _allPerms) p.IsSelected = perms.Contains(p.Value);
            RefreshChecks();
            UpdateCount();
        }
        catch
        {
            ShowError("Errore di connessione API.");
        }
    }

    private void RefreshChecks()
    {
        // Forza il refresh dei binding TwoWay dopo aver impostato IsSelected
        foreach (var section in PermSections.Children)
            if (section is WrapPanel wrap)
                foreach (var child in wrap.Children)
                    if (child is CheckBox cb && cb.DataContext is SelectableItem item)
                        cb.IsChecked = item.IsSelected;
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        HideError();

        var name = NameBox.Text.Trim();
        var description = string.IsNullOrWhiteSpace(DescriptionBox.Text) ? null : DescriptionBox.Text.Trim();
        var permissionCodes = _allPerms.Where(p => p.IsSelected).Select(p => p.Value).ToList();

        if (string.IsNullOrWhiteSpace(name)) { ShowError("Il nome è obbligatorio."); return; }

        SaveBtn.IsEnabled = false;
        try
        {
            ApiWriteResult result;
            if (IsEdit)
            {
                result = await _api.UpdateRoleAsync(_roleId!.Value,
                    new UpdateRoleDto(name, description, permissionCodes));
            }
            else
            {
                var code = CodeBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(code)) { ShowError("Il codice è obbligatorio."); return; }
                result = await _api.CreateRoleAsync(new CreateRoleDto(code, name, description, permissionCodes));
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

/// <summary>Catalogo permessi lato client (allineato a KBM.Application.Security.PermissionCodes).</summary>
public static class PermissionCatalog
{
    public sealed record PermGroup(string Title, IReadOnlyList<(string Code, string Label)> Items);

    public static IReadOnlyList<PermGroup> Groups { get; } = new[]
    {
        new PermGroup("Utenti", new[]
        {
            ("core.users.read", "Visualizza utenti"),
            ("core.users.create", "Crea utenti"),
            ("core.users.edit", "Modifica utenti"),
            ("core.users.delete", "Disabilita/Elimina utenti"),
        }),
        new PermGroup("Ruoli", new[]
        {
            ("core.roles.read", "Visualizza ruoli"),
            ("core.roles.create", "Crea ruoli"),
            ("core.roles.edit", "Modifica ruoli"),
            ("core.roles.delete", "Elimina ruoli"),
        }),
        new PermGroup("Aziende", new[]
        {
            ("core.companies.read", "Visualizza aziende"),
            ("core.companies.create", "Crea aziende"),
            ("core.companies.edit", "Modifica aziende"),
        }),
        new PermGroup("Sistema", new[]
        {
            ("core.audit.read", "Visualizza audit log"),
        }),
    };
}
