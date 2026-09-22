using Domain.Entities.Execution;
using Domain.Entities.TestCases;

namespace Infrastructure.Runtime.TestExecution;

public sealed class TestsLoadedHandler
{
    public IEnumerable<TestExecutionStep> Flatten(List<Test> tests)
        => tests.SelectMany(test => test.Workflows
            .SelectMany(workflow => workflow.Steps
                .Select(step => new TestExecutionStep(test, workflow, step))));
}
