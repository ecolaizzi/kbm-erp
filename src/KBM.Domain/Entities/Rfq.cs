using KBM.Domain.Common;

namespace KBM.Domain.Entities;

/// <summary>
/// Richiesta di Offerta (RDO): richiesta inviata a un fornitore per ottenere quotazioni.
/// Puo nascere da una RDA. Stati: Generato, Inviata, OffertaRicevuta, Confermato, Annullato.
/// </summary>
public class Rfq : AuditableTenantEntity
{
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;

    public long SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;

    public long? PurchaseRequestId { get; set; }
    public PurchaseRequest? PurchaseRequest { get; set; }

    public DateTime? ResponseDueDate { get; set; }
    public string Status { get; set; } = "Generato";
    public string? Notes { get; set; }

    public ICollection<RfqLine> Lines { get; set; } = [];
}
