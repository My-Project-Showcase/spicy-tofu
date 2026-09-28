using System.Text.Json;
using Microsoft.Extensions.Options;

using Application.Logging;
using Application.Runtime.JsonService;

using Domain.Entities.TestCases;
using Domain.Runtime.Environment.Configuration;

namespace Infrastructure.Runtime.JsonService;

public class JsonService : IJsonService
{
    private readonly IOptions<Projects> _projectsConfig;
    private readonly ILogger _logger;

    public JsonService(IOptions<Projects> projectsConfig, ILogger logger)
    {
        _projectsConfig = projectsConfig;
        _logger = logger;
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
                _logger.Warning($"{testFile} was unable to load.");
            }
        }

        return Tuple.Create(true, tests);
    }
}
