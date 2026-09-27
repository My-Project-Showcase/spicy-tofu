using Action = SharedKernel.Attributes.ActionAttribute;

using Domain.Events;
using Domain.Entities.Execution;

namespace Infrastructure.Events;

[Action("click")]
public class ClickEvent: ITestEvent
{
   public Task<TestExecutionResult> ExecuteAsync(TestExecutionStep step)
    {
        return Task.FromResult(new TestExecutionResult
        {
            IsSuccess = true,
            Error = "",
            Locator = ""
        });
    }
}
