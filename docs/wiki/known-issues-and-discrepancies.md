---
title: Known Issues and Discrepancies
updated: 2026-09-23
sources:
  - ../../AGENTS.md
  - ../../docs/README.md
  - ../../CHANGELOG.md
  - ../../Application/Automation/Web/IWebDriver.cs
  - ../../Application/Automation/IAutomationDriver.cs
  - ../../Domain/Runtime/Environment/TofuConfiguration.cs
  - ../../Domain/Runtime/Environment/Configuration/Projects.cs
  - ../../Application/Locators/LocatorCandidate.cs
  - ../../Application/Logging/ILogger.cs
  - ../../Infrastructure/Logging/OutputModeDetector.cs
  - ../../Infrastructure/Logging/ConsolePrintStrategy.cs
  - ../../Infrastructure/Extensions/DependencyInjection.cs
  - ../../Infrastructure/Runtime/JsonService/JsonService.cs
  - ../../Infrastructure/Runtime/RunService/RunService.cs
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
- `docs/README.md` was an empty placeholder; this task filled it.
- `CHANGELOG.md` was an empty placeholder; this task filled it.

## Driver startup on both platforms

Both entry points now resolve `IRunService` and call `RunAsync()`, which starts the selected driver (`IAutomationDriver.StartAsync`) before loading test JSON and stops it in a `finally`. For the web platform this launches a browser and creates the `default` context during a run; for mobile it starts the Appium server and a device on first session start. The hosts themselves still apply lazy init semantics: nothing external starts during `Build()` or before the first session is requested.

## Configuration binding gaps

- `SpicyTofuConfig.Environment` is initialized with `String.Empty` while `Platform` uses `string.Empty`; style-consistent initialization is absent.

## Naming and type oddities

- `Domain.Shared.AggregateRoot.cs` contains a class named `AggregrateRoot` (typo). Test case entities inherit it.
- `Domain/Entities/TestCases/TestStep.cs` contains a class named `TestSteps`.
- `Test` and `Workflow` redeclare `Id` and `Name`, hiding the base `AggregrateRoot` members; Debug builds emit `CS0108` warnings (not errors because warnings-as-errors only applies to Release).
- Domain entity properties that are non-nullable but uninitialized (for example `Test.Id`, `Workflow.Steps`) emit `CS8618` in Debug builds.
- `ILogger.Error` triggers `CA1716` (member name conflicts with the reserved language keyword `Error`), in the same class of tolerated warnings as `Domain.Shared`. The name is kept because `Error` is the idiomatic logging API name and matches the `LogLevel.Error` enum member.

## Format check

`dotnet format spicy-tofu.sln --verify-no-changes` still reports violations in source files (whitespace, final-newline, and using-ordering diagnostics across Application, Domain, Infrastructure, Mobile, and Web). The runtime-pipeline, logging, and automation-wiring files (`JsonService`, `RunService`, `TestsLoadedHandler`, `TestExecutionStep`, `IJsonService`, the `Application/Logging` contracts, the `Application/Locators` locator code, the `Infrastructure/Logging` implementation, `DependencyInjection`, the `IWebDriver` and `IMobileDriver` automation interfaces, and both `Program.cs` entry points) are clean; the remaining failures are all in files written before the web runner and runtime-pipeline work.

## What is deliberately not recorded

- Planned or unmerged feature work is not documented as if it exists.
- Unverified behavior claims are marked as such (see [Platform Notes](./platform-notes.md)).

## Related pages

- [Architecture Overview](../technical/architecture-overview.md)
- [Configuration](../technical/configuration.md)
- [Setup and Commands](../technical/setup-and-commands.md)
- [Design Decisions](./design-decisions.md)
- [Logging](../technical/logging.md)