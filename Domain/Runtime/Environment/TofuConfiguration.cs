using Domain.Runtime.Environment.Configuration;

namespace Domain.Runtime.Environment;

public class TofuConfiguration
{
    public SpicyTofuConfig SpicyTofuConfig { get; set; } = new();
    public PlaywrightConfig PlaywrightConfig { get; set; } = new();
    public TestExecution TestExecution { get; set; } = new();
}
