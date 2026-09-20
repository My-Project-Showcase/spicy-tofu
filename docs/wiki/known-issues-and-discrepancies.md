---
title: Known Issues and Discrepancies
updated: 2026-09-20
sources:
  - ../../AGENTS.md
  - ../../docs/README.md
  - ../../CHANGELOG.md
  - ../../Application/Automation/Web/IWebDriver.cs
  - ../../Domain/Runtime/Environment/TofuConfiguration.cs
  - ../../Domain/Runtime/Environment/Configuration/Projects.cs
  - ../../Application/Locators/LocatorStrategy.cs
  - ../../Domain/Class1.cs
  - ../technical/architecture-overview.md
  - ../technical/configuration.md
---

# Known Issues and Discrepancies

This page records observable gaps between the documented intent (AGENTS.md, older pages) and the current code, plus empty scaffolding that is not yet wired. Claims here are things that exist in code but are unused, or things AGENTS.md describes that do not exist yet.

## Stale AGENTS.md claims

- AGENTS.md describes a reflection-based resolver that discovers platform implementations. No such type exists; wiring is explicit in `AddWebAutomation`.
- AGENTS.md says the platform implementation lives in the "Web project". The Playwright implementation actually lives in `Infrastructure.Automation.Web`.
- AGENTS.md says "Mobile references nothing yet". `Mobile/Mobile.csproj` references `Domain`.

## Empty or unused code

- `Domain/Class1.cs` is an empty file.
- `Domain.Runtime.Environment.TofuConfiguration` is never bound or consumed.
- `Application.Locators.LocatorStrategy` is an empty record, unused by any driver.
- `Projects` (internal) is never bound; its properties (`RootWebDirectory`, `RootMobileDirectory`) do not match the `Projects:RootDirectory` key in `appsettings.json`.
- `PlaywrightConfig.TimeOut` is unused; the framework reads `NavigationTimeoutMs`.
- `docs/README.md` was an empty placeholder; this task filled it.
- `CHANGELOG.md` was an empty placeholder; this task filled it.

## Hosts are built but never started

- `Web/Program.cs` calls `Host.CreateDefaultBuilder(...).Build()` but never `Run()`.
- `Mobile/Program.cs` ends with the builder chain; it never calls `Build()` at all.

## Configuration binding gaps

- `Projects:RootDirectory` binds to nothing.
- `Appium:PlatformVersion` in `Mobile/appsettings.json` has no matching property on the `Appium` class.
- `SpicyTofuConfig.Environment` is initialized with `String.Empty` while `Platform` uses `string.Empty`; style-consistent initialization is absent.

## Naming and type oddities

- `Domain.Shared.AggregateRoot.cs` contains a class named `AggregrateRoot` (typo). Test case entities inherit it.
- `Domain/Entities/TestCases/TestStep.cs` contains a class named `TestSteps`.
- `Test` and `Workflow` redeclare `Id` and `Name`, hiding the base `AggregrateRoot` members; Debug builds emit `CS0108` warnings (not errors because warnings-as-errors only applies to Release).
- Domain entity properties that are non-nullable but uninitialized (for example `Test.Id`, `Workflow.Steps`) emit `CS8618` in Debug builds.

## Format check

`dotnet format spicy-tofu.sln --verify-no-changes` currently reports violations in source files (whitespace, final-newline, and using-ordering diagnostics across Domain, Application, Infrastructure, Web, and Mobile). Those files were not touched by the documentation task; the check fails on code written before it.

## What is deliberately not recorded

- Planned or unmerged feature work is not documented as if it exists.
- Unverified behavior claims are marked as such (see [Platform Notes](./platform-notes.md)).

## Related pages

- [Architecture Overview](../technical/architecture-overview.md)
- [Configuration](../technical/configuration.md)
- [Setup and Commands](../technical/setup-and-commands.md)
- [Design Decisions](./design-decisions.md)