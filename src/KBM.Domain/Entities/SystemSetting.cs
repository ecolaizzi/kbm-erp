namespace KBM.Domain.Entities;

/// <summary>
/// Impostazione chiave/valore. CompanyId null = impostazione globale (tecnica);
/// valorizzato = configurazione specifica dell'azienda utilizzatrice.
/// Raggiungibile solo dalla modalita sviluppatore (gesture + permesso).
/// </summary>
public class SystemSetting
{
    public long Id { get; set; }
    public long? CompanyId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; }
}
