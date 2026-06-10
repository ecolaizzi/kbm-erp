using KBM.Domain.Common;

namespace KBM.Domain.Entities;

/// <summary>
/// Voce del piano dei conti (sistema dei mastri stile NTS Business).
/// Gerarchia auto-referenziata: Mastro &#8594; Conto &#8594; Sottoconto.
/// Natura e segno si propagano a cascata; le registrazioni avvengono solo sui sottoconti.
/// </summary>
public class AccountMaster : AuditableTenantEntity
{
    public AccountLevel Level { get; set; } = AccountLevel.Mastro;

    public long? ParentId { get; set; }
    public AccountMaster? Parent { get; set; }
    public ICollection<AccountMaster> Children { get; set; } = [];

    /// <summary>Codice del segmento (proprio del livello).</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Codice gerarchico completo (mastro.conto.sottoconto), univoco per azienda.</summary>
    public string FullCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public AccountNature Nature { get; set; } = AccountNature.Patrimoniale;
    public AccountSign Sign { get; set; } = AccountSign.Dare;
    public AccountSubKind SubKind { get; set; } = AccountSubKind.Standard;

    /// <summary>True solo per i sottoconti: livello su cui sono ammesse le registrazioni.</summary>
    public bool AllowsPosting { get; set; }

    /// <summary>Codici di riclassificazione bilancio CEE (saldo dare / avere).</summary>
    public string? BilCeeDare { get; set; }
    public string? BilCeeAvere { get; set; }

    public string Status { get; set; } = "Active";
}
