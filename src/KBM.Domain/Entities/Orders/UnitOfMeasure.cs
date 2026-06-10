using KBM.Domain.Common;

namespace KBM.Domain.Entities.Orders;

/// <summary>Unità di misura (tabella base ordini).</summary>
public class UnitOfMeasure : AuditableTenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DecimalPlaces { get; set; }
    public string Status { get; set; } = "Active";
}
