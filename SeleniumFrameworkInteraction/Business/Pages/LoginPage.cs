using Core.Base;
using Core.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Business.Pages;

public class LoginPage : BasePage
{
    private static readonly By LoginInput  = By.CssSelector("input[name='login']");
    private static readonly By PasswordInput = By.CssSelector("input[name='password']");
    private static readonly By LoginButton = By.XPath("//button[.='Login']");
    private static readonly By LogoutLink  = By.XPath(
        "//*[contains(@class,'sidebarButton__nav-link') and contains(.,'Logout')]");

    public void Login(string username, string password)
    {
        Logger.LogInformation("Logging in as {Username}", username);
        Driver.Navigate().GoToUrl(AppConfiguration.BaseUrl + "ui/");
        Driver.Manage().Cookies.DeleteAllCookies();
        ((IJavaScriptExecutor)Driver).ExecuteScript("localStorage.clear(); sessionStorage.clear();");
        NavigateTo(AppConfiguration.BaseUrl + "ui/#login");
        var input = FindElement(LoginInput);
        input.Clear();
        input.SendKeys(username);
        FindElement(PasswordInput).SendKeys(password);
        WaitUntilClickable(LoginButton).Click();
        Wait.Until(d => !d.Url.Contains("#login"));
        Logger.LogInformation("Login successful: {Username}", username);
    }

    public void Logout()
    {
        Logger.LogInformation("Logging out");
        var el = FindElement(LogoutLink);
        ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", el);
        Wait.Until(d => d.Url.Contains("#login"));
        Logger.LogInformation("Logged out");
    }
}
