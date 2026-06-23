using Business.Clients;
using Business.Data;
using Business.Model.API;
using Business.Model.DTO;
using Core.Configuration;
using Microsoft.Extensions.Logging;

namespace Business.Helpers;

/// <summary>
/// Ensures all test users from the CSV are present in ReportPortal before the test run.
/// For each missing user: creates the account and assigns the correct project role.
/// Runs as part of <see cref="GlobalSetup"/> one-time setup.
/// </summary>
public class UserProvisioningService
{
    private const string SuperAdminLogin = "superadmin";

    private readonly IAuthClient _authClient;
    private readonly IUserApiClient _userApiClient;
    private readonly IAppConfiguration _appConfig;
    private readonly ILogger _logger;

    public UserProvisioningService(
        IAuthClient authClient,
        IUserApiClient userApiClient,
        IAppConfiguration appConfig,
        ILoggerFactory loggerFactory)
    {
        _authClient = authClient;
        _userApiClient = userApiClient;
        _appConfig = appConfig;
        _logger = loggerFactory.CreateLogger<UserProvisioningService>();
    }

    /// <summary>
    /// Checks all users from <see cref="TestDataProvider"/> and creates any that are missing.
    /// </summary>
    public async Task EnsureUsersExistAsync(CancellationToken cancellationToken = default)
    {
        var superadmin = TestDataProvider.GetUser(SuperAdminLogin);
        var tokenResponse = await _authClient.GetTokenAsync(
            superadmin.Login,
            superadmin.Password,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
        {
            _logger.LogError("Cannot obtain superadmin token. User provisioning skipped.");
            return;
        }

        var token = tokenResponse.AccessToken;
        var existingUsers = await _userApiClient.GetAllUsersAsync(token, cancellationToken);
        var existingLogins = existingUsers.Content
            .Select(u => u.UserId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var alias in TestDataProvider.LoginAliases)
        {
            if (alias.Equals(SuperAdminLogin, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var user = TestDataProvider.GetUser(alias);

            if (existingLogins.Contains(user.Login))
            {
                _logger.LogDebug("User '{Login}' already exists, skipping provisioning.", user.Login);
                continue;
            }

            await CreateUserAsync(token, user, cancellationToken);
        }
    }

    private async Task CreateUserAsync(
        string token,
        UserModel user,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating missing user '{Login}'.", user.Login);

            await _userApiClient.CreateUserAsync(token, new CreateUserRq
            {
                Login = user.Login,
                Password = user.Password,
                Email = user.Email,
                FullName = user.FullName,
                AccountType = string.IsNullOrWhiteSpace(user.Type) ? "INTERNAL" : user.Type,
                AccountRole = "USER"
            }, cancellationToken);

            var projectRole = ResolveProjectRole(user, _appConfig.ProjectName);
            await _userApiClient.AssignToProjectAsync(
                token,
                _appConfig.ProjectName,
                new Dictionary<string, string> { [user.Login] = projectRole },
                cancellationToken);

            _logger.LogInformation(
                "User '{Login}' created and assigned to '{Project}' with role '{Role}'.",
                user.Login, _appConfig.ProjectName, projectRole);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to provision user '{Login}', skipping.", user.Login);
        }
    }

    /// <summary>
    /// Parses <c>ProjectsAndRoles</c> (e.g. "report_portal - MEMBER, member_personal - PROJECT_MANAGER")
    /// and returns the role for the given project name. Falls back to "MEMBER" if not found.
    /// </summary>
    private static string ResolveProjectRole(UserModel user, string projectName)
    {
        foreach (var entry in user.ProjectsAndRoles.Split(',', StringSplitOptions.TrimEntries))
        {
            var parts = entry.Split(" - ", StringSplitOptions.TrimEntries);
            if (parts.Length == 2
                && parts[0].Equals(projectName, StringComparison.OrdinalIgnoreCase))
            {
                return parts[1];
            }
        }

        return "MEMBER";
    }
}
