namespace KBM.Application.Orders;

public interface IVatCodeService
{
    Task<IReadOnlyList<VatCodeListItem>> ListAsync(string? search = null, CancellationToken ct = default);
    Task<VatCodeDetail?> GetAsync(long id, CancellationToken ct = default);
    Task<VatCodeDetail> CreateAsync(SaveVatCodeRequest request, CancellationToken ct = default);
    Task<VatCodeDetail?> UpdateAsync(long id, SaveVatCodeRequest request, CancellationToken ct = default);
    Task<bool> SetEnabledAsync(long id, bool enabled, CancellationToken ct = default);
    Task<int> SeedStandardAsync(CancellationToken ct = default);
}

public interface IOrderLookupService
{
    Task<IReadOnlyList<LookupListItem>> ListUnitsAsync(string? search = null, CancellationToken ct = default);
    Task<LookupListItem> CreateUnitAsync(SaveLookupRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<LookupListItem>> ListZonesAsync(string? search = null, CancellationToken ct = default);
    Task<LookupListItem> CreateZoneAsync(SaveLookupRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<LookupListItem>> ListCarriersAsync(string? search = null, CancellationToken ct = default);
    Task<LookupListItem> CreateCarrierAsync(SaveLookupRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<LookupListItem>> ListPortTypesAsync(string? search = null, CancellationToken ct = default);
    Task<LookupListItem> CreatePortTypeAsync(SaveLookupRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<LookupListItem>> ListDocumentTypesAsync(int? category = null, CancellationToken ct = default);
    Task<LookupListItem> CreateDocumentTypeAsync(SaveLookupRequest request, int category, CancellationToken ct = default);
    Task<IReadOnlyList<LookupListItem>> ListCurrenciesAsync(CancellationToken ct = default);
    Task<LookupListItem> CreateCurrencyAsync(SaveLookupRequest request, string symbol, bool isDefault, CancellationToken ct = default);
    Task<int> SeedLookupsAsync(CancellationToken ct = default);
}

public interface IWarehouseService
{
    Task<IReadOnlyList<WarehouseListItem>> ListAsync(string? search = null, CancellationToken ct = default);
    Task<WarehouseDetail?> GetAsync(long id, CancellationToken ct = default);
    Task<WarehouseDetail> CreateAsync(SaveWarehouseRequest request, CancellationToken ct = default);
    Task<WarehouseDetail?> UpdateAsync(long id, SaveWarehouseRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<WarehouseReasonListItem>> ListReasonsAsync(CancellationToken ct = default);
    Task<WarehouseReasonListItem> CreateReasonAsync(SaveWarehouseReasonRequest request, CancellationToken ct = default);
    Task<int> SeedStandardAsync(CancellationToken ct = default);
}

public interface IPriceListService
{
    Task<IReadOnlyList<PriceListListItem>> ListAsync(string? search = null, int? kind = null, CancellationToken ct = default);
    Task<PriceListDetail?> GetAsync(long id, CancellationToken ct = default);
    Task<PriceListDetail> CreateAsync(CreatePriceListRequest request, CancellationToken ct = default);
    Task<PriceListDetail?> SaveAsync(long id, SavePriceListRequest request, CancellationToken ct = default);
}

public interface ISalesOrderService
{
    Task<IReadOnlyList<SalesOrderListItem>> ListAsync(string? search = null, CancellationToken ct = default);
    Task<SalesOrderDetail?> GetAsync(long id, CancellationToken ct = default);
    Task<SalesOrderDetail> CreateAsync(CreateSalesOrderRequest request, CancellationToken ct = default);
    Task<SalesOrderDetail?> SaveAsync(long id, SaveSalesOrderRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(long id, CancellationToken ct = default);
}

public interface IPurchaseOrderService
{
    Task<IReadOnlyList<PurchaseOrderListItem>> ListAsync(string? search = null, CancellationToken ct = default);
    Task<PurchaseOrderDetail?> GetAsync(long id, CancellationToken ct = default);
    Task<PurchaseOrderDetail> CreateAsync(CreatePurchaseOrderRequest request, CancellationToken ct = default);
    Task<PurchaseOrderDetail?> SaveAsync(long id, SavePurchaseOrderRequest request, CancellationToken ct = default);
    Task<PurchaseOrderDetail?> CreateFromPurchaseRequestAsync(long purchaseRequestId, CancellationToken ct = default);
    Task<bool> DeleteAsync(long id, CancellationToken ct = default);
}
