---
title: Configuration
updated: 2026-09-25
sources:
  - ../../Domain/Runtime/Environment/Configuration/SpicyTofuConfig.cs
  - ../../Domain/Runtime/Environment/Configuration/TestExecution.cs
  - ../../Domain/Runtime/Environment/Configuration/Projects.cs
  - ../../Domain/Runtime/Environment/Configuration/PlaywrightConfig.cs
  - ../../Domain/Runtime/Environment/Configuration/AppiumConfig.cs
  - ../../Domain/Runtime/Environment/Configuration/LoggingConfig.cs
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

`Projects` is a core section in both JSON files. Both platforms bind it to the `Projects` option class. `Projects:RootDirectory` (`D:\Projects\QA\Spicy-Tofu` in `Web/appsettings.json`) points at the folder holding the test-definition JSON files; `Mobile/appsettings.json` keeps the default `./projects` value.

`Logging` is a fourth core section. It is bound to `LoggingConfig` in `AddServices` (shared through `AddInfrastructureDependencies`), so both platforms read it. The section is not present in either `appsettings.json`; the defaults live on `LoggingConfig` (`Level` = `Information`, `OutputMode` = empty, `Timestamps` = `false`, `MaxColumnWidth` = `40`, `MaxLineWidth` = `135`). See [Logging](./logging.md).

## Platform sections

`Playwright` is bound only by the web project:

- `Browser` = `Chromium`
- `Headless` = `false`
- `NavigationTimeoutMs` = `30000`

The `PlaywrightConfig` class also has a `TimeOut` property that is not set by JSON and is not used by the framework.

`Appium` is bound only by the mobile project (`AppiumConfig` class):

- `ServerUrl` = `http://127.0.0.1:4723`
- `PlatformName` = `Android`
- `AutomationName` = `UiAutomator2`
- `DeviceName` = `Pixel_7_API_34`
- `PlatformVersion` = `14`
- `App` = `./apps/sample.apk`
- `AvdName` = `` (empty; the user sets it to the AVD to boot)
- `AndroidSdkPath` = `` (empty; falls back to the `ANDROID_HOME` environment variable)
- `IosSimulatorUdid` = `` (empty; only needed on macOS)
- `AppiumServerExecutable` = `appium` (default)
- `NoReset` = `false`
- `NewCommandTimeoutSec` = `120`

`Appium` appears only in `Mobile/appsettings.json`. The web file has no `Appium` section, and the web project never binds `AppiumConfig`.

## Environment variable overlay

`TOFU_`-prefixed environment variables override values. In `WebExtensions.AddWebExtensions` the extension builds a merged configuration with `AddConfiguration(configuration)` followed by `AddEnvironmentVariables("TOFU_")`; `TOFU__` maps to `:` so `TOFU__SpicyTofu__Platform` maps to `SpicyTofu:Platform`. The mobile `MobileExtensions.AddEnvCompatibility` does the same overlay.

## Where configuration is read

- `WebExtensions.AddWebExtensions` binds `SpicyTofuConfig`, `Projects`, `PlaywrightConfig`, and `TestExecution`.
- `MobileExtensions.AddConfigProperties` binds `SpicyTofuConfig`, `Projects`, `TestExecution`, and `AppiumConfig`.

`SpicyTofu:Platform` is read by `AddAutomation` (shared, in `Infrastructure.Extensions`), the single composition point that selects the driver. No other code reads it. See [Dependency Injection](./dependency-injection.md).

Option classes live in `Domain.Runtime.Environment.Configuration` so both platforms share them.

## Related pages

- [Dependency Injection](./dependency-injection.md)
- [Domain Layer](./domain-layer.md)
- [Setup and Commands](./setup-and-commands.md)
- [Platform Notes](../wiki/platform-notes.md)
- [Known Issues and Discrepancies](../wiki/known-issues-and-discrepancies.md)
- [Logging](./logging.md)