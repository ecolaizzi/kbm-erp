using KBM.Domain.Common;

namespace KBM.Domain.Entities.Orders;

/// <summary>Ordine cliente (OV) — archivio ordini vendita.</summary>
public class SalesOrder : AuditableTenantEntity
{
    public string Number { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public long CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public string? CustomerOrderReference { get; set; }
    public long? BillingAddressId { get; set; }
    public long? ShippingAddressId { get; set; }
    public long? PriceListId { get; set; }
    public PriceList? PriceList { get; set; }
    public long? PaymentTermId { get; set; }
    public PaymentTerm? PaymentTerm { get; set; }
    public long? AgentUserId { get; set; }
    public long? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public long? CurrencyId { get; set; }
    public Currency? Currency { get; set; }
    public long? CarrierId { get; set; }
    public Carrier? Carrier { get; set; }
    public long? PortTypeId { get; set; }
    public PortType? PortType { get; set; }
    public long? DocumentTypeId { get; set; }
    public DocumentType? DocumentType { get; set; }

    public decimal ExchangeRate { get; set; } = 1m;
    public decimal HeaderDiscountPercent { get; set; }
    public decimal NetAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public OrderDocumentStatus Status { get; set; } = OrderDocumentStatus.Draft;
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? Notes { get; set; }

    public ICollection<SalesOrderLine> Lines { get; set; } = [];
}

public class SalesOrderLine : AuditableTenantEntity
{
    public long SalesOrderId { get; set; }
    public SalesOrder SalesOrder { get; set; } = null!;

    public int LineNumber { get; set; }
    public long? ItemId { get; set; }
    public Item? Item { get; set; }

    public string Description { get; set; } = string.Empty;
    public decimal OrderedQuantity { get; set; }
    public decimal DeliveredQuantity { get; set; }
    public decimal InvoicedQuantity { get; set; }

    public string? UnitOfMeasure { get; set; }
    public long? VatCodeId { get; set; }
    public VatCode? VatCode { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineDiscountPercent { get; set; }
    public decimal VatPercent { get; set; }
    public decimal NetAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public OrderLineStatus LineStatus { get; set; } = OrderLineStatus.Open;
}
