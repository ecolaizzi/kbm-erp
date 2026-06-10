using KBM.Domain.Common;

namespace KBM.Domain.Entities;

public class Company : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? VatNumber { get; set; }
    public string Status { get; set; } = "Active";

    public ICollection<UserCompany> UserCompanies { get; set; } = [];
}
