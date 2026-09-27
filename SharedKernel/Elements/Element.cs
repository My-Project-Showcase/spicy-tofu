using SharedKernel.Locators;

namespace SharedKernel.Elements;

public sealed record Element(string Name, IReadOnlyList<Locator> Locators);
