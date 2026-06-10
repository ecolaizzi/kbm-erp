using KBM.Domain.Common;

namespace KBM.Domain.Entities.Orders;

/// <summary>Magazzino / deposito / terzista (tabella base ordini).</summary>
public class Warehouse : AuditableTenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public WarehouseKind Kind { get; set; } = WarehouseKind.Own;
    public string? Address { get; set; }
    public bool IsDefault { get; set; }
    public string Status { get; set; } = "Active";
}
