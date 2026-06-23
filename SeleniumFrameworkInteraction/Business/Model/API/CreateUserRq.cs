using System.Text.Json.Serialization;

namespace Business.Model.API;

public class CreateUserRq
{
    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("accountType")]
    public string AccountType { get; set; } = "INTERNAL";

    [JsonPropertyName("accountRole")]
    public string AccountRole { get; set; } = "USER";
}
