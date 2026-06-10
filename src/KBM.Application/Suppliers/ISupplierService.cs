namespace KBM.Application.Suppliers;

public interface ISupplierService
{
    Task<IReadOnlyList<SupplierListItem>> ListAsync(string? search = null, CancellationToken ct = default);
    Task<SupplierDetail?> GetAsync(long id, CancellationToken ct = default);
    Task<SupplierDetail?> UpdateAsync(long id, UpdateSupplierRequest request, CancellationToken ct = default);
    Task<bool> SetEnabledAsync(long id, bool enabled, CancellationToken ct = default);

    Task<SupplierAggregate?> GetFullAsync(long id, CancellationToken ct = default);
    Task<SupplierAggregate> CreateFullAsync(CreateSupplierAggregateRequest request, CancellationToken ct = default);
    Task<SupplierAggregate?> SaveFullAsync(long id, SaveSupplierAggregateRequest request, CancellationToken ct = default);
}
