using System.Text.Json;

namespace Core.Helpers;

public static class JsonReader
{
    private static readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public static T Read<T>(string filePath) =>
        JsonSerializer.Deserialize<T>(File.ReadAllText(filePath), _options)
            ?? throw new InvalidOperationException($"Failed to deserialize '{filePath}' as {typeof(T).Name}.");
}
