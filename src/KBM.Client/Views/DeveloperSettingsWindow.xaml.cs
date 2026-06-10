using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class DeveloperSettingsWindow : Window
{
    private readonly ApiClient _api;
    private readonly long _companyId;

    private readonly ObservableCollection<SettingVm> _companySettings = new();
    private readonly ObservableCollection<SettingVm> _techSettings = new();
    private readonly ObservableCollection<ReportVm> _reports = new();

    public DeveloperSettingsWindow(ApiClient api, long companyId, string companyName)
    {
        InitializeComponent();
        _api = api;
        _companyId = companyId;
        ContextText.Text = $"Azienda corrente: {companyName} (#{companyId}) - Riservato ai profili con permesso sviluppatore";
        CompanyGrid.ItemsSource = _companySettings;
        TechGrid.ItemsSource = _techSettings;
        ReportList.ItemsSource = _reports;
        Loaded += async (_, _) => await LoadAllAsync();
    }

    private async Task LoadAllAsync()
    {
        await LoadReportsAsync();
        await LoadCompanyAsync();
        await LoadTechAsync();
    }

    private async Task LoadReportsAsync()
    {
        HideError();
        var defs = await _api.GetReportDefinitionsAsync(null);
        if (defs is null) { ShowError("Impossibile caricare le definizioni report."); return; }
        _reports.Clear();
        foreach (var d in defs) _reports.Add(ReportVm.From(d));
    }

    private async Task LoadCompanyAsync()
    {
        var s = await _api.GetSettingsAsync(_companyId);
        _companySettings.Clear();
        if (s is not null) foreach (var x in s) _companySettings.Add(SettingVm.From(x));
    }

    private async Task LoadTechAsync()
    {
        var s = await _api.GetSettingsAsync(null);
        _techSettings.Clear();
        if (s is not null) foreach (var x in s) _techSettings.Add(SettingVm.From(x));
    }

    private void ReportList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ReportList.SelectedItem is not ReportVm r) { ReportForm.IsEnabled = false; return; }
        ReportForm.IsEnabled = true;
        RepKeyBox.Text = r.Key;
        RepTitleBox.Text = r.Title;
        SelectByTag(RepEngineCombo, r.Engine);
        SelectByTag(RepFormatCombo, r.OutputFormat);
        RepTemplateBox.Text = r.TemplatePathOrName ?? "";
        RepEnabledCheck.IsChecked = r.Enabled;
    }

    private async void SaveReport_Click(object sender, RoutedEventArgs e)
    {
        if (ReportList.SelectedItem is not ReportVm r) return;
        HideError();
        var dto = new UpsertReportDefinitionDto(
            r.Id, r.CompanyId, r.Key, RepTitleBox.Text.Trim(),
            TagInt(RepEngineCombo), TagInt(RepFormatCombo),
            string.IsNullOrWhiteSpace(RepTemplateBox.Text) ? null : RepTemplateBox.Text.Trim(),
            RepEnabledCheck.IsChecked == true);
        var res = await _api.UpsertReportDefinitionAsync(dto);
        if (!res.Ok) { ShowError(res.Error ?? "Salvataggio non riuscito."); return; }
        await LoadReportsAsync();
    }

    private void AddCompanySetting_Click(object sender, RoutedEventArgs e) =>
        _companySettings.Add(new SettingVm { Category = "Azienda" });
    private void AddTechSetting_Click(object sender, RoutedEventArgs e) =>
        _techSettings.Add(new SettingVm { Category = "Tecnica" });

    private async void SaveCompanySettings_Click(object sender, RoutedEventArgs e) => await SaveSettingsAsync(_companySettings, _companyId);
    private async void SaveTechSettings_Click(object sender, RoutedEventArgs e) => await SaveSettingsAsync(_techSettings, null);
    private async void ReloadCompany_Click(object sender, RoutedEventArgs e) => await LoadCompanyAsync();
    private async void ReloadTech_Click(object sender, RoutedEventArgs e) => await LoadTechAsync();

    private async Task SaveSettingsAsync(ObservableCollection<SettingVm> source, long? companyId)
    {
        HideError();
        CompanyGrid.CommitEdit(); TechGrid.CommitEdit();
        foreach (var s in source.Where(s => !string.IsNullOrWhiteSpace(s.Key)))
        {
            var res = await _api.UpsertSettingAsync(new UpsertSettingDto(
                companyId, s.Key.Trim(), s.Value ?? "", string.IsNullOrWhiteSpace(s.Category) ? "General" : s.Category, s.Description));
            if (!res.Ok) { ShowError(res.Error ?? "Salvataggio impostazione non riuscito."); return; }
        }
        if (companyId.HasValue) await LoadCompanyAsync(); else await LoadTechAsync();
    }

    private static void SelectByTag(ComboBox combo, int tag)
    {
        foreach (var item in combo.Items)
            if (item is ComboBoxItem cbi && cbi.Tag is string t && int.TryParse(t, out var v) && v == tag)
            { combo.SelectedItem = cbi; return; }
        combo.SelectedIndex = 0;
    }

    private static int TagInt(ComboBox combo) =>
        combo.SelectedItem is ComboBoxItem cbi && cbi.Tag is string t && int.TryParse(t, out var v) ? v : 0;

    private void ShowError(string message) { ErrorText.Text = message; ErrorBox.Visibility = Visibility.Visible; }
    private void HideError() => ErrorBox.Visibility = Visibility.Collapsed;
}

public sealed class SettingVm
{
    public long Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string? Description { get; set; }

    public static SettingVm From(SystemSettingDto d) => new()
    { Id = d.Id, Key = d.Key, Value = d.Value, Category = d.Category, Description = d.Description };
}

public sealed class ReportVm
{
    public long Id { get; set; }
    public long? CompanyId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Engine { get; set; }
    public int OutputFormat { get; set; }
    public string? TemplatePathOrName { get; set; }
    public bool Enabled { get; set; }

    public string Display => $"{Key}  -  {Title}";

    public static ReportVm From(ReportDefinitionDto d) => new()
    {
        Id = d.Id, CompanyId = d.CompanyId, Key = d.Key, Title = d.Title,
        Engine = d.Engine, OutputFormat = d.OutputFormat, TemplatePathOrName = d.TemplatePathOrName, Enabled = d.Enabled
    };
}
