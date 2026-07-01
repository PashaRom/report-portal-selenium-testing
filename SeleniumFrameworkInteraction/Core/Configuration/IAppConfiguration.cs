namespace Core.Configuration;

public interface IAppConfiguration
{
    DriverSettings DriverSettings { get; }
    LogSettings LogSettings { get; }
    string BaseUrl { get; }
    string ProjectName { get; }
    string TestDataDirectory { get; }
    int ExplicitWaitTimeoutSeconds { get; }
}
