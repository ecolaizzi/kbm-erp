using System.Windows;
using System.Windows.Controls;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class CompanyEditView : UserControl
{
    private readonly ApiClient _api;
    private readonly long? _companyId;
    private bool IsEdit => _companyId.HasValue;

    public event Action? Saved;
    public event Action? Cancelled;

    public CompanyEditView(ApiClient api, long? companyId)
    {
        InitializeComponent();
        _api = api;
        _companyId = companyId;

        if (IsEdit)
        {
            FormTitle.Text = "Modifica azienda";
            CodeBox.IsEnabled = false;
            StatusPanel.Visibility = Visibility.Visible;
            Loaded += async (_, _) => await LoadAsync();
        }

        FormShortcuts.Apply(this,
            save: () => Save_Click(this, new RoutedEventArgs()),
            exit: () => Cancel_Click(this, new RoutedEventArgs()));
    }

    private async Task LoadAsync()
    {
        HideError();
        try
        {
            var detail = await _api.GetCompanyAsync(_companyId!.Value);
            if (detail is null) { ShowError("Impossibile caricare l'azienda."); return; }

            CodeBox.Text = detail.Code;
            BusinessNameBox.Text = detail.BusinessName;
            LegalNameBox.Text = detail.LegalName ?? "";
            VatBox.Text = detail.VatNumber ?? "";
            FormSubtitle.Text = $"{detail.Code} · {detail.BusinessName}";
            SelectStatus(detail.Status);
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

        var business = BusinessNameBox.Text.Trim();
        var legal = string.IsNullOrWhiteSpace(LegalNameBox.Text) ? null : LegalNameBox.Text.Trim();
        var vat = string.IsNullOrWhiteSpace(VatBox.Text) ? null : VatBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(business)) { ShowError("La ragione sociale è obbligatoria."); return; }

        SaveBtn.IsEnabled = false;
        try
        {
            ApiWriteResult result;
            if (IsEdit)
            {
                var status = (StatusCombo.SelectedItem as ComboBoxItem)?.Tag as string ?? "Active";
                result = await _api.UpdateCompanyAsync(_companyId!.Value,
                    new UpdateCompanyDto(business, legal, vat, status));
            }
            else
            {
                var code = CodeBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(code)) { ShowError("Il codice è obbligatorio."); return; }
                result = await _api.CreateCompanyAsync(new CreateCompanyDto(code, business, legal, vat));
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
