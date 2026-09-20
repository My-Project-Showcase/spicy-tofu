---
title: Mobile Automation
updated: 2026-09-20
sources:
  - ../../Infrastructure/Automation/Mobile/MobileHost.cs
  - ../../Infrastructure/Automation/Mobile/MobileDriver.cs
  - ../../Infrastructure/Automation/Mobile/MobileSession.cs
  - ../../Infrastructure/Automation/Mobile/AppiumServerLauncher.cs
  - ../../Infrastructure/Automation/Mobile/Devices/IDeviceLauncher.cs
  - ../../Infrastructure/Automation/Mobile/Devices/AndroidEmulatorLauncher.cs
  - ../../Infrastructure/Automation/Mobile/Devices/IosSimulatorLauncher.cs
  - ../../Infrastructure/Extensions/DependencyInjection.cs
---

# Mobile Automation

The mobile implementation lives in `Infrastructure.Automation.Mobile`. It is the only code that touches Appium and Selenium types from `Appium.WebDriver` (8.3.2).

## Lifecycle model

Unlike the web platform, the mobile side must stand up two external processes before a session can start: the Appium server and a device (an Android emulator or an iOS simulator). `MobileHost` owns both launchers and runs them once, lazily, on first session start:

1. `AppiumServerLauncher` ensures a server responds on `Appium:ServerUrl`.
2. `IDeviceLauncher` ensures a device is booted; the concrete launcher is chosen by `Appium:PlatformName`.
3. `MobileDriver` then creates an Appium driver against the server, which opens a session on the device.

If a server is already listening on the port, or a device is already booted, the corresponding launcher treats it as pre-existing and does not start it. On dispose, only the processes this run started are shut down; pre-existing server and device are left running.

## MobileHost

`MobileHost` is a concrete class registered as a singleton.

- Owns an `AppiumServerLauncher` and the `IDeviceLauncher` chosen for the configured platform (`IosSimulatorLauncher` when `PlatformName` equals `iOS`, otherwise `AndroidEmulatorLauncher`).
- `EnsureEnvironmentReadyAsync` starts the server, then the device, in order. It is guarded by a `SemaphoreSlim(1, 1)` with a ready flag, so concurrent racing callers only start once.
- `DisposeAsync` shuts down whatever this host started and disposes the semaphore.

There is no `IMobileHost` interface; DI registers the concrete `MobileHost`.

## AppiumServerLauncher

`AppiumServerLauncher` owns one Appium server process.

- `EnsureStartedAsync` first TCP-connects to the `ServerUrl` port. If something already listens there, it is treated as an existing server and not started.
- Otherwise it launches the configured executable (`AppiumServerExecutable`, default `appium`) with `--address` and `--port` derived from `ServerUrl`. On Windows the launch goes through `cmd.exe /c`, because the npm-installed `appium` is a `.cmd`/`.ps1` shim, not a real executable. It then polls the `ServerUrl` until it returns success or `StartupWaitMs` elapses.
- `ShutdownIfStartedAsync` kills the process only when this launcher started it.

## Device launchers

Both implement `Infrastructure.Automation.Mobile.Devices.IDeviceLauncher`, which mirrors the server contract: `EnsureStartedAsync`, `ShutdownIfStartedAsync`, `IAsyncDisposable`.

### AndroidEmulatorLauncher

- Resolves `emulator` and `adb`: from `Appium:AndroidSdkPath` if set, otherwise from `ANDROID_HOME`, otherwise by bare name on `PATH`.
- If `adb devices` already shows an `emulator-*` device, the emulator is treated as pre-existing and not started.
- Otherwise it launches `emulator -avd <AvdName>`, marks the emulator as started by us, and polls `adb shell getprop sys.boot_completed` until it returns `1` or `BootTimeoutMs` (180s) elapses.
- `ShutdownIfStartedAsync` kills the emulator process only when this launcher started it.

### IosSimulatorLauncher

- Only supported on macOS; on Windows it throws `PlatformNotSupportedException` when asked to start.
- A missing `Appium:IosSimulatorUdid` throws `InvalidOperationException` before any reuse or boot check.
- If `xcrun simctl list devices booted` already shows the configured `Appium:IosSimulatorUdid`, the simulator is treated as pre-existing and not started.
- Otherwise it runs `xcrun simctl boot <udid>`, marks the simulator as started by us, and waits for `xcrun simctl bootstatus <udid> -b` to exit 0.
- `ShutdownIfStartedAsync` runs `xcrun simctl shutdown <udid>` only when this launcher started it.

## MobileDriver

`MobileDriver` is registered as scoped and implements `IMobileDriver`.

- Holds a `ConcurrentDictionary<string, MobileSession>` keyed by session name. The default session name is the constant `"default"`.
- `StartAsync` starts the `default` session via `StartSessionAsync("default")`.
- `StartSessionAsync(name, options)` calls `MobileHost.EnsureEnvironmentReadyAsync`, then builds an Appium driver and wraps it in a `MobileSession`. `MobileContextOptions` carries no settings yet, so it is accepted and ignored.
- The driver is `IOSDriver` when `PlatformName` is `iOS` (case-insensitive), otherwise `AndroidDriver`. `PlatformVersion`, `noReset`, and `newCommandTimeout` are set as additional Appium options.
- The command timeout comes from `TestExecution.DefaultTimeoutMs`, passed to the driver constructor.
- Starting a session whose name already exists throws `InvalidOperationException`; getting an unknown session name throws `InvalidOperationException`.
- `StopAsync` disposes every registered driver and clears the registry; `DisposeAsync` delegates to `StopAsync`.

## MobileSession

`MobileSession` implements `IMobileSession` with `Name`. It holds the underlying Appium driver, currently exposed only to the containing driver. There is no page or element surface on the contract yet.

## DI registration

`AddMobileAutomation` registers `MobileHost` as a singleton and `IMobileDriver`/`MobileDriver` as scoped, gated on `SpicyTofu:Platform` = `Mobile`. See [Dependency Injection](./dependency-injection.md).

## Lifecycle summary

| Action | Component | Behavior |
|---|---|---|
| First session start | `MobileHost` | starts Appium server, then the device |
| `StartAsync` | `MobileDriver` | starts the `default` session |
| `StartSessionAsync(name, ...)` | `MobileDriver` | creates an Appium driver and registers a `MobileSession` |
| `StopAsync` / dispose | `MobileDriver` | disposes every registered driver |
| Host dispose | `MobileHost` | shuts down only what it started |

## Related pages

- [Automation Driver Contract](./automation-driver-contract.md)
- [Dependency Injection](./dependency-injection.md)
- [Configuration](./configuration.md)
- [Platform Notes](../wiki/platform-notes.md)
- [Design Decisions](../wiki/design-decisions.md)