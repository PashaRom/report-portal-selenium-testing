using Business.Components;
using Business.Pages;
using Core.DI;
using Core.Drivers;
using Core.Helpers;
using Core.Structures;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using Core.Base;
using Microsoft.Extensions.Logging;
using Core.Enum;

namespace Business.Steps;

public class DashboardSteps : BaseSteps
{
    private readonly DashboardScenarioContext _context = new();
    private readonly DashboardListPage _listPage;
    private readonly DashboardPage _dashboardPage;
    private readonly SystemAlertDialog _systemAlertDialog;

    public DashboardSteps(DashboardListPage listPage, DashboardPage dashboardPage, SystemAlertDialog systemAlertDialog)
    {
        _listPage = listPage;
        _dashboardPage = dashboardPage;
        _systemAlertDialog = systemAlertDialog;
    }

    public bool HasCreatedDashboard => _context.HasCreatedDashboard;
    public long CreatedDashboardId => _context.CreatedDashboardId;
    public string CreatedDashboardName => _context.CreatedDashboardName;

    // ── Dialog ──────────────────────────────────────────────

    public AddDashboardDialog Dialog => _listPage.AddDashboardDialog;

    public void OpenAddDialog() => _listPage.OpenAddDialog();
    public bool IsAddDialogOpen() => _listPage.AddDashboardDialog.IsOpen();
    public bool IsAddDialogClosed() => _listPage.AddDashboardDialog.IsClosed();

    // ── Dashboard lifecycle ──────────────────────────────────

    public void CreateDashboardWithUniqueName()
    {
        var name = $"DC_{Guid.NewGuid():N}";
        CreateDashboardWithName(name);
    }

    public void CreateDashboardWithName(string name)
    {
        var dasboardUrlPattern = @"dashboard/(\d+)";
        _context.SetCreatedDashboardName(name);
        OpenAddDialog();
        _listPage.AddDashboardDialog.FillName(name);
        _listPage.AddDashboardDialog.ClickAdd();
        WaitHelper.Until(d => Regex.IsMatch(d.Url, dasboardUrlPattern), timeout: Timeouts.Sec20);
        var match = Regex.Match(
            ServiceLocator.GetService<IDriverManager>().Current.Url, dasboardUrlPattern);
        if (match.Success)
        {
            _context.SetCreatedDashboard(name, long.Parse(match.Groups[1].Value));
        }
    }

    public void NavigateToCreatedDashboard() =>
        _dashboardPage.NavigateToDashboard(_context.CreatedDashboardId);

    public void DeleteDashboard()
    {
        _dashboardPage.DeleteDashboard();
        _context.Reset();
    }

    public bool IsDashboardInList(string name) => _listPage.IsDashboardInList(name);

    // ── Widget operations ────────────────────────────────────

    public void AddWidget(string widgetType, string? widgetName = null)
    {
        _dashboardPage.AddWidget(widgetType, widgetName);
        WaitHelper.Until(_ => _systemAlertDialog.IsDisplayed, timeout: Timeouts.Sec2);
        if (_systemAlertDialog.IsDisplayed)
        {
            _systemAlertDialog.Close();
        }
    }

    public void AddWidgets(List<string> widgetNames)
    {
        foreach (string widgetName in widgetNames)
        {
            AddWidget(widgetName);
        }
    }

    public bool IsWidgetVisible(string widgetName) =>
        _dashboardPage.IsWidgetVisible(widgetName);

    public IReadOnlyList<string> CollectVisibleWidgetNames(IEnumerable<string> expected) =>
        _dashboardPage.CollectVisibleWidgetNames(expected);

    // ── Lock / Unlock ────────────────────────────────────────

    public void LockDashboard() => _dashboardPage.ClickLock();
    public void UnlockDashboard() => _dashboardPage.ClickUnlock();
    public bool IsLockAvailable() => _dashboardPage.IsLockAvailable();
    public bool IsUnlockAvailable() => _dashboardPage.IsUnlockAvailable();

    public void ResizeWidget(string widgetName, int offsetX, int offsetY)
    {
        var widgetList = _dashboardPage.GetVisibleWidgets();
        var targetWidget = widgetList.FirstOrDefault(e => e.TitleText.Equals(widgetName));
        ActionHelper.ScrollToElementTop(targetWidget.Element, targetWidget.TitleText);
        targetWidget?.Resize(offsetX, offsetY);

    }

    public List<WidgetComponent> GetVisibleWidgets() => _dashboardPage.GetVisibleWidgets();

    public WidgetComponent? GetWidgetByName(string widgetName)
    {
        var widgetList = _dashboardPage.GetVisibleWidgets();

        foreach (var widget in widgetList)
        {
            try
            {
                if (widget.TitleText.Equals(widgetName))
                    return widget;
            }
            catch (NoSuchElementException)
            {
                Logger.LogInformation("Widget '{WidgetName}' not found in the list as a visible widget.", widgetName);
            }
        }
        return null;
    }

    public void MoveWidget(string widgetName, int? offset, Movement movement)
    {
        var widget = GetWidgetByName(widgetName);
        _dashboardPage.MoveWidgetOffset(widget, offset, movement);
    }
}
