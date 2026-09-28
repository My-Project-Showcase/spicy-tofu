using Xunit;

using Infrastructure.Elements;

namespace Tests.Elements;

public sealed class SampleElementRepositoryTests
{
    [Fact]
    public void Get_ByTarget_ReturnsElement()
    {
        var repository = new SampleElementRepository();

        var element = repository.Get("anything", "LoginButton");

        Assert.Equal("LoginButton", element.Name);
    }

    [Fact]
    public void Get_ByAttribute_ReturnsElement()
    {
        var repository = new SampleElementRepository();

        var element = repository.Get("Username", "unknown-target");

        Assert.Equal("UsernameInput", element.Name);
    }

    [Fact]
    public void Get_UnknownAttributeAndTarget_Throws()
    {
        var repository = new SampleElementRepository();

        Assert.Throws<InvalidOperationException>(() => repository.Get("nope", "nope"));
    }
}
