using Business.Components;
using Business.Pages;
using Core.Drivers;

namespace Business.Steps;

public class DashboardSteps
{
    private readonly DashboardScenarioContext _context = new();
    private readonly DashboardListPage _listPage      = new();
    private readonly DashboardPage     _dashboardPage = new();

    public bool HasCreatedDashboard => _context.HasCreatedDashboard;
    public long CreatedDashboardId  => _context.CreatedDashboardId;
    public string CreatedDashboardName => _context.CreatedDashboardName;

    // ── Dialog ──────────────────────────────────────────────

    public AddDashboardDialog Dialog => _listPage.AddDashboardDialog;

    public void OpenAddDialog()              => _listPage.OpenAddDialog();
    public bool IsAddDialogOpen()            => _listPage.AddDashboardDialog.IsOpen();
    public bool IsAddDialogClosed()          => _listPage.AddDashboardDialog.IsClosed();

    // ── Dashboard lifecycle ──────────────────────────────────

    public void CreateDashboardWithUniqueName()
    {
        var name = $"DC_{Guid.NewGuid():N}";
        CreateDashboardWithName(name);
    }

    public void CreateDashboardWithName(string name)
    {
        _context.SetCreatedDashboardName(name);
        _listPage.CreateDashboard(name);
        var match = System.Text.RegularExpressions.Regex.Match(
            DriverManager.Current.Url, @"dashboard/(\d+)");
        if (match.Success)
            _context.SetCreatedDashboard(name, long.Parse(match.Groups[1].Value));
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

    public void LockDashboard()      => _dashboardPage.ClickLock();
    public void UnlockDashboard()    => _dashboardPage.ClickUnlock();
    public bool IsLockAvailable()    => _dashboardPage.IsLockAvailable();
    public bool IsUnlockAvailable()  => _dashboardPage.IsUnlockAvailable();
}
