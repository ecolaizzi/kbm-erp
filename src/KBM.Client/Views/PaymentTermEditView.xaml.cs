using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class PaymentTermEditView : UserControl
{
    private readonly ApiClient _api;
    private readonly long? _id;
    private bool IsEdit => _id.HasValue;

    public event Action? Saved;
    public event Action? Cancelled;

    public PaymentTermEditView(ApiClient api, long? id)
    {
        InitializeComponent();
        _api = api;
        _id = id;

        if (IsEdit)
        {
            FormTitle.Text = "Modifica condizione di pagamento";
            CodeBox.IsEnabled = false;
            StatusPanel.Visibility = Visibility.Visible;
        }

        FormShortcuts.Apply(this,
            save: () => Save_Click(this, new RoutedEventArgs()),
            exit: () => Cancel_Click(this, new RoutedEventArgs()));

        Loaded += async (_, _) =>
        {
            if (IsEdit) await LoadAsync();
            RefreshPreview();
        };
    }

    private async Task LoadAsync()
    {
        HideError();
        try
        {
            var d = await _api.GetPaymentTermAsync(_id!.Value);
            if (d is null) { ShowError("Impossibile caricare la condizione di pagamento."); return; }

            CodeBox.Text = d.Code;
            DescriptionBox.Text = d.Description;
            InstallmentsBox.Text = d.InstallmentsCount.ToString(CultureInfo.CurrentCulture);
            FirstDueBox.Text = d.FirstDueDays.ToString(CultureInfo.CurrentCulture);
            IntervalBox.Text = d.IntervalDays.ToString(CultureInfo.CurrentCulture);
            EndOfMonthCheck.IsChecked = d.EndOfMonth;
            MethodBox.Text = d.PaymentMethod ?? "";
            NotesBox.Text = d.Notes ?? "";
            FormSubtitle.Text = $"{d.Code} \u00b7 {d.Description}";
            SelectStatus(d.Status);
            RefreshPreview();
        }
        catch { ShowError("Errore di connessione API."); }
    }

    private void SelectStatus(string status)
    {
        foreach (var item in StatusCombo.Items)
            if (item is ComboBoxItem cbi && (string)cbi.Tag == status) { StatusCombo.SelectedItem = cbi; return; }
        StatusCombo.SelectedIndex = 0;
    }

    private void Recalc(object sender, RoutedEventArgs e) => RefreshPreview();

    private void RefreshPreview()
    {
        if (PreviewList is null) return;
        var count = Math.Max(1, ParseInt(InstallmentsBox.Text, 1));
        var firstDue = Math.Max(0, ParseInt(FirstDueBox.Text, 0));
        var interval = Math.Max(0, ParseInt(IntervalBox.Text, 0));
        var endOfMonth = EndOfMonthCheck.IsChecked == true;

        const decimal sample = 1000m;
        var per = Math.Round(sample / count, 2, MidpointRounding.AwayFromZero);
        var today = DateTime.Today;
        decimal allocated = 0m;
        var lines = new List<string>();
        for (var i = 0; i < count && i < 12; i++)
        {
            var due = today.AddDays(firstDue + (interval * i));
            if (endOfMonth) due = new DateTime(due.Year, due.Month, DateTime.DaysInMonth(due.Year, due.Month));
            var value = i == count - 1 ? sample - allocated : per;
            allocated += value;
            lines.Add($"Rata {i + 1}: {due:dd/MM/yyyy} \u00b7 {value:N2} \u20ac");
        }
        if (count > 12) lines.Add($"\u2026 e altre {count - 12} rate");
        PreviewList.ItemsSource = lines;
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        HideError();
        var description = DescriptionBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(description)) { ShowError("La descrizione \u00e8 obbligatoria."); return; }

        var installments = Math.Max(1, ParseInt(InstallmentsBox.Text, 1));
        var firstDue = Math.Max(0, ParseInt(FirstDueBox.Text, 0));
        var interval = Math.Max(0, ParseInt(IntervalBox.Text, 0));
        var endOfMonth = EndOfMonthCheck.IsChecked == true;

        SaveBtn.IsEnabled = false;
        try
        {
            ApiWriteResult result;
            if (IsEdit)
            {
                var status = (StatusCombo.SelectedItem as ComboBoxItem)?.Tag as string ?? "Active";
                var dto = new UpdatePaymentTermDto(description, installments, firstDue, interval, endOfMonth,
                    V(MethodBox), V(NotesBox), status);
                result = await _api.UpdatePaymentTermAsync(_id!.Value, dto);
            }
            else
            {
                var code = CodeBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(code)) { ShowError("Il codice \u00e8 obbligatorio."); return; }
                var dto = new CreatePaymentTermDto(code, description, installments, firstDue, interval, endOfMonth,
                    V(MethodBox), V(NotesBox));
                result = await _api.CreatePaymentTermAsync(dto);
            }

            if (result.Ok) { ToastService.Success("Condizione di pagamento salvata."); Saved?.Invoke(); }
            else ShowError(result.Error ?? "Salvataggio non riuscito.");
        }
        catch { ShowError("Errore di connessione API durante il salvataggio."); }
        finally { SaveBtn.IsEnabled = true; }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Cancelled?.Invoke();

    private static string? V(TextBox t) => string.IsNullOrWhiteSpace(t.Text) ? null : t.Text.Trim();

    private static int ParseInt(string? text, int fallback) =>
        int.TryParse((text ?? "").Trim(), NumberStyles.Integer, CultureInfo.CurrentCulture, out var v) ? v : fallback;

    private void ShowError(string message) { ErrorText.Text = message; ErrorBox.Visibility = Visibility.Visible; }
    private void HideError() => ErrorBox.Visibility = Visibility.Collapsed;
}
