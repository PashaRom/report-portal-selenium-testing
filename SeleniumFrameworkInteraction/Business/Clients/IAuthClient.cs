using Business.Model.API;

namespace Business.Clients;

public interface IAuthClient
{
    Task<TokenResponse> GetTokenAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);
}