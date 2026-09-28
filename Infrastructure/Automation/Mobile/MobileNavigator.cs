using Application.Automation;
using Application.Automation.Mobile;

namespace Infrastructure.Automation.Mobile;

public sealed class MobileNavigator : INavigator
{
    private const string DefaultSessionName = "default";

    private readonly IMobileDriver _driver;

    public MobileNavigator(IMobileDriver driver)
    {
        _driver = driver;
    }

    public Task NavigateAsync(string url) => _driver.GetSession(DefaultSessionName).NavigateAsync(url);
}
