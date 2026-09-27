using Application.Automation;
using Application.Logging;
using Application.Runtime.JsonService;
using Application.Runtime.RunService;

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

    public async Task RunAsync()
    {
        _logger.Section("Test Execution");

        try
        {
            await _driver.StartAsync();

            _logger.Info("Loading tests.");

            var testResult = await _jsonService.LoadJson();

            if (!testResult.Item1)
            {
                _logger.Warning("The test directory could not be loaded.");
                return;
            }

            var steps = _testsLoadedHandler.Flatten(testResult.Item2);

            foreach (var step in steps)
            {
                _logger.ActionStarted(step);

                var testEvent = _eventRegistry.Get(step.Step.Type);

                var result = await testEvent.ExecuteAsync(step);

                if (result.IsSuccess)
                {
                    _logger.ActionCompleted(step);
                }
                else
                {
                    _logger.ActionFailed(step, result.Error);
                }
            }
        }
        finally
        {
            await _driver.StopAsync();
        }
    }
}
