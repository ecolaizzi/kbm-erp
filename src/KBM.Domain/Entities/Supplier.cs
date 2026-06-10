using KBM.Domain.Common;

namespace KBM.Domain.Entities;

/// <summary>Anagrafica fornitore (parità funzionale con il cliente, condizioni di acquisto).</summary>
public class Supplier : AuditableTenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;

    // Dati fiscali italiani
    public string? VatNumber { get; set; }
    public string? FiscalCode { get; set; }
    public string? SdiCode { get; set; }
    public string? PecEmail { get; set; }

    // Contatti
    public string? Email { get; set; }
    public string? Phone { get; set; }

    // Sede legale
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "IT";

    // Condizioni di acquisto / bancarie
    public string? Iban { get; set; }
    public string? PaymentTerms { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PriceListCode { get; set; }       // listino acquisto
    public string? Zone { get; set; }
    public decimal? CreditLimit { get; set; }
    public decimal? DiscountPercent { get; set; }
    public bool SplitPayment { get; set; }
    public bool WithholdingTax { get; set; }
    public string? VatExemptionCode { get; set; }
    public string? AccountCode { get; set; }

    public string? Notes { get; set; }
    public string Status { get; set; } = "Active";

    public ICollection<SupplierAddress> Addresses { get; set; } = [];
    public ICollection<SupplierContact> Contacts { get; set; } = [];
    public ICollection<SupplierBank> Banks { get; set; } = [];
}
