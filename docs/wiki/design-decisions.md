---
title: Design Decisions
updated: 2026-09-23
sources:
  - ../technical/architecture-overview.md
  - ../technical/automation-driver-contract.md
  - ../technical/dependency-injection.md
  - ../technical/web-automation.md
  - ../technical/mobile-automation.md
  - ../../Application/Automation/Web/IWebPage.cs
  - ../../Infrastructure/Automation/Web/BrowserHost.cs
  - ../../Infrastructure/Automation/Web/WebDriver.cs
  - ../../Infrastructure/Automation/Mobile/MobileHost.cs
  - ../../Infrastructure/Automation/Mobile/AppiumServerLauncher.cs
  - ../../Infrastructure/Automation/Mobile/Devices/AndroidEmulatorLauncher.cs
  - ../../Infrastructure/Automation/Mobile/Devices/IosSimulatorLauncher.cs
  - ../../Infrastructure/Extensions/DependencyInjection.cs
---

# Design Decisions

This page records why the framework is shaped the way it is. Where a decision comes from the task brief rather than from observable code, it is flagged as such; verify against the code and the AGENTS.md constraints before relying on it.

## IWebPage and IWebSession keep Playwright out of the core

The application contract exposes `IWebPage` and `IWebSession` instead of Playwright types. The web implementation wraps Playwright's `IPage` behind `WebPage`, which implements the empty marker `IWebPage`. This satisfies the AGENTS.md constraint that Playwright APIs stay inside their own platform assembly: only `Infrastructure` references `Microsoft.Playwright`, and application code depends on the interfaces in `Application.Automation.Web`.

## Named sessions instead of a single session

`IWebDriver` supports starting sessions by name (`StartSessionAsync(name, ...)`), with a `default` session used by the convenience `Page` property. This allows several browser contexts to exist at once, which supports scenarios where tests run in parallel. `WebDriver` keeps sessions in a `ConcurrentDictionary` keyed by name and throws `InvalidOperationException` on duplicate start or unknown lookup.

## Browser singleton with semaphore-guarded lazy startup

`BrowserHost` is a singleton that creates the Playwright instance and launches the browser lazily. Startup is guarded by `SemaphoreSlim(1, 1)` so concurrent requests from multiple sessions produce a single browser. Registered directly as the concrete `BrowserHost`, with no `IBrowserHost` abstraction. This is a pragmatic choice for a single-owned browser process; the reasoning for a concrete registration is that no second implementation exists yet.

## Single composition point instead of a resolver

Platform selection happens exactly once, in `AddAutomation`, which reads `SpicyTofu:Platform` and registers exactly one driver as `IAutomationDriver`. The value is checked once during service registration, and a missing or unknown value throws `InvalidOperationException` at composition time. There is no reflection-based resolver, no factory, and no service locator; execution code never branches on platform. AGENTS.md describes this single composition point directly.

The reasoning: platform selection is a wiring concern, not a runtime concern. Reading the configured platform once at composition keeps every layer below the composition root free of `if (isMobile)` style checks, so `RunService` and the domain never see a platform enum. Fail-fast composition means a bad platform config cannot fail halfway through a run with a confusing resolution error.

## Driver lifecycle owned by the run service

`RunService` receives the selected driver as `IAutomationDriver` and calls `StartAsync` before loading tests and `StopAsync` in a `finally`, so the driver is always stopped whether the run succeeds or throws. The entry points never touch the driver and never construct one. This centralizes lifecycle in one place: the web and mobile executables are identical apart from their `SpicyTofu:Platform` value.

## Timeout injection through options

Sessions apply timeouts at start time from bound options. `WebDriver` receives `IOptions<TestExecution>` and `IOptions<PlaywrightConfig>` and calls `SetDefaultTimeout` and `SetDefaultNavigationTimeout` on each new context. This keeps timeout policy in configuration rather than hard-coded in the driver.

## Empty infrastructure passthrough

`AddInfrastructureDependencies` returns the collection unchanged. It exists so the web composition root has a stable call site for infrastructure services that have not been added yet. This is scaffolding, not behavior.

## Implicit environment startup with reuse-and-own lifecycle

Mobile automation needs a running Appium server and a booted device, which the user should not have to stand up manually. `MobileHost` starts both lazily on first session start, in the order server then device. Each launcher adopts the principle that what already runs is reused and only what the framework started is shut down: a pre-booted emulator or a manually started Appium server is left running, while a process this run created is killed on dispose. This keeps the cleanup predictable and avoids surprising the user by tearing down their own emulator.

## Concrete host registration on both platforms

`BrowserHost` and `MobileHost` are both registered as their concrete types with no abstraction in front of them. No second implementation of either exists, so an interface would be speculative. The difference between the platforms is in what each host owns: `BrowserHost` owns a browser, while `MobileHost` owns the server and device launchers.

## AppiumConfig naming to avoid a namespace collision

The mobile config class is `AppiumConfig`, not `Appium`. `Appium.WebDriver` exposes a top-level assembly namespace literally named `Appium`; a class with the same name becomes unresolvable in code that imports the client (CS0118). Naming the class `AppiumConfig` (matching the existing `PlaywrightConfig`) avoids the collision and keeps the JSON section `Appium` unchanged. See [Configuration](../technical/configuration.md).

## Unimplemented resolver vs the single composition point

Older AGENTS.md text described a reflection-based resolver for platform implementations. That was never implemented, and the current decision is to keep platform selection explicit: one `AddAutomation` method, one config read, one `IAutomationDriver` registration. See [Dependency Injection](../technical/dependency-injection.md).

## Related pages

- [Automation Driver Contract](../technical/automation-driver-contract.md)
- [Dependency Injection](../technical/dependency-injection.md)
- [Web Automation](../technical/web-automation.md)
- [Known Issues and Discrepancies](./known-issues-and-discrepancies.md)