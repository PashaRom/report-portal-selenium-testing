using System.Text.Json.Serialization;

namespace Business.Model.API;

public class UserListResponse
{
    [JsonPropertyName("content")]
    public List<UserResource> Content { get; set; } = [];
}
