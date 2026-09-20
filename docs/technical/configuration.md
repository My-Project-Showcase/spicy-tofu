---
title: Configuration
updated: 2026-09-20
sources:
  - ../../Domain/Runtime/Environment/Configuration/SpicyTofuConfig.cs
  - ../../Domain/Runtime/Environment/Configuration/TestExecution.cs
  - ../../Domain/Runtime/Environment/Configuration/Projects.cs
  - ../../Domain/Runtime/Environment/Configuration/PlaywrightConfig.cs
  - ../../Domain/Runtime/Environment/Configuration/Appium.cs
  - ../../Web/Extension/WebExtensions.cs
  - ../../Mobile/Extensions/MobileExtensions.cs
  - ../../Web/appsettings.json
  - ../../Mobile/appsettings.json
---

# Configuration

Settings live in `appsettings.json`, split by owner. Core sections are platform-neutral and are bound by both platforms. `Playwright` is read only by the web project and `Appium` only by the mobile project; this split is enforced per assembly by which option classes each platform's DI registers.

## Core sections

`SpicyTofu` and `TestExecution` appear in both `Web/appsettings.json` and `Mobile/appsettings.json` and bind to the matching option classes through both platforms' DI extensions.

Example values (identical across both files):

- `SpicyTofu:Environment` = `Development`; `SpicyTofu:Platform` differs: `Web` in the web file, `Mobile` in the mobile file.
- `TestExecution:Workers` = `1`, `Retries` = `0`, `DefaultTimeoutMs` = `30000`. The `TestExecution` class also has a `Parallel` flag that the JSON files do not set.

`Projects` is a core section in both JSON files (`Projects:RootDirectory` = `./projects`) but no option class is bound for it. The internal `Projects` class exposes `RootWebDirectory` and `RootMobileDirectory`, which do not match the JSON key. See [Known Issues and Discrepancies](../wiki/known-issues-and-discrepancies.md).

## Platform sections

`Playwright` is bound only by the web project:

- `Browser` = `Chromium`
- `Headless` = `true`
- `NavigationTimeoutMs` = `30000`

The `PlaywrightConfig` class also has a `TimeOut` property that is not set by JSON and is not used by the framework.

`Appium` is bound only by the mobile project (`Appium` class):

- `ServerUrl` = `http://127.0.0.1:4723`
- `PlatformName` = `Android`
- `AutomationName` = `UiAutomator2`
- `DeviceName` = `Pixel_7_API_34`
- `App` = `./apps/sample.apk`
- `NoReset` = `false`
- `NewCommandTimeoutSec` = `120`

`Mobile/appsettings.json` also carries `Appium:PlatformVersion` = `14`, but the `Appium` class has no `PlatformVersion` property, so that key binds to nothing.

`Web/appsettings.json` also carries an `Appium` section for reference, but the web project never binds it.

## Environment variable overlay

`TOFU_`-prefixed environment variables override values. In `WebExtensions.AddWebExtensions` the extension builds a merged configuration with `AddConfiguration(configuration)` followed by `AddEnvironmentVariables("TOFU_")`; `TOFU__` maps to `:` so `TOFU__SpicyTofu__Platform` maps to `SpicyTofu:Platform`. The mobile `MobileExtensions.AddEnvCompatibility` does the same overlay.

## Where configuration is read

- `WebExtensions.AddWebExtensions` binds `SpicyTofuConfig`, `TestExecution`, and `PlaywrightConfig`.
- `MobileExtensions.AddConfigProperties` binds `SpicyTofuConfig`, `TestExecution`, and `Appium`.

Option classes live in `Domain.Runtime.Environment.Configuration` so both platforms share them.

## Related pages

- [Dependency Injection](./dependency-injection.md)
- [Domain Layer](./domain-layer.md)
- [Setup and Commands](./setup-and-commands.md)
- [Platform Notes](../wiki/platform-notes.md)
- [Known Issues and Discrepancies](../wiki/known-issues-and-discrepancies.md)