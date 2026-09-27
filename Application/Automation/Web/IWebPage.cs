using Application.Elements;

using SharedKernel.Locators;

namespace Application.Automation.Web;

public interface IWebPage
{
    Task<IResolvedElement?> ResolveAsync(Locator locator);
}
