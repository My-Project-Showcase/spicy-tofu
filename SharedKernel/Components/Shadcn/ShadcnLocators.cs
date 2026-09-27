using SharedKernel.Locators;

namespace SharedKernel.Components.Shadcn;

public static class ShadcnLocators
{
    public static IReadOnlyList<Locator> Button(string label, string testId) =>
    [
        new Locator(LocatorStrategy.Role, "button", label),
        new Locator(LocatorStrategy.TestId, testId),
        new Locator(LocatorStrategy.Css, "[data-slot='button']"),
        new Locator(LocatorStrategy.XPath, $"//button[normalize-space()='{label}']")
    ];

    public static IReadOnlyList<Locator> Input(string label, string testId) =>
    [
        new Locator(LocatorStrategy.Label, label),
        new Locator(LocatorStrategy.Placeholder, label),
        new Locator(LocatorStrategy.TestId, testId),
        new Locator(LocatorStrategy.Css, "[data-slot='input']"),
        new Locator(LocatorStrategy.XPath, $"//input[@name='{testId}']")
    ];

    public static IReadOnlyList<Locator> Switch(string label, string testId) =>
    [
        new Locator(LocatorStrategy.Role, "switch", label),
        new Locator(LocatorStrategy.TestId, testId),
        new Locator(LocatorStrategy.Css, "[data-slot='switch']"),
        new Locator(LocatorStrategy.XPath, $"//button[@role='switch'][@name='{testId}']")
    ];
}
