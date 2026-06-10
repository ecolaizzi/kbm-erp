namespace KBM.Application.Companies;

public interface ICompanyService
{
    Task<IReadOnlyList<CompanyListItem>> ListAsync(CancellationToken ct = default);
    Task<CompanyDetail?> GetAsync(long id, CancellationToken ct = default);
    Task<CompanyDetail> CreateAsync(CreateCompanyRequest request, CancellationToken ct = default);
    Task<CompanyDetail?> UpdateAsync(long id, UpdateCompanyRequest request, CancellationToken ct = default);
}
