namespace KBM.Application.Companies;

public record CompanyListItem(long Id, string Code, string BusinessName, string Status);

public record CompanyDetail(
    long Id,
    string Code,
    string BusinessName,
    string? LegalName,
    string? VatNumber,
    string Status);

public record CreateCompanyRequest(
    string Code,
    string BusinessName,
    string? LegalName,
    string? VatNumber);

public record UpdateCompanyRequest(
    string BusinessName,
    string? LegalName,
    string? VatNumber,
    string Status);
