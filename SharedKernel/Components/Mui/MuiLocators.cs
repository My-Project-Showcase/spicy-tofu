using SharedKernel.Locators;

namespace SharedKernel.Components.Mui;

public static class MuiLocators
{
    public static IReadOnlyList<Locator> Button(string label, string testId) =>
    [
        new Locator(LocatorStrategy.Role, "button", label),
        new Locator(LocatorStrategy.TestId, testId),
        new Locator(LocatorStrategy.XPath, $"//button[normalize-space()='{label}']")
    ];

    public static IReadOnlyList<Locator> TextField(string label, string testId) =>
    [
        new Locator(LocatorStrategy.Label, label),
        new Locator(LocatorStrategy.Placeholder, label),
        new Locator(LocatorStrategy.TestId, testId),
        new Locator(LocatorStrategy.XPath, $"//input[@name='{testId}']")
    ];

    public static IReadOnlyList<Locator> Switch(string label, string testId) =>
    [
        new Locator(LocatorStrategy.Role, "checkbox", label),
        new Locator(LocatorStrategy.TestId, testId),
        new Locator(LocatorStrategy.XPath, $"//input[@type='checkbox'][@name='{testId}']")
    ];
}
