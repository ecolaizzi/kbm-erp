using KBM.Domain.Common;

namespace KBM.Domain.Entities.Orders;

/// <summary>Tipo bolla/fattura/ordine — numerazione e categoria (tabella base ordini).</summary>
public class DocumentType : AuditableTenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DocumentCategory Category { get; set; } = DocumentCategory.SalesOrder;
    public string? NumberingPrefix { get; set; }
    public string Status { get; set; } = "Active";
}
