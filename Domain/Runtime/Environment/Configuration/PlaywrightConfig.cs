namespace Domain.Runtime.Environment.Configuration;

public sealed class PlaywrightConfig
{
    public string Browser { get; set; } = string.Empty;
    public bool Headless { get; set; }
    public int TimeOut { get; set; }
    public int NavigationTimeout { get; set; }
}
