using KBM.Domain.Common;

namespace KBM.Domain.Entities.Orders;

/// <summary>Zona / area commerciale (tabella base ordini).</summary>
public class Zone : AuditableTenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
}
