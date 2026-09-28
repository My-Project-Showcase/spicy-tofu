using Application.Logging;

using Domain.Entities.Execution;

using SharedKernel.Locators;

namespace Tests.Support;

public sealed class FakeLogger : ILogger
{
    public List<string> Failures { get; } = new();

    public void Debug(string message)
    {
    }

    public void Info(string message)
    {
    }

    public void Warning(string message)
    {
    }

    public void Error(string message)
    {
    }

    public void Section(string title)
    {
    }

    public void ActionStarted(TestExecutionStep executionStep)
    {
    }

    public void LocatorResolution(
        TestExecutionStep executionStep,
        IReadOnlyList<Locator> candidates,
        Locator? selected = null)
    {
    }

    public void ActionCompleted(TestExecutionStep executionStep)
    {
    }

    public void ActionFailed(TestExecutionStep executionStep, string reason)
        => Failures.Add(reason);
}
