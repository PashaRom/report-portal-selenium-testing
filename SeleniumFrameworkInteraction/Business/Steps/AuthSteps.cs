using System.Text.Json;
using Business.Clients;
using Business.Data;
using Business.Pages;
using Core.Configuration;
using Core.Drivers;
using Core.DI;
using Core.Helpers;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Core.Base;

namespace Business.Steps;

public class AuthSteps : BaseSteps
{
    private readonly LoginPage _loginPage;
    private readonly IAuthClient _authClient;
    private readonly IAppConfiguration _config;

    public AuthSteps(LoginPage loginPage, IAuthClient authClient, IAppConfiguration config)
    {
        _loginPage = loginPage;
        _authClient = authClient;
        _config = config;
    }

    /// <summary>
    /// Logs in via UI form. Kept as fallback; prefer <see cref="LoginViaApi"/> for speed.
    /// </summary>
    public void LoginAs(string loginAlias)
    {
        var user = TestDataProvider.GetUser(loginAlias);
        _loginPage.Login(user.Login, user.Password);
    }

    /// <summary>
    /// Obtains a JWT via the RP OAuth endpoint, injects it into localStorage,
    /// and navigates directly to the project dashboard — bypassing the login form.
    /// </summary>
    public void LoginViaApi(string loginAlias)
    {
        var user = TestDataProvider.GetUser(loginAlias);
        Logger.LogInformation("[AuthSteps] LoginViaApi: obtaining token for '{Login}'", user.Login);

        var token = _authClient.GetTokenAsync(user.Login, user.Password).GetAwaiter().GetResult();

        var driver = ServiceLocator.GetService<IDriverManager>().Current;

        // Navigate to the app origin so localStorage is accessible for this origin
        driver.Navigate().GoToUrl(_config.BaseUrl + "ui/");

        // Wait for the SPA shell to settle on a real HTTP(S) page — not a browser error/blank page.
        // Under Grid load, GoToUrl can temporarily land on chrome-error:// or data: pages.
        WaitHelper.Until(
            d => (d.Url.StartsWith("http://") || d.Url.StartsWith("https://")) &&
                 ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState")?.Equals("complete") == true,
            timeout: TimeSpan.FromSeconds(20),
            driver: driver);

        ActionHelper.JsClearBrowserStorage();

        // RP UI expects: localStorage["token"] = {"type":"Bearer","value":"<jwt>"}
        var tokenJson = JsonSerializer.Serialize(new { type = "Bearer", value = token.AccessToken });
        ActionHelper.JsSetLocalStorageItem("token", tokenJson);

        // Prevent the onboarding wizard from intercepting the first post-login navigation
        ActionHelper.JsSetLocalStorageItem("applicationSettings", "{\"shouldRequestOnboarding\":false}");

        // Set activity timestamp to now so RP does not consider the session idle/expired
        var tsMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        ActionHelper.JsSetLocalStorageItem("activityTimestamp", tsMs);

        // Force a full page reload so React re-initializes and re-reads the token from localStorage
        driver.Navigate().Refresh();

        // Wait for React to authenticate: URL should have a hash route (e.g. /#/project/dashboard)
        // that is not the login route. The exact project depends on the user's default project.
        WaitHelper.Until(
            d => d.Url.Contains("/#") && !d.Url.Contains("login"),
            timeout: TimeSpan.FromSeconds(15),
            driver: driver);

        Logger.LogInformation("[AuthSteps] LoginViaApi: authenticated as '{Login}'", user.Login);
    }

    public void Logout() => _loginPage.Logout();
}
