using Business.Components;
using Core.Base;
using Core.Elements;
using Core.Helpers;
using OpenQA.Selenium;

namespace Business.Pages;

public class DashboardListPage : BasePage
{
    public DashboardListPage(AddDashboardDialog addDashboardDialog) : base("Dashboard List Page")
    {
        AddDashboardDialog = addDashboardDialog;
    }

    private Button AddNewDashboardBtn => new(
        By.XPath("//button[not(@disabled) and contains(.,'Add New Dashboard')]"),
        "Add New Dashboard Button");

    public AddDashboardDialog AddDashboardDialog { get; }

    public void Navigate()
    {
        var url = $"{Configuration.BaseUrl}ui/#{Configuration.ProjectName}/dashboard";
        NavigateAndWaitForReady(url);
    }

    public void OpenAddDialog()
    {
        Navigate();
        AddNewDashboardBtn.Click();
        WaitHelper.Until(d => AddDashboardDialog.IsOpen());
    }

    public bool IsDashboardInList(string name)
    {
        Navigate();
        return Driver.FindElements(
                By.XPath($"//*[contains(@class,'dashboardPageHeader__link') and text()='{name}']"))
            .Any(e => e.Displayed);
    }
}
