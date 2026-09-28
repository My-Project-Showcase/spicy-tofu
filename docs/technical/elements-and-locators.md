---
title: Elements and Locators
updated: 2026-09-28
sources:
  - ../../SharedKernel/Locators/LocatorStrategy.cs
  - ../../SharedKernel/Locators/Locator.cs
  - ../../SharedKernel/Elements/Element.cs
  - ../../SharedKernel/Components/Mui/MuiLocators.cs
  - ../../SharedKernel/Components/Shadcn/ShadcnLocators.cs
  - ../../SharedKernel/Components/Syncfusion/SyncfusionLocators.cs
  - ../../Application/Automation/INavigator.cs
  - ../../Application/Elements/IElementRepository.cs
  - ../../Application/Elements/IResolvedElement.cs
  - ../../Application/Locators/ILocatorResolver.cs
  - ../../Application/Automation/Web/IWebPage.cs
  - ../../Application/Automation/Mobile/IMobileSession.cs
  - ../../Infrastructure/Elements/SampleElementRepository.cs
  - ../../Infrastructure/Automation/Web/WebPage.cs
  - ../../Infrastructure/Automation/Web/WebLocatorResolver.cs
  - ../../Infrastructure/Automation/Web/WebResolvedElement.cs
  - ../../Infrastructure/Automation/Mobile/MobileSession.cs
  - ../../Infrastructure/Automation/Mobile/MobileLocatorResolver.cs
  - ../../Infrastructure/Automation/Mobile/MobileResolvedElement.cs
  - ../../Infrastructure/Events/ClickEvent.cs
  - ../../Infrastructure/Events/FillEvent.cs
  - ../../Infrastructure/Events/NavigateEvent.cs
  - ../../Infrastructure/Extensions/DependencyInjection.cs
---

# Elements and Locators

The element and locator layer turns a logical element from a test step into a resolved element on the running platform. It sits under the events: an event asks the repository which element a step refers to, asks the resolver to find it, and reports the outcome.

## Flow

```text
TestExecutionStep (Attribute, Target)
    |
    v
IElementRepository.Get(attribute, target)
    |
    v
Element (ordered locator candidates)
    |
    v
ILocatorResolver.ResolveAsync(element)
    |
    v
IResolvedElement (matched Locator + interaction)
```

`RunService` never sees this layer. `ITestEvent` implementations depend on `IElementRepository` and `ILocatorResolver`, both of which are platform-neutral.

## Boundaries

- **Events** contain no XPath, no application knowledge (element names, ids, classes, pages), and no component-library knowledge. An event knows only the generic action and the abstractions.
- **IElementRepository** owns application-specific knowledge: which UI element a logical `Attribute`/`Target` maps to.
- **SharedKernel** owns reusable component-library knowledge: how a component type (MUI button, Shadcn input, Syncfusion grid) can be located. It is platform-independent.
- **ILocatorResolver** owns locator selection: it tries the element's candidates in order and returns the first that resolves.
- **Platform implementations** translate a platform-neutral `Locator` into Playwright or Appium calls.

## Models (SharedKernel)

`SharedKernel.Locators.LocatorStrategy`: `Role`, `Label`, `Placeholder`, `Text`, `TestId`, `Id`, `Name`, `Css`, `XPath`, `AccessibilityId`.

`SharedKernel.Locators.Locator`: `sealed record Locator(LocatorStrategy Strategy, string Value, string? Name = null)`. `Role` uses `Value` for the ARIA role and `Name` for the accessible name; every other strategy uses `Value`. XPath is a valid strategy here and in component definitions; it never appears in an event.

`SharedKernel.Elements.Element`: `sealed record Element(string Name, IReadOnlyList<Locator> Locators)`. The list is ordered by priority, best candidate first.

## IElementRepository

`Application.Elements.IElementRepository`:

```csharp
Element Get(string attribute, string target);
```

`SampleElementRepository` (in `Infrastructure.Elements`) is the concrete implementation. It is seeded with the elements the current test data uses (`LoginButton`, `UsernameInput`, `PasswordInput`, `RememberMeSwitch`, `FirstNameInput`, `LastNameInput`, `EmailInput`, `RegisterButton`, `SearchInput`, `SearchButton`, `SearchResult`), each composed from a `SharedKernel` component library. Lookup is case-insensitive: `Target` first, then `Attribute`. An unknown element throws `InvalidOperationException`.

The sample repository is illustrative: the repository has no application-under-test project, so it stands in for a real application element repository.

## ILocatorResolver

`Application.Locators.ILocatorResolver`:

```csharp
Task<IResolvedElement> ResolveAsync(Element element);
```

Platform implementations live in the platform namespaces and are selected at composition time:

- `WebLocatorResolver` (`Infrastructure.Automation.Web`) iterates `element.Locators` and calls `IWebDriver.Page.ResolveAsync(locator)`.
- `MobileLocatorResolver` (`Infrastructure.Automation.Mobile`) iterates `element.Locators` and calls the `default` `IMobileSession.ResolveAsync(locator)`.

Both return the first `IResolvedElement` that is not null, or throw `InvalidOperationException` when nothing resolves. `IResolvedElement` exposes the matched `Locator` plus the interaction methods `ClickAsync`, `FillAsync(string value)`, and `GetTextAsync`. The platform wrappers (`WebResolvedElement`, `MobileResolvedElement`) hold the native handle and implement those methods with the platform API.

## Platform resolution

`IWebPage` and `IMobileSession` each expose `Task<IResolvedElement?> ResolveAsync(Locator locator)`. Returning null means the locator did not match; the resolver then tries the next candidate.

`WebPage` builds a Playwright locator from the strategy: `GetByRole` (Role), `GetByLabel` (Label), `GetByPlaceholder` (Placeholder), `GetByText` (Text), `GetByTestId` (TestId), `Locator("[id='...']")` (Id), `Locator("[name='...']")` (Name), `Locator(css)` (Css), `Locator("xpath=...")` (XPath). It checks presence with `CountAsync`. `AccessibilityId` is not supported on web and returns null.

`MobileSession` builds an Appium `By`: `MobileBy.Id` (Id), `MobileBy.Name` (Name), `MobileBy.AccessibilityId` (AccessibilityId), `MobileBy.XPath` (XPath). It checks presence with `FindElements`. Web-only strategies (Role, Label, Placeholder, Text, TestId, Css) return null, so the resolver skips them on mobile.

## Component libraries (SharedKernel)

Reusable component-library locators live under `SharedKernel/Components`, one static class per library:

- `MuiLocators`: `Button`, `TextField`, `Switch`.
- `ShadcnLocators`: `Button`, `Input`, `Switch`.
- `SyncfusionLocators`: `Button`, `Input`, `CheckBox`.

Each method takes the application-supplied label and test id and returns an ordered `IReadOnlyList<Locator>`: native strategies first (role, label, placeholder, test id), then a component-specific CSS hint, then XPath as the last resort. Component libraries never reference Playwright or Appium.

The distinction is:

- `MuiLocators.Button` is reusable component knowledge and lives in `SharedKernel`.
- `LoginButton` is application knowledge and lives in the application element repository.

## Events

`ClickEvent` and `FillEvent` are the element-based events. `ClickEvent` is the reference:

```csharp
var element = _elementRepository.Get(step.Step.Attribute ?? string.Empty, step.Step.Target ?? string.Empty);
var resolved = await _locatorResolver.ResolveAsync(element);
_logger.LocatorResolution(step, element.Locators, resolved.Locator);
await resolved.ClickAsync();
return new TestExecutionResult { IsSuccess = true };
```

`FillEvent` is identical except it calls `resolved.FillAsync(step.Step.Value ?? string.Empty)`. Neither contains XPath, application knowledge, or platform types. On a failed lookup or resolution, or a failed interaction, they return `TestExecutionResult { IsSuccess = false, Error = ... }` and log the candidates with no selection, so `RunService` reports `ActionFailed` and the run continues.

`NavigateEvent` is the exception to the element pattern: navigation is not element-scoped, so it drives the platform-neutral `INavigator` seam with `step.Step.Value` as the URL and does not touch the repository or resolver.

## Registration

`AddServices` registers `IElementRepository` (`SampleElementRepository`) as a singleton with `TryAddSingleton`, so an application can register its own `IElementRepository` before calling `AddInfrastructureDependencies` and have it win. `AddAutomation` registers `ILocatorResolver` per platform: `WebLocatorResolver` for `Web`, `MobileLocatorResolver` for `Mobile`, and the matching `INavigator`. See [Dependency Injection](./dependency-injection.md).

## Related pages

- [Automation Driver Contract](./automation-driver-contract.md)
- [Web Automation](./web-automation.md)
- [Mobile Automation](./mobile-automation.md)
- [Runtime Pipeline](./runtime-pipeline.md)
- [Logging](./logging.md)
- [Design Decisions](../wiki/design-decisions.md)
