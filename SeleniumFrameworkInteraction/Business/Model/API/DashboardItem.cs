using System.Text.Json.Serialization;

namespace Business.Model.API;

public class DashboardItem
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
