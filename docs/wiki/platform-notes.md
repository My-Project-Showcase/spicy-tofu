---
title: Platform Notes
updated: 2026-09-20
sources:
  - ../../Web/appsettings.json
  - ../../Mobile/appsettings.json
  - ../../Web/Extension/WebExtensions.cs
  - ../../Mobile/Extensions/MobileExtensions.cs
  - ../../Infrastructure/Automation/Web/BrowserHost.cs
  - ../technical/setup-and-commands.md
  - ../technical/web-automation.md
  - ../technical/configuration.md
---

# Platform Notes

This page collects per-platform knowledge. It covers both platforms because both are documented in one place per the agreed scope.

## Playwright (web)

- Playwright is referenced only by `Infrastructure` (package 1.62.0). The `Web` executable is the composition root; it does not reference Playwright itself.
- Browsers have to be installed once per machine through the `playwright.ps1` helper in the build output (see [Setup and Commands](../technical/setup-and-commands.md)). Browsers are not reinstalled by build or test.
- Browser selection comes from `PlaywrightConfig.Browser`: `firefox` maps to Firefox, `webkit` maps to WebKit, anything else (including `Chromium`) maps to Chromium.
- `PlaywrightConfig.Headless` controls headless mode.
- Timeouts are applied per browser context. `TestExecution.DefaultTimeoutMs` feeds Playwright's default timeout and `PlaywrightConfig.NavigationTimeoutMs` feeds the navigation timeout.
- `WebDriver` keeps contexts keyed by session name; the default session is `default`. Duplicate start throws `InvalidOperationException`. See [Web Automation](../technical/web-automation.md).
- The web host is built but never started (`Web/Program.cs` has no `Run()`); no browser starts during build or test.

## Appium (mobile)

- The mobile project is a placeholder. `Mobile/Program.cs` wires `AddMobileDependencies`, which binds `SpicyTofuConfig`, `TestExecution`, and `Appium`, but the host is never built (`Build()` is not called) in `Program.cs`.
- `Appium` class options come from `Mobile/appsettings.json`: `ServerUrl` (`http://127.0.0.1:4723`), `PlatformName` (`Android`), `AutomationName` (`UiAutomator2`), `DeviceName`, `App`, `NoReset`, `NewCommandTimeoutSec`.
- `Appium:PlatformVersion` exists in the JSON but has no matching `Appium` class property, so it binds to nothing.
- The mobile project references `Domain` only. It has no Appium driver implementation and no session lifecycle yet.
- `LocatorAttribute.MobileSupported` flags which locators work on mobile (`Id`, `Class`, `XPath`, `AccessibilityId` marked supported; `Name`, `Css`, `Text` not), but nothing consumes the flag yet. Unverified: whether a mobile driver will gate locators on this flag.

## Shared platform notes

- Configuration overlays apply in both platforms through `AddEnvironmentVariables("TOFU_")`; see [Configuration](../technical/configuration.md).
- `Projects:RootDirectory` in both JSON files is not bound to any option class. The internal `Projects` class has `RootWebDirectory` and `RootMobileDirectory` instead. See [Known Issues and Discrepancies](./known-issues-and-discrepancies.md).

## Related pages

- [Setup and Commands](../technical/setup-and-commands.md)
- [Configuration](../technical/configuration.md)
- [Known Issues and Discrepancies](./known-issues-and-discrepancies.md)
- [Design Decisions](./design-decisions.md)