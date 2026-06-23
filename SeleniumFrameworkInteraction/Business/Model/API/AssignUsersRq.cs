using System.Text.Json.Serialization;

namespace Business.Model.API;

public class AssignUsersRq
{
    [JsonPropertyName("userNames")]
    public Dictionary<string, string> UserNames { get; set; } = new();
}
