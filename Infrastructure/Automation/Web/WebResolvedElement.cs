using Microsoft.Playwright;

using Application.Elements;

using SharedKernel.Locators;

namespace Infrastructure.Automation.Web;

public sealed class WebResolvedElement : IResolvedElement
{
    public WebResolvedElement(Locator locator, ILocator handle)
    {
        Locator = locator;
        Handle = handle;
    }

    public Locator Locator { get; }

    public ILocator Handle { get; }

    public Task ClickAsync() => Handle.ClickAsync();

    public Task FillAsync(string value) => Handle.FillAsync(value);

    public async Task<string> GetTextAsync() => await Handle.TextContentAsync() ?? string.Empty;
}
