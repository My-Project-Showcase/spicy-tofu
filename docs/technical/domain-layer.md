---
title: Domain Layer
updated: 2026-09-23
sources:
  - ../../Domain/Domain.csproj
  - ../../Domain/Shared/AggregateRoot.cs
  - ../../Domain/Entities/Enums/LocatorAttribute.cs
  - ../../Domain/Entities/TestCases/Test.cs
  - ../../Domain/Entities/TestCases/Workflow.cs
  - ../../Domain/Entities/TestCases/TestStep.cs
  - ../../Domain/Entities/Execution/TestExecutionStep.cs
  - ../../Domain/Runtime/Environment/TofuConfiguration.cs
  - ../../Domain/Runtime/Environment/Configuration/SpicyTofuConfig.cs
  - ../../Domain/Runtime/Environment/Configuration/TestExecution.cs
  - ../../Domain/Runtime/Environment/Configuration/Projects.cs
  - ../../Domain/Runtime/Environment/Configuration/PlaywrightConfig.cs
  - ../../Domain/Runtime/Environment/Configuration/AppiumConfig.cs
  - ../../Domain/Runtime/Environment/Configuration/LoggingConfig.cs
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
- `Domain.Entities.TestCases.TestSteps`: note the file `TestStep.cs` contains a class named `TestSteps`, not `TestStep`. It holds `Type`, `Attribute`, `Target`, and `Value` strings. They are flattened into execution steps by the runtime pipeline, but no action executes them yet.
- `Domain.Entities.Execution.TestExecutionStep`: a record composing a `Test`, a `Workflow`, and a `TestSteps` instance. It is the execution representation produced when the loaded test hierarchy is flattened. See [Runtime Pipeline](./runtime-pipeline.md).

## Configuration option classes

The `Domain.Runtime.Environment.Configuration` namespace holds one class per configuration section. Class names do not always match section names:

- `SpicyTofuConfig`: `Environment`, `Platform` strings. Note `Environment` is initialized with `String.Empty` while `Platform` uses `string.Empty`.
- `TestExecution`: `Parallel` (bool), `Workers`, `Retries`, `DefaultTimeoutMs` (ints).
- `Projects`: `public sealed`, with a single `RootDirectory` string. The web project binds it and `JsonService` reads it.
- `PlaywrightConfig`: `Browser`, `Headless` (bool), `TimeOut`, `NavigationTimeoutMs` (ints). `TimeOut` is unused; the framework reads `NavigationTimeoutMs`.
- `AppiumConfig`: `ServerUrl`, `PlatformName`, `AutomationName`, `DeviceName`, `PlatformVersion`, `App`, `AvdName`, `AndroidSdkPath`, `IosSimulatorUdid` strings, `AppiumServerExecutable` (string, defaults to `appium`), `NoReset` (bool), `NewCommandTimeoutSec` (int). The class name differs from the `Appium` section it binds; the `Appium` name is avoided because the Appium client package exposes a top-level `Appium` namespace that would collide.
- `LoggingConfig`: `Level` (default `Information`), `OutputMode` (empty), `Timestamps` (`false`), `MaxColumnWidth` (`40`), `MaxLineWidth` (`100`). Bound to the `Logging` section in `AddServices`, so both platforms read it. See [Logging](./logging.md).

`Domain.Runtime.Environment.TofuConfiguration` is a facade that surfaces `SpicyTofuConfig`, `PlaywrightConfig`, and `TestExecution`, but nothing binds or consumes it yet. See [Known Issues and Discrepancies](../wiki/known-issues-and-discrepancies.md).

## Related pages

- [Architecture Overview](./architecture-overview.md)
- [Configuration](./configuration.md)
- [Automation Driver Contract](./automation-driver-contract.md)