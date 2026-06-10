using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class ChartAccountEditView : UserControl
{
    private readonly ApiClient _api;
    private readonly long? _id;
    private List<ParentOption> _parents = new();
    private bool IsEdit => _id.HasValue;

    public event Action? Saved;
    public event Action? Cancelled;

    public ChartAccountEditView(ApiClient api, long? id)
    {
        InitializeComponent();
        _api = api;
        _id = id;

        if (IsEdit)
        {
            FormTitle.Text = "Modifica conto";
            CodeBox.IsEnabled = false;
            ParentCombo.IsEnabled = false;
            NatureCombo.IsEnabled = false;
            StatusPanel.Visibility = Visibility.Visible;
        }

        FormShortcuts.Apply(this,
            save: () => Save_Click(this, new RoutedEventArgs()),
            exit: () => Cancel_Click(this, new RoutedEventArgs()));

        Loaded += async (_, _) => await InitAsync();
    }

    private async Task InitAsync()
    {
        HideError();
        try
        {
            var accounts = await _api.GetChartAccountsAsync();
            _parents = new List<ParentOption> { ParentOption.Root };
            if (accounts is not null)
            {
                _parents.AddRange(accounts
                    .Where(a => a.Level != 3) // solo mastri e conti possono avere figli
                    .OrderBy(a => a.FullCode)
                    .Select(a => new ParentOption(a.Id, a.FullCode, a.Name, a.Nature, a.Sign)));
            }
            ParentCombo.ItemsSource = _parents;
            ParentCombo.SelectedIndex = 0;

            if (IsEdit) await LoadAsync();
            else { ApplyParentRules(); Recalc(this, null!); }
        }
        catch { ShowError("Errore di connessione API."); }
    }

    private async Task LoadAsync()
    {
        var d = await _api.GetChartAccountAsync(_id!.Value);
        if (d is null) { ShowError("Impossibile caricare il conto."); return; }

        CodeBox.Text = d.Code;
        FullCodeBox.Text = d.FullCode;
        NameBox.Text = d.Name;
        CeeDareBox.Text = d.BilCeeDare ?? "";
        CeeAvereBox.Text = d.BilCeeAvere ?? "";
        FormSubtitle.Text = $"{d.FullCode} \u00b7 {ChartAccountLabels.Level(d.Level)}";

        ParentCombo.SelectedItem = _parents.FirstOrDefault(p => p.Id == d.ParentId) ?? ParentOption.Root;
        SelectByTag(NatureCombo, d.Nature);
        SelectByTag(SignCombo, d.Sign);
        SelectByTag(SubKindCombo, d.SubKind);
        foreach (var item in StatusCombo.Items)
            if (item is ComboBoxItem cbi && (string)cbi.Tag == d.Status) { StatusCombo.SelectedItem = cbi; break; }
    }

    private void Parent_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (IsEdit) return;
        ApplyParentRules();
        Recalc(sender, null!);
    }

    private void ApplyParentRules()
    {
        var parent = ParentCombo.SelectedItem as ParentOption;
        var isMastro = parent is null || parent.Id is null;
        // La natura si definisce solo sul mastro; per conti/sottoconti viene ereditata dal padre.
        NatureCombo.IsEnabled = isMastro;
        if (!isMastro && parent is not null)
        {
            SelectByTag(NatureCombo, parent.Nature);
            SelectByTag(SignCombo, parent.Sign);
            NatureHint.Text = $"Ereditata dal padre: {ChartAccountLabels.Nature(parent.Nature)}";
        }
        else
        {
            NatureHint.Text = "Definita sul mastro, ereditata a cascata";
        }
    }

    private void Recalc(object sender, RoutedEventArgs e)
    {
        if (FullCodeBox is null) return;
        var parent = ParentCombo.SelectedItem as ParentOption;
        var code = CodeBox.Text.Trim();
        FullCodeBox.Text = (parent is null || parent.Id is null) ? code : $"{parent.FullCode}.{code}";
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        HideError();
        var name = NameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name)) { ShowError("La descrizione \u00e8 obbligatoria."); return; }

        var sign = TagInt(SignCombo, 1);
        var subKind = TagInt(SubKindCombo, 0);

        SaveBtn.IsEnabled = false;
        try
        {
            ApiWriteResult result;
            if (IsEdit)
            {
                var status = (StatusCombo.SelectedItem as ComboBoxItem)?.Tag as string ?? "Active";
                var dto = new UpdateChartAccountDto(name, sign, subKind, V(CeeDareBox), V(CeeAvereBox), status);
                result = await _api.UpdateChartAccountAsync(_id!.Value, dto);
            }
            else
            {
                var code = CodeBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(code)) { ShowError("Il codice \u00e8 obbligatorio."); return; }
                var parent = ParentCombo.SelectedItem as ParentOption;
                long? parentId = parent?.Id;
                int? nature = parentId is null ? TagInt(NatureCombo, 1) : null; // natura solo sul mastro; ereditata altrimenti
                var dto = new CreateChartAccountDto(parentId, code, name, nature, sign, subKind, V(CeeDareBox), V(CeeAvereBox));
                result = await _api.CreateChartAccountAsync(dto);
            }

            if (result.Ok) { ToastService.Success("Conto salvato."); Saved?.Invoke(); }
            else ShowError(result.Error ?? "Salvataggio non riuscito.");
        }
        catch { ShowError("Errore di connessione API durante il salvataggio."); }
        finally { SaveBtn.IsEnabled = true; }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Cancelled?.Invoke();

    private static void SelectByTag(ComboBox combo, int value)
    {
        foreach (var item in combo.Items)
            if (item is ComboBoxItem cbi && int.TryParse((string)cbi.Tag, out var t) && t == value)
            { combo.SelectedItem = cbi; return; }
        combo.SelectedIndex = 0;
    }

    private static int TagInt(ComboBox combo, int fallback) =>
        combo.SelectedItem is ComboBoxItem cbi && int.TryParse((string)cbi.Tag, out var v) ? v : fallback;

    private static string? V(TextBox t) => string.IsNullOrWhiteSpace(t.Text) ? null : t.Text.Trim();

    private void ShowError(string message) { ErrorText.Text = message; ErrorBox.Visibility = Visibility.Visible; }
    private void HideError() => ErrorBox.Visibility = Visibility.Collapsed;

    public sealed record ParentOption(long? Id, string FullCode, string Name, int Nature, int Sign)
    {
        public static ParentOption Root { get; } = new(null, "", "(nessuno \u2192 Mastro di 1\u00b0 livello)", 1, 1);
        public string Display => Id is null ? Name : $"{FullCode} \u2014 {Name}";
    }
}
