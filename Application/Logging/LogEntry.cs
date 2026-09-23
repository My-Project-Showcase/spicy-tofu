using Application.Locators;

using Domain.Entities.Execution;

namespace Application.Logging;

public enum LogLevel
{
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3
}

public enum LogKind
{
    Message,
    Section,
    ActionStarted,
    ActionCompleted,
    ActionFailed,
    LocatorResolution
}

public sealed record LogEntry(
    LogLevel Level,
    DateTimeOffset Timestamp,
    LogKind Kind,
    string? Message = null,
    TestExecutionStep? Step = null,
    IReadOnlyList<LocatorCandidate>? LocatorCandidates = null,
    LocatorCandidate? SelectedLocator = null);
