namespace KBM.Application.Items;

public record ItemCategoryListItem(long Id, string Code, string Name, string Status);
public record ItemCategoryDetail(long Id, string Code, string Name, string? Description, string Status);
public record CreateItemCategoryRequest(string Code, string Name, string? Description);
public record UpdateItemCategoryRequest(string Name, string? Description, string Status);

public record ItemListItem(
    long Id,
    string Code,
    string Description,
    string? CategoryName,
    string UnitOfMeasure,
    decimal BasePrice,
    decimal VatRate,
    string Status);

public record ItemDetail(
    long Id,
    string Code,
    string Description,
    long? CategoryId,
    string UnitOfMeasure,
    string? Barcode,
    string? SupplierItemCode,
    decimal BasePrice,
    decimal VatRate,
    string? RevenueAccount,
    string? CostAccount,
    string? Notes,
    string Status);

public record CreateItemRequest(
    string Code,
    string Description,
    long? CategoryId,
    string UnitOfMeasure,
    string? Barcode,
    string? SupplierItemCode,
    decimal BasePrice,
    decimal VatRate,
    string? RevenueAccount,
    string? CostAccount,
    string? Notes);

public record UpdateItemRequest(
    string Description,
    long? CategoryId,
    string UnitOfMeasure,
    string? Barcode,
    string? SupplierItemCode,
    decimal BasePrice,
    decimal VatRate,
    string? RevenueAccount,
    string? CostAccount,
    string? Notes,
    string Status);
