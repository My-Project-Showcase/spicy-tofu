namespace Domain.Runtime.Environment.Configuration;

public sealed class LoggingConfig
{
    public string Level { get; set; } = "Information";

    public string OutputMode { get; set; } = string.Empty;

    public bool Timestamps { get; set; }

    public int MaxColumnWidth { get; set; } = 40;

    public int MaxLineWidth { get; set; } = 135;
}
