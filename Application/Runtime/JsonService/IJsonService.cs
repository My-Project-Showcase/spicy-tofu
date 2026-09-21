using Domain.Entities.TestCases;

namespace Application.Runtime.JsonService;

public interface IJsonService
{
    Task<Tuple<bool, List<Test>>> LoadJson();
}
