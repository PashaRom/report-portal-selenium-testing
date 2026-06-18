using System.Text.Json;
using Core.Configuration;

namespace Core.Clients;

public sealed class RpApiClient : IRpApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly HttpClient Http = new();

    public async Task<T> ExecuteAsync<T>(ApiRequest request, CancellationToken cancellationToken = default)
        where T : class, new()
    {
        using var requestMessage = new HttpRequestMessage(
            request.Method,
            AppConfiguration.BaseUrl + request.RelativeUrl)
        {
            Content = request.Content
        };

        foreach (var (key, value) in request.Headers)
        {
            if (!requestMessage.Headers.TryAddWithoutValidation(key, value) && requestMessage.Content is not null)
            {
                requestMessage.Content.Headers.TryAddWithoutValidation(key, value);
            }
        }

        using var response = await Http.SendAsync(requestMessage, cancellationToken);
        if (request.EnsureSuccessStatusCode)
        {
            response.EnsureSuccessStatusCode();
        }
        else if (!response.IsSuccessStatusCode)
        {
            return new T();
        }

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(payload))
        {
            return new T();
        }

        return JsonSerializer.Deserialize<T>(payload, JsonOptions) ?? new T();
    }
}