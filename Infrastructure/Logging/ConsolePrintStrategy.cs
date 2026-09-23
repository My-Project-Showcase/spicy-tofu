using System.Text;

using Application.Locators;
using Application.Logging;

using Domain.Entities.Execution;
using Domain.Runtime.Environment.Configuration;

using Microsoft.Extensions.Options;

namespace Infrastructure.Logging;

public sealed class ConsolePrintStrategy : IPrintStrategy
{
    private const int ColumnGap = 3;
    private const string ContinuationIndent = "    ";
    private const int InteractiveIndent = 4;
    private const int InteractiveLabelWidth = 12;

    private readonly LoggingConfig _config;
    private readonly OutputMode _mode;
    private readonly object _syncRoot = new();

    public ConsolePrintStrategy(IOptions<LoggingConfig> config)
    {
        _config = config.Value;
        _mode = OutputModeDetector.Resolve(_config);
    }

    public void Render(LogEntry entry)
    {
        foreach (var line in Format(entry))
        {
            lock (_syncRoot)
            {
                Console.WriteLine(line);
            }
        }
    }

    private List<string> Format(LogEntry entry) =>
        entry.Kind switch
        {
            LogKind.Section => FormatSection(entry),
            LogKind.ActionStarted => FormatActionStarted(entry),
            LogKind.ActionCompleted => FormatActionCompleted(entry),
            LogKind.ActionFailed => FormatActionFailed(entry),
            LogKind.LocatorResolution => FormatLocatorResolution(entry),
            _ => FormatMessage(entry)
        };

    private List<string> FormatMessage(LogEntry entry)
    {
        var prefix = string.Empty;
        if (_config.Timestamps)
        {
            prefix = $"[{entry.Timestamp.ToLocalTime():HH:mm:ss}] ";
        }

        if (entry.Level == LogLevel.Debug)
        {
            prefix += "DEBUG: ";
        }
        else if (entry.Level == LogLevel.Warning)
        {
            prefix += "WARN: ";
        }
        else if (entry.Level == LogLevel.Error)
        {
            prefix += "ERROR: ";
        }

        return WrapLine(entry.Message ?? string.Empty, _config.MaxLineWidth, prefix, ContinuationIndent);
    }

    private static List<string> FormatSection(LogEntry entry)
    {
        var title = (entry.Message ?? string.Empty).ToUpperInvariant();
        var rule = new string('-', Math.Max(40, title.Length));

        return new List<string>
        {
            string.Empty,
            title,
            rule,
            string.Empty
        };
    }

    private List<string> FormatActionStarted(LogEntry entry)
    {
        var fields = StepFields(entry.Step);
        return WrapLine(
            $"Action: {fields.Action}, Attribute: {fields.Attribute}, Target: {fields.Target}, Value: {fields.Value}, Workflow: {fields.Workflow}, TestCase: {fields.TestCase}",
            _config.MaxLineWidth,
            string.Empty,
            ContinuationIndent);
    }

    private List<string> FormatActionCompleted(LogEntry entry)
    {
        var fields = StepFields(entry.Step);
        return WrapLine($"Action completed: {fields.Action}", _config.MaxLineWidth, string.Empty, ContinuationIndent);
    }

    private List<string> FormatActionFailed(LogEntry entry)
    {
        var fields = StepFields(entry.Step);
        var reason = entry.Message ?? string.Empty;

        if (_mode == OutputMode.Ci)
        {
            var line = reason.Length == 0
                ? $"ERROR: Action failed: {fields.Action}"
                : $"ERROR: Action failed: {fields.Action}: {reason}";
            return WrapLine(line, _config.MaxLineWidth, string.Empty, ContinuationIndent);
        }

        var lines = new List<string> { $"Action failed: {fields.Action}" };
        if (reason.Length > 0)
        {
            lines.AddRange(WrapLine($"Reason: {reason}", _config.MaxLineWidth, ContinuationIndent, new string(' ', ContinuationIndent.Length + "Reason: ".Length)));
        }

        return lines;
    }

    private List<string> FormatLocatorResolution(LogEntry entry)
    {
        var fields = StepFields(entry.Step);
        var lines = new List<string> { "Locator Resolution" };

        if (_mode == OutputMode.Ci)
        {
            lines.AddRange(WrapLine($"TestCase: {fields.TestCase}", _config.MaxLineWidth, string.Empty, ContinuationIndent));
            lines.AddRange(WrapLine($"Workflow: {fields.Workflow}", _config.MaxLineWidth, string.Empty, ContinuationIndent));
            lines.AddRange(WrapLine($"Action: {fields.Action}", _config.MaxLineWidth, string.Empty, ContinuationIndent));
            lines.AddRange(WrapLine($"Target: {fields.Target}", _config.MaxLineWidth, string.Empty, ContinuationIndent));
        }
        else
        {
            lines.AddRange(Field("TestCase", fields.TestCase));
            lines.AddRange(Field("Workflow", fields.Workflow));
            lines.AddRange(Field("Action", fields.Action));
            lines.AddRange(Field("Target", fields.Target));
        }

        lines.Add(string.Empty);
        lines.AddRange(FormatLocatorTable(entry.LocatorCandidates));
        lines.Add(string.Empty);

        var selected = entry.SelectedLocator;
        if (selected is not null)
        {
            lines.AddRange(WrapLine($"Selected: {selected.Strategy}, {selected.Value}", _config.MaxLineWidth, string.Empty, ContinuationIndent));
        }
        else if (entry.LocatorCandidates is not null && entry.LocatorCandidates.Count > 0)
        {
            lines.Add("No locator selected.");
        }

        return lines;
    }

    private List<string> FormatLocatorTable(IReadOnlyList<LocatorCandidate>? candidates)
    {
        if (candidates is null || candidates.Count == 0)
        {
            return new List<string> { "(no locator candidates)" };
        }

        var headers = new List<string> { "Strategy", "Locator" };
        var rows = candidates
            .Select(candidate => (IReadOnlyList<string>)new string[] { candidate.Strategy, candidate.Value })
            .ToList();

        return FormatTable(headers, rows);
    }

    private List<string> FormatTable(List<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
    {
        var widths = Enumerable.Range(0, headers.Count)
            .Select(index => ColumnWidth(headers[index], index, rows))
            .ToArray();

        var lines = new List<string>();
        lines.AddRange(RenderRow(headers, widths));

        var separatorWidth = widths.Sum() + ColumnGap * (widths.Length - 1);
        lines.Add(new string('-', separatorWidth));

        foreach (var row in rows)
        {
            lines.AddRange(RenderRow(row, widths));
        }

        return lines;
    }

    private List<string> Field(string label, string value)
    {
        var prefix = new string(' ', InteractiveIndent) + label.PadRight(InteractiveLabelWidth) + ": ";
        return WrapLine(value, _config.MaxLineWidth, prefix, new string(' ', prefix.Length));
    }

    private int ColumnWidth(string header, int index, IReadOnlyList<IReadOnlyList<string>> rows)
    {
        var width = header.Length;
        foreach (var row in rows)
        {
            if (index < row.Count)
            {
                width = Math.Max(width, row[index].Length);
            }
        }

        return Math.Min(Math.Max(1, _config.MaxColumnWidth), width);
    }

    private static List<string> RenderRow(IReadOnlyList<string> row, int[] widths)
    {
        var cells = widths
            .Select((width, index) => Wrap(index < row.Count ? row[index] : string.Empty, width))
            .ToList();

        var lineCount = cells.Max(cell => cell.Count);
        var lines = new List<string>();

        for (var lineIndex = 0; lineIndex < lineCount; lineIndex++)
        {
            var line = new StringBuilder();
            for (var column = 0; column < widths.Length; column++)
            {
                var cellLines = cells[column];
                var value = lineIndex < cellLines.Count ? cellLines[lineIndex] : string.Empty;
                if (column > 0)
                {
                    line.Append(new string(' ', ColumnGap));
                }

                line.Append(value.PadRight(widths[column]));
            }

            lines.Add(line.ToString());
        }

        return lines;
    }

    private static (string Action, string Attribute, string Target, string Value, string Workflow, string TestCase) StepFields(TestExecutionStep? step) =>
        step is null
            ? (string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)
            : (step.Step.Type, step.Step.Attribute, step.Step.Target, step.Step.Value, step.Workflow.Name, step.Test.Name);

    private static List<string> WrapLine(string text, int width, string prefix, string continuationIndent)
    {
        var raw = Wrap(text, Math.Max(1, width - prefix.Length));
        var lines = new List<string>(raw.Count);
        for (var i = 0; i < raw.Count; i++)
        {
            lines.Add(i == 0 ? prefix + raw[i] : continuationIndent + raw[i]);
        }

        return lines;
    }

    private static List<string> Wrap(string text, int width)
    {
        var lines = new List<string>();
        var words = (text ?? string.Empty).Split(' ');
        var current = new StringBuilder();

        foreach (var word in words)
        {
            if (current.Length == 0)
            {
                current.Append(word);
            }
            else if (current.Length + 1 + word.Length <= width)
            {
                current.Append(' ');
                current.Append(word);
            }
            else
            {
                lines.Add(current.ToString());
                current.Clear();
                current.Append(word);
            }

            while (current.Length >= width)
            {
                lines.Add(current.ToString(0, width));
                current.Remove(0, width);
            }
        }

        if (current.Length > 0)
        {
            lines.Add(current.ToString());
        }

        if (lines.Count == 0)
        {
            lines.Add(string.Empty);
        }

        return lines;
    }
}
