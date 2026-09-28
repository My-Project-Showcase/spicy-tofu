using Application.Automation;
using Application.Logging;
using Application.Runtime.JsonService;
using Application.Runtime.RunService;

using Domain.Entities.Execution;
using Domain.Events.EventsRegistry;

using Infrastructure.Runtime.TestExecution;

namespace Infrastructure.Runtime.RunServices;

public sealed class RunService : IRunService
{
    private readonly IAutomationDriver _driver;
    private readonly IJsonService _jsonService;
    private readonly ILogger _logger;
    private readonly IEventRegistry _eventRegistry;
    private readonly TestsLoadedHandler _testsLoadedHandler;

    public RunService(
        IAutomationDriver driver,
        IJsonService jsonService,
        ILogger logger,
        IEventRegistry eventRegistry,
        TestsLoadedHandler testsLoadedHandler)
    {
        _driver = driver;
        _jsonService = jsonService;
        _logger = logger;
        _eventRegistry = eventRegistry;
        _testsLoadedHandler = testsLoadedHandler;
    }

    public async Task<RunResult> RunAsync()
    {
        _logger.Section("Test Execution");

        var executed = 0;
        var failed = 0;

        try
        {
            await _driver.StartAsync();

            _logger.Info("Loading tests.");

            var testResult = await _jsonService.LoadJson();

            if (!testResult.Item1)
            {
                _logger.Warning("The test directory could not be loaded.");
                return new RunResult(executed, failed);
            }

            var steps = _testsLoadedHandler.Flatten(testResult.Item2);

            foreach (var step in steps)
            {
                _logger.ActionStarted(step);
                executed++;

                var result = await ExecuteStepAsync(step);

                if (result.IsSuccess)
                {
                    _logger.ActionCompleted(step);
                }
                else
                {
                    failed++;
                    _logger.ActionFailed(step, result.Error);
                }
            }

            _logger.Info($"Executed {executed} step(s), {failed} failed.");

            return new RunResult(executed, failed);
        }
        finally
        {
            await _driver.StopAsync();
        }
    }

    private async Task<TestExecutionResult> ExecuteStepAsync(TestExecutionStep step)
    {
        var action = step.Step.Type ?? string.Empty;

        if (!_eventRegistry.TryGet(action, out var testEvent) || testEvent is null)
        {
            return new TestExecutionResult
            {
                IsSuccess = false,
                Error = $"No event registered for action '{action}'.",
            };
        }

        try
        {
            return await testEvent.ExecuteAsync(step);
        }
        catch (Exception ex)
        {
            return new TestExecutionResult { IsSuccess = false, Error = ex.Message };
        }
    }
}
