using System.Windows;
using KBM.Client.Services;

namespace KBM.Client;

public partial class LoginWindow : Window
{
    private readonly AuthApiClient _api = new();

    public LoginWindow()
    {
        InitializeComponent();
        UsernameBox.Text = "admin";
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Visibility = Visibility.Collapsed;
        var password = PasswordBox.Password;
        if (string.IsNullOrWhiteSpace(UsernameBox.Text) || string.IsNullOrEmpty(password))
        {
            ShowError("Inserire username e password.");
            return;
        }

        try
        {
            var outcome = await _api.LoginAsync(UsernameBox.Text.Trim(), password);
            if (outcome is null)
            {
                ShowError("Credenziali non valide.");
                return;
            }

            if (outcome.IsDisabled)
            {
                ShowError("Account disabilitato, contattare amministratore.");
                return;
            }

            if (outcome.RequiresCompanySelection && outcome.Companies?.Count > 0)
            {
                var picker = new CompanyPickerWindow(outcome.Companies, UsernameBox.Text.Trim()) { Owner = this };
                if (picker.ShowDialog() != true || picker.SelectedCompanyId is null)
                    return;

                outcome = await _api.LoginAsync(UsernameBox.Text.Trim(), password, picker.SelectedCompanyId);
                if (outcome?.Session is null)
                {
                    ShowError("Selezione azienda non valida.");
                    return;
                }
            }

            if (outcome.Session is null)
            {
                ShowError("Credenziali non valide.");
                return;
            }

            var shell = new ShellWindow(outcome.Session);
            shell.Show();
            Close();
        }
        catch (Exception)
        {
            ShowError("Impossibile contattare l'API. Avviare KBM.Api.");
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }
}
