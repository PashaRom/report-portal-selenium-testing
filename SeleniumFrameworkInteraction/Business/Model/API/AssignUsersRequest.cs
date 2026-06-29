using System.Text.Json.Serialization;

namespace Business.Model.API;

public class AssignUsersRequest
{
    [JsonPropertyName("userNames")]
    public Dictionary<string, string> UserNames { get; set; } = new();
}
