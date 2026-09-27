namespace Domain.Entities.Execution;

public class TestExecutionResult
{
    public bool IsSuccess { get; set; }
    public string Error { get; set; } = string.Empty;
    public string Locator { get; set; } = string.Empty;
}
