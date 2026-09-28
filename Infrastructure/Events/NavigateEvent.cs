using Application.Automation;

using Domain.Entities.Execution;
using Domain.Events;

using Action = SharedKernel.Attributes.ActionAttribute;

namespace Infrastructure.Events;

[Action("navigate")]
public sealed class NavigateEvent : ITestEvent
{
    private readonly INavigator _navigator;

    public NavigateEvent(INavigator navigator)
    {
        _navigator = navigator;
    }

    public async Task<TestExecutionResult> ExecuteAsync(TestExecutionStep step)
    {
        var url = step.Step.Value;

        if (string.IsNullOrWhiteSpace(url))
        {
            return new TestExecutionResult
            {
                IsSuccess = false,
                Error = "No URL was provided for the navigate action."
            };
        }

        try
        {
            await _navigator.NavigateAsync(url);

            return new TestExecutionResult { IsSuccess = true };
        }
        catch (Exception ex)
        {
            return new TestExecutionResult { IsSuccess = false, Error = ex.Message };
        }
    }
}
