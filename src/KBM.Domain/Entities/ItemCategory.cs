using KBM.Domain.Common;

namespace KBM.Domain.Entities;

/// <summary>Categoria merceologica articoli.</summary>
public class ItemCategory : AuditableTenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Active";

    public ICollection<Item> Items { get; set; } = [];
}
