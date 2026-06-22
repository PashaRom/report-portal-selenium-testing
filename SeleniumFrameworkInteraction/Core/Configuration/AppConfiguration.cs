using Microsoft.Extensions.Configuration;

namespace Core.Configuration;

public class AppConfiguration : IAppConfiguration
{
    public AppConfiguration()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        DriverSettings = config.GetSection("DriverSettings").Get<DriverSettings>() ?? new DriverSettings();
        LogSettings = config.GetSection("LogSettings").Get<LogSettings>() ?? new LogSettings();
        BaseUrl = (config["BaseUrl"] ?? "http://localhost:8080/").TrimEnd('/') + '/';
        ProjectName = config["ProjectName"] ?? "superadmin_personal";
        UserPassword = config["UserPassword"] ?? "1q2w3e";
        TestDataDirectory = config["TestDataDirectory"] is { Length: > 0 } dir
            ? dir
            : Path.Combine(AppContext.BaseDirectory, "Data");
        ExplicitWaitTimeoutSeconds = int.TryParse(config["ExplicitWaitTimeoutSeconds"], out var t) ? t : 0;
    }

    public DriverSettings DriverSettings { get; }
    public LogSettings LogSettings { get; }
    public string BaseUrl { get; }
    public string ProjectName { get; }
    public string UserPassword { get; }
    public string TestDataDirectory { get; }
    public int ExplicitWaitTimeoutSeconds { get; }
}

