using Application.Locators;
using Application.Logging;

using Domain.Entities.Execution;
using Domain.Runtime.Environment.Configuration;

using Microsoft.Extensions.Options;

namespace Infrastructure.Logging;

public sealed class Logger : ILogger
{
    private readonly IPrintStrategy _printStrategy;
    private readonly LogLevel _threshold;

    public Logger(IPrintStrategy printStrategy, IOptions<LoggingConfig> config)
    {
        _printStrategy = printStrategy;
        _threshold = ParseLevel(config.Value.Level);
    }

    public void Debug(string message) => Write(LogLevel.Debug, LogKind.Message, message);

    public void Info(string message) => Write(LogLevel.Info, LogKind.Message, message);

    public void Warning(string message) => Write(LogLevel.Warning, LogKind.Message, message);

    public void Error(string message) => Write(LogLevel.Error, LogKind.Message, message);

    public void Section(string title) => Write(LogLevel.Info, LogKind.Section, title);

    public void ActionStarted(TestExecutionStep executionStep) => WriteExecution(LogKind.ActionStarted, executionStep);

    public void LocatorResolution(TestExecutionStep executionStep, IReadOnlyList<LocatorCandidate> candidates, LocatorCandidate? selected = null)
    {
        if (_threshold > LogLevel.Info)
        {
            return;
        }

        var entry = new LogEntry(
            LogLevel.Info,
            DateTimeOffset.UtcNow,
            LogKind.LocatorResolution,
            Step: executionStep,
            LocatorCandidates: candidates,
            SelectedLocator: selected);

        _printStrategy.Render(entry);
    }

    public void ActionCompleted(TestExecutionStep executionStep) => WriteExecution(LogKind.ActionCompleted, executionStep);

    public void ActionFailed(TestExecutionStep executionStep, string reason) => WriteExecution(LogKind.ActionFailed, executionStep, reason);

    private void WriteExecution(LogKind kind, TestExecutionStep executionStep, string? reason = null)
    {
        if (_threshold > LogLevel.Info)
        {
            return;
        }

        var entry = new LogEntry(LogLevel.Info, DateTimeOffset.UtcNow, kind, reason, executionStep);
        _printStrategy.Render(entry);
    }

    private void Write(LogLevel level, LogKind kind, string message)
    {
        if (level < _threshold)
        {
            return;
        }

        var entry = new LogEntry(level, DateTimeOffset.UtcNow, kind, message);
        _printStrategy.Render(entry);
    }

    private static LogLevel ParseLevel(string level) =>
        Enum.TryParse<LogLevel>(level, ignoreCase: true, out var parsed)
            ? parsed
            : LogLevel.Info;
}
