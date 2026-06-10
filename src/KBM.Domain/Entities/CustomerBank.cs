using KBM.Domain.Common;

namespace KBM.Domain.Entities;

/// <summary>Coordinate bancarie di un cliente.</summary>
public class CustomerBank : AuditableTenantEntity
{
    public long CustomerId { get; set; }

    public string BankName { get; set; } = string.Empty;
    public string? Iban { get; set; }
    public string? Swift { get; set; }   // BIC/SWIFT
    public string? Abi { get; set; }
    public string? Cab { get; set; }
    public bool IsDefault { get; set; }

    public Customer Customer { get; set; } = null!;
}
