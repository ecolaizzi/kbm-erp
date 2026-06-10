using KBM.Domain.Common;

namespace KBM.Domain.Entities;

/// <summary>Riga di una RDO: articolo richiesto e quotazione ricevuta dal fornitore.</summary>
public class RfqLine : AuditableTenantEntity
{
    public long RfqId { get; set; }

    public long? ItemId { get; set; }
    public Item? Item { get; set; }

    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1m;
    public string? UnitOfMeasure { get; set; }

    public decimal? UnitPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public bool Available { get; set; } = true;
    public string? Notes { get; set; }

    public Rfq Rfq { get; set; } = null!;
}
