---
title: Platform Notes
updated: 2026-09-23
sources:
  - ../../Web/appsettings.json
  - ../../Mobile/appsettings.json
  - ../../Web/Extension/WebExtensions.cs
  - ../../Mobile/Extensions/MobileExtensions.cs
  - ../../Infrastructure/Automation/Web/BrowserHost.cs
  - ../../Infrastructure/Automation/Mobile/MobileHost.cs
  - ../../Infrastructure/Automation/Mobile/MobileDriver.cs
  - ../../Infrastructure/Automation/Mobile/AppiumServerLauncher.cs
  - ../../Infrastructure/Automation/Mobile/Devices/AndroidEmulatorLauncher.cs
  - ../../Infrastructure/Automation/Mobile/Devices/IosSimulatorLauncher.cs
  - ../technical/setup-and-commands.md
  - ../technical/web-automation.md
  - ../technical/mobile-automation.md
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
- `Web/Program.cs` builds the host with `using IHost host = ...`, then resolves `IRunService` and calls `RunAsync()`. The driver start inside `RunAsync` launches the browser and creates the `default` context during a run; nothing starts during `Build()`. The host disposal at scope end closes the browser through `BrowserHost`.

## Appium (mobile)

- `Mobile/Program.cs` builds the host and calls `AddMobileDependencies`, `AddInfrastructureDependencies`, and `AddAutomation`. It resolves `IRunService` and calls `RunAsync()`, the same path as web. The Appium server and device launchers only run on first session start, not during build; starting on a machine without Appium installed fails cleanly when the launch is attempted. The host disposal at scope end shuts down whatever this run started.
- `AppiumConfig` options come from `Mobile/appsettings.json`: `ServerUrl` (`http://127.0.0.1:4723`), `PlatformName` (`Android`), `AutomationName` (`UiAutomator2`), `DeviceName`, `PlatformVersion`, `App`, `AvdName`, `AndroidSdkPath`, `IosSimulatorUdid`, `AppiumServerExecutable`, `NoReset`, `NewCommandTimeoutSec`.
- The `AppiumConfig` class name does not match the `Appium` JSON section. The `Appium` name is avoided because `Appium.WebDriver` exposes a top-level `Appium` namespace that collides with a class of the same name.
- `MobileHost` starts the Appium server (from `AppiumServerExecutable`, default `appium`) and then the device. The device launcher is chosen by `PlatformName`: `iOS` selects the simulator launcher, anything else the Android emulator launcher. On Windows the server launch is wrapped in `cmd.exe /c` because npm installs Appium as a `.cmd`/`.ps1` shim, not a real executable.
- Android resolution order for `emulator`/`adb`: `Appium:AndroidSdkPath`, then `ANDROID_HOME`, then `PATH`. The AVD comes from `Appium:AvdName`.
- iOS simulator support is macOS-only (`xcrun simctl`); on Windows the launcher throws `PlatformNotSupportedException`. The simulator is chosen by `Appium:IosSimulatorUdid`.
- Launchers reuse an already-running server or device and only shut down what they started themselves. The user can pre-boot an emulator or start Appium manually; the framework leaves those alone.
- `MobileDriver` creates one Appium driver per named session, choosing `AndroidDriver` or `IOSDriver` from `PlatformName`. `TestExecution.DefaultTimeoutMs` feeds the command timeout.
- `LocatorAttribute.MobileSupported` flags which locators work on mobile (`Id`, `Class`, `XPath`, `AccessibilityId` marked supported; `Name`, `Css`, `Text` not), but nothing consumes the flag yet. Unverified: whether a mobile driver will gate locators on this flag.

## Shared platform notes

- Configuration overlays apply in both platforms through `AddEnvironmentVariables("TOFU_")`; see [Configuration](../technical/configuration.md).
- `Projects:RootDirectory` binds on both platforms through the `Projects` option class. Web points it at the test-definition folder; Mobile keeps the default `./projects`.

## Related pages

- [Setup and Commands](../technical/setup-and-commands.md)
- [Configuration](../technical/configuration.md)
- [Known Issues and Discrepancies](./known-issues-and-discrepancies.md)
- [Design Decisions](./design-decisions.md)