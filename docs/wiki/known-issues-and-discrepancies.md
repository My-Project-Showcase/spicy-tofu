---
title: Known Issues and Discrepancies
updated: 2026-09-28
sources:
  - ../../AGENTS.md
  - ../../docs/README.md
  - ../../CHANGELOG.md
  - ../../Application/Automation/Web/IWebDriver.cs
  - ../../Application/Automation/IAutomationDriver.cs
  - ../../Domain/Runtime/Environment/TofuConfiguration.cs
  - ../../Domain/Runtime/Environment/Configuration/Projects.cs
  - ../../Application/Locators/ILocatorResolver.cs
  - ../../Application/Elements/IResolvedElement.cs
  - ../../SharedKernel/Locators/Locator.cs
  - ../../Application/Logging/ILogger.cs
  - ../../Infrastructure/Logging/OutputModeDetector.cs
  - ../../Infrastructure/Logging/ConsolePrintStrategy.cs
  - ../../Infrastructure/Extensions/DependencyInjection.cs
  - ../../Infrastructure/Runtime/JsonService/JsonService.cs
  - ../../Infrastructure/Runtime/RunService/RunService.cs
  - ../../Infrastructure/Events/ClickEvent.cs
  - ../../Infrastructure/Events/FillEvent.cs
  - ../../Infrastructure/Events/NavigateEvent.cs
  - ../../Mobile/Extensions/MobileExtensions.cs
  - ../technical/architecture-overview.md
  - ../technical/configuration.md
---

# Known Issues and Discrepancies

This page records observable gaps between the documented intent (AGENTS.md, older pages) and the current code, plus empty scaffolding that is not yet wired. Claims here are things that exist in code but are unused, or things AGENTS.md describes that do not exist yet.

## Stale AGENTS.md claims

- AGENTS.md says the platform implementation lives in the "Web project". The Playwright implementation actually lives in `Infrastructure.Automation.Web`.
- The `[0.1.0]` changelog added "Mobile now references `Infrastructure`", and AGENTS.md's Conventions section was updated to say both `Web` and `Mobile` reference `Application`, `Domain`, and `Infrastructure`. See [Architecture Overview](../technical/architecture-overview.md).

## Empty or unused code

- `Domain.Runtime.Environment.TofuConfiguration` is never bound or consumed.
- `PlaywrightConfig.TimeOut` is unused; the framework reads `NavigationTimeoutMs`.

## Element and locator layer

- `ClickEvent` and `FillEvent` now perform the click and fill against the resolved element, and `NavigateEvent` drives the platform-neutral `INavigator`. A test step whose action is not registered, or whose event throws, is recorded as a failed step rather than aborting the run.
- `SampleElementRepository` is illustrative. The repository has no application-under-test project, so the sample stands in for a real application element repository. `AddServices` registers it with `TryAddSingleton`, so an application can register its own `IElementRepository` before calling `AddInfrastructureDependencies` and have it win.
- Locator resolution on a real page is unverified end to end: the repository ships no sample test definition, so a real browser run depends on an application-supplied test JSON and element repository.

## Driver startup on both platforms

Both entry points resolve `IRunService`, call `RunAsync()`, and map the returned `RunResult` to the process exit code (`0` when no step failed, `1` otherwise). `RunAsync` starts the selected driver (`IAutomationDriver.StartAsync`) before loading test JSON and stops it in a `finally`. For the web platform this launches a browser and creates the `default` context during a run; for mobile it starts the Appium server and a device on first session start. The hosts themselves still apply lazy init semantics: nothing external starts during `Build()` or before the first session is requested.

## Configuration binding gaps

- `SpicyTofuConfig.Environment` is initialized with `String.Empty` while `Platform` uses `string.Empty`; style-consistent initialization is absent.

## Naming and type oddities

- `Domain.Shared.AggregateRoot.cs` contains a class named `AggregrateRoot` (typo). Test case entities inherit it.
- `Domain/Entities/TestCases/TestStep.cs` contains a class named `TestSteps`.
- `ILogger.Error` triggers `CA1716` (member name conflicts with the reserved language keyword `Error`), in the same class of tolerated warnings as `Domain.Shared`. The name is kept because `Error` is the idiomatic logging API name and matches the `LogLevel.Error` enum member.
- `IEventRegistry.Get` and `IElementRepository.Get` also trigger `CA1716` (reserved keyword `Get`); the names are kept for the same reason. `RunService` uses `IEventRegistry.TryGet` so an unknown action does not throw.

## Format check and compiler warnings

`dotnet format spicy-tofu.sln --verify-no-changes` passes. The previous whitespace, final-newline, and analyzer violations were fixed, including making the domain model properties nullable and marking the `Test` overrides with `new`, which removed the `CS8618` and `CS0108` warnings. Import ordering is not checked: the import sorter is disabled in `.editorconfig`, so using-directive order is a documented convention applied by hand (external first, then `Application`, `Domain`, `Infrastructure`, `SharedKernel`, `Web`, `Mobile`). See AGENTS.md and the csharp-developer skill (`references/using-directives.md`).

## Automated tests

The `Tests` project (xUnit) covers `TestsLoadedHandler`, `EventService`, `RunService` (including the unknown-action and throwing-event paths and driver stop-on-failure), `JsonService`, and `SampleElementRepository`. It does not exercise a real browser or device; those paths remain unverified until an application under test is wired in.

## What is deliberately not recorded

- Planned or unmerged feature work is not documented as if it exists.
- Unverified behavior claims are marked as such (see [Platform Notes](./platform-notes.md)).

## Related pages

- [Architecture Overview](../technical/architecture-overview.md)
- [Configuration](../technical/configuration.md)
- [Setup and Commands](../technical/setup-and-commands.md)
- [Design Decisions](./design-decisions.md)
- [Logging](../technical/logging.md)
