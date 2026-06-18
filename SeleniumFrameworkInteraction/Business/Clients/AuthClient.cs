using System.Text;
using Business.Model.API;
using Core.Clients;

namespace Business.Clients;

public class AuthClient : IAuthClient
{
    protected readonly IRpApiClient RpApiClient;

    public AuthClient(IRpApiClient rpApiClient)
    {
        RpApiClient = rpApiClient;
    }

    public async Task<TokenResponse> GetTokenAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var request = new ApiRequest
        {
            Method = HttpMethod.Post,
            RelativeUrl = "uat/sso/oauth/token",
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = username,
                ["password"] = password
            }),
            Headers = new Dictionary<string, string>
            {
                ["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("ui:uiman"))
            }
        };

        return await RpApiClient.ExecuteAsync<TokenResponse>(request, cancellationToken);
    }
}