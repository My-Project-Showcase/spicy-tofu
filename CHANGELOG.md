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
- Event-driven test loading pipeline: `IJsonService.TestsLoaded` raises after JSON deserialization with the loaded `List<Test>`, `TestsLoadedHandler` flattens the test hierarchy into `TestExecutionStep` records, and `RunService` subscribes inside `RunAsync`, converts through the handler, and runs the resulting steps.
- Logging infrastructure: the `ILogger` facade with level filtering and `LogEntry` as the presentation-neutral handoff in `Application.Logging`, and `IPrintStrategy` as the rendering seam, implemented by `ConsolePrintStrategy` in `Infrastructure.Logging`. `Logging` is a core configuration section bound in `AddServices` (`LoggingConfig`), and `RunService` now reports its steps through `ILogger` instead of `Console.WriteLine`.
- Driver lifecycle wiring: the single `AddAutomation` composition point reads `SpicyTofu:Platform` once and registers exactly one platform as `IAutomationDriver`, throwing on a missing or unknown value. `IWebDriver` and `IMobileDriver` inherit the `IAutomationDriver` lifecycle contract and `IAsyncDisposable`. `RunService` starts the selected driver before loading tests and stops it in a `finally`, and both `Web/Program.cs` and `Mobile/Program.cs` resolve `IRunService`, drive the shared pipeline, and dispose the host with `using IHost`. `MobileExtensions` now binds `Projects`.
- Root `README.md` as the primary entry point for the repository: overview, current status against the actual implementation, runtime flow, test definition model, architecture, platform selection, configuration, repository layout, getting started, development commands, and a documentation map. It links into `docs/technical/` and `docs/wiki/` rather than duplicating them, and states explicitly that steps are loaded and reported but not yet executed against a driver.

### Changed

- Logging refined into an execution-oriented API: the generic `KeyValue` and `Table` methods left `ILogger` (their only caller was the placeholder step print), replaced by `ActionStarted`, `LocatorResolution`, `ActionCompleted`, and `ActionFailed`, which take the `TestExecutionStep` directly. `LocatorCandidate(string Strategy, string Value)` replaces the empty `LocatorStrategy` record, `OutputModeDetector` is now consumed by `ConsolePrintStrategy`, and `Interactive` and `Ci` output render with different layouts while sharing the identical logger API. The action line is a single comma-separated kwargs line in both modes (shape `Action: {action}, Attribute: {attribute}, Target: {target}, Value: {value}, Workflow: {workflow}, TestCase: {testCase}`), wrapping at `Logging:MaxLineWidth`.
- Unified platform selection: `AddAutomation` replaces the platform-specific `AddWebAutomation` and `AddMobileAutomation`, and the web and mobile drivers are registered as singletons (with `IAutomationDriver` forwarded to the same instance) instead of scoped, which avoids a singleton-over-scoped validation failure under the selected platform.
- Corrected stale claims in the technical documentation: `configuration.md` and `domain-layer.md` now report the real `Logging:MaxLineWidth` default of `135` (previously `100`) and the real `Playwright:Headless` value of `false`; the false claim that `Web/appsettings.json` carries an `Appium` section was removed; `setup-and-commands.md` now records that both platforms bind `Projects` and that `Directory.Build.props` carries the `Version` property.

### Fixed

- The iOS simulator "not configured" guard ran after the booted check, so an empty `IosSimulatorUdid` was never reported; the guard now runs first.
- Appium server and emulator processes could stall when their redirected output filled the OS pipe buffer; their stdout and stderr are now drained asynchronously.
- The default `appium` executable failed to launch on Windows because npm installs it as a `.cmd`/`.ps1` shim; the launcher now runs shim executables through `cmd.exe /c` on Windows.