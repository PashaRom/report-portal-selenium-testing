namespace Business.Steps;

public sealed class DashboardScenarioContext
{
    public long CreatedDashboardId { get; private set; }
    public string CreatedDashboardName { get; private set; } = string.Empty;

    public bool HasCreatedDashboard => CreatedDashboardId > 0;

    public void SetCreatedDashboard(string name, long id)
    {
        CreatedDashboardName = name;
        CreatedDashboardId = id;
    }

    public void SetCreatedDashboardName(string name)
    {
        CreatedDashboardName = name;
    }
}