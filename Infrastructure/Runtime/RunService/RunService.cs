using Application.Logging;
using Application.Runtime.JsonService;
using Application.Runtime.RunService;

using Domain.Entities.TestCases;

using Infrastructure.Runtime.TestExecution;

namespace Infrastructure.Runtime.RunServices;

public class RunService : IRunService
{
    private readonly IJsonService _jsonService;
    private readonly ILogger _logger;
    private readonly TestsLoadedHandler _testsLoadedHandler;

    public RunService(IJsonService jsonService, ILogger logger, TestsLoadedHandler testsLoadedHandler)
    {
        _jsonService = jsonService;
        _logger = logger;
        _testsLoadedHandler = testsLoadedHandler;
    }

    public async Task RunAsync()
    {
        _logger.Section("Test Execution");
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

    private void OnTestsLoaded(List<Test> tests)
    {
        var steps = _testsLoadedHandler.Flatten(tests);

        foreach (var step in steps)
        {
            _logger.ActionStarted(step);
        }
    }
}
