using Core.Enum;
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

        var driverSettings = config.GetSection("DriverSettings").Get<DriverSettings>() ?? new DriverSettings();

        // Support short-form: BROWSERS=Chrome,Firefox (overrides appsettings + DriverSettings__Browsers__)
        var browsersEnv = Environment.GetEnvironmentVariable("BROWSERS");
        if (!string.IsNullOrWhiteSpace(browsersEnv))
        {
            driverSettings.Browsers = browsersEnv
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(b => System.Enum.Parse<BrowserType>(b, ignoreCase: true))
                .ToList();
        }

        if(driverSettings?.Browsers?.Count <= 0)
        {
            driverSettings.Browsers = new List<BrowserType> { BrowserType.Chrome };
        }

        var remoteUrl = Environment.GetEnvironmentVariable("REMOTE_URL");
        if (!string.IsNullOrWhiteSpace(remoteUrl))
        {
            driverSettings.RemoteUri = remoteUrl;
        }

        DriverSettings = driverSettings;
        LogSettings = config.GetSection("LogSettings").Get<LogSettings>() ?? new LogSettings();
        BaseUrl = (config["BaseUrl"] ?? "http://localhost:8080/").TrimEnd('/') + '/';
        ProjectName = config["ProjectName"] ?? "superadmin_personal";
        TestDataDirectory = config["TestDataDirectory"] is { Length: > 0 } dir
            ? dir
            : Path.Combine(AppContext.BaseDirectory, "Data");
        ExplicitWaitTimeoutSeconds = int.TryParse(config["ExplicitWaitTimeoutSeconds"], out var t) ? t : 0;
    }

    public DriverSettings DriverSettings { get; }
    public LogSettings LogSettings { get; }
    public string BaseUrl { get; }
    public string ProjectName { get; }
    public string TestDataDirectory { get; }
    public int ExplicitWaitTimeoutSeconds { get; }
}

