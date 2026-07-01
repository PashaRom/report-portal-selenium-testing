using System.Text.Json;
using Allure.NUnit.Attributes;
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

    [AllureStep("Login via UI as '{loginAlias}'")]
    public void LoginAs(string loginAlias)
    {
        var user = TestDataProvider.GetUser(loginAlias);
        _loginPage.Login(user.Login, user.Password);
    }

    [AllureStep("Login via API as '{loginAlias}'")]
    public void LoginViaApi(string loginAlias)
    {
        var user = TestDataProvider.GetUser(loginAlias);
        Logger.LogInformation("[AuthSteps] LoginViaApi: obtaining token for '{Login}'", user.Login);
        var token = _authClient.GetTokenAsync(user.Login, user.Password).GetAwaiter().GetResult();
        if (token == null || string.IsNullOrEmpty(token.AccessToken))
        {
            throw new InvalidOperationException($"Failed to obtain token for user '{user.Login}'");
        }
        var driver = ServiceLocator.GetService<IDriverManager>().Current;

        driver.Navigate().GoToUrl(_config.BaseUrl + "ui/");

        WaitHelper.Until(
            d => (d.Url.StartsWith("http://") || d.Url.StartsWith("https://")) &&
                 ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState")?.Equals("complete") == true,
            timeout: TimeSpan.FromSeconds(20),
            driver: driver);

        ActionHelper.JsClearBrowserStorage();

        var tokenJson = JsonSerializer.Serialize(new { type = "Bearer", value = token.AccessToken });

        ActionHelper.JsSetLocalStorageItem("token", tokenJson);
        ActionHelper.JsSetLocalStorageItem("applicationSettings", "{\"shouldRequestOnboarding\":false}");

        var tsMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        ActionHelper.JsSetLocalStorageItem("activityTimestamp", tsMs);

        driver.Navigate().Refresh();

        WaitHelper.Until(
            d => d.Url.Contains("/#") && !d.Url.Contains("login"),
            timeout: TimeSpan.FromSeconds(15),
            driver: driver);

        Logger.LogInformation("[AuthSteps] LoginViaApi: authenticated as '{Login}'", user.Login);
    }

    [AllureStep("Logout")]
    public void Logout() => _loginPage.Logout();
}