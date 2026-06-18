using Core.Configuration;
using Core.Helpers;

namespace Business.Data;

public static class WidgetTypesProvider
{
    private static readonly Lazy<IReadOnlyDictionary<string, string>> _map = new(Load);

    /// <summary>
    /// UI labels in the active language — use these to interact with and verify the UI.
    /// </summary>
    public static IReadOnlyList<string> All => _map.Value.Values.ToList();

    /// <summary>
    /// Full key → label map. Key is language-neutral; value is the UI text in the active locale.
    /// Use the key to look up a specific widget label regardless of language.
    /// </summary>
    public static IReadOnlyDictionary<string, string> Map => _map.Value;

    public static string Label(string key) =>
        _map.Value.TryGetValue(key, out var label)
            ? label
            : throw new KeyNotFoundException($"Widget key '{key}' not found in '{AppConfiguration.WidgetTypesFile}'.");

    private static IReadOnlyDictionary<string, string> Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, AppConfiguration.WidgetTypesFile);
        return JsonReader.Read<IReadOnlyDictionary<string, string>>(path);
    }
}
