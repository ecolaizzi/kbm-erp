namespace KBM.Application.Purchasing;

public interface IPurchaseRequestService
{
    Task<IReadOnlyList<PurchaseRequestListItem>> ListAsync(string? search = null, CancellationToken ct = default);
    Task<PurchaseRequestDetail?> GetAsync(long id, CancellationToken ct = default);
    Task<PurchaseRequestDetail> CreateAsync(CreatePurchaseRequestRequest request, CancellationToken ct = default);
    Task<PurchaseRequestDetail?> SaveAsync(long id, SavePurchaseRequestRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(long id, CancellationToken ct = default);
}

public interface IRfqService
{
    Task<IReadOnlyList<RfqListItem>> ListAsync(string? search = null, CancellationToken ct = default);
    Task<RfqDetail?> GetAsync(long id, CancellationToken ct = default);
    Task<RfqDetail> CreateAsync(CreateRfqRequest request, CancellationToken ct = default);
    Task<RfqDetail> CreateFromPurchaseRequestAsync(CreateRfqFromRequest request, CancellationToken ct = default);
    Task<RfqDetail?> SaveAsync(long id, SaveRfqRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(long id, CancellationToken ct = default);
}
