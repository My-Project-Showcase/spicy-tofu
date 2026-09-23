---
title: Automation Driver Contract
updated: 2026-09-23
sources:
  - ../../Application/Application.csproj
  - ../../Application/Automation/IAutomationDriver.cs
  - ../../Application/Automation/Web/IWebDriver.cs
  - ../../Application/Automation/Web/IWebPage.cs
  - ../../Application/Automation/Web/IWebSession.cs
  - ../../Application/Automation/Web/WebContextOptions.cs
  - ../../Application/Automation/Mobile/IMobileDriver.cs
  - ../../Application/Automation/Mobile/IMobileSession.cs
  - ../../Application/Automation/Mobile/MobileContextOptions.cs
---

# Automation Driver Contract

The `Application` project owns the driver abstraction every platform implements. The interfaces keep Playwright and Appium types out of the contract.

## IAutomationDriver

`Application.Automation.IAutomationDriver`: the base contract every platform driver implements.

- `Task StartAsync()`
- `Task StopAsync()`

It does not carry class members; it is a plain interface. There is no shared dispatcher type in `Application` that resolves a driver; `RunService` receives the resolved `IAutomationDriver` through DI and owns its lifecycle. See [Dependency Injection](./dependency-injection.md) and [Runtime Pipeline](./runtime-pipeline.md).

## IWebDriver

`Application.Automation.Web.IWebDriver` inherits `IAutomationDriver` and implements `IAsyncDisposable`. It adds web-specific surface:

- `IWebPage Page`: the page of the session named `default`.
- `Task<IWebSession> StartSessionAsync(string name, WebContextOptions? options = null)`: start a named session.
- `IWebSession GetSession(string name)`: return a running session or throw.

The lifecycle methods `StartAsync` and `StopAsync` come from `IAutomationDriver`; they are not redeclared here.

## IWebPage

`Application.Automation.Web.IWebPage`: an empty marker interface. The web implementation wraps a Playwright `IPage`. It is the contract seam that lets application code depend on a page without referencing Playwright. Behavioral surface is not yet defined.

## IWebSession

`Application.Automation.Web.IWebSession`:

- `string Name`
- `IWebPage Page`

## WebContextOptions

`Application.Automation.Web.WebContextOptions`: a sealed record with a single property `string? BaseUrl`. Passed to `StartSessionAsync` for per-session context configuration. `WebDriver` maps it to Playwright's `BaseURL` when starting a browser context.

## IMobileDriver

`Application.Automation.Mobile.IMobileDriver` mirrors `IWebDriver`: it inherits `IAutomationDriver` and implements `IAsyncDisposable`, and adds mobile-specific surface:

- `Task<IMobileSession> StartSessionAsync(string name, MobileContextOptions? options = null)`: start a named session.
- `IMobileSession GetSession(string name)`: return a running session or throw.

Like `IWebDriver`, the lifecycle methods come from `IAutomationDriver` and are not redeclared. There is no page property yet because mobile interaction surface is not defined.

## IMobileSession

`Application.Automation.Mobile.IMobileSession`:

- `string Name`

It does not expose a page or screen yet. The implementation wraps an Appium driver. Behavioral surface is not yet defined.

## MobileContextOptions

`Application.Automation.Mobile.MobileContextOptions`: a sealed record with no members. It exists so `StartSessionAsync` has a stable options parameter, mirroring `WebContextOptions`. There are no mobile per-session options yet.

## Platform wiring

Platform selection happens once at composition time in `AddAutomation`, which reads `SpicyTofu:Platform` and registers exactly one driver as `IAutomationDriver`. Execution code never branches on platform; `RunService` takes the selected driver and calls only `IAutomationDriver` members. See [Dependency Injection](./dependency-injection.md) and [Known Issues and Discrepancies](../wiki/known-issues-and-discrepancies.md).

## Related pages

- [Architecture Overview](./architecture-overview.md)
- [Dependency Injection](./dependency-injection.md)
- [Web Automation](./web-automation.md)
- [Mobile Automation](./mobile-automation.md)
- [Design Decisions](../wiki/design-decisions.md)