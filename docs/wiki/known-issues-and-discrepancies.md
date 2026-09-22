---
title: Known Issues and Discrepancies
updated: 2026-09-22
sources:
  - ../../AGENTS.md
  - ../../docs/README.md
  - ../../CHANGELOG.md
  - ../../Application/Automation/Web/IWebDriver.cs
  - ../../Domain/Runtime/Environment/TofuConfiguration.cs
  - ../../Domain/Runtime/Environment/Configuration/Projects.cs
  - ../../Application/Locators/LocatorStrategy.cs
  - ../../Infrastructure/Runtime/JsonService/JsonService.cs
  - ../../Infrastructure/Runtime/RunService/RunService.cs
  - ../technical/architecture-overview.md
  - ../technical/configuration.md
---

# Known Issues and Discrepancies

This page records observable gaps between the documented intent (AGENTS.md, older pages) and the current code, plus empty scaffolding that is not yet wired. Claims here are things that exist in code but are unused, or things AGENTS.md describes that do not exist yet.

## Stale AGENTS.md claims

- AGENTS.md describes a reflection-based resolver that discovers platform implementations. No such type exists; wiring is explicit in `AddWebAutomation` and `AddMobileAutomation`.
- AGENTS.md says the platform implementation lives in the "Web project". The Playwright implementation actually lives in `Infrastructure.Automation.Web`.
- The `[0.1.0]` changelog added "Mobile now references `Infrastructure`", and AGENTS.md's Conventions section was updated to say both `Web` and `Mobile` reference `Application`, `Domain`, and `Infrastructure`. See [Architecture Overview](../technical/architecture-overview.md).

## Empty or unused code

- `Domain.Runtime.Environment.TofuConfiguration` is never bound or consumed.
- `Application.Locators.LocatorStrategy` is an empty record, unused by any driver.
- `Projects` (public) is bound and consumed only by the web project through `JsonService`, which reads `RootDirectory`. The mobile project never binds or consumes it.
- `PlaywrightConfig.TimeOut` is unused; the framework reads `NavigationTimeoutMs`.
- `docs/README.md` was an empty placeholder; this task filled it.
- `CHANGELOG.md` was an empty placeholder; this task filled it.

## Hosts are built but never started

- `Web/Program.cs` resolves `IRunService` and calls `RunAsync()`, which loads test JSON and prints the flattened steps. It does not start a browser yet.
- `Mobile/Program.cs` calls `Build()` but never `Run()` and never calls `RunAsync()`.

The platform hosts (`BrowserHost`, `MobileHost`) still run lazily only when a session is requested at runtime.

## Configuration binding gaps

- `Projects:RootDirectory` binds on the web project only; the mobile project leaves `Projects` unbound.
- `SpicyTofuConfig.Environment` is initialized with `String.Empty` while `Platform` uses `string.Empty`; style-consistent initialization is absent.

## Naming and type oddities

- `Domain.Shared.AggregateRoot.cs` contains a class named `AggregrateRoot` (typo). Test case entities inherit it.
- `Domain/Entities/TestCases/TestStep.cs` contains a class named `TestSteps`.
- `Test` and `Workflow` redeclare `Id` and `Name`, hiding the base `AggregrateRoot` members; Debug builds emit `CS0108` warnings (not errors because warnings-as-errors only applies to Release).
- Domain entity properties that are non-nullable but uninitialized (for example `Test.Id`, `Workflow.Steps`) emit `CS8618` in Debug builds.

## Format check

`dotnet format spicy-tofu.sln --verify-no-changes` still reports violations in source files (whitespace, final-newline, and using-ordering diagnostics across Application, Domain, Infrastructure, Mobile, and Web). The runtime-pipeline files (`JsonService`, `RunService`, `TestsLoadedHandler`, `TestExecutionStep`, `IJsonService`) are clean; the remaining failures are all in files written before the web runner and runtime-pipeline work.

## What is deliberately not recorded

- Planned or unmerged feature work is not documented as if it exists.
- Unverified behavior claims are marked as such (see [Platform Notes](./platform-notes.md)).

## Related pages

- [Architecture Overview](../technical/architecture-overview.md)
- [Configuration](../technical/configuration.md)
- [Setup and Commands](../technical/setup-and-commands.md)
- [Design Decisions](./design-decisions.md)