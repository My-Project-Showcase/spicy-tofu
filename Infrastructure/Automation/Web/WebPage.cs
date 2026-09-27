using Microsoft.Playwright;

using Application.Automation.Web;
using Application.Elements;

using SharedKernel.Locators;

namespace Infrastructure.Automation.Web;

public sealed class WebPage : IWebPage
{
    public WebPage(IPage page)
    {
        Page = page;
    }

    public IPage Page { get; }

    public async Task<IResolvedElement?> ResolveAsync(Locator locator)
    {
        var handle = TryBuildLocator(locator);
        if (handle is null)
        {
            return null;
        }

        return await handle.CountAsync() > 0 ? new WebResolvedElement(locator, handle) : null;
    }

    private ILocator? TryBuildLocator(Locator locator) =>
        locator.Strategy switch
        {
            LocatorStrategy.Role => Page.GetByRole(ParseRole(locator.Value), new PageGetByRoleOptions { Name = locator.Name }),
            LocatorStrategy.Label => Page.GetByLabel(locator.Value),
            LocatorStrategy.Placeholder => Page.GetByPlaceholder(locator.Value),
            LocatorStrategy.Text => Page.GetByText(locator.Value),
            LocatorStrategy.TestId => Page.GetByTestId(locator.Value),
            LocatorStrategy.Id => Page.Locator($"[id='{locator.Value}']"),
            LocatorStrategy.Name => Page.Locator($"[name='{locator.Value}']"),
            LocatorStrategy.Css => Page.Locator(locator.Value),
            LocatorStrategy.XPath => Page.Locator($"xpath={locator.Value}"),
            _ => null
        };

    private static AriaRole ParseRole(string role) =>
        Enum.TryParse<AriaRole>(role, ignoreCase: true, out var parsed) ? parsed : AriaRole.Generic;
}
