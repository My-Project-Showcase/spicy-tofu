using Application.Runtime.JsonService;
using Application.Runtime.RunService;

using Domain.Entities.TestCases;

using Infrastructure.Runtime.TestExecution;

namespace Infrastructure.Runtime.RunServices;

public class RunService : IRunService
{
    private readonly IJsonService _jsonService;
    private readonly TestsLoadedHandler _testsLoadedHandler;

    public RunService(IJsonService jsonService, TestsLoadedHandler testsLoadedHandler)
    {
        _jsonService = jsonService;
        _testsLoadedHandler = testsLoadedHandler;
    }

    public async Task RunAsync()
    {
        _jsonService.TestsLoaded += OnTestsLoaded;

        try
        {
            var testResult = await _jsonService.LoadJson();

            if (!testResult.Item1)
            {
                Console.WriteLine("Oops! Something went wrong");
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
            Console.WriteLine($"Test: {step.Test.Name}");
            Console.WriteLine($"Workflow: {step.Workflow.Name}");
            Console.WriteLine($"Step: {step.Step.Type}");
        }
    }
}
