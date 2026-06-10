using System.Text.Json;
using KBM.Application.Security;
using KBM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace KBM.Infrastructure.Persistence.Audit;

public sealed class AuditSaveChangesInterceptor(ICurrentUserContext currentUser) : SaveChangesInterceptor
{
    private static readonly HashSet<string> AuditedTypes =
    [
        nameof(User), nameof(Company), nameof(Role), nameof(UserRole), nameof(UserCompany),
        nameof(Customer), nameof(CustomerAddress), nameof(CustomerContact), nameof(CustomerBank),
        nameof(Supplier), nameof(SupplierAddress), nameof(SupplierContact), nameof(SupplierBank),
        nameof(Item), nameof(ItemCategory),
        nameof(PurchaseRequest), nameof(PurchaseRequestLine), nameof(PurchaseRequestLineSupplier),
        nameof(Rfq), nameof(RfqLine)
    ];

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is KbmDbContext db)
            await WriteAuditEntriesAsync(db, cancellationToken);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private Task WriteAuditEntriesAsync(KbmDbContext db, CancellationToken ct)
    {
        var entries = db.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => AuditedTypes.Contains(e.Entity.GetType().Name))
            .ToList();

        if (entries.Count == 0) return Task.CompletedTask;

        var now = DateTime.UtcNow;
        foreach (var entry in entries)
        {
            var action = entry.State switch
            {
                EntityState.Added => "Create",
                EntityState.Modified => "Update",
                EntityState.Deleted => "Delete",
                _ => "Unknown"
            };

            string? oldValue = null;
            string? newValue = null;
            long? entityId = null;

            if (entry.State == EntityState.Added)
            {
                newValue = JsonSerializer.Serialize(entry.CurrentValues.ToObject());
            }
            else if (entry.State == EntityState.Deleted)
            {
                oldValue = JsonSerializer.Serialize(entry.OriginalValues.ToObject());
                entityId = entry.Property("Id").CurrentValue as long?;
            }
            else
            {
                var changes = entry.Properties
                    .Where(p => p.IsModified)
                    .ToDictionary(p => p.Metadata.Name, p => new { old = p.OriginalValue, @new = p.CurrentValue });
                oldValue = JsonSerializer.Serialize(changes.ToDictionary(k => k.Key, k => k.Value.old));
                newValue = JsonSerializer.Serialize(changes.ToDictionary(k => k.Key, k => k.Value.@new));
                entityId = entry.Property("Id").CurrentValue as long?;
            }

            entityId ??= entry.Property("Id").CurrentValue as long?;

            db.AuditLogs.Add(new AuditLog
            {
                Timestamp = now,
                UserId = currentUser.UserId,
                CompanyId = currentUser.CompanyId,
                Action = action,
                EntityType = entry.Entity.GetType().Name,
                EntityId = entityId,
                OldValue = oldValue,
                NewValue = newValue,
                IpAddress = currentUser.IpAddress,
                UserAgent = currentUser.UserAgent,
                CorrelationId = currentUser.CorrelationId
            });
        }

        return Task.CompletedTask;
    }
}
