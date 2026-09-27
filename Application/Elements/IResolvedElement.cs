using SharedKernel.Locators;

namespace Application.Elements;

public interface IResolvedElement
{
    Locator Locator { get; }
}
