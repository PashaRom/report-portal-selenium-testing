using Microsoft.Extensions.Configuration;

namespace Core.Configuration;

public static class AppConfiguration
{
    private static readonly IConfiguration _config = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
        .Build();

    public static DriverSettings DriverSettings { get; } =
        _config.GetSection("DriverSettings").Get<DriverSettings>() ?? new DriverSettings();

    public static LogSettings LogSettings { get; } =
        _config.GetSection("LogSettings").Get<LogSettings>() ?? new LogSettings();

    public static string BaseUrl { get; } =
        (_config["BaseUrl"] ?? "http://localhost:8080/").TrimEnd('/') + '/';

    public static string ProjectName { get; } =
        _config["ProjectName"] ?? "superadmin_personal";

    public static string UserPassword { get; } =
        _config["UserPassword"] ?? "1q2w3e";

    public static string UsersDataFile { get; } =
        _config["UsersDataFile"] ?? "RP_USERS_CSV_Report.csv";

    public static string WidgetTypesFile { get; } =
        _config["WidgetTypesFile"] ?? "widget_types.en.json";
}
