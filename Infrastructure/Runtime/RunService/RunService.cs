using Application.Automation;
using Application.Logging;
using Application.Runtime.JsonService;
using Application.Runtime.RunService;

using Domain.Entities.TestCases;

using Infrastructure.Runtime.TestExecution;

namespace Infrastructure.Runtime.RunServices;

public sealed class RunService : IRunService
{
    private readonly IAutomationDriver _driver;
    private readonly IJsonService _jsonService;
    private readonly ILogger _logger;
    private readonly TestsLoadedHandler _testsLoadedHandler;

    public RunService(
        IAutomationDriver driver,
        IJsonService jsonService,
        ILogger logger,
        TestsLoadedHandler testsLoadedHandler)
    {
        _driver = driver;
        _jsonService = jsonService;
        _logger = logger;
        _testsLoadedHandler = testsLoadedHandler;
    }

    public async Task RunAsync()
    {
        _logger.Section("Test Execution");

        try
        {
            await _driver.StartAsync();

            _logger.Info("Loading tests.");

            _jsonService.TestsLoaded += OnTestsLoaded;

            try
            {
                var testResult = await _jsonService.LoadJson();

                if (!testResult.Item1)
                {
                    _logger.Warning("The test directory could not be loaded.");
                }
            }
            finally
            {
                _jsonService.TestsLoaded -= OnTestsLoaded;
            }
        }
        finally
        {
            await _driver.StopAsync();
        }
    }

    private void OnTestsLoaded(List<Test> tests)
    {
        var steps = _testsLoadedHandler.Flatten(tests);

        foreach (var step in steps)
        {
            _logger.ActionStarted(step);
        }
    }
}
