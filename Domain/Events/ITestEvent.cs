using Domain.Entities.Execution;

namespace Domain.Events;

public interface ITestEvent
{
    Task<TestExecutionResult> ExecuteAsync(TestExecutionStep step);
}
