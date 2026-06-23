using Core.Configuration;
using Core.Enum;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;

namespace Core.Drivers;

public class WebDriverFactory : IWebDriverFactory
{
    private readonly IAppConfiguration _configuration;

    public WebDriverFactory(IAppConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IWebDriver Create(BrowserType browser)
    {
        var settings = _configuration.DriverSettings;
        var driver = CreateDriver(settings, browser);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
        return driver;
    }

    private static IWebDriver CreateDriver(DriverSettings settings, BrowserType browser)
    {
        if (settings.Remote && !string.IsNullOrWhiteSpace(settings.RemoteUri))
        {
            return new RemoteWebDriver(new Uri(settings.RemoteUri), BuildOptions(settings, browser, isRemote: true));
        }

        return browser switch
        {
            BrowserType.Chrome => new ChromeDriver(BuildChromeOptions(settings)),
            BrowserType.Firefox => CreateFirefoxDriver(settings),
            BrowserType.Edge => new EdgeDriver(BuildEdgeOptions(settings)),
            _ => throw new ArgumentOutOfRangeException(nameof(browser), browser, "Unsupported browser type.")
        };
    }

    private static DriverOptions BuildOptions(DriverSettings settings, BrowserType browser, bool isRemote) => browser switch
    {
        BrowserType.Firefox => BuildFirefoxOptions(settings, isRemote),
        BrowserType.Edge => BuildEdgeOptions(settings, isRemote),
        _ => BuildChromeOptions(settings, isRemote)
    };

    // Firefox needs window size set after creation for non-headless local runs
    private static IWebDriver CreateFirefoxDriver(DriverSettings settings)
    {
        var driver = new FirefoxDriver(BuildFirefoxOptions(settings));
        if (!settings.Headless)
        {
            driver.Manage().Window.Size = new System.Drawing.Size(settings.WindowWidth, settings.WindowHeight);
        }
        return driver;
    }

    private static ChromeOptions BuildChromeOptions(DriverSettings settings, bool isRemote = false)
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
        if (isRemote || settings.Headless)
        {
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
        }
        if (isRemote)
        {
            options.AddArgument("--disable-gpu");
        }
        if (settings.Headless)
        {
            options.AddArgument("--headless=new");
        }
        return options;
    }

    private static FirefoxOptions BuildFirefoxOptions(DriverSettings settings, bool isRemote = false)
    {
        var options = new FirefoxOptions();
        if (isRemote || settings.Headless)
        {
            options.AddArgument($"--width={settings.WindowWidth}");
            options.AddArgument($"--height={settings.WindowHeight}");
        }
        if (settings.Headless)
        {
            options.AddArgument("--headless");
        }
        return options;
    }

    private static EdgeOptions BuildEdgeOptions(DriverSettings settings, bool isRemote = false)
    {
        var options = new EdgeOptions();
        options.AddArgument($"--window-size={settings.WindowWidth},{settings.WindowHeight}");
        if (isRemote || settings.Headless)
        {
            options.AddArgument("--no-sandbox");
        }
        if (isRemote)
        {
            options.AddArgument("--disable-dev-shm-usage");
        }
        if (settings.Headless)
        {
            options.AddArgument("--headless=new");
        }
        return options;
    }
}
