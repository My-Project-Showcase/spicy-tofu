using Application.Automation.Mobile;

using OpenQA.Selenium.Appium;

namespace Infrastructure.Automation.Mobile;

public sealed class MobileSession : IMobileSession
{
    public MobileSession(string name, AppiumDriver driver)
    {
        Name = name;
        Driver = driver;
    }

    public string Name { get; }

    public AppiumDriver Driver { get; }
}
