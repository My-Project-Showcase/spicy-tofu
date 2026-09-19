using Domain.Shared;

namespace Domain.Entities.TestCases;

public class Test : AggregrateRoot
{
    public List<Workflow> Workflows { get; set; }
}
