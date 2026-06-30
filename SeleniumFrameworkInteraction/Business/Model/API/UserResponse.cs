using System.Text.Json.Serialization;

namespace Business.Model.API;

public class UserResponse : InfoResponse
{
    [JsonPropertyName("warning")]
    public string? Warning { get; set; }

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("uuid")]
    public string? Uuid { get; set; }

    [JsonPropertyName("login")]
    public string? Login { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("fullName")]
    public string? FullName { get; set; }

    [JsonPropertyName("accountRole")]
    public string? AccountRole { get; set; }

    [JsonPropertyName("accountType")]
    public string? AccountType { get; set; }

    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("externalId")]
    public string? ExternalId { get; set; }
}
