using Application.Automation;
using Application.Automation.Web;

namespace Infrastructure.Automation.Web;

public sealed class WebNavigator : INavigator
{
    private readonly IWebDriver _driver;

    public WebNavigator(IWebDriver driver)
    {
        _driver = driver;
    }

    public Task NavigateAsync(string url) => _driver.Page.NavigateAsync(url);
}
