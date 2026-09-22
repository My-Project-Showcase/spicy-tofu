using Domain.Entities.TestCases;

namespace Application.Runtime.JsonService;

public interface IJsonService
{
    event Action<List<Test>>? TestsLoaded;

    Task<Tuple<bool, List<Test>>> LoadJson();
}
