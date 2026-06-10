using KBM.Domain.Common;

namespace KBM.Domain.Entities;

/// <summary>Coordinate bancarie di un fornitore.</summary>
public class SupplierBank : AuditableTenantEntity
{
    public long SupplierId { get; set; }

    public string BankName { get; set; } = string.Empty;
    public string? Iban { get; set; }
    public string? Swift { get; set; }
    public string? Abi { get; set; }
    public string? Cab { get; set; }
    public bool IsDefault { get; set; }

    public Supplier Supplier { get; set; } = null!;
}
