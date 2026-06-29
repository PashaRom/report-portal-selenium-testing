using System.Text.Json.Serialization;

namespace Business.Model.API;


public class InfoResponse
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    [JsonPropertyName("errorCode")]
    public string? ErrorCode { get; set; }
    [JsonPropertyName("stackTrace")]
    public string? StackTrace { get; set; }
}
