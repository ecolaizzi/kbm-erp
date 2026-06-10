using KBM.Domain.Common;

namespace KBM.Domain.Entities.Orders;

/// <summary>Tipo porto (franco/assegnato — tabella base ordini).</summary>
public class PortType : AuditableTenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PortCharge Charge { get; set; } = PortCharge.Franco;
    public string Status { get; set; } = "Active";
}
