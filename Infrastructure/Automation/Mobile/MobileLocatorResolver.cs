using Application.Automation.Mobile;
using Application.Elements;
using Application.Locators;

using SharedKernel.Elements;

namespace Infrastructure.Automation.Mobile;

public sealed class MobileLocatorResolver : ILocatorResolver
{
    private const string DefaultSessionName = "default";

    private readonly IMobileDriver _driver;

    public MobileLocatorResolver(IMobileDriver driver)
    {
        _driver = driver;
    }

    public async Task<IResolvedElement> ResolveAsync(Element element)
    {
        var session = _driver.GetSession(DefaultSessionName);

        foreach (var locator in element.Locators)
        {
            var resolved = await session.ResolveAsync(locator);
            if (resolved is not null)
            {
                return resolved;
            }
        }

        throw new InvalidOperationException($"No locator resolved for element '{element.Name}'.");
    }
}
