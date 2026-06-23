using Core.Enum;

namespace Core.Configuration;

public class DriverSettings
{
    /// <summary>
    /// List of browsers to run tests against.
    /// Each entry creates a separate fixture instance (cross-browser run).
    /// Defaults to Chrome when not configured.
    /// </summary>
    public List<BrowserType> Browsers { get; set; } = new() { BrowserType.Chrome };

    public bool Remote { get; set; } = false;
    public string? RemoteUri { get; set; }
    public bool Headless { get; set; } = false;
    public int WindowWidth { get; set; } = 1920;
    public int WindowHeight { get; set; } = 1080;
}
