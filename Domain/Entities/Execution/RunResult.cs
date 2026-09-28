namespace Domain.Entities.Execution;

public sealed record RunResult(int Executed, int Failed)
{
    public bool IsSuccess => Failed == 0;
}
