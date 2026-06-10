using KBM.Domain.Common;

namespace KBM.Domain.Entities;

/// <summary>Articolo di magazzino/vendita.</summary>
public class Item : AuditableTenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public long? CategoryId { get; set; }
    public ItemCategory? Category { get; set; }

    public string UnitOfMeasure { get; set; } = "NR";   // UM principale
    public string? Barcode { get; set; }                 // EAN
    public string? SupplierItemCode { get; set; }        // codice articolo fornitore

    // Dati commerciali
    public decimal BasePrice { get; set; }
    public decimal VatRate { get; set; } = 22m;
    public string? RevenueAccount { get; set; }          // conto ricavi
    public string? CostAccount { get; set; }             // conto costi

    public string? Notes { get; set; }
    public string Status { get; set; } = "Active";
}
