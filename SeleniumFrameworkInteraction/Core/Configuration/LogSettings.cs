namespace Core.Configuration;

public class LogSettings
{
    public bool EnableConsole { get; set; } = true;
    public bool EnableFile { get; set; } = false;
    public string FilePath { get; set; } = "logs/test-run.log";
    public string MinLevel { get; set; } = "Information";
}
