using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class CustomerEditView : UserControl
{
    private readonly ApiClient _api;
    private readonly long? _customerId;
    private bool IsEdit => _customerId.HasValue;

    private readonly ObservableCollection<AddressRowVm> _addresses = new();
    private readonly ObservableCollection<ContactRowVm> _contacts = new();
    private readonly ObservableCollection<BankRowVm> _banks = new();
    private readonly FilterableGrid _addressGrid = new();
    private readonly FilterableGrid _contactGrid = new();
    private readonly FilterableGrid _bankGrid = new();

    public event Action? Saved;
    public event Action? Cancelled;

    private static readonly string[] AddressTypes = { "Sede", "Spedizione", "Fatturazione", "Altro" };

    public CustomerEditView(ApiClient api, long? customerId)
    {
        InitializeComponent();
        _api = api;
        _customerId = customerId;

        BuildAddressGrid();
        BuildContactGrid();
        BuildBankGrid();

        FormShortcuts.Apply(this,
            save: () => Save_Click(this, new RoutedEventArgs()),
            exit: () => Cancel_Click(this, new RoutedEventArgs()));

        if (IsEdit)
        {
            FormTitle.Text = "Modifica cliente";
            CodeBox.IsEnabled = false;
            StatusPanel.Visibility = Visibility.Visible;
            Loaded += async (_, _) => await LoadAsync();
        }
    }

    private void BuildAddressGrid()
    {
        _addressGrid.AddComboColumn("Tipo", nameof(AddressRowVm.AddressType), AddressTypes, 120);
        _addressGrid.AddTextColumn("Descrizione", nameof(AddressRowVm.Description), star: true, editable: true);
        _addressGrid.AddTextColumn("Indirizzo", nameof(AddressRowVm.Address), width: 200, editable: true);
        _addressGrid.AddTextColumn("Citt\u00e0", nameof(AddressRowVm.City), width: 150, editable: true);
        _addressGrid.AddTextColumn("Prov.", nameof(AddressRowVm.Province), width: 60, editable: true);
        _addressGrid.AddTextColumn("CAP", nameof(AddressRowVm.PostalCode), width: 70, editable: true);
        _addressGrid.AddTextColumn("Telefono", nameof(AddressRowVm.Phone), width: 130, editable: true);
        _addressGrid.AddCheckColumn("Default", nameof(AddressRowVm.IsDefault), 70);
        _addressGrid.SetItems(_addresses);
        AddressGridHost.Content = _addressGrid;
    }

    private void BuildContactGrid()
    {
        _contactGrid.AddTextColumn("Nome", nameof(ContactRowVm.Name), star: true, editable: true);
        _contactGrid.AddTextColumn("Ruolo", nameof(ContactRowVm.Role), width: 150, editable: true);
        _contactGrid.AddTextColumn("Email", nameof(ContactRowVm.Email), width: 200, editable: true);
        _contactGrid.AddTextColumn("Telefono", nameof(ContactRowVm.Phone), width: 130, editable: true);
        _contactGrid.AddTextColumn("Cellulare", nameof(ContactRowVm.Mobile), width: 130, editable: true);
        _contactGrid.AddCheckColumn("Primario", nameof(ContactRowVm.IsPrimary), 80);
        _contactGrid.SetItems(_contacts);
        ContactGridHost.Content = _contactGrid;
    }

    private void BuildBankGrid()
    {
        _bankGrid.AddTextColumn("Banca", nameof(BankRowVm.BankName), star: true, editable: true);
        _bankGrid.AddTextColumn("IBAN", nameof(BankRowVm.Iban), width: 240, editable: true);
        _bankGrid.AddTextColumn("BIC/SWIFT", nameof(BankRowVm.Swift), width: 110, editable: true);
        _bankGrid.AddTextColumn("ABI", nameof(BankRowVm.Abi), width: 70, editable: true);
        _bankGrid.AddTextColumn("CAB", nameof(BankRowVm.Cab), width: 70, editable: true);
        _bankGrid.AddCheckColumn("Default", nameof(BankRowVm.IsDefault), 70);
        _bankGrid.SetItems(_banks);
        BankGridHost.Content = _bankGrid;
    }

    private async Task LoadAsync()
    {
        HideError();
        try
        {
            var agg = await _api.GetCustomerFullAsync(_customerId!.Value);
            if (agg is null) { ShowError("Impossibile caricare il cliente."); return; }

            var c = agg.Detail;
            CodeBox.Text = c.Code;
            BusinessNameBox.Text = c.BusinessName;
            VatBox.Text = c.VatNumber ?? "";
            FiscalCodeBox.Text = c.FiscalCode ?? "";
            SdiBox.Text = c.SdiCode ?? "";
            PecBox.Text = c.PecEmail ?? "";
            EmailBox.Text = c.Email ?? "";
            PhoneBox.Text = c.Phone ?? "";
            AddressBox.Text = c.Address ?? "";
            CityBox.Text = c.City ?? "";
            ProvinceBox.Text = c.Province ?? "";
            PostalCodeBox.Text = c.PostalCode ?? "";
            IbanBox.Text = c.Iban ?? "";
            PaymentTermsBox.Text = c.PaymentTerms ?? "";
            NotesBox.Text = c.Notes ?? "";
            FormSubtitle.Text = $"{c.Code} \u00b7 {c.BusinessName}";
            SelectStatus(c.Status);

            var a = agg.Accounting;
            PaymentMethodBox.Text = a.PaymentMethod ?? "";
            PriceListBox.Text = a.PriceListCode ?? "";
            AgentBox.Text = a.AgentCode ?? "";
            ZoneBox.Text = a.Zone ?? "";
            CreditLimitBox.Text = a.CreditLimit?.ToString(CultureInfo.CurrentCulture) ?? "";
            DiscountBox.Text = a.DiscountPercent?.ToString(CultureInfo.CurrentCulture) ?? "";
            SplitPaymentCheck.IsChecked = a.SplitPayment;
            WithholdingCheck.IsChecked = a.WithholdingTax;
            VatExemptionBox.Text = a.VatExemptionCode ?? "";
            AccountCodeBox.Text = a.AccountCode ?? "";

            _addresses.Clear();
            foreach (var ad in agg.Addresses) _addresses.Add(AddressRowVm.From(ad));
            _contacts.Clear();
            foreach (var k in agg.Contacts) _contacts.Add(ContactRowVm.From(k));
            _banks.Clear();
            foreach (var b in agg.Banks) _banks.Add(BankRowVm.From(b));
        }
        catch { ShowError("Errore di connessione API."); }
    }

    private void SelectStatus(string status)
    {
        foreach (var item in StatusCombo.Items)
            if (item is ComboBoxItem cbi && (string)cbi.Tag == status) { StatusCombo.SelectedItem = cbi; return; }
        StatusCombo.SelectedIndex = 0;
    }

    // ===================== Add/Remove righe figlie =====================
    private void AddrAdd_Click(object s, RoutedEventArgs e) => _addresses.Add(new AddressRowVm { AddressType = "Spedizione", Country = "IT" });
    private void AddrDel_Click(object s, RoutedEventArgs e) { if (_addressGrid.SelectedItem is AddressRowVm v) _addresses.Remove(v); }
    private void ContactAdd_Click(object s, RoutedEventArgs e) => _contacts.Add(new ContactRowVm());
    private void ContactDel_Click(object s, RoutedEventArgs e) { if (_contactGrid.SelectedItem is ContactRowVm v) _contacts.Remove(v); }
    private void BankAdd_Click(object s, RoutedEventArgs e) => _banks.Add(new BankRowVm());
    private void BankDel_Click(object s, RoutedEventArgs e) { if (_bankGrid.SelectedItem is BankRowVm v) _banks.Remove(v); }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        HideError();
        _addressGrid.Grid.CommitEdit(); _contactGrid.Grid.CommitEdit(); _bankGrid.Grid.CommitEdit();

        var business = BusinessNameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(business)) { ShowError("La ragione sociale \u00e8 obbligatoria."); Tabs.SelectedIndex = 0; return; }

        var accounting = new CustomerAccountingDto(
            V(PaymentMethodBox), V(PriceListBox), V(AgentBox), V(ZoneBox),
            ParseDecimal(CreditLimitBox.Text), ParseDecimal(DiscountBox.Text),
            SplitPaymentCheck.IsChecked == true, WithholdingCheck.IsChecked == true,
            V(VatExemptionBox), V(AccountCodeBox));

        var addresses = _addresses.Select(a => a.ToDto()).ToList();
        var contacts = _contacts.Select(c => c.ToDto()).ToList();
        var banks = _banks.Select(b => b.ToDto()).ToList();

        SaveBtn.IsEnabled = false;
        try
        {
            ApiWriteResult result;
            if (IsEdit)
            {
                var status = (StatusCombo.SelectedItem as ComboBoxItem)?.Tag as string ?? "Active";
                var core = new UpdateCustomerDto(business, V(VatBox), V(FiscalCodeBox), V(SdiBox), V(PecBox),
                    V(EmailBox), V(PhoneBox), V(AddressBox), V(CityBox), V(ProvinceBox), V(PostalCodeBox), null,
                    V(IbanBox), V(PaymentTermsBox), V(NotesBox), status);
                result = await _api.SaveCustomerFullAsync(_customerId!.Value,
                    new SaveCustomerAggregateDto(core, accounting, addresses, contacts, banks));
            }
            else
            {
                var code = CodeBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(code)) { ShowError("Il codice \u00e8 obbligatorio."); Tabs.SelectedIndex = 0; return; }
                var core = new CreateCustomerDto(code, business, V(VatBox), V(FiscalCodeBox), V(SdiBox), V(PecBox),
                    V(EmailBox), V(PhoneBox), V(AddressBox), V(CityBox), V(ProvinceBox), V(PostalCodeBox), null,
                    V(IbanBox), V(PaymentTermsBox), V(NotesBox));
                result = await _api.CreateCustomerFullAsync(
                    new CreateCustomerAggregateDto(core, accounting, addresses, contacts, banks));
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

// ===================== Row-model griglie figlie =====================
public sealed class AddressRowVm
{
    public long Id { get; set; }
    public string AddressType { get; set; } = "Spedizione";
    public string Description { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "IT";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsDefault { get; set; }

    public static AddressRowVm From(CustomerAddressDto d) => new()
    {
        Id = d.Id, AddressType = d.AddressType, Description = d.Description, Address = d.Address, City = d.City,
        Province = d.Province, PostalCode = d.PostalCode, Country = d.Country, Phone = d.Phone, Email = d.Email, IsDefault = d.IsDefault
    };

    public CustomerAddressDto ToDto() => new(Id, AddressType, Description ?? "", Address, City, Province, PostalCode,
        string.IsNullOrWhiteSpace(Country) ? "IT" : Country, Phone, Email, IsDefault);
}

public sealed class ContactRowVm
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Notes { get; set; }
    public bool IsPrimary { get; set; }

    public static ContactRowVm From(CustomerContactDto d) => new()
    {
        Id = d.Id, Name = d.Name, Role = d.Role, Email = d.Email, Phone = d.Phone, Mobile = d.Mobile, Notes = d.Notes, IsPrimary = d.IsPrimary
    };

    public CustomerContactDto ToDto() => new(Id, Name ?? "", Role, Email, Phone, Mobile, Notes, IsPrimary);
}

public sealed class BankRowVm
{
    public long Id { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string? Iban { get; set; }
    public string? Swift { get; set; }
    public string? Abi { get; set; }
    public string? Cab { get; set; }
    public bool IsDefault { get; set; }

    public static BankRowVm From(CustomerBankDto d) => new()
    {
        Id = d.Id, BankName = d.BankName, Iban = d.Iban, Swift = d.Swift, Abi = d.Abi, Cab = d.Cab, IsDefault = d.IsDefault
    };

    public CustomerBankDto ToDto() => new(Id, BankName ?? "", Iban, Swift, Abi, Cab, IsDefault);
}
