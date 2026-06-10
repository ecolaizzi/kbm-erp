namespace KBM.Application.BaseTables;

/// <summary>Condizioni di pagamento (macro-area Tabelle e Archivi).</summary>
public interface IPaymentTermService
{
    Task<IReadOnlyList<PaymentTermListItem>> ListAsync(string? search = null, CancellationToken ct = default);
    Task<PaymentTermDetail?> GetAsync(long id, CancellationToken ct = default);
    Task<PaymentTermDetail> CreateAsync(CreatePaymentTermRequest request, CancellationToken ct = default);
    Task<PaymentTermDetail?> UpdateAsync(long id, UpdatePaymentTermRequest request, CancellationToken ct = default);
    Task<bool> SetEnabledAsync(long id, bool enabled, CancellationToken ct = default);
}
