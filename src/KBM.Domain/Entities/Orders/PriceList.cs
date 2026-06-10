using KBM.Domain.Common;

namespace KBM.Domain.Entities.Orders;

/// <summary>Archivio listini (vendita/acquisto).</summary>
public class PriceList : AuditableTenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PriceListKind Kind { get; set; } = PriceListKind.Sales;
    public long? CurrencyId { get; set; }
    public Currency? Currency { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string Status { get; set; } = "Active";

    public ICollection<PriceListLine> Lines { get; set; } = [];
}

public class PriceListLine : AuditableTenantEntity
{
    public long PriceListId { get; set; }
    public PriceList PriceList { get; set; } = null!;

    public long ItemId { get; set; }
    public Item Item { get; set; } = null!;

    public decimal UnitPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? MinQuantity { get; set; }
}
