---
title: Automation Driver Contract
updated: 2026-09-20
sources:
  - ../../Application/Application.csproj
  - ../../Application/Automation/IAutomationDriver.cs
  - ../../Application/Automation/Web/IWebDriver.cs
  - ../../Application/Automation/Web/IWebPage.cs
  - ../../Application/Automation/Web/IWebSession.cs
  - ../../Application/Automation/Web/WebContextOptions.cs
---

# Automation Driver Contract

The `Application` project owns the driver abstraction every platform implements. The interfaces keep Playwright and Appium types out of the contract.

## IAutomationDriver

`Application.Automation.IAutomationDriver`: the base contract every platform driver implements.

- `Task StartAsync()`
- `Task StopAsync()`

It does not carry class members; it is a plain interface. There is no shared dispatcher type in `Application` that resolves a driver; callers resolve `IAutomationDriver` from DI. See [Dependency Injection](./dependency-injection.md).

## IWebDriver

`Application.Automation.Web.IWebDriver` implements `IAsyncDisposable` and adds web-specific surface:

- `IWebPage Page`: the page of the session named `default`.
- `Task StartAsync()`: start the `default` session.
- `Task StopAsync()`: stop all sessions.
- `Task<IWebSession> StartSessionAsync(string name, WebContextOptions? options = null)`: start a named session.
- `IWebSession GetSession(string name)`: return a running session or throw.

Note that `IWebDriver` does not inherit `IAutomationDriver`; it declares the same `StartAsync` and `StopAsync` methods itself.

## IWebPage

`Application.Automation.Web.IWebPage`: an empty marker interface. The web implementation wraps a Playwright `IPage`. It is the contract seam that lets application code depend on a page without referencing Playwright. Behavioral surface is not yet defined.

## IWebSession

`Application.Automation.Web.IWebSession`:

- `string Name`
- `IWebPage Page`

## WebContextOptions

`Application.Automation.Web.WebContextOptions`: a sealed record with a single property `string? BaseUrl`. Passed to `StartSessionAsync` for per-session context configuration. `WebDriver` maps it to Playwright's `BaseURL` when starting a browser context.

## Resolver note

AGENTS.md describes a "reflection-based resolver" that discovers platform implementations. No such type exists in the code today. Platform wiring is performed explicitly through `AddWebAutomation`. See [Known Issues and Discrepancies](../wiki/known-issues-and-discrepancies.md).

## Related pages

- [Architecture Overview](./architecture-overview.md)
- [Dependency Injection](./dependency-injection.md)
- [Web Automation](./web-automation.md)
- [Design Decisions](../wiki/design-decisions.md)