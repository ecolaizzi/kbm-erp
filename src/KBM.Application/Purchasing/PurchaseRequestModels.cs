namespace KBM.Application.Purchasing;

public record PurchaseRequestListItem(
    long Id, string Number, DateTime Date, string? RequestingUnit, int LineCount, string Status);

public record PurchaseRequestLineDto(
    long Id, long? ItemId, string? ItemCode, string Description, decimal Quantity, string? UnitOfMeasure,
    DateTime? RequiredDate, decimal? ProposedPrice, string LineStatus, IReadOnlyList<long> SupplierIds);

public record PurchaseRequestDetail(
    long Id, string Number, DateTime Date, string? RequestingUnit, string Status, string? Notes,
    IReadOnlyList<PurchaseRequestLineDto> Lines);

public record SavePurchaseRequestLine(
    long Id, long? ItemId, string Description, decimal Quantity, string? UnitOfMeasure,
    DateTime? RequiredDate, decimal? ProposedPrice, string LineStatus, IReadOnlyList<long> SupplierIds);

public record CreatePurchaseRequestRequest(
    DateTime Date, string? RequestingUnit, string? Notes, IReadOnlyList<SavePurchaseRequestLine> Lines);

public record SavePurchaseRequestRequest(
    DateTime Date, string? RequestingUnit, string Status, string? Notes, IReadOnlyList<SavePurchaseRequestLine> Lines);
