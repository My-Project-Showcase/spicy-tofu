---
title: Design Decisions
updated: 2026-09-20
sources:
  - ../technical/architecture-overview.md
  - ../technical/automation-driver-contract.md
  - ../technical/dependency-injection.md
  - ../technical/web-automation.md
  - ../../Application/Automation/Web/IWebPage.cs
  - ../../Infrastructure/Automation/Web/BrowserHost.cs
  - ../../Infrastructure/Automation/Web/WebDriver.cs
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

## Platform-gated registration in AddWebAutomation

`AddWebAutomation` checks `configuration["SpicyTofu:Platform"]` and only registers web services when the value equals `Web` (case-insensitive). This keeps core code free of platform checks: the check happens in the composition wiring, matching AGENTS.md, which allows branching on `SpicyTofu:Platform` at the composition root. Web and mobile do not reference each other, and each platform adds its own services through its own composition root.

## Timeout injection through options

Sessions apply timeouts at start time from bound options. `WebDriver` receives `IOptions<TestExecution>` and `IOptions<PlaywrightConfig>` and calls `SetDefaultTimeout` and `SetDefaultNavigationTimeout` on each new context. This keeps timeout policy in configuration rather than hard-coded in the driver.

## Empty infrastructure passthrough

`AddInfrastructureDependencies` returns the collection unchanged. It exists so the web composition root has a stable call site for infrastructure services that have not been added yet. This is scaffolding, not behavior.

## Unimplemented resolver

AGENTS.md describes a reflection-based resolver for platform implementations. No such component exists in the code. Platform wiring is done explicitly. Treat the resolver as planned, not present.

## Related pages

- [Automation Driver Contract](../technical/automation-driver-contract.md)
- [Dependency Injection](../technical/dependency-injection.md)
- [Web Automation](../technical/web-automation.md)
- [Known Issues and Discrepancies](./known-issues-and-discrepancies.md)