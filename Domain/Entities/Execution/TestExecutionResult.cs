namespace Domain.Entities.Execution;

public class TestExecutionResult
{
    public bool IsSuccess { get; set; }
    public string Error { get; set; }
    public string Locator { get; set; }
}
