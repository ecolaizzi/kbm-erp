namespace KBM.Application.Orders;

// ----- Tabelle base (ambsor02) -----
public record LookupListItem(long Id, string Code, string Description, string Status);

public record VatCodeListItem(long Id, string Code, string Description, decimal Rate, string? NatureCode, string Status);
public record VatCodeDetail(long Id, string Code, string Description, decimal Rate, string? NatureCode, decimal? DeductibilityPercent, string Status);
public record SaveVatCodeRequest(string Code, string Description, decimal Rate, string? NatureCode, decimal? DeductibilityPercent);

public record WarehouseListItem(long Id, string Code, string Description, int Kind, bool IsDefault, string Status);
public record WarehouseDetail(long Id, string Code, string Description, int Kind, string? Address, bool IsDefault, string Status);
public record SaveWarehouseRequest(string Code, string Description, int Kind, string? Address, bool IsDefault);

public record WarehouseReasonListItem(long Id, string Code, string Description, int MovementSign, int Category, string Status);
public record SaveWarehouseReasonRequest(string Code, string Description, int MovementSign, bool AffectsStock, int Category);

public record SaveLookupRequest(string Code, string Description);

// ----- Listini -----
public record PriceListListItem(long Id, string Code, string Description, int Kind, int LineCount, string Status);
public record PriceListLineDto(long Id, long ItemId, string? ItemCode, string? ItemDescription, decimal UnitPrice, decimal? DiscountPercent, decimal? MinQuantity);
public record PriceListDetail(long Id, string Code, string Description, int Kind, long? CurrencyId, DateTime? ValidFrom, DateTime? ValidTo, string Status, IReadOnlyList<PriceListLineDto> Lines);
public record SavePriceListLineRequest(long? Id, long ItemId, decimal UnitPrice, decimal? DiscountPercent, decimal? MinQuantity);
public record CreatePriceListRequest(string Code, string Description, int Kind, long? CurrencyId, DateTime? ValidFrom, DateTime? ValidTo, IReadOnlyList<SavePriceListLineRequest> Lines);
public record SavePriceListRequest(string Description, int Kind, long? CurrencyId, DateTime? ValidFrom, DateTime? ValidTo, string Status, IReadOnlyList<SavePriceListLineRequest> Lines);

// ----- Ordini -----
public record OrderLineDto(
    long? Id, int LineNumber, long? ItemId, string? ItemCode, string Description,
    decimal OrderedQuantity, decimal FulfilledQuantity, decimal InvoicedQuantity,
    string? UnitOfMeasure, long? VatCodeId, decimal UnitPrice, decimal LineDiscountPercent,
    decimal VatPercent, decimal NetAmount, decimal VatAmount, decimal TotalAmount, int LineStatus);

public record SalesOrderListItem(long Id, string Number, DateTime OrderDate, string CustomerName, int LineCount, decimal TotalAmount, int Status);
public record SalesOrderDetail(
    long Id, string Number, DateTime OrderDate, long CustomerId, string CustomerName,
    string? CustomerOrderReference, long? BillingAddressId, long? ShippingAddressId,
    long? PriceListId, long? PaymentTermId, long? WarehouseId, long? CurrencyId,
    long? CarrierId, long? PortTypeId, long? DocumentTypeId,
    decimal ExchangeRate, decimal HeaderDiscountPercent,
    decimal NetAmount, decimal VatAmount, decimal TotalAmount,
    int Status, DateTime? ExpectedDeliveryDate, string? Notes,
    IReadOnlyList<OrderLineDto> Lines);

public record PurchaseOrderListItem(long Id, string Number, DateTime OrderDate, string SupplierName, int LineCount, decimal TotalAmount, int Status);
public record PurchaseOrderDetail(
    long Id, string Number, DateTime OrderDate, long SupplierId, string SupplierName,
    string? SupplierOrderReference, long? DeliveryAddressId, long? PaymentTermId,
    long? WarehouseId, long? CurrencyId, long? DocumentTypeId, long? PurchaseRequestId,
    decimal ExchangeRate, decimal HeaderDiscountPercent,
    decimal NetAmount, decimal VatAmount, decimal TotalAmount,
    int Status, DateTime? ExpectedDeliveryDate, string? Notes,
    IReadOnlyList<OrderLineDto> Lines);

public record SaveOrderLineRequest(
    long? Id, long? ItemId, string Description, decimal OrderedQuantity,
    string? UnitOfMeasure, long? VatCodeId, decimal UnitPrice, decimal LineDiscountPercent, decimal VatPercent);

public record CreateSalesOrderRequest(
    DateTime OrderDate, long CustomerId, string? CustomerOrderReference,
    long? BillingAddressId, long? ShippingAddressId, long? PriceListId, long? PaymentTermId,
    long? WarehouseId, long? CurrencyId, long? CarrierId, long? PortTypeId, long? DocumentTypeId,
    decimal ExchangeRate, decimal HeaderDiscountPercent, DateTime? ExpectedDeliveryDate, string? Notes,
    IReadOnlyList<SaveOrderLineRequest> Lines);

public record SaveSalesOrderRequest(
    DateTime OrderDate, string? CustomerOrderReference,
    long? BillingAddressId, long? ShippingAddressId, long? PriceListId, long? PaymentTermId,
    long? WarehouseId, long? CurrencyId, long? CarrierId, long? PortTypeId, long? DocumentTypeId,
    decimal ExchangeRate, decimal HeaderDiscountPercent, int Status,
    DateTime? ExpectedDeliveryDate, string? Notes,
    IReadOnlyList<SaveOrderLineRequest> Lines);

public record CreatePurchaseOrderRequest(
    DateTime OrderDate, long SupplierId, string? SupplierOrderReference,
    long? DeliveryAddressId, long? PaymentTermId, long? WarehouseId, long? CurrencyId,
    long? DocumentTypeId, long? PurchaseRequestId,
    decimal ExchangeRate, decimal HeaderDiscountPercent, DateTime? ExpectedDeliveryDate, string? Notes,
    IReadOnlyList<SaveOrderLineRequest> Lines);

public record SavePurchaseOrderRequest(
    DateTime OrderDate, string? SupplierOrderReference,
    long? DeliveryAddressId, long? PaymentTermId, long? WarehouseId, long? CurrencyId,
    long? DocumentTypeId,
    decimal ExchangeRate, decimal HeaderDiscountPercent, int Status,
    DateTime? ExpectedDeliveryDate, string? Notes,
    IReadOnlyList<SaveOrderLineRequest> Lines);
