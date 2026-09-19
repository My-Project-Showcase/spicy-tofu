namespace Domain.Runtime.Environment.Configuration;

public class Appium
{
    public string ServerUrl { get; set; } = string.Empty;
    public string PlatformName { get; set; } = string.Empty;
    public string AutomationName { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string App { get; set; } = string.Empty;
    public bool NoReset { get; set; }
    public int NewCommandTimeoutSec { get; set; }
}
