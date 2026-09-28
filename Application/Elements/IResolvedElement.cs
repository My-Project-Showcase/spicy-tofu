using SharedKernel.Locators;

namespace Application.Elements;

public interface IResolvedElement
{
    Locator Locator { get; }

    Task ClickAsync();

    Task FillAsync(string value);

    Task<string> GetTextAsync();
}
