using System.Windows;
using KBM.Client.Services;

namespace KBM.Client;

public partial class SetupWizardWindow : Window
{
    private readonly AuthApiClient _api = new();
    private int _step = 1;

    public SetupWizardWindow()
    {
        InitializeComponent();
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void Next_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Visibility = Visibility.Collapsed;

        if (_step == 1)
        {
            if (string.IsNullOrWhiteSpace(CompanyCodeBox.Text) || string.IsNullOrWhiteSpace(BusinessNameBox.Text))
            {
                ShowError("Compilare codice e ragione sociale.");
                return;
            }
            _step = 2;
            StepCompany.Visibility = Visibility.Collapsed;
            StepAdmin.Visibility = Visibility.Visible;
            StepTitle.Text = "Step 2 - Amministratore";
            BackBtn.Visibility = Visibility.Visible;
            NextBtn.Content = "Completa setup";
            return;
        }

        _ = CompleteSetupAsync();
    }

    private async Task CompleteSetupAsync()
    {
        if (AdminPasswordBox.Password.Length < 8)
        {
            ShowError("Password minimo 8 caratteri.");
            return;
        }

        try
        {
            var ok = await _api.CompleteSetupAsync(new SetupCompleteRequest(
                CompanyCodeBox.Text.Trim(),
                BusinessNameBox.Text.Trim(),
                string.IsNullOrWhiteSpace(VatBox.Text) ? null : VatBox.Text.Trim(),
                AdminUserBox.Text.Trim(),
                AdminEmailBox.Text.Trim(),
                AdminPasswordBox.Password,
                FirstNameBox.Text.Trim(),
                LastNameBox.Text.Trim()));

            if (!ok)
            {
                ShowError("Setup non riuscito. Verificare i dati.");
                return;
            }

            var login = new LoginWindow();
            login.Show();
            Close();
        }
        catch (Exception)
        {
            ShowError("Errore API setup. Avviare KBM.Api.");
        }
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        _step = 1;
        StepAdmin.Visibility = Visibility.Collapsed;
        StepCompany.Visibility = Visibility.Visible;
        StepTitle.Text = "Step 1 - Azienda";
        BackBtn.Visibility = Visibility.Collapsed;
        NextBtn.Content = "Avanti";
    }

    private void ShowError(string msg)
    {
        ErrorText.Text = msg;
        ErrorText.Visibility = Visibility.Visible;
    }
}
