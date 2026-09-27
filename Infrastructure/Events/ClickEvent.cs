using Domain.Entities.Execution;
using Domain.Events;

using Action = SharedKernel.Attributes.ActionAttribute;

namespace Infrastructure.Events;

[Action("click")]
public sealed class ClickEvent : ITestEvent
{
    public Task<TestExecutionResult> ExecuteAsync(TestExecutionStep step)
    {
        return Task.FromResult(new TestExecutionResult
        {
            IsSuccess = true
        });
    }
}
