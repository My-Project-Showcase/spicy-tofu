using Application.Runtime.RunService;
using Application.Runtime.JsonService;
using Domain.Entities.TestCases;

namespace Infrastructure.Runtime.RunServices;

public class RunService: IRunService
{
    private readonly IJsonService _jsonService;

    public RunService(IJsonService jsonService)
    {
        _jsonService = jsonService;
    }
    
    public async Task RunAsync()
    {
        var testResult = await _jsonService.LoadJson();

        if (!testResult.Item1){
            Console.WriteLine("Oops! Something went wrong");
        }

        var testList = testResult.Item2;
        var steps = testList
            .SelectMany(test => test.Workflows
                .SelectMany(workflow => workflow.Steps
                    .Select(step => new
                    {
                        Test = test,
                        Workflow = workflow,
                        Step = step
                    })));
        
        foreach (var item in steps)
        {
            Console.WriteLine($"Test: {item.Test.Name}");
            Console.WriteLine($"Workflow: {item.Workflow.Name}");
            Console.WriteLine($"Step: {item.Step.Type}");
        }
    }
}
