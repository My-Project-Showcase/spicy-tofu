using Xunit;

using Domain.Entities.Execution;
using Domain.Entities.TestCases;

using Infrastructure.Runtime.TestExecution;

namespace Tests.Runtime;

public sealed class TestsLoadedHandlerTests
{
    private readonly TestsLoadedHandler _handler = new();

    [Fact]
    public void Flatten_ProducesOneStepPerTestStepInOrder()
    {
        var tests = new List<Test>
        {
            new()
            {
                Id = "TC-1",
                Name = "Test 1",
                Workflows = new List<Workflow>
                {
                    new()
                    {
                        Id = "WF-1",
                        Name = "Workflow 1",
                        Steps = new List<TestSteps>
                        {
                            new() { Type = "navigate", Value = "https://example.com" },
                            new() { Type = "click", Attribute = "LoginButton", Target = "LoginButton" },
                        },
                    },
                },
            },
        };

        var steps = _handler.Flatten(tests).ToList();

        Assert.Equal(2, steps.Count);
        Assert.Equal("navigate", steps[0].Step.Type);
        Assert.Equal("click", steps[1].Step.Type);
        Assert.Same(tests[0], steps[0].Test);
        Assert.Same(tests[0].Workflows![0], steps[0].Workflow);
    }

    [Fact]
    public void Flatten_WithNullWorkflows_ReturnsEmpty()
    {
        var tests = new List<Test> { new() { Workflows = null } };

        var steps = _handler.Flatten(tests).ToList();

        Assert.Empty(steps);
    }

    [Fact]
    public void Flatten_WithNullSteps_SkipsWorkflow()
    {
        var tests = new List<Test>
        {
            new()
            {
                Workflows = new List<Workflow> { new() { Steps = null } },
            },
        };

        var steps = _handler.Flatten(tests).ToList();

        Assert.Empty(steps);
    }
}
