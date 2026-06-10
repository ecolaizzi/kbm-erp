using KBM.Application.Users;

namespace KBM.Application.Audit;

public interface IAuditQueryService
{
    Task<PagedResult<AuditLogItem>> ListAsync(AuditListQuery query, CancellationToken ct = default);
    Task<AuditLogDetail?> GetAsync(long id, CancellationToken ct = default);
}
