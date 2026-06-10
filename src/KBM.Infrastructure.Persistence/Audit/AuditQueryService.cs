using KBM.Application.Audit;
using KBM.Application.Security;
using KBM.Application.Users;
using Microsoft.EntityFrameworkCore;

namespace KBM.Infrastructure.Persistence.Audit;

public sealed class AuditQueryService(KbmDbContext db, ICurrentUserContext currentUser) : IAuditQueryService
{
    public async Task<PagedResult<AuditLogItem>> ListAsync(AuditListQuery query, CancellationToken ct = default)
    {
        var companyId = currentUser.CompanyId
            ?? throw new InvalidOperationException("Contesto azienda mancante.");

        var q = db.AuditLogs.Where(a => a.CompanyId == companyId || a.CompanyId == null);

        if (query.From.HasValue) q = q.Where(a => a.Timestamp >= query.From.Value);
        if (query.To.HasValue) q = q.Where(a => a.Timestamp <= query.To.Value);
        if (query.UserId.HasValue) q = q.Where(a => a.UserId == query.UserId);
        if (!string.IsNullOrWhiteSpace(query.Action)) q = q.Where(a => a.Action == query.Action);
        if (!string.IsNullOrWhiteSpace(query.EntityType)) q = q.Where(a => a.EntityType == query.EntityType);

        var total = await q.CountAsync(ct);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var items = await q
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogItem(
                a.Id, a.Timestamp, a.UserId, a.CompanyId, a.Action,
                a.EntityType, a.EntityId, a.IpAddress))
            .ToListAsync(ct);

        return new PagedResult<AuditLogItem>(items, total, page, pageSize);
    }

    public async Task<AuditLogDetail?> GetAsync(long id, CancellationToken ct = default)
    {
        var companyId = currentUser.CompanyId
            ?? throw new InvalidOperationException("Contesto azienda mancante.");

        var log = await db.AuditLogs.FirstOrDefaultAsync(
            a => a.Id == id && (a.CompanyId == companyId || a.CompanyId == null), ct);

        return log is null ? null : new AuditLogDetail(
            log.Id, log.Timestamp, log.UserId, log.CompanyId, log.Action,
            log.EntityType, log.EntityId, log.OldValue, log.NewValue,
            log.IpAddress, log.UserAgent, log.CorrelationId);
    }
}
