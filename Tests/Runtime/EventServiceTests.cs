using Xunit;

using Domain.Entities.Execution;
using Domain.Events;

using Infrastructure.Runtime.EventsService;

using SharedKernel.Attributes;

namespace Tests.Runtime;

public sealed class EventServiceTests
{
    [Fact]
    public void Get_IsCaseInsensitive()
    {
        var service = new EventService(new ITestEvent[] { new ClickTestEvent() });

        var resolved = service.Get("CLICK");

        Assert.IsType<ClickTestEvent>(resolved);
    }

    [Fact]
    public void TryGet_UnknownAction_ReturnsFalse()
    {
        var service = new EventService(new ITestEvent[] { new ClickTestEvent() });

        var found = service.TryGet("does-not-exist", out var testEvent);

        Assert.False(found);
        Assert.Null(testEvent);
    }

    [Fact]
    public void Get_UnknownAction_Throws()
    {
        var service = new EventService(new ITestEvent[] { new ClickTestEvent() });

        Assert.Throws<InvalidOperationException>(() => service.Get("missing"));
    }

    [Action("click")]
    private sealed class ClickTestEvent : ITestEvent
    {
        public Task<TestExecutionResult> ExecuteAsync(TestExecutionStep step)
            => Task.FromResult(new TestExecutionResult { IsSuccess = true });
    }
}
