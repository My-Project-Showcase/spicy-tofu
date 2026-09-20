namespace Domain.Runtime.Environment.Configuration;

public sealed class TestExecution
{
    public bool Parallel { get; set; }
    public int Workers { get; set; }
    public int Retries { get; set; }
    public int DefaultTimeoutMs { get; set; }
}
