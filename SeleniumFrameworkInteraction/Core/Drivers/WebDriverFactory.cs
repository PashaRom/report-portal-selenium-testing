using Core.Configuration;
using Core.Enum;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;

namespace Core.Drivers;

public static class WebDriverFactory
{
    public static IWebDriver Create(DriverSettings? settings = null)
    {
        settings ??= AppConfiguration.DriverSettings;

        if (settings.Browser == BrowserType.Remote)
        {
            return CreateRemoteDriver(settings);
        }

        return settings.Browser switch
        {
            BrowserType.Chrome  => CreateChromeDriver(settings),
            BrowserType.Firefox => CreateFirefoxDriver(settings),
            BrowserType.Edge    => CreateEdgeDriver(settings),
            _ => throw new ArgumentOutOfRangeException(nameof(settings.Browser), settings.Browser, "Unsupported browser type.")
        };
    }

    private static IWebDriver CreateChromeDriver(DriverSettings settings)
    {
        var options = new ChromeOptions();
        options.AddArgument($"--window-size={settings.WindowWidth},{settings.WindowHeight}");
        options.AddArgument("--disable-features=PasswordLeakDetection,PasswordManagerOnboarding,AutofillServerCommunication");
        options.AddArgument("--disable-notifications");
        options.AddArgument("--disable-save-password-bubble");
        options.AddExcludedArgument("enable-automation");
        options.AddArgument("--disable-password-manager-reauthentication");
        options.AddArgument("--disable-password-generation-popup");
        options.AddLocalStatePreference("credentials_enable_service", false);
        options.AddLocalStatePreference("profile.password_manager_enabled", false);
        options.AddUserProfilePreference("credentials_enable_service", false);
        options.AddUserProfilePreference("profile.password_manager_enabled", false);
        options.AddUserProfilePreference("profile.password_manager_leak_detection", false);
        options.AddUserProfilePreference("autofill.profile_enabled", false);
        options.AddUserProfilePreference("autofill.credit_card_enabled", false);
        if (settings.Headless)
        {
            options.AddArgument("--headless=new");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
        }
        return new ChromeDriver(options);
    }

    private static IWebDriver CreateFirefoxDriver(DriverSettings settings)
    {
        var options = new FirefoxOptions();
        if (settings.Headless)
        {
            options.AddArgument("--headless");
            options.AddArgument($"--width={settings.WindowWidth}");
            options.AddArgument($"--height={settings.WindowHeight}");
        }
        var driver = new FirefoxDriver(options);
        if (!settings.Headless)
        {
            driver.Manage().Window.Size = new System.Drawing.Size(settings.WindowWidth, settings.WindowHeight);
        }
        return driver;
    }

    private static IWebDriver CreateEdgeDriver(DriverSettings settings)
    {
        var options = new EdgeOptions();
        options.AddArgument($"--window-size={settings.WindowWidth},{settings.WindowHeight}");
        if (settings.Headless)
        {
            options.AddArgument("--headless=new");
            options.AddArgument("--no-sandbox");
        }
        return new EdgeDriver(options);
    }

    private static IWebDriver CreateRemoteDriver(DriverSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.RemoteUri))
        {
            throw new InvalidOperationException("RemoteUri must be set in DriverSettings when using BrowserType.Remote.");
        }

        var options = new ChromeOptions();
        options.AddArgument($"--window-size={settings.WindowWidth},{settings.WindowHeight}");
        options.AddArgument("--disable-features=PasswordLeakDetection,PasswordManagerOnboarding,AutofillServerCommunication");
        options.AddArgument("--disable-notifications");
        options.AddArgument("--disable-save-password-bubble");
        options.AddExcludedArgument("enable-automation");
        options.AddArgument("--disable-password-manager-reauthentication");
        options.AddArgument("--disable-password-generation-popup");
        options.AddLocalStatePreference("credentials_enable_service", false);
        options.AddLocalStatePreference("profile.password_manager_enabled", false);
        options.AddUserProfilePreference("credentials_enable_service", false);
        options.AddUserProfilePreference("profile.password_manager_enabled", false);
        options.AddUserProfilePreference("profile.password_manager_leak_detection", false);
        options.AddUserProfilePreference("autofill.profile_enabled", false);
        options.AddUserProfilePreference("autofill.credit_card_enabled", false);
        if (settings.Headless)
        {
            options.AddArgument("--headless=new");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
        }
        return new RemoteWebDriver(new Uri(settings.RemoteUri), options);
    }
}
