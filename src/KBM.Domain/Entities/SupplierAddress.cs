using KBM.Domain.Common;

namespace KBM.Domain.Entities;

/// <summary>Indirizzo aggiuntivo di un fornitore.</summary>
public class SupplierAddress : AuditableTenantEntity
{
    public long SupplierId { get; set; }

    public string AddressType { get; set; } = "Shipping";
    public string Description { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "IT";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsDefault { get; set; }

    public Supplier Supplier { get; set; } = null!;
}
