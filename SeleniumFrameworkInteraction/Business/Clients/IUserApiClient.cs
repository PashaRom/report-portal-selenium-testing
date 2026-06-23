using Business.Model.API;

namespace Business.Clients;

public interface IUserApiClient
{
    Task<UserListResponse> GetAllUsersAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task CreateUserAsync(
        string token,
        CreateUserRq request,
        CancellationToken cancellationToken = default);

    Task AssignToProjectAsync(
        string token,
        string projectName,
        Dictionary<string, string> userRoles,
        CancellationToken cancellationToken = default);
}
