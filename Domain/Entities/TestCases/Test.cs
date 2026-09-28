using Domain.Shared;

namespace Domain.Entities.TestCases;

/// <summary>
/// Defines the structure each test case needs to follow
/// -> Highlights the higher level structure design
/// <summary>
public class Test : AggregrateRoot
{
    public new string? Id { get; set; }
    public new string? Name { get; set; }
    public List<Workflow>? Workflows { get; set; }
}
