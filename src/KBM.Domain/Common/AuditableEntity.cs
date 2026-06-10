namespace KBM.Domain.Common;

public abstract class AuditableEntity
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public long CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

public abstract class AuditableTenantEntity : AuditableEntity
{
    public long CompanyId { get; set; }
}
