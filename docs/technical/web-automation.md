---
title: Web Automation
updated: 2026-09-23
sources:
  - ../../Infrastructure/Automation/Web/BrowserHost.cs
  - ../../Infrastructure/Automation/Web/WebDriver.cs
  - ../../Infrastructure/Automation/Web/BrowserSession.cs
  - ../../Infrastructure/Automation/Web/WebPage.cs
  - ../../Infrastructure/Extensions/DependencyInjection.cs
---

# Web Automation

The web implementation lives in `Infrastructure.Automation.Web`. It is the only code that touches Playwright types.

## BrowserHost

`BrowserHost` is a concrete class registered as a singleton. It is the Playwright entry point.

- On first use it lazily creates a Playwright instance and launches a browser.
- Browser choice comes from `PlaywrightConfig.Browser`: `firefox` selects Firefox, `webkit` selects WebKit, anything else including `Chromium` selects Chromium.
- `Headless` from `PlaywrightConfig` controls headless mode.
- Concurrent singleton creation is guarded with a semaphore (`SemaphoreSlim(1, 1)`), so two racing consumers get the same browser instance.
- `DisposeAsync` closes the browser, disposes the Playwright instance and the semaphore.

There is no `IBrowserHost` interface; DI registers the concrete `BrowserHost`.

## WebDriver

`WebDriver` is registered as a singleton (as `WebDriver`, `IWebDriver`, and `IAutomationDriver`, all resolving to the same instance) and implements `IWebDriver`.

- Holds a `ConcurrentDictionary<string, BrowserSession>` keyed by session name. The default session name is the constant `"default"`.
- `StartAsync` starts the `default` session via `StartSessionAsync("default")`.
- `StartSessionAsync(name, options)` opens a browser context from `BrowserHost`, creates a `BrowserSession`, registers it under the given name, and applies timeouts. `options.BaseUrl` maps to Playwright's `BaseURL`.
- Default and navigation timeouts come from `TestExecution.DefaultTimeoutMs` and `PlaywrightConfig.NavigationTimeoutMs` respectively, applied through Playwright's `SetDefaultTimeout` and `SetDefaultNavigationTimeout`.
- `Page` returns `GetSession("default").Page`.
- Starting a session whose name already exists throws `InvalidOperationException`; getting an unknown session name throws `InvalidOperationException`.
- `StopAsync` disposes every registered session and clears the registry; `DisposeAsync` delegates to `StopAsync`.

## BrowserSession

`BrowserSession` implements `IWebSession`.

- `Name`: the session name.
- `Page`: an `IWebPage` wrapping the Playwright `IPage` of the session context.
- `Context`: the Playwright `IBrowserContext`.
- `DisposeAsync` closes the context.

## WebPage

`WebPage` implements `IWebPage` by holding the wrapped Playwright `IPage`. It is a seam: application code depends on `IWebPage` and never sees Playwright.

## Lifecycle summary

| Action | Component | Behavior |
|---|---|---|
| First page access | `BrowserHost` | lazily launches Playwright browser |
| `StartAsync` | `RunService` via `IAutomationDriver` | starts the `default` session |
| `StartSessionAsync(name, ...)` | `WebDriver` | opens context and registers `BrowserSession` |
| `Page` | `WebDriver` | `GetSession("default").Page` |
| `StopAsync` / dispose | `RunService` via `IAutomationDriver` | closes every registered context |
| Host dispose | `BrowserHost` | closes the browser and Playwright |

## Related pages

- [Automation Driver Contract](./automation-driver-contract.md)
- [Dependency Injection](./dependency-injection.md)
- [Configuration](./configuration.md)
- [Platform Notes](../wiki/platform-notes.md)
- [Design Decisions](../wiki/design-decisions.md)