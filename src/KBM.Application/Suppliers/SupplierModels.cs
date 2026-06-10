namespace KBM.Application.Suppliers;

public record SupplierListItem(
    long Id,
    string Code,
    string BusinessName,
    string? VatNumber,
    string? City,
    string Status);

public record SupplierDetail(
    long Id,
    string Code,
    string BusinessName,
    string? VatNumber,
    string? FiscalCode,
    string? SdiCode,
    string? PecEmail,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? Province,
    string? PostalCode,
    string Country,
    string? Iban,
    string? PaymentTerms,
    string? Notes,
    string Status);

public record CreateSupplierRequest(
    string Code,
    string BusinessName,
    string? VatNumber,
    string? FiscalCode,
    string? SdiCode,
    string? PecEmail,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? Province,
    string? PostalCode,
    string? Country,
    string? Iban,
    string? PaymentTerms,
    string? Notes);

public record UpdateSupplierRequest(
    string BusinessName,
    string? VatNumber,
    string? FiscalCode,
    string? SdiCode,
    string? PecEmail,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? Province,
    string? PostalCode,
    string? Country,
    string? Iban,
    string? PaymentTerms,
    string? Notes,
    string Status);

public record SupplierAddressDto(
    long Id, string AddressType, string Description, string? Address, string? City, string? Province,
    string? PostalCode, string Country, string? Phone, string? Email, bool IsDefault);

public record SupplierContactDto(
    long Id, string Name, string? Role, string? Email, string? Phone, string? Mobile, string? Notes, bool IsPrimary);

public record SupplierBankDto(
    long Id, string BankName, string? Iban, string? Swift, string? Abi, string? Cab, bool IsDefault);

public record SupplierAccounting(
    string? PaymentMethod, string? PriceListCode, string? Zone, decimal? CreditLimit, decimal? DiscountPercent,
    bool SplitPayment, bool WithholdingTax, string? VatExemptionCode, string? AccountCode);

public record SupplierAggregate(
    SupplierDetail Detail,
    SupplierAccounting Accounting,
    IReadOnlyList<SupplierAddressDto> Addresses,
    IReadOnlyList<SupplierContactDto> Contacts,
    IReadOnlyList<SupplierBankDto> Banks);

public record SaveSupplierAggregateRequest(
    UpdateSupplierRequest Core,
    SupplierAccounting Accounting,
    IReadOnlyList<SupplierAddressDto> Addresses,
    IReadOnlyList<SupplierContactDto> Contacts,
    IReadOnlyList<SupplierBankDto> Banks);

public record CreateSupplierAggregateRequest(
    CreateSupplierRequest Core,
    SupplierAccounting Accounting,
    IReadOnlyList<SupplierAddressDto> Addresses,
    IReadOnlyList<SupplierContactDto> Contacts,
    IReadOnlyList<SupplierBankDto> Banks);
