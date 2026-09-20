namespace Domain.Runtime.Environment.Configuration;

public sealed class AppiumConfig
{
    public string ServerUrl { get; set; } = string.Empty;
    public string PlatformName { get; set; } = string.Empty;
    public string AutomationName { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string PlatformVersion { get; set; } = string.Empty;
    public string App { get; set; } = string.Empty;
    public string AvdName { get; set; } = string.Empty;
    public string AndroidSdkPath { get; set; } = string.Empty;
    public string IosSimulatorUdid { get; set; } = string.Empty;
    public string AppiumServerExecutable { get; set; } = "appium";
    public bool NoReset { get; set; }
    public int NewCommandTimeoutSec { get; set; }
}
