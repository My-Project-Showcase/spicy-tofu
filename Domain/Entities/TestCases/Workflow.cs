namespace Domain.Entities.TestCases;

public class Workflow
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<TestSteps> Steps { get; set; }
}
