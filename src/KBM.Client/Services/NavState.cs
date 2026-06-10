namespace KBM.Client.Services;

/// <summary>
/// Stato di navigazione persistito: preferiti e funzioni recenti.
/// Memorizza chiavi di funzione (es. "users") gestite da <see cref="NavigationRegistry"/>.
/// </summary>
public static class NavState
{
    private const string FavKey = "nav-favorites";
    private const string RecentKey = "nav-recent";
    private const int MaxRecent = 8;

    private static List<string> _favorites = UiSettings.LoadList(FavKey);
    private static List<string> _recent = UiSettings.LoadList(RecentKey);

    public static event Action? Changed;

    public static IReadOnlyList<string> Favorites => _favorites;
    public static IReadOnlyList<string> Recent => _recent;

    public static bool IsFavorite(string key) => _favorites.Contains(key);

    public static void ToggleFavorite(string key)
    {
        if (!_favorites.Remove(key))
            _favorites.Add(key);
        UiSettings.SaveList(FavKey, _favorites);
        Changed?.Invoke();
    }

    public static void TrackRecent(string key)
    {
        // Solo funzioni reali (non le home/dashboard né le home di modulo)
        if (key is "home" or "dashboard") return;
        if (key.StartsWith(NavigationRegistry.ModulePrefix)) return;
        if (NavigationRegistry.Find(key) is null) return;

        _recent.Remove(key);
        _recent.Insert(0, key);
        if (_recent.Count > MaxRecent)
            _recent = _recent.Take(MaxRecent).ToList();
        UiSettings.SaveList(RecentKey, _recent);
        Changed?.Invoke();
    }
}
