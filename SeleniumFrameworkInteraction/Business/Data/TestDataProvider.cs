using Business.Models;
using Core.Configuration;
using Core.Helpers;

namespace Business.Data;

public static class TestDataProvider
{
    private static readonly Lazy<IReadOnlyDictionary<string, UserModel>> _users = new(LoadUsers);

    public static IEnumerable<string> LoginAliases => _users.Value.Keys;

    /// <summary>
    /// Returns (login, dashboardName, widgetName) triples for CRUD tests.
    /// Names are derived from the login: underscores → spaces, each word title-cased.
    /// </summary>
    public static IEnumerable<object[]> DashboardCrudCases =>
        _users.Value.Values.Select(u =>
        {
            var display = ToTitleCase(u.Login);
            return new object[] { u.Login, $"{display} CRUD Dashboard", $"{display} Widget" };
        });

    /// <summary>
    /// Returns (login, canManageDashboard) pairs derived from each user's role
    /// in the configured project. PROJECT_MANAGER → true, all other roles → false.
    /// </summary>
    public static IEnumerable<object[]> DashboardManagePermissions =>
        _users.Value.Values.Select(u => new object[] { u.Login, CanManageDashboard(u) });

    public static UserModel GetUser(string login) =>
        _users.Value.TryGetValue(login, out var user)
            ? user
            : throw new ArgumentException($"User '{login}' not found in test data.");

    private static string ToTitleCase(string login) =>
        string.Join(" ", login.Split('_').Select(w => char.ToUpper(w[0]) + w[1..]));

    private static bool CanManageDashboard(UserModel user) =>
        user.ProjectsAndRoles
            .Split(',', StringSplitOptions.TrimEntries)
            .Any(entry =>
            {
                var parts = entry.Split(" - ", StringSplitOptions.TrimEntries);
                return parts.Length == 2
                    && parts[0].Equals(AppConfiguration.ProjectName, StringComparison.OrdinalIgnoreCase)
                    && parts[1].Equals("PROJECT_MANAGER", StringComparison.OrdinalIgnoreCase);
            });

    private static IReadOnlyDictionary<string, UserModel> LoadUsers()
    {
        var path = Path.Combine(AppContext.BaseDirectory, AppConfiguration.UsersDataFile);
        return CsvReader.Read<UserModel>(path).ToDictionary(u => u.Login);
    }
}
