using Application.Elements;

using SharedKernel.Locators;

namespace Application.Automation.Mobile;

public interface IMobileSession
{
    string Name { get; }

    Task<IResolvedElement?> ResolveAsync(Locator locator);
}
