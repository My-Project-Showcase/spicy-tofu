using System.Text.Json;

using Application.Runtime.JsonService;

using Domain.Entities.TestCases;
using Domain.Runtime.Environment.Configuration;

using Microsoft.Extensions.Options;

namespace Infrastructure.Runtime.JsonService;

public class JsonService : IJsonService
{
    private readonly IOptions<Projects> _projectsConfig;

    public event Action<List<Test>>? TestsLoaded;

    public JsonService(IOptions<Projects> projectsConfig)
    {
        _projectsConfig = projectsConfig;
    }

    public async Task<Tuple<bool, List<Test>>> LoadJson()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var directory = _projectsConfig.Value.RootDirectory;
        if (!Directory.Exists(directory))
        {
            return Tuple.Create(false, new List<Test>());
        }

        var tests = new List<Test>();

        foreach (var testFile in Directory.EnumerateFiles(directory, "*.json"))
        {
            try
            {
                var json = await File.ReadAllTextAsync(testFile);

                var test = JsonSerializer.Deserialize<Test>(json, options);

                if (test != null)
                {
                    tests.Add(test);
                }
            }
            catch (JsonException)
            {
                Console.WriteLine($"{testFile} was unable to load.");
            }
        }

        TestsLoaded?.Invoke(tests);

        return Tuple.Create(true, tests);
    }
}
