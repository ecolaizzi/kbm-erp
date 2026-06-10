using KBM.Domain.Common;

namespace KBM.Domain.Entities;

/// <summary>
/// Richiesta di Acquisto (RDA): documento testata/righe con cui un reparto
/// comunica i materiali/lavorazioni da acquistare. Stati: Generato, EmessaRDA,
/// ConfermataRDA, EmessaRDO, Confermato, EmessoOrdine.
/// </summary>
public class PurchaseRequest : AuditableTenantEntity
{
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string? RequestingUnit { get; set; }
    public string Status { get; set; } = "Generato";
    public string? Notes { get; set; }

    public ICollection<PurchaseRequestLine> Lines { get; set; } = [];
}
