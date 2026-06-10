using System.IO;
using System.Text.Json;

namespace KBM.Client.Services;

/// <summary>
/// Persistenza leggera di preferenze UI (toolbar configurabile, wings) in %AppData%/KBM.
/// </summary>
public static class UiSettings
{
    private static readonly string Dir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KBM");

    private static string PathFor(string key) => Path.Combine(Dir, $"{key}.json");

    public static Dictionary<string, bool> LoadFlags(string key)
    {
        try
        {
            var file = PathFor(key);
            if (!File.Exists(file)) return new();
            var json = File.ReadAllText(file);
            return JsonSerializer.Deserialize<Dictionary<string, bool>>(json) ?? new();
        }
        catch
        {
            return new();
        }
    }

    public static void SaveFlags(string key, Dictionary<string, bool> flags)
    {
        try
        {
            Directory.CreateDirectory(Dir);
            File.WriteAllText(PathFor(key), JsonSerializer.Serialize(flags));
        }
        catch
        {
            // best-effort
        }
    }

    public static List<string> LoadList(string key)
    {
        try
        {
            var file = PathFor(key);
            if (!File.Exists(file)) return new();
            return JsonSerializer.Deserialize<List<string>>(File.ReadAllText(file)) ?? new();
        }
        catch
        {
            return new();
        }
    }

    public static void SaveList(string key, List<string> items)
    {
        try
        {
            Directory.CreateDirectory(Dir);
            File.WriteAllText(PathFor(key), JsonSerializer.Serialize(items));
        }
        catch
        {
            // best-effort
        }
    }

    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

    /// <summary>Carica un oggetto tipizzato (es. layout griglia); null se assente o errore.</summary>
    public static T? Load<T>(string key) where T : class
    {
        try
        {
            var file = PathFor(key);
            if (!File.Exists(file)) return null;
            return JsonSerializer.Deserialize<T>(File.ReadAllText(file));
        }
        catch
        {
            return null;
        }
    }

    public static void Save<T>(string key, T value) where T : class
    {
        try
        {
            Directory.CreateDirectory(Dir);
            File.WriteAllText(PathFor(key), JsonSerializer.Serialize(value, JsonOpts));
        }
        catch
        {
            // best-effort
        }
    }
}
