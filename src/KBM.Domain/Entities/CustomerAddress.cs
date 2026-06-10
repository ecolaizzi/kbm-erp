using KBM.Domain.Common;

namespace KBM.Domain.Entities;

/// <summary>Indirizzo aggiuntivo / destinazione diversa di un cliente (stile NTS "Destinazioni diverse").</summary>
public class CustomerAddress : AuditableTenantEntity
{
    public long CustomerId { get; set; }

    public string AddressType { get; set; } = "Shipping"; // Legal, Shipping, Billing, Other
    public string Description { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "IT";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsDefault { get; set; }

    public Customer Customer { get; set; } = null!;
}
