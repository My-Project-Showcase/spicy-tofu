using Domain.Entities.Execution;

using SharedKernel.Locators;

namespace Application.Logging;

public interface ILogger
{
    void Debug(string message);

    void Info(string message);

    void Warning(string message);

    void Error(string message);

    void Section(string title);

    void ActionStarted(TestExecutionStep executionStep);

    void LocatorResolution(TestExecutionStep executionStep, IReadOnlyList<Locator> candidates, Locator? selected = null);

    void ActionCompleted(TestExecutionStep executionStep);

    void ActionFailed(TestExecutionStep executionStep, string reason);
}
