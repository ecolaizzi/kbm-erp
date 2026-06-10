using System.Windows;
using System.Windows.Input;
using KBM.Client.Services;

namespace KBM.Client;

public partial class CompanyPickerWindow : Window
{
    public long? SelectedCompanyId { get; private set; }

    public CompanyPickerWindow(IReadOnlyList<CompanyOption> companies, string? operatorName = null)
    {
        InitializeComponent();
        CompaniesList.ItemsSource = companies;
        if (companies.Count > 0)
            CompaniesList.SelectedIndex = 0;
        OperatorText.Text = string.IsNullOrWhiteSpace(operatorName)
            ? "Sessione di lavoro"
            : $"Operatore: {operatorName}";
        Loaded += (_, _) => CompaniesList.Focus();
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    private void CompaniesList_MouseDoubleClick(object sender, MouseButtonEventArgs e) => Confirm();

    private void Continue_Click(object sender, RoutedEventArgs e) => Confirm();

    private void Confirm()
    {
        if (CompaniesList.SelectedItem is CompanyOption company)
        {
            SelectedCompanyId = company.Id;
            DialogResult = true;
        }
    }
}
