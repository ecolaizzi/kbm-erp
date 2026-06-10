using KBM.Domain.Common;

namespace KBM.Domain.Entities;

/// <summary>Referente/contatto di un cliente (contatti multipli).</summary>
public class CustomerContact : AuditableTenantEntity
{
    public long CustomerId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Role { get; set; }       // ruolo/funzione
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Notes { get; set; }
    public bool IsPrimary { get; set; }

    public Customer Customer { get; set; } = null!;
}
