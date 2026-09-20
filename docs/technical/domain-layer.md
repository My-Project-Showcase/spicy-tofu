---
title: Domain Layer
updated: 2026-09-20
sources:
  - ../../Domain/Domain.csproj
  - ../../Domain/Shared/AggregateRoot.cs
  - ../../Domain/Entities/Enums/LocatorAttribute.cs
  - ../../Domain/Entities/TestCases/Test.cs
  - ../../Domain/Entities/TestCases/Workflow.cs
  - ../../Domain/Entities/TestCases/TestStep.cs
  - ../../Domain/Runtime/Environment/TofuConfiguration.cs
  - ../../Domain/Runtime/Environment/Configuration/SpicyTofuConfig.cs
  - ../../Domain/Runtime/Environment/Configuration/TestExecution.cs
  - ../../Domain/Runtime/Environment/Configuration/Projects.cs
  - ../../Domain/Runtime/Environment/Configuration/PlaywrightConfig.cs
  - ../../Domain/Runtime/Environment/Configuration/Appium.cs
---

# Domain Layer

The `Domain` project models the framework's own domain. It does not model the business domain of an app under test. `Domain` depends on nothing.

## Shared base type

`Domain.Shared.AggregrateRoot`: note the typo in the type name. The file is `AggregateRoot.cs` but the class is spelled `AggregrateRoot`. It holds `Id`, `Name`, and `Description` string properties. Test case entity classes inherit from it.

## Locator attributes

`Domain.Entities.Enums.LocatorAttribute`: an `Ardalis.SmartEnum` subclass. It defines seven static values (`Id`, `Name`, `Class`, `Css`, `XPath`, `Text`, `AccessibilityId`), each carrying a `MobileSupported` flag. The flag marks which locators are usable on mobile, but no logic consumes it yet. This is the only SmartEnum in the project and the reason `Ardalis.SmartEnum` is referenced.

## Test case models

- `Domain.Entities.TestCases.Test`: inherits `AggregrateRoot`. Re-declares `Id` and `Name` (strings) and adds `Workflows` (`List<Workflow>`). Re-declaring `Id` and `Name` hides the base members, which produces `CS0108` compiler warnings when `TreatWarningsAsErrors` is off (Debug builds).
- `Domain.Entities.TestCases.Workflow`: holds `Id`, `Name`, and `Steps` (`List<TestSteps>`).
- `Domain.Entities.TestCases.TestSteps`: note the file `TestStep.cs` contains a class named `TestSteps`, not `TestStep`. It holds `Type`, `Attribute`, `Target`, and `Value` strings. None of these are processed by the framework yet.

`Domain/Class1.cs` exists and is empty.

## Configuration option classes

The `Domain.Runtime.Environment.Configuration` namespace holds one class per configuration section. Class names do not always match section names:

- `SpicyTofuConfig`: `Environment`, `Platform` strings. Note `Environment` is initialized with `String.Empty` while `Platform` uses `string.Empty`.
- `TestExecution`: `Parallel` (bool), `Workers`, `Retries`, `DefaultTimeoutMs` (ints).
- `Projects`: `internal sealed`, with `RootWebDirectory` and `RootMobileDirectory` strings.
- `PlaywrightConfig`: `Browser`, `Headless` (bool), `TimeOut`, `NavigationTimeoutMs` (ints). `TimeOut` is unused; the framework reads `NavigationTimeoutMs`.
- `Appium`: `ServerUrl`, `PlatformName`, `AutomationName`, `DeviceName`, `App` strings, `NoReset` (bool), `NewCommandTimeoutSec` (int). There is no `PlatformVersion` property, even though `Mobile/appsettings.json` contains an `Appium:PlatformVersion` key.

`Domain.Runtime.Environment.TofuConfiguration` is a facade that surfaces `SpicyTofuConfig`, `PlaywrightConfig`, and `TestExecution`, but nothing binds or consumes it yet. See [Known Issues and Discrepancies](../wiki/known-issues-and-discrepancies.md).

## Related pages

- [Architecture Overview](./architecture-overview.md)
- [Configuration](./configuration.md)
- [Automation Driver Contract](./automation-driver-contract.md)