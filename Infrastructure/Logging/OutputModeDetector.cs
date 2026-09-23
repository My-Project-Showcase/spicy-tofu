using Domain.Runtime.Environment.Configuration;

namespace Infrastructure.Logging;

public static class OutputModeDetector
{
    public static OutputMode Resolve(LoggingConfig config)
    {
        if (!string.IsNullOrWhiteSpace(config.OutputMode))
        {
            return Enum.TryParse<OutputMode>(config.OutputMode, ignoreCase: true, out var parsed)
                ? parsed
                : OutputMode.Interactive;
        }

        var ci = Environment.GetEnvironmentVariable("CI");
        return !string.IsNullOrWhiteSpace(ci) && !bool.FalseString.Equals(ci, StringComparison.OrdinalIgnoreCase)
            ? OutputMode.Ci
            : OutputMode.Interactive;
    }
}
