using System.Text.Json.Serialization;

namespace Business.Model.API;

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
}
