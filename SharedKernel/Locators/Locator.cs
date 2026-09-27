namespace SharedKernel.Locators;

public sealed record Locator(LocatorStrategy Strategy, string Value, string? Name = null);
