---
title: Logging
updated: 2026-09-23
sources:
  - ../../Application/Logging/ILogger.cs
  - ../../Application/Logging/LogEntry.cs
  - ../../Application/Logging/IPrintStrategy.cs
  - ../../Application/Locators/LocatorCandidate.cs
  - ../../Infrastructure/Logging/Logger.cs
  - ../../Infrastructure/Logging/ConsolePrintStrategy.cs
  - ../../Infrastructure/Logging/OutputMode.cs
  - ../../Infrastructure/Logging/OutputModeDetector.cs
  - ../../Domain/Runtime/Environment/Configuration/LoggingConfig.cs
  - ../../Infrastructure/Extensions/DependencyInjection.cs
---

# Logging

The framework prints test-execution progress as plain text through a small pipeline: `ILogger` builds `LogEntry` values and `IPrintStrategy` renders each entry to the console. It is an execution-oriented logging system for the framework itself, not a generic application logger, so there is no Serilog, NLog, `Microsoft.Extensions.Logging`, message bus, or event pipeline behind it.

## Flow

`ILogger` (the facade) and `IPrintStrategy` (the rendering seam) are core interfaces in `Application.Logging`. Their implementations live in `Infrastructure.Logging`. Consumers such as `RunService` take `ILogger` and never touch `Console` directly.

```
Execution Layer
      | calls ILogger
      v
LogEntry (structured, presentation-neutral)
      |
      v
IPrintStrategy
      |
      v
Formatted plain text
      |
      v
Console
```

- `Logger` filters entries below the configured `Logging:Level` threshold and stamps each entry with a UTC timestamp, then hands the entry to the strategy.
- `ConsolePrintStrategy` turns the `LogEntry` into one or more plain-text lines and writes each line with a single `Console.WriteLine` under a lock, so a line is never interleaved with a concurrent write.
- `LogEntry` is presentation-neutral: it carries the level, timestamp, kind, and structured content, but no layout decisions.

## Logger API

| Method | `LogKind` | Purpose |
|---|---|---|
| `Debug`, `Info`, `Warning`, `Error` | Message | Framework message lines with a `DEBUG:` / `WARN:` / `ERROR:` prefix for non-info levels. |
| `Section` | Section | Run-level structure: blank line, title, dash rule, blank line. |
| `ActionStarted(TestExecutionStep)` | ActionStarted | A single structured kwargs line describing the action about to run, comma-separated in the same shape for both modes. |
| `LocatorResolution(TestExecutionStep, IReadOnlyList<LocatorCandidate>, LocatorCandidate? selected)` | LocatorResolution | The locator candidate table and the selected locator. |
| `ActionCompleted(TestExecutionStep)` | ActionCompleted | A completion line for the action. |
| `ActionFailed(TestExecutionStep, string reason)` | ActionFailed | A failure line for the action with the failure reason. |

There is no generic `KeyValue` or `Table` method on `ILogger`. Table rendering belongs to the printing strategy below the interface, so the facade stays execution-focused.

## LogEntry

```csharp
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
```

One record carries all kinds. `ActionFailed` puts its reason in `Message`. There is no log-event class per concept.

## Execution context

The execution methods take the existing `TestExecutionStep` directly. It already composes `Test`, `Workflow`, and `TestSteps` (whose `Type`, `Attribute`, `Target`, and `Value` are the action fields), so no duplicate context class exists. A call looks like `_logger.ActionStarted(executionStep)`.

## Locator candidates

`Application.Locators.LocatorCandidate(string Strategy, string Value)` is the structured handoff for locator information. The strategy is a plain string, so the logger never couples to Playwright or Appium locator classes, nor to the `Domain.Entities.Enums.LocatorAttribute` SmartEnum. The selected locator is the same record, reusing one shape for candidate rows and the selected line.

## Output mode

`OutputMode` (`Interactive`, `Ci`) is resolved by `OutputModeDetector`: explicit `Logging:OutputMode`, then the `CI` environment variable, then `Interactive`. The logger API is identical in both modes; only `ConsolePrintStrategy` diverges in layout.

The CI rendering example:

```text
TEST EXECUTION
----------------------------------------

Loading tests.
Action: Fill, Attribute: Id, Target: username, Value: alice, Workflow: Login Flow, TestCase: Valid Login
Locator Resolution
TestCase: Valid Login
Workflow: Login Flow
Action: Fill
Target: username

Strategy        Locator
--------------  ------------
Id              username
Name            username
CssSelector     #username
XPath           //input[@name='username']

Selected: CssSelector, #username

Action completed: Fill
```

The Interactive rendering example (same call; the action line matches CI, Locator Resolution uses spaced fields):

```text
TEST EXECUTION
----------------------------------------

Loading tests.
Action: Fill, Attribute: Id, Target: username, Value: alice, Workflow: Login Flow, TestCase: Valid Login

Locator Resolution
            TestCase: Valid Login
            Workflow: Login Flow
            Action: Fill
            Target: username

Strategy        Locator
--------------  ------------
Id              username
Name            username
CssSelector     #username
XPath           //input[@name='username']

Selected: CssSelector, #username

Action completed: Fill
```

An action failure renders as `Action failed: Fill` plus a wrapped `Reason:` line (Interactive) or a single `ERROR: Action failed: Fill: <reason>` line (CI).

Neither mode uses ANSI escape sequences, colours, cursor manipulation, terminal-width reads, interactive-terminal behaviour, or emojis. All output survives as ordinary CI log text. These renderings are current proposals; the exact shapes can evolve without changing the logger API.

## Table formatting

Table rendering (column sizing, word wrap, the dash separator) lives inside `ConsolePrintStrategy` and is used today by `LocatorResolution`. It is a private formatting helper, not a generic public "pretty table" feature. A future test-execution summary kind can reuse the same helper.

## Level filtering

`Logger` compares each call's level with the configured threshold:

- `Debug` requires `Logging:Level` = `Debug`.
- `Information` is the default threshold, so `Info`, `Section`, and the execution calls render by default.
- `Warning` and `Error` render when the threshold is `Warning` or higher.

`Logging:Level` parses case-insensitively against the `LogLevel` enum (`Debug`, `Info`, `Warning`, `Error`); an unknown value falls back to `Information`.

## Formatting rules

- All output is plain text; message and value text wraps at `Logging:MaxLineWidth` (100 by default) with continuation lines indented 4 spaces.
- Table columns wrap at `Logging:MaxColumnWidth` (40 by default) with a 3-space gap between columns.
- Layout is chosen per entry, not per run block, so parallel workers cannot desynchronize shared output.

## Configuration

`Logging` is a core section bound by both platforms in `AddServices`. The keys and defaults:

| Key | Default | Meaning |
|---|---|---|
| `Logging:Level` | `Information` | Log threshold. |
| `Logging:OutputMode` | empty | `Interactive` or `Ci`; see Output mode. |
| `Logging:Timestamps` | `false` | Prints `[HH:mm:ss]` before message lines. |
| `Logging:MaxColumnWidth` | `40` | Cap for table column widths. |
| `Logging:MaxLineWidth` | `100` | Cap for message line width. |

`TOFU_Logging__Level` and the other keys override these through the existing environment-variable overlay (the `TOFU_` prefix separates from the section, `__` separates segments). The section is not present in the `appsettings.json` files; the defaults above apply unless overlaid.

## Registration

`AddServices` in `Infrastructure.Extensions.DependencyInjection` binds `LoggingConfig` to the `Logging` section and registers `ILogger` (`Logger`) and `IPrintStrategy` (`ConsolePrintStrategy`) as singletons. No other registrations are needed for execution-oriented logging.

## Future file output

A `FilePrintStrategy : IPrintStrategy` can reuse the same plain-text rendering and is swappable in DI without touching `Logger` or any consumer.

## Related pages

- [Runtime Pipeline](./runtime-pipeline.md)
- [Dependency Injection](./dependency-injection.md)
- [Configuration](./configuration.md)