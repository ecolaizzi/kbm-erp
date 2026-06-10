using KBM.Domain.Common;

namespace KBM.Domain.Entities;

/// <summary>Referente/contatto di un fornitore.</summary>
public class SupplierContact : AuditableTenantEntity
{
    public long SupplierId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Notes { get; set; }
    public bool IsPrimary { get; set; }

    public Supplier Supplier { get; set; } = null!;
}
