using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class ItemEditView : UserControl
{
    private readonly ApiClient _api;
    private readonly long? _itemId;
    private bool IsEdit => _itemId.HasValue;

    public event Action? Saved;
    public event Action? Cancelled;

    public ItemEditView(ApiClient api, long? itemId)
    {
        InitializeComponent();
        _api = api;
        _itemId = itemId;

        if (IsEdit)
        {
            FormTitle.Text = "Modifica articolo";
            CodeBox.IsEnabled = false;
            StatusPanel.Visibility = Visibility.Visible;
        }

        FormShortcuts.Apply(this,
            save: () => Save_Click(this, new RoutedEventArgs()),
            exit: () => Cancel_Click(this, new RoutedEventArgs()));

        Loaded += async (_, _) => await InitAsync();
    }

    private async Task InitAsync()
    {
        await LoadCategoriesAsync();
        if (IsEdit) await LoadAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var cats = await _api.GetItemCategoriesAsync() ?? new List<ItemCategoryListItem>();
            var list = new List<ItemCategoryListItem> { new(0, "", "(nessuna)", "Active") };
            list.AddRange(cats.Where(c => c.Status == "Active"));
            CategoryCombo.ItemsSource = list;
            CategoryCombo.SelectedValue = 0L;
        }
        catch { /* categorie non disponibili: combo resta vuota */ }
    }

    private async Task LoadAsync()
    {
        HideError();
        try
        {
            var d = await _api.GetItemAsync(_itemId!.Value);
            if (d is null) { ShowError("Impossibile caricare l'articolo."); return; }

            CodeBox.Text = d.Code;
            DescriptionBox.Text = d.Description;
            UmBox.Text = d.UnitOfMeasure;
            BarcodeBox.Text = d.Barcode ?? "";
            SupplierCodeBox.Text = d.SupplierItemCode ?? "";
            PriceBox.Text = d.BasePrice.ToString(CultureInfo.CurrentCulture);
            VatBox.Text = d.VatRate.ToString(CultureInfo.CurrentCulture);
            RevenueBox.Text = d.RevenueAccount ?? "";
            CostBox.Text = d.CostAccount ?? "";
            NotesBox.Text = d.Notes ?? "";
            FormSubtitle.Text = $"{d.Code} \u00b7 {d.Description}";
            CategoryCombo.SelectedValue = d.CategoryId ?? 0L;
            SelectStatus(d.Status);
        }
        catch { ShowError("Errore di connessione API."); }
    }

    private void SelectStatus(string status)
    {
        foreach (var item in StatusCombo.Items)
            if (item is ComboBoxItem cbi && (string)cbi.Tag == status) { StatusCombo.SelectedItem = cbi; return; }
        StatusCombo.SelectedIndex = 0;
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        HideError();
        var description = DescriptionBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(description)) { ShowError("La descrizione \u00e8 obbligatoria."); return; }

        var categoryId = CategoryCombo.SelectedValue is long cid && cid > 0 ? cid : (long?)null;
        var um = string.IsNullOrWhiteSpace(UmBox.Text) ? "NR" : UmBox.Text.Trim();
        var price = ParseDecimal(PriceBox.Text) ?? 0m;
        var vat = ParseDecimal(VatBox.Text) ?? 22m;

        SaveBtn.IsEnabled = false;
        try
        {
            ApiWriteResult result;
            if (IsEdit)
            {
                var status = (StatusCombo.SelectedItem as ComboBoxItem)?.Tag as string ?? "Active";
                var dto = new UpdateItemDto(description, categoryId, um, V(BarcodeBox), V(SupplierCodeBox),
                    price, vat, V(RevenueBox), V(CostBox), V(NotesBox), status);
                result = await _api.UpdateItemAsync(_itemId!.Value, dto);
            }
            else
            {
                var code = CodeBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(code)) { ShowError("Il codice \u00e8 obbligatorio."); return; }
                var dto = new CreateItemDto(code, description, categoryId, um, V(BarcodeBox), V(SupplierCodeBox),
                    price, vat, V(RevenueBox), V(CostBox), V(NotesBox));
                result = await _api.CreateItemAsync(dto);
            }

            if (result.Ok) Saved?.Invoke();
            else ShowError(result.Error ?? "Salvataggio non riuscito.");
        }
        catch { ShowError("Errore di connessione API durante il salvataggio."); }
        finally { SaveBtn.IsEnabled = true; }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Cancelled?.Invoke();

    private static string? V(TextBox t) => string.IsNullOrWhiteSpace(t.Text) ? null : t.Text.Trim();

    private static decimal? ParseDecimal(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        text = text.Trim().Replace(",", ".");
        return decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;
    }

    private void ShowError(string message) { ErrorText.Text = message; ErrorBox.Visibility = Visibility.Visible; }
    private void HideError() => ErrorBox.Visibility = Visibility.Collapsed;
}
