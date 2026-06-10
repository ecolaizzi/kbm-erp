using KBM.Domain.Common;

namespace KBM.Domain.Entities;

/// <summary>Riga di una RDA: materiale/lavorazione richiesta, con i fornitori da interpellare.</summary>
public class PurchaseRequestLine : AuditableTenantEntity
{
    public long PurchaseRequestId { get; set; }

    public long? ItemId { get; set; }
    public Item? Item { get; set; }

    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1m;
    public string? UnitOfMeasure { get; set; }
    public DateTime? RequiredDate { get; set; }
    public decimal? ProposedPrice { get; set; }
    public string LineStatus { get; set; } = "Aperta";

    public PurchaseRequest PurchaseRequest { get; set; } = null!;
    public ICollection<PurchaseRequestLineSupplier> Suppliers { get; set; } = [];
}
