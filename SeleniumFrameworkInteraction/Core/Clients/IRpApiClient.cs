namespace Core.Clients;

public interface IRpApiClient
{
    Task<T> ExecuteAsync<T>(ApiRequest request, CancellationToken cancellationToken = default)
        where T : class, new();
}
