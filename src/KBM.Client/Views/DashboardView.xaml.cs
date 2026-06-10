using System.Windows;
using System.Windows.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class DashboardView : UserControl
{
    public event Action? NavigateUsers;
    public event Action? NavigateCompanies;
    public event Action? NavigateRoles;

    public DashboardView(LoginSession session, ApiClient api)
    {
        InitializeComponent();
        var hour = DateTime.Now.Hour;
        var saluto = hour < 12 ? "Buongiorno" : hour < 18 ? "Buon pomeriggio" : "Buonasera";
        WelcomeText.Text = $"{saluto}, {session.DisplayName}.";
        DateText.Text = DateTime.Now.ToString("dddd d MMMM yyyy", new System.Globalization.CultureInfo("it-IT"));
        SessionInfo.Text = $"Utente: {session.Username}\nAzienda: {session.CompanyName}\nRuoli: {string.Join(", ", session.Roles ?? [])}";
        Loaded += async (_, _) => await LoadKpiAsync(api);
    }

    private async Task LoadKpiAsync(ApiClient api)
    {
        try
        {
            var users = await api.GetUsersAsync();
            var companies = await api.GetCompaniesAsync();
            KpiUsers.Text = users?.Count.ToString() ?? "0";
            KpiCompanies.Text = companies?.Count.ToString() ?? "0";
        }
        catch
        {
            KpiUsers.Text = "—";
            KpiCompanies.Text = "—";
        }
    }

    private void GoUsers_Click(object sender, RoutedEventArgs e) => NavigateUsers?.Invoke();
    private void GoCompanies_Click(object sender, RoutedEventArgs e) => NavigateCompanies?.Invoke();
    private void GoRoles_Click(object sender, RoutedEventArgs e) => NavigateRoles?.Invoke();
}
