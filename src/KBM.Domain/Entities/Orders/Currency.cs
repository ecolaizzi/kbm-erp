using KBM.Domain.Common;

namespace KBM.Domain.Entities.Orders;

/// <summary>Valuta estera (tabella base ordini).</summary>
public class Currency : AuditableTenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Symbol { get; set; } = "€";
    public int DecimalPlaces { get; set; } = 2;
    public bool IsDefault { get; set; }
    public string Status { get; set; } = "Active";
}
