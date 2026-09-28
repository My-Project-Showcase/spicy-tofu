using Xunit;

using Application.Automation;
using Application.Runtime.JsonService;

using Domain.Entities.Execution;
using Domain.Entities.TestCases;
using Domain.Events;
using Domain.Events.EventsRegistry;

using Infrastructure.Runtime.RunServices;
using Infrastructure.Runtime.TestExecution;

using Tests.Support;

namespace Tests.Runtime;

public sealed class RunServiceTests
{
    [Fact]
    public async Task RunAsync_StartsAndStopsDriver()
    {
        var driver = new FakeDriver();
        var service = CreateService(driver, new List<Test>());

        var result = await service.RunAsync();

        Assert.True(driver.Started);
        Assert.True(driver.Stopped);
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Executed);
    }

    [Fact]
    public async Task RunAsync_WhenLoadFails_ReturnsEmptyResultAndStopsDriver()
    {
        var driver = new FakeDriver();
        var service = CreateService(driver, new List<Test>(), loaded: false);

        var result = await service.RunAsync();

        Assert.True(driver.Stopped);
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Executed);
    }

    [Fact]
    public async Task RunAsync_FailingStep_ContinuesAndCountsFailures()
    {
        var logger = new FakeLogger();
        var registry = new FakeRegistry(new Dictionary<string, ITestEvent>
        {
            ["click"] = new StubEvent(new TestExecutionResult { IsSuccess = false, Error = "not found" }),
        });

        var service = CreateService(new FakeDriver(), SingleStepTest("click"), registry: registry, logger: logger);

        var result = await service.RunAsync();

        Assert.Equal(1, result.Executed);
        Assert.Equal(1, result.Failed);
        Assert.False(result.IsSuccess);
        Assert.Single(logger.Failures);
    }

    [Fact]
    public async Task RunAsync_UnknownAction_ReportsFailureAndContinues()
    {
        var logger = new FakeLogger();
        var service = CreateService(new FakeDriver(), SingleStepTest("unknown"), logger: logger);

        var result = await service.RunAsync();

        Assert.Equal(1, result.Executed);
        Assert.Equal(1, result.Failed);
        Assert.Contains("No event registered", logger.Failures[0]);
    }

    [Fact]
    public async Task RunAsync_EventThrows_ReportsFailureInsteadOfAborting()
    {
        var logger = new FakeLogger();
        var registry = new FakeRegistry(new Dictionary<string, ITestEvent>
        {
            ["click"] = new ThrowingEvent(),
        });

        var service = CreateService(new FakeDriver(), SingleStepTest("click"), registry: registry, logger: logger);

        var result = await service.RunAsync();

        Assert.Equal(1, result.Failed);
        Assert.Contains("boom", logger.Failures[0]);
    }

    [Fact]
    public async Task RunAsync_WhenStepThrows_StillStopsDriver()
    {
        var driver = new FakeDriver();
        var registry = new FakeRegistry(new Dictionary<string, ITestEvent>
        {
            ["click"] = new ThrowingEvent(),
        });

        var service = CreateService(driver, SingleStepTest("click"), registry: registry);

        await service.RunAsync();

        Assert.True(driver.Stopped);
    }

    private static RunService CreateService(
        FakeDriver driver,
        List<Test> tests,
        bool loaded = true,
        FakeRegistry? registry = null,
        FakeLogger? logger = null)
        => new(
            driver,
            new FakeJsonService(loaded, tests),
            logger ?? new FakeLogger(),
            registry ?? new FakeRegistry(new Dictionary<string, ITestEvent>()),
            new TestsLoadedHandler());

    private static List<Test> SingleStepTest(string type)
        => new()
        {
            new Test
            {
                Workflows = new List<Workflow>
                {
                    new()
                    {
                        Steps = new List<TestSteps>
                        {
                            new() { Type = type, Attribute = "A", Target = "T", Value = "V" },
                        },
                    },
                },
            },
        };

    private sealed class FakeDriver : IAutomationDriver
    {
        public bool Started { get; private set; }

        public bool Stopped { get; private set; }

        public Task StartAsync()
        {
            Started = true;
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            Stopped = true;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeJsonService : IJsonService
    {
        private readonly bool _loaded;
        private readonly List<Test> _tests;

        public FakeJsonService(bool loaded, List<Test> tests)
        {
            _loaded = loaded;
            _tests = tests;
        }

        public Task<Tuple<bool, List<Test>>> LoadJson()
            => Task.FromResult(Tuple.Create(_loaded, _tests));
    }

    private sealed class FakeRegistry : IEventRegistry
    {
        private readonly Dictionary<string, ITestEvent> _events;

        public FakeRegistry(Dictionary<string, ITestEvent> events)
        {
            _events = events;
        }

        public ITestEvent Get(string action) => _events[action];

        public bool TryGet(string action, out ITestEvent? testEvent)
            => _events.TryGetValue(action, out testEvent);
    }

    private sealed class StubEvent : ITestEvent
    {
        private readonly TestExecutionResult _result;

        public StubEvent(TestExecutionResult result)
        {
            _result = result;
        }

        public Task<TestExecutionResult> ExecuteAsync(TestExecutionStep step)
            => Task.FromResult(_result);
    }

    private sealed class ThrowingEvent : ITestEvent
    {
        public Task<TestExecutionResult> ExecuteAsync(TestExecutionStep step)
            => throw new InvalidOperationException("boom");
    }
}
