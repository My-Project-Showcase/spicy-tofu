# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - Unreleased

### Added

- Automation driver contract in `Application.Automation`: the `IAutomationDriver` base contract and the web-specific `IWebDriver`, `IWebPage`, `IWebSession`, and `WebContextOptions` interfaces.
- Web automation implementation in `Infrastructure.Automation.Web`: `BrowserHost` (lazy Playwright startup guarded by a semaphore), `WebDriver`, `BrowserSession`, and `WebPage`.
  - Named sessions with a `default` session and a registry backed by a concurrent dictionary.
  - Default and navigation timeouts sourced from the bound `TestExecution` and `PlaywrightConfig` options.
  - Platform-gated registration: `AddWebAutomation` registers `BrowserHost` (singleton) and `IWebDriver` (scoped) only when `SpicyTofu:Platform` is `Web`.
- Configuration keys and option classes: `TestExecution:DefaultTimeoutMs` and `Playwright:NavigationTimeoutMs`, bound through `IOptions`.
- Mobile automation contract in `Application.Automation.Mobile`: `IMobileDriver`, `IMobileSession`, and `MobileContextOptions`, mirroring the web contract.
- Mobile automation implementation in `Infrastructure.Automation.Mobile`: `MobileHost` (lazy Appium server and device startup guarded by a semaphore), `AppiumServerLauncher`, the Android emulator and iOS simulator launchers, `MobileDriver`, and `MobileSession`.
  - The Appium server and a device are started automatically on first session start; a server already listening on the port and a device already booted are reused, and only processes this run started are shut down.
  - Named sessions with a `default` session and a registry backed by a concurrent dictionary, mirroring the web driver.
  - Command timeout sourced from the bound `TestExecution` option.
  - Platform-gated registration: `AddMobileAutomation` registers `MobileHost` (singleton) and `IMobileDriver` (scoped) only when `SpicyTofu:Platform` is `Mobile`.
- Mobile configuration keys and option class: `AppiumServerExecutable`, `AndroidSdkPath`, `AvdName`, `IosSimulatorUdid`, and `PlatformVersion` on `AppiumConfig`, bound through `IOptions` by the mobile project.
- `Mobile` now references `Infrastructure` and wires `AddMobileAutomation`, and `Mobile/Program.cs` builds the host.
- Appium.WebDriver package (8.3.2) referenced by `Infrastructure` only.
- Documentation: `docs/technical/` and `docs/wiki/` pages (including the new `docs/technical/mobile-automation.md`), the `docs/README.md` table of contents, and the wiki index and log.

### Fixed

- The iOS simulator "not configured" guard ran after the booted check, so an empty `IosSimulatorUdid` was never reported; the guard now runs first.
- Appium server and emulator processes could stall when their redirected output filled the OS pipe buffer; their stdout and stderr are now drained asynchronously.
- The default `appium` executable failed to launch on Windows because npm installs it as a `.cmd`/`.ps1` shim; the launcher now runs shim executables through `cmd.exe /c` on Windows.