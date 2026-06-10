using KBM.Domain.Common;

namespace KBM.Domain.Entities.Orders;

/// <summary>Vettore / corriere (tabella base ordini).</summary>
public class Carrier : AuditableTenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? VatNumber { get; set; }
    public string? Phone { get; set; }
    public string Status { get; set; } = "Active";
}
