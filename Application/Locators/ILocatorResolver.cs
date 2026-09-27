using Application.Elements;

using SharedKernel.Elements;

namespace Application.Locators;

public interface ILocatorResolver
{
    Task<IResolvedElement> ResolveAsync(Element element);
}
