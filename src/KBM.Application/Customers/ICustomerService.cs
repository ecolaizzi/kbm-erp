namespace KBM.Application.Customers;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerListItem>> ListAsync(string? search = null, CancellationToken ct = default);
    Task<CustomerDetail?> GetAsync(long id, CancellationToken ct = default);
    Task<CustomerDetail> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default);
    Task<CustomerDetail?> UpdateAsync(long id, UpdateCustomerRequest request, CancellationToken ct = default);
    Task<bool> SetEnabledAsync(long id, bool enabled, CancellationToken ct = default);

    Task<CustomerAggregate?> GetFullAsync(long id, CancellationToken ct = default);
    Task<CustomerAggregate> CreateFullAsync(CreateCustomerAggregateRequest request, CancellationToken ct = default);
    Task<CustomerAggregate?> SaveFullAsync(long id, SaveCustomerAggregateRequest request, CancellationToken ct = default);
}
