namespace KBM.Application.Accounting;

/// <summary>Piano dei conti (sistema dei mastri: Mastro &#8594; Conto &#8594; Sottoconto).</summary>
public interface IChartAccountService
{
    /// <summary>Elenco gerarchico ordinato per codice completo.</summary>
    Task<IReadOnlyList<ChartAccountListItem>> ListAsync(string? search = null, bool postableOnly = false, CancellationToken ct = default);
    Task<ChartAccountDetail?> GetAsync(long id, CancellationToken ct = default);
    Task<ChartAccountDetail> CreateAsync(CreateChartAccountRequest request, CancellationToken ct = default);
    Task<ChartAccountDetail?> UpdateAsync(long id, UpdateChartAccountRequest request, CancellationToken ct = default);
    Task<bool> SetEnabledAsync(long id, bool enabled, CancellationToken ct = default);

    /// <summary>Crea un piano dei conti standard italiano se l'azienda non ne ha ancora. Restituisce il numero di voci create.</summary>
    Task<int> SeedStandardAsync(CancellationToken ct = default);
}
