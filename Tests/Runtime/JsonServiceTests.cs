using Microsoft.Extensions.Options;
using Xunit;

using Domain.Runtime.Environment.Configuration;

using Infrastructure.Runtime.JsonService;

using Tests.Support;

namespace Tests.Runtime;

public sealed class JsonServiceTests
{
    [Fact]
    public async Task LoadJson_MissingDirectory_ReturnsFalse()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var service = CreateService(directory);

        var result = await service.LoadJson();

        Assert.False(result.Item1);
        Assert.Empty(result.Item2);
    }

    [Fact]
    public async Task LoadJson_ValidFile_ReturnsDeserializedTest()
    {
        var directory = CreateTempDirectory();
        try
        {
            File.WriteAllText(
                Path.Combine(directory, "login.json"),
                """
                {
                  "Id": "TC-001",
                  "Name": "Valid login",
                  "Workflows": [
                    {
                      "Id": "WF-001",
                      "Name": "Login",
                      "Steps": [
                        { "Type": "click", "Attribute": "LoginButton", "Target": "LoginButton", "Value": "" }
                      ]
                    }
                  ]
                }
                """);

            var service = CreateService(directory);

            var result = await service.LoadJson();

            Assert.True(result.Item1);
            var test = Assert.Single(result.Item2);
            Assert.Equal("TC-001", test.Id);
            Assert.Equal("click", test.Workflows![0].Steps![0].Type);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public async Task LoadJson_MalformedFile_IsSkipped()
    {
        var directory = CreateTempDirectory();
        try
        {
            File.WriteAllText(Path.Combine(directory, "broken.json"), "{ not valid json");

            var service = CreateService(directory);

            var result = await service.LoadJson();

            Assert.True(result.Item1);
            Assert.Empty(result.Item2);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    private static JsonService CreateService(string directory)
        => new(Options.Create(new Projects { RootDirectory = directory }), new FakeLogger());

    private static string CreateTempDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(directory);
        return directory;
    }
}
