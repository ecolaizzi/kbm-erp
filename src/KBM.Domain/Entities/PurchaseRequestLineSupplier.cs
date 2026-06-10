using KBM.Domain.Common;

namespace KBM.Domain.Entities;

/// <summary>Fornitore proposto/interpellato per una riga di RDA.</summary>
public class PurchaseRequestLineSupplier : AuditableTenantEntity
{
    public long PurchaseRequestLineId { get; set; }
    public long SupplierId { get; set; }

    public PurchaseRequestLine PurchaseRequestLine { get; set; } = null!;
    public Supplier Supplier { get; set; } = null!;
}
