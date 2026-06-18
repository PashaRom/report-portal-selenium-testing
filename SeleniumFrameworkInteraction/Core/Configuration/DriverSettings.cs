using Core.Enum;

namespace Core.Configuration;

public class DriverSettings
{
    public BrowserType Browser { get; set; } = BrowserType.Chrome;
    public string? RemoteUri { get; set; }
    public bool Headless { get; set; } = false;
    public int WindowWidth  { get; set; } = 1920;
    public int WindowHeight { get; set; } = 1080;
}
