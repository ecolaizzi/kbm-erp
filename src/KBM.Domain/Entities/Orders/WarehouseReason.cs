using KBM.Domain.Common;

namespace KBM.Domain.Entities.Orders;

/// <summary>Causale di magazzino: segno movimento e categoria documento (tabella base ordini).</summary>
public class WarehouseReason : AuditableTenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public StockMovementSign MovementSign { get; set; } = StockMovementSign.Out;
    public bool AffectsStock { get; set; } = true;
    public DocumentCategory Category { get; set; } = DocumentCategory.SalesOrder;
    public string Status { get; set; } = "Active";
}
