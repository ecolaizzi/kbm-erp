using KBM.Domain.Common;

namespace KBM.Domain.Entities;

/// <summary>
/// Anagrafica cliente (tenant-scoped). Dati anagrafici + fiscali italiani + condizioni base.
/// </summary>
public class Customer : AuditableTenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;

    // Dati fiscali (IT)
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

    // Condizioni commerciali / bancarie
    public string? Iban { get; set; }
    public string? PaymentTerms { get; set; }

    // Dati contabili / commerciali (stile NTS - scheda Dati contabili)
    public string? PaymentMethod { get; set; }      // modalità di pagamento
    public string? PriceListCode { get; set; }      // listino associato
    public string? AgentCode { get; set; }          // agente
    public string? Zone { get; set; }               // zona
    public decimal? CreditLimit { get; set; }       // fido
    public decimal? DiscountPercent { get; set; }   // sconto %
    public bool SplitPayment { get; set; }          // scissione pagamenti
    public bool WithholdingTax { get; set; }        // ritenuta d'acconto
    public string? VatExemptionCode { get; set; }   // codice esenzione IVA
    public string? AccountCode { get; set; }         // sottoconto contabile

    public string? Notes { get; set; }
    public string Status { get; set; } = "Active";

    public ICollection<CustomerAddress> Addresses { get; set; } = [];
    public ICollection<CustomerContact> Contacts { get; set; } = [];
    public ICollection<CustomerBank> Banks { get; set; } = [];
}
