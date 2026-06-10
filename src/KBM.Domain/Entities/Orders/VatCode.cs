using KBM.Domain.Common;

namespace KBM.Domain.Entities.Orders;

/// <summary>Codice IVA (tabella base ordini — ambsor02).</summary>
public class VatCode : AuditableTenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string? NatureCode { get; set; }
    public decimal? DeductibilityPercent { get; set; }
    public string Status { get; set; } = "Active";
}
