namespace KBM.Application.Purchasing;

public record RfqListItem(
    long Id, string Number, DateTime Date, long SupplierId, string SupplierName, int LineCount, string Status);

public record RfqLineDto(
    long Id, long? ItemId, string? ItemCode, string Description, decimal Quantity, string? UnitOfMeasure,
    decimal? UnitPrice, decimal? DiscountPercent, bool Available, string? Notes);

public record RfqDetail(
    long Id, string Number, DateTime Date, long SupplierId, string SupplierName,
    long? PurchaseRequestId, string? PurchaseRequestNumber, DateTime? ResponseDueDate, string Status, string? Notes,
    IReadOnlyList<RfqLineDto> Lines);

public record SaveRfqLine(
    long Id, long? ItemId, string Description, decimal Quantity, string? UnitOfMeasure,
    decimal? UnitPrice, decimal? DiscountPercent, bool Available, string? Notes);

public record CreateRfqRequest(
    DateTime Date, long SupplierId, long? PurchaseRequestId, DateTime? ResponseDueDate, string? Notes,
    IReadOnlyList<SaveRfqLine> Lines);

public record SaveRfqRequest(
    DateTime Date, long SupplierId, DateTime? ResponseDueDate, string Status, string? Notes,
    IReadOnlyList<SaveRfqLine> Lines);

public record CreateRfqFromRequest(long PurchaseRequestId, long SupplierId, DateTime? ResponseDueDate);
