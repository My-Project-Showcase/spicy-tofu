namespace SharedKernel.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ActionAttribute : Attribute
{
    public string Name { get; set; }

    public ActionAttribute(string name)
    {
        Name = name;
    }
}
