namespace KBM.Client.Services;

/// <summary>Descrittore di colonna (chiave logica + titolo) usato dal dialog di personalizzazione.</summary>
public sealed record GridColumnInfo(string Key, string Title);

/// <summary>Stato persistito di una colonna in un layout di griglia (stile NTS "Personalizzazione griglie").</summary>
public sealed class GridColumnState
{
    public string Key { get; set; } = "";
    public bool Visible { get; set; } = true;
    public int Order { get; set; }
    /// <summary>Larghezza in pixel; 0 = usa default (auto/star).</summary>
    public double Width { get; set; }
}

/// <summary>Layout nominato di una griglia (es. "Standard", "Qta/Prezzo/Valore").</summary>
public sealed class GridLayout
{
    public string Name { get; set; } = "Standard";
    public List<GridColumnState> Columns { get; set; } = new();
}

/// <summary>Insieme dei layout salvati per una griglia + layout attivo.</summary>
public sealed class GridLayoutStore
{
    public string ActiveLayout { get; set; } = "Standard";
    public List<GridLayout> Layouts { get; set; } = new();
}
