using System.Reflection;
using System.Text;

namespace Core.Helpers;

public static class CsvReader
{
    public static IReadOnlyList<T> Read<T>(string filePath) where T : new()
    {
        var lines = File.ReadAllLines(filePath);
        if (lines.Length < 2)
        {
            return [];
        }

        var headers = Tokenize(lines[0]);
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var mapping = new Dictionary<int, PropertyInfo>();
        for (var i = 0; i < headers.Count; i++)
        {
            var prop = props.FirstOrDefault(p => Normalize(p.Name) == Normalize(headers[i]));
            if (prop != null)
            {
                mapping[i] = prop;
            }
        }

        var result = new List<T>();
        for (var row = 1; row < lines.Length; row++)
        {
            if (string.IsNullOrWhiteSpace(lines[row]))
            {
                continue;
            }
            var values = Tokenize(lines[row]);
            var obj = new T();
            foreach (var (col, prop) in mapping)
            {
                if (col < values.Count)
                {
                    prop.SetValue(obj, Convert.ChangeType(values[col], prop.PropertyType));
                }
            }
            result.Add(obj);
        }
        return result;
    }

    // Strips spaces and lowercases so "Full name" matches "FullName", "Last login" → "LastLogin", etc.
    private static string Normalize(string s) =>
        new(s.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());

    private static List<string> Tokenize(string line)
    {
        var tokens = new List<string>();
        var current = new StringBuilder();
        var inQuote = false;

        foreach (var c in line)
        {
            if (c == '"') inQuote = !inQuote;
            else if (c == ',' && !inQuote) { tokens.Add(current.ToString()); current.Clear(); }
            else current.Append(c);
        }
        tokens.Add(current.ToString());
        return tokens;
    }
}
