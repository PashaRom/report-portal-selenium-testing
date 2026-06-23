using System.Text;
using System.Text.Json;
using Business.Model.API;
using Core.Clients;

namespace Business.Clients;

public class UserApiClient : IUserApiClient
{
    private readonly IRpApiClient _rpApiClient;

    public UserApiClient(IRpApiClient rpApiClient)
    {
        _rpApiClient = rpApiClient;
    }

    public async Task<UserListResponse> GetAllUsersAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var request = new ApiRequest
        {
            Method = HttpMethod.Get,
            RelativeUrl = "api/users/all?page.page=1&page.size=500",
            Headers = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {token}"
            },
            EnsureSuccessStatusCode = false
        };

        return await _rpApiClient.ExecuteAsync<UserListResponse>(request, cancellationToken);
    }

    public async Task CreateUserAsync(
        string token,
        CreateUserRq rq,
        CancellationToken cancellationToken = default)
    {
        var request = new ApiRequest
        {
            Method = HttpMethod.Post,
            RelativeUrl = "api/users",
            Headers = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {token}"
            },
            Content = new StringContent(JsonSerializer.Serialize(rq), Encoding.UTF8, "application/json"),
            EnsureSuccessStatusCode = false
        };

        await _rpApiClient.ExecuteAsync<EmptyResponse>(request, cancellationToken);
    }

    public async Task AssignToProjectAsync(
        string token,
        string projectName,
        Dictionary<string, string> userRoles,
        CancellationToken cancellationToken = default)
    {
        var rq = new AssignUsersRq { UserNames = userRoles };
        var request = new ApiRequest
        {
            Method = HttpMethod.Put,
            RelativeUrl = $"api/v1/project/{projectName}/assign",
            Headers = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {token}"
            },
            Content = new StringContent(JsonSerializer.Serialize(rq), Encoding.UTF8, "application/json"),
            EnsureSuccessStatusCode = false
        };

        await _rpApiClient.ExecuteAsync<EmptyResponse>(request, cancellationToken);
    }
}
