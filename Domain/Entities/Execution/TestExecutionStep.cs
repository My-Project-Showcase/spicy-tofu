using Domain.Entities.TestCases;

namespace Domain.Entities.Execution;

public record TestExecutionStep(Test Test, Workflow Workflow, TestSteps Step);
