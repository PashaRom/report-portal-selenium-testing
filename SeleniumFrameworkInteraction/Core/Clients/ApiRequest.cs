namespace Core.Clients;

public sealed class ApiRequest
{
    public required HttpMethod Method { get; init; }
    public required string RelativeUrl { get; init; }
    public HttpContent? Content { get; init; }
    public IDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();
    public bool EnsureSuccessStatusCode { get; init; } = true;
}
