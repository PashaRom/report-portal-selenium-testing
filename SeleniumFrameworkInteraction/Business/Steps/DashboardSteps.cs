using Business.Components;
using Business.Pages;
using Core.DI;
using Core.Drivers;
using Core.Helpers;
using System.Text.RegularExpressions;

namespace Business.Steps;

public class DashboardSteps
{
    private readonly DashboardScenarioContext _context = new();
    private readonly DashboardListPage _listPage;
    private readonly DashboardPage _dashboardPage;

    public DashboardSteps(DashboardListPage listPage, DashboardPage dashboardPage)
    {
        _listPage = listPage;
        _dashboardPage = dashboardPage;
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
        WaitHelper.Until(d => Regex.IsMatch(d.Url, dasboardUrlPattern));
        var match = Regex.Match(
            ServiceLocator.GetService<IDriverManager>().Current.Url, dasboardUrlPattern);
        if (match.Success)
        {
            _context.SetCreatedDashboard(name, long.Parse(match.Groups[1].Value));
        }
    }

    public void NavigateToCreatedDashboard() =>
        _dashboardPage.NavigateToDashboard(_context.CreatedDashboardId);

    public void DeleteDashboard() => _dashboardPage.DeleteDashboard();

    public bool IsDashboardInList(string name) => _listPage.IsDashboardInList(name);

    // ── Widget operations ────────────────────────────────────

    public void AddWidget(string widgetType, string widgetName) =>
        _dashboardPage.AddWidget(widgetType, widgetName);

    public bool IsWidgetVisible(string widgetName) =>
        _dashboardPage.IsWidgetVisible(widgetName);

    public IReadOnlyList<string> CollectVisibleWidgetNames(IEnumerable<string> expected) =>
        _dashboardPage.CollectVisibleWidgetNames(expected);

    // ── Lock / Unlock ────────────────────────────────────────

    public void LockDashboard() => _dashboardPage.ClickLock();
    public void UnlockDashboard() => _dashboardPage.ClickUnlock();
    public bool IsLockAvailable() => _dashboardPage.IsLockAvailable();
    public bool IsUnlockAvailable() => _dashboardPage.IsUnlockAvailable();
}
