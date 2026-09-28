using Domain.Entities.Execution;

namespace Application.Runtime.RunService;

public interface IRunService
{
    Task<RunResult> RunAsync();
}
