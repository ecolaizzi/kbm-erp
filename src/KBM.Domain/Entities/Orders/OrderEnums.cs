namespace KBM.Domain.Entities.Orders;

public enum WarehouseKind
{
    Own = 0,
    ThirdParty = 1
}

public enum StockMovementSign
{
    In = 1,
    Out = -1
}

public enum DocumentCategory
{
    SalesOrder = 1,
    PurchaseOrder = 2,
    DeliveryNote = 3,
    Invoice = 4,
    Transfer = 5
}

public enum PortCharge
{
    Franco = 0,
    Assegnato = 1
}

public enum PriceListKind
{
    Sales = 1,
    Purchase = 2
}

public enum OrderDocumentStatus
{
    Draft = 0,
    Confirmed = 1,
    PartiallyFulfilled = 2,
    Fulfilled = 3,
    Invoiced = 4,
    Cancelled = 5
}

public enum OrderLineStatus
{
    Open = 0,
    PartiallyFulfilled = 1,
    Closed = 2,
    Cancelled = 3
}
