using Core.Clients;
using Core.Configuration;
using Core.Drivers;
using Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Core.DI;

/// <summary>
/// Single static gateway to the DI container for the test framework.
/// Replaces individual static classes (AppConfiguration, TestLoggerFactory, etc.)
/// with a single composition root. Initialised lazily on first access.
/// </summary>
public static class ServiceLocator
{
    private static volatile IServiceProvider? _provider;
    private static readonly object _lock = new();
    private static Action<IServiceCollection>? _additionalRegistrations;

    public static IServiceProvider Provider => _provider ?? Build();

    public static T GetService<T>() where T : notnull =>
        Provider.GetRequiredService<T>();

    /// <summary>
    /// Registers additional services from higher-level assemblies (e.g. Business layer).
    /// If called before the provider is first accessed, the registrations are applied on first build.
    /// If called after the provider was already built, the provider is reset and rebuilt on next access.
    /// </summary>
    public static void SetAdditionalRegistrations(Action<IServiceCollection> configure)
    {
        lock (_lock)
        {
            _additionalRegistrations = configure;
            _provider = null;
        }
    }

    private static IServiceProvider Build()
    {
        lock (_lock)
        {
            if (_provider is null)
            {
                _provider = BuildProvider();
            }

            return _provider;
        }
    }

    private static IServiceProvider BuildProvider()
    {
        var configuration = new AppConfiguration();

        var services = new ServiceCollection();

        services.AddSingleton<IAppConfiguration>(configuration);
        services.AddSingleton<IWebDriverFactory, WebDriverFactory>();
        services.AddSingleton<IDriverManager, DriverManager>();
        services.AddSingleton<IRpApiClient, RpApiClient>();

        services.AddLogging(builder =>
        {
            var logSettings = configuration.LogSettings;

            if (!System.Enum.TryParse<LogLevel>(logSettings.MinLevel, ignoreCase: true, out var minLevel))
            {
                minLevel = LogLevel.Information;
            }

            builder.SetMinimumLevel(minLevel);

            if (logSettings.EnableConsole)
            {
                builder.AddConsole();
            }

            if (logSettings.EnableFile)
            {
                builder.AddProvider(new FileLoggerProvider(logSettings.FilePath));
            }
        });

        _additionalRegistrations?.Invoke(services);

        return services.BuildServiceProvider();
    }
}
