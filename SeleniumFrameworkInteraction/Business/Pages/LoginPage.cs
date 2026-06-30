using Core.Base;
using Core.Elements;
using Core.Helpers;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Business.Pages;

public class LoginPage : BasePage
{
    public LoginPage() : base("Login Page") { }

    public Text LoginInput => new(By.CssSelector("input[name='login']"), "Login Input");
    public Text PasswordInput => new(By.CssSelector("input[name='password']"), "Password Input");
    public Button LoginButton => new(By.XPath("//button[.='Login']"), "Login Button");
    public Link LogoutLink => new(
        By.XPath("//*[contains(@class,'sidebarButton__nav-link') and contains(.,'Logout')]"),
        "Logout Link");

    public void Login(string username, string password)
    {
        Logger.LogInformation("[{Page}] Logging in as {Username}", Name, username);
        Driver.Navigate().GoToUrl(Configuration.BaseUrl + "ui/");
        Driver.Manage().Cookies.DeleteAllCookies();
        ActionHelper.JsClearBrowserStorage();
        NavigateTo(Configuration.BaseUrl + "ui/#login");
        LoginInput.SetValue(username);
        PasswordInput.SetValue(password);
        LoginButton.Click();
        WaitHelper.Until(d => !d.Url.Contains("#login"));
        Logger.LogInformation("[{Page}] Login successful: {Username}", Name, username);
    }

    public void Logout()
    {
        Logger.LogInformation("[{Page}] Logging out", Name);
        var el = LogoutLink.Element;
        ActionHelper.JsClick(el, "Logout Link");
        WaitHelper.Until(d => d.Url.Contains("#login"));
        Logger.LogInformation("[{Page}] Logged out", Name);
    }

    public bool IsLoginFormVisible => LoginButton.IsDisplayed;
    public bool IsLoggedIn => LogoutLink.IsDisplayed;
}