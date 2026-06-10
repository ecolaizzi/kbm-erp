namespace KBM.Domain.Entities;

/// <summary>Livello gerarchico del piano dei conti (stile NTS: mastro &#8594; conto &#8594; sottoconto).</summary>
public enum AccountLevel
{
    Mastro = 1,
    Conto = 2,
    Sottoconto = 3
}

/// <summary>Natura/tipo del conto (si propaga a cascata dal mastro ai livelli inferiori).</summary>
public enum AccountNature
{
    Patrimoniale = 1,
    Economico = 2,
    Ordine = 3
}

/// <summary>Segno di bilancio: Dare = attivo/costo, Avere = passivo/ricavo.</summary>
public enum AccountSign
{
    Dare = 1,
    Avere = 2
}

/// <summary>Tipologia operativa del conto (mastro intestabile o sottoconto specifico).</summary>
public enum AccountSubKind
{
    Standard = 0,
    Cliente = 1,
    Fornitore = 2,
    Banca = 3,
    Cassa = 4,
    Iva = 5,
    Ritenuta = 6
}
