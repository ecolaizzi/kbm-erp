namespace KBM.Client.Services;

/// <summary>Voce di navigazione (funzione) di un modulo. Code = mnemonico digitabile (stile NTS, es. ANACL).</summary>
public sealed record NavFeature(string Key, string Label, string Glyph, string Description = "", bool Enabled = true, string Code = "");

/// <summary>Gruppo/modulo di navigazione con le sue funzioni.</summary>
public sealed record NavGroup(string Key, string Label, string Glyph, IReadOnlyList<NavFeature> Features, bool Expanded = false);

/// <summary>
/// Registro centrale dei moduli e funzioni dell'applicazione.
/// Punto unico per aggiungere nuovi moduli: lo shell costruisce
/// l'albero di navigazione, le home di modulo e i breadcrumb da qui.
/// </summary>
public static class NavigationRegistry
{
    public const string ModulePrefix = "module:";

    public static readonly NavFeature Home = new("home", "Home", "\uE80F", "Preferiti, recenti e moduli", Code: "HOME");
    public static readonly NavFeature Dashboard = new("dashboard", "Dashboard", "\uE9D9", "Indicatori e riepilogo", Code: "DASH");

    public static IReadOnlyList<NavGroup> Groups { get; } = new[]
    {
        new NavGroup("amministrazione", "Amministrazione", "\uE8F1", new[]
        {
            new NavFeature("users", "Utenti", "\uE716", "Gestione utenti, accessi e credenziali", Code: "UTE"),
            new NavFeature("companies", "Aziende", "\uE825", "Anagrafica aziende e multi-azienda", Code: "AZI"),
            new NavFeature("roles", "Ruoli e permessi", "\uE72E", "Profili autorizzativi e permessi (RBAC)", Code: "RUO"),
            new NavFeature("audit", "Audit Log", "\uE7BA", "Tracciamento operazioni e sicurezza", Enabled: false, Code: "AUD"),
        }, Expanded: true),

        new NavGroup("anagrafiche", "Anagrafiche", "\uE8F1", new[]
        {
            new NavFeature("customers", "Clienti", "\uE77B", "Anagrafica clienti e condizioni", Code: "ANACL"),
            new NavFeature("suppliers", "Fornitori", "\uE7EE", "Anagrafica fornitori e condizioni di acquisto", Code: "ANAFO"),
            new NavFeature("items", "Articoli", "\uE7B8", "Anagrafica articoli e categorie", Code: "ANART"),
        }),

        new NavGroup("tabelle", "Tabelle e archivi", "\uE8FD", new[]
        {
            new NavFeature("payment-terms", "Condizioni di pagamento", "\uE8C7", "Rate, scadenze e scadenziere", Code: "PAG"),
            new NavFeature("vat-codes", "Codici IVA", "\uE9D2", "Aliquote, nature esenzione, indeducibilit\u00e0", Code: "IVA"),
            new NavFeature("warehouses", "Magazzini", "\uE7B8", "Depositi e terzisti", Code: "MAG"),
            new NavFeature("units", "Unit\u00e0 di misura", "\uE9E9", "UM e fattori di conversione", Enabled: false, Code: "UM"),
            new NavFeature("zones", "Zone", "\uE909", "Aree commerciali e geografiche", Enabled: false, Code: "ZONE"),
        }),

        new NavGroup("acquisti", "Acquisti", "\uE7BD", new[]
        {
            new NavFeature("purchase-requests", "Richieste di acquisto", "\uE7C1", "RDA: fabbisogni e materiali da acquistare", Code: "RDA"),
            new NavFeature("rfqs", "Richieste di offerta", "\uE8A5", "RDO: richieste di quotazione ai fornitori", Code: "RDO"),
        }),

        new NavGroup("workflow", "Workflow", "\uE9D5", new[]
        {
            new NavFeature("wf-console", "Consolle workflow", "\uE8FD", "I miei task e processi da gestire", Code: "WFCON"),
            new NavFeature("wf-instances", "Processi", "\uE9D9", "Processi avviati e relativo stato", Code: "WFPRO"),
            new NavFeature("wf-definitions", "Modelli", "\uE8B7", "Modelli di processo (template e step)", Code: "WFMOD"),
        }),

        new NavGroup("contabilita", "Contabilit\u00e0", "\uE8C7", new[]
        {
            new NavFeature("chart-accounts", "Piano dei conti", "\uE8FD", "Sistema dei mastri: mastro, conto, sottoconto", Code: "PDC"),
            new NavFeature("journal", "Prima nota", "\uE70F", "Registrazioni contabili in partita doppia", Enabled: false, Code: "PNOTA"),
            new NavFeature("vat-registers", "Registri IVA", "\uE9D2", "Registri IVA vendite/acquisti e liquidazione", Enabled: false, Code: "REGIVA"),
        }),

        new NavGroup("ordini", "Ordini", "\uE7BF", new[]
        {
            new NavFeature("sales-orders", "Ordini cliente", "\uE7BF", "OV: ordini di vendita con evasione per riga", Code: "OV"),
            new NavFeature("purchase-orders", "Ordini fornitore", "\uE7BD", "ODA: ordini di acquisto (anche da RDA)", Code: "ODA"),
            new NavFeature("pricelists", "Listini", "\uE719", "Archivio listini vendita/acquisto", Code: "LIST"),
        }),

        new NavGroup("commerciale", "Commerciale", "\uE8F1", new[]
        {
            new NavFeature("ddt", "DDT / Bolle", "\uE8A5", "Documenti di trasporto", Enabled: false, Code: "DDT"),
            new NavFeature("invoices", "Fatturazione", "\uE8C7", "Fatture attive e passive", Enabled: false, Code: "FT"),
            new NavFeature("stock", "Giacenze", "\uE7B8", "Disponibilit\u00e0 e movimenti magazzino", Enabled: false, Code: "GIA"),
        }),
    };

    private static readonly Dictionary<string, NavFeature> _byKey = BuildIndex();
    private static readonly Dictionary<string, NavGroup> _groupsByKey =
        Groups.ToDictionary(g => g.Key, g => g);

    private static Dictionary<string, NavFeature> BuildIndex()
    {
        var map = new Dictionary<string, NavFeature> { [Home.Key] = Home, [Dashboard.Key] = Dashboard };
        foreach (var g in Groups)
            foreach (var f in g.Features)
                map[f.Key] = f;
        return map;
    }

    public static NavFeature? Find(string key) => _byKey.GetValueOrDefault(key);

    public static NavGroup? FindGroup(string key) =>
        _groupsByKey.GetValueOrDefault(key.StartsWith(ModulePrefix) ? key[ModulePrefix.Length..] : key);

    /// <summary>Tutte le funzioni (incluse Home e Dashboard).</summary>
    public static IEnumerable<NavFeature> AllFeatures()
    {
        yield return Home;
        yield return Dashboard;
        foreach (var g in Groups)
            foreach (var f in g.Features)
                yield return f;
    }

    /// <summary>Trova una funzione abilitata dal codice mnemonico (case-insensitive).</summary>
    public static NavFeature? FindByCode(string code) =>
        string.IsNullOrWhiteSpace(code)
            ? null
            : AllFeatures().FirstOrDefault(f => f.Enabled && f.Code.Equals(code.Trim(), StringComparison.OrdinalIgnoreCase));

    public static string GlyphFor(string key)
    {
        if (key.StartsWith(ModulePrefix)) return FindGroup(key)?.Glyph ?? "\uE8A5";
        return Find(key)?.Glyph ?? "\uE8A5";
    }

    public static string LabelFor(string key)
    {
        if (key.StartsWith(ModulePrefix)) return FindGroup(key)?.Label ?? key;
        return Find(key)?.Label ?? key;
    }
}
