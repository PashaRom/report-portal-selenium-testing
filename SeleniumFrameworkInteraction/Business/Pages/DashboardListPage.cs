using Business.Components;
using Core.Base;
using Core.Configuration;
using Core.Drivers;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Business.Pages;

public class DashboardListPage : BasePage
{
    private static readonly By AddButton  = By.XPath(
        "//button[not(@disabled) and contains(.,'Add New Dashboard')]");
    private static readonly By ModalReady = By.CssSelector(
        "#modal-root [class*='modalLayout__modal-window']");

    public AddDashboardDialog AddDashboardDialog { get; } = new();

    public void Navigate()
    {
        var url = $"{AppConfiguration.BaseUrl}ui/#{AppConfiguration.ProjectName}/dashboard";
        NavigateAndWaitForReady(url);
    }

    public void OpenAddDialog()
    {
        Navigate();
        WaitUntilClickable(AddButton).Click();
        Wait.Until(d => d.FindElements(ModalReady).Any(e => e.Displayed));
    }

    public void CreateDashboard(string name)
    {
        Logger.LogInformation("Creating dashboard: {Name}", name);
        OpenAddDialog();
        AddDashboardDialog.FillName(name);
        AddDashboardDialog.ClickAdd();
        Wait.Until(d => System.Text.RegularExpressions.Regex.IsMatch(d.Url, @"dashboard/\d+"));
    }

    public bool IsDashboardInList(string name)
    {
        Navigate();
        return Driver.FindElements(
                By.XPath($"//*[contains(@class,'dashboardPageHeader__link') and text()='{name}']"))
            .Any(e => e.Displayed);
    }
}
