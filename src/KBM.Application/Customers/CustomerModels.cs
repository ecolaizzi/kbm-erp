namespace KBM.Application.Customers;

public record CustomerListItem(
    long Id,
    string Code,
    string BusinessName,
    string? VatNumber,
    string? City,
    string Status);

public record CustomerDetail(
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

public record CreateCustomerRequest(
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

public record UpdateCustomerRequest(
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

// ===================== Aggregato (anagrafica a schede) =====================
public record CustomerAddressDto(
    long Id,
    string AddressType,
    string Description,
    string? Address,
    string? City,
    string? Province,
    string? PostalCode,
    string Country,
    string? Phone,
    string? Email,
    bool IsDefault);

public record CustomerContactDto(
    long Id,
    string Name,
    string? Role,
    string? Email,
    string? Phone,
    string? Mobile,
    string? Notes,
    bool IsPrimary);

public record CustomerBankDto(
    long Id,
    string BankName,
    string? Iban,
    string? Swift,
    string? Abi,
    string? Cab,
    bool IsDefault);

public record CustomerAccounting(
    string? PaymentMethod,
    string? PriceListCode,
    string? AgentCode,
    string? Zone,
    decimal? CreditLimit,
    decimal? DiscountPercent,
    bool SplitPayment,
    bool WithholdingTax,
    string? VatExemptionCode,
    string? AccountCode);

public record CustomerAggregate(
    CustomerDetail Detail,
    CustomerAccounting Accounting,
    IReadOnlyList<CustomerAddressDto> Addresses,
    IReadOnlyList<CustomerContactDto> Contacts,
    IReadOnlyList<CustomerBankDto> Banks);

public record SaveCustomerAggregateRequest(
    UpdateCustomerRequest Core,
    CustomerAccounting Accounting,
    IReadOnlyList<CustomerAddressDto> Addresses,
    IReadOnlyList<CustomerContactDto> Contacts,
    IReadOnlyList<CustomerBankDto> Banks);

public record CreateCustomerAggregateRequest(
    CreateCustomerRequest Core,
    CustomerAccounting Accounting,
    IReadOnlyList<CustomerAddressDto> Addresses,
    IReadOnlyList<CustomerContactDto> Contacts,
    IReadOnlyList<CustomerBankDto> Banks);
