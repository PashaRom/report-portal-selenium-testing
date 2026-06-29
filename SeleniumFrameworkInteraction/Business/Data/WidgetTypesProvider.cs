using Core.Configuration;
using Core.DI;
using Core.Helpers;

namespace Business.Data;

public static class WidgetTypesProvider
{
    private static IAppConfiguration Configuration => ServiceLocator.GetService<IAppConfiguration>();

    private static readonly Lazy<IReadOnlyDictionary<string, string>> _map = new(Load);

    /// <summary>
    /// UI labels in the active language — use these to interact with and verify the UI.
    /// </summary>
    public static IReadOnlyList<string> GetAllWidgets() => _map.Value.Values.ToList();

    /// <summary>
    /// Full key → label map. Key is language-neutral; value is the UI text in the active locale.
    /// Use the key to look up a specific widget label regardless of language.
    /// </summary>
    public static IReadOnlyDictionary<string, string> Map => _map.Value;

    public static string Label(string key) =>
        _map.Value.TryGetValue(key, out var label)
            ? label
            : throw new KeyNotFoundException($"Widget key '{key}' not found.");

    private static IReadOnlyDictionary<string, string> Load()
    {
        var path = Path.Combine(Configuration.TestDataDirectory, "Templates", "widget_types.en.json");
        return JsonReader.Read<IReadOnlyDictionary<string, string>>(path);
    }
}
