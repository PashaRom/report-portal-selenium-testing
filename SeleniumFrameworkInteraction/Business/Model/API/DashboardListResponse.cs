using System.Text.Json.Serialization;

namespace Business.Model.API;

public class DashboardListResponse
{
    [JsonPropertyName("content")]
    public List<DashboardItem> Content { get; set; } = [];
}
