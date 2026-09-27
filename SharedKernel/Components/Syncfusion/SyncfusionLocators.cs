using SharedKernel.Locators;

namespace SharedKernel.Components.Syncfusion;

public static class SyncfusionLocators
{
    public static IReadOnlyList<Locator> Button(string label, string testId) =>
    [
        new Locator(LocatorStrategy.Role, "button", label),
        new Locator(LocatorStrategy.TestId, testId),
        new Locator(LocatorStrategy.Css, ".e-btn"),
        new Locator(LocatorStrategy.XPath, $"//button[normalize-space()='{label}']")
    ];

    public static IReadOnlyList<Locator> Input(string label, string testId) =>
    [
        new Locator(LocatorStrategy.Label, label),
        new Locator(LocatorStrategy.Placeholder, label),
        new Locator(LocatorStrategy.TestId, testId),
        new Locator(LocatorStrategy.Css, "input.e-input"),
        new Locator(LocatorStrategy.XPath, $"//input[@name='{testId}']")
    ];

    public static IReadOnlyList<Locator> CheckBox(string label, string testId) =>
    [
        new Locator(LocatorStrategy.Role, "checkbox", label),
        new Locator(LocatorStrategy.TestId, testId),
        new Locator(LocatorStrategy.Css, ".e-checkbox-wrapper"),
        new Locator(LocatorStrategy.XPath, $"//input[@type='checkbox'][@name='{testId}']")
    ];
}
