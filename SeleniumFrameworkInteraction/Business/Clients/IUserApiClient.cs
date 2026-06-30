using Business.Model.API;

namespace Business.Clients;

public interface IUserApiClient
{
    Task<UserListResponse> GetAllUsersAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<UserResponse> CreateUserAsync(
        string token,
        CreateUserRequest request,
        CancellationToken cancellationToken = default);

    Task<InfoResponse> AssignToProjectAsync(
        string token,
        string projectName,
        Dictionary<string, string> userRoles,
        CancellationToken cancellationToken = default);
}
