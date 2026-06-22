using Business.Components;
using Core.Base;
using Core.Elements;
using Core.Helpers;
using Core.Structures;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Text.RegularExpressions;

namespace Business.Pages;

public class DashboardPage : BasePage
{
    public DashboardPage(AddWidgetDialog addWidgetDialog, DeleteDashboardDialog deleteDashboardDialog) : base("Dashboard Page")
    {
        AddWidgetDialog = addWidgetDialog;
        DeleteDashboardDialog = deleteDashboardDialog;
    }

    private Button AddNewWidgetBtn => new(By.XPath("(//button[contains(.,'Add new widget')])[1]"), "Add New Widget Button");
    private Button DeleteBtn => new(By.XPath("//button[.='Delete']"), "Delete Button");
    private Button LockBtn => new(By.XPath("//button[.='Lock']"), "Lock Button");
    private Button UnlockBtn => new(By.XPath("//button[.='Unlock']"), "Unlock Button");

    public AddWidgetDialog AddWidgetDialog { get; }
    public DeleteDashboardDialog DeleteDashboardDialog { get; }

    public void NavigateToDashboard(long dashboardId)
    {
        var url = $"{Configuration.BaseUrl}ui/#{Configuration.ProjectName}/dashboard/{dashboardId}";
        Logger.LogInformation("[{Page}] Navigating to dashboard {Id}", Name, dashboardId);
        NavigateAndWaitForReady(url);
    }

    public void AddWidget(string widgetType, string widgetName)
    {
        Logger.LogInformation("[{Page}] Opening Add widget wizard for {Type}", Name, widgetType);
        AddNewWidgetBtn.Click();
        AddWidgetDialog.WaitUntilVisible();
        AddWidgetDialog.Submit(widgetType, widgetName);
    }

    public bool IsWidgetVisible(string widgetName)
    {
        try
        {
            return WaitHelper.Until(d =>
                d.FindElements(By.XPath($"//*[contains(text(),'{widgetName}')]"))
                 .Any(e => e.Displayed), timeout: Timeouts.Sec1);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "[{Page}] Timeout waiting for widget '{WidgetName}' to appear", Name, widgetName);
            return false;
        }
    }

    public IReadOnlyList<string> CollectVisibleWidgetNames(IEnumerable<string> expectedNames)
    {
        var names = expectedNames.ToList();
        var found = new HashSet<string>(StringComparer.Ordinal);

        foreach (var name in names)
        {
            if (Driver.FindElements(By.XPath($"//*[contains(text(),'{name}')]")).Any())
            {
                found.Add(name);
            }
        }

        if (found.Count == names.Count)
        {
            return found.ToList();
        }

        var container = ActionHelper.JsFindScrollableContainer();

        if (container == null)
        {
            return found.ToList();
        }

        ActionHelper.JsScrollToTop(container);
        WaitHelper.Until(_ => ActionHelper.JsGetScrollTop(container) == 0, timeout: Timeouts.Sec5);

        long lastScrollTop = -1;
        while (true)
        {
            foreach (var name in names)
            {
                if (!found.Contains(name) &&
                    Driver.FindElements(By.XPath($"//*[contains(text(),'{name}')]")).Any())
                {
                    found.Add(name);
                }
            }

            if (found.Count == names.Count)
            {
                break;
            }

            ActionHelper.JsScrollBy(container, 600);

            var missing = names.Where(n => !found.Contains(n)).ToList();
            var anyMissingXPath = "//*[" +
                string.Join(" or ", missing.Select(n => $"contains(text(),'{n}')")) + "]";
            try
            {
                WaitHelper.Until(d =>
                    d.FindElements(By.XPath(anyMissingXPath)).Any(), timeout: Timeouts.Sec3);
            }
            catch (WebDriverTimeoutException)
            {
                Logger.LogDebug("[{Page}] No missing widgets appeared after scroll step, continuing", Name);
            }

            var scrollTop = ActionHelper.JsGetScrollTop(container);
            if (scrollTop == lastScrollTop)
            {
                break;
            }
            lastScrollTop = scrollTop;
        }

        return found.ToList();
    }

    public void DeleteDashboard()
    {
        Logger.LogInformation("[{Page}] Deleting dashboard", Name);
        DeleteBtn.ClickWithActions();
        DeleteDashboardDialog.ClickDelete();
        WaitHelper.Until(d => !Regex.IsMatch(d.Url, @"dashboard/\d+$"));
    }

    public void ClickLock()
    {
        Logger.LogInformation("[{Page}] Clicking Lock button", Name);
        LockBtn.Click();
    }

    public void ClickUnlock()
    {
        Logger.LogInformation("[{Page}] Clicking Unlock button", Name);
        UnlockBtn.Click();
    }

    public bool IsLockAvailable()
    {
        try
        {
            return WaitHelper.Until(_ => LockBtn.IsDisplayed, timeout: Timeouts.Sec2);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "[{Page}] Lock button not found or not visible", Name);
            return false;
        }
    }

    public bool IsUnlockAvailable()
    {
        try
        {
            return WaitHelper.Until(_ => UnlockBtn.IsDisplayed, timeout: Timeouts.Sec2);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "[{Page}] Unlock button not found or not visible", Name);
            return false;
        }
    }
}
