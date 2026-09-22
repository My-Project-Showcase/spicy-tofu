using Application.Runtime.JsonService;

using Domain.Runtime.Environment.Configuration;
using Domain.Entities.TestCases;

using Microsoft.Extensions.Options;

using System.Text.Json;

namespace Infrastructure.Runtime.JsonService;

public class JsonService: IJsonService
{
    private readonly IOptions<Projects> _projectsConfig;
    
    


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
        Console.WriteLine($"Directory: {directory}");
        if (!Directory.Exists(directory))
        {
            Console.WriteLine($"Directory {directory} does not exist");
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
            }catch (JsonException){
                Console.WriteLine($"{testFile} was unable to load.");
            }
        }

        return Tuple.Create(true, tests);
    }
}
