## [2026-09-28] update | Web interaction slice: navigate, click, fill, results, tests

- Added interaction methods to `IResolvedElement` (`ClickAsync`, `FillAsync`, `GetTextAsync`) and `NavigateAsync` to `IWebPage` and `IMobileSession`; added the platform-neutral `INavigator` seam with `WebNavigator` and `MobileNavigator`, registered per platform in `AddAutomation`.
- `ClickEvent` now performs the click; added `FillEvent` and implemented the empty `NavigateEvent` stub; `AddEvents` registers all three.
- `RunService` now isolates each step with `IEventRegistry.TryGet` and a try/catch, counts executed and failed steps, and returns a `RunResult`; `Web/Program.cs` and `Mobile/Program.cs` map `IsSuccess` to the process exit code.
- `IElementRepository` registration switched to `TryAddSingleton` so an application can supply its own repository; `TestsLoadedHandler` now tolerates null `Workflows`/`Steps`; `JsonService` reports parse failures through `ILogger`.
- Made domain model properties nullable and marked the `Test` overrides with `new`, clearing the `CS8618` and `CS0108` warnings; fixed the remaining whitespace and final-newline violations so the format check passes.
- Added the `Tests` xUnit project covering `TestsLoadedHandler`, `EventService`, `RunService`, `JsonService`, and `SampleElementRepository`.
- Updated `runtime-pipeline.md`, `elements-and-locators.md`, `automation-driver-contract.md`, `web-automation.md`, `mobile-automation.md`, `domain-layer.md`, `dependency-injection.md`, `architecture-overview.md`, `setup-and-commands.md`, `design-decisions.md`, `known-issues-and-discrepancies.md`, `index.md`, `docs/README.md`, `AGENTS.md`, and `CHANGELOG.md`.

## [2026-09-27] update | Element repository and locator resolver

- Added the element and locator layer: `SharedKernel` element and locator models plus MUI, Shadcn, and Syncfusion component libraries; `Application` `IElementRepository`, `ILocatorResolver`, and `IResolvedElement`; and `Infrastructure` `SampleElementRepository`, `WebLocatorResolver`, `MobileLocatorResolver`, `WebResolvedElement`, and `MobileResolvedElement`, with `ResolveAsync` on `IWebPage` and `IMobileSession`.
- Replaced `Domain.Entities.Enums.LocatorAttribute` and `Application.Locators.LocatorCandidate` with the `SharedKernel.Locators.Locator` model; logging now uses it.
- `ClickEvent` now resolves the step's element through the repository and resolver.
- Added `docs/technical/elements-and-locators.md`; updated the automation, architecture, domain, logging, DI, runtime, and setup pages, plus `platform-notes.md`, `known-issues-and-discrepancies.md`, `design-decisions.md`, `index.md`, the root `README.md`, `AGENTS.md`, and `CHANGELOG.md`.

## [2026-09-27] update | Using directive order

- Defined a fixed using-directive order: external (`System.*` first, then Microsoft and third-party), then `Application`, `Domain`, `Infrastructure`, `SharedKernel`, `Web`, `Mobile`, with one blank line between groups.
- Disabled the `dotnet format` import sorter in `.editorconfig` (removed `dotnet_sort_system_directives_first`), because it forces strict alphabetical order and cannot express the group order. Using order is now a documented convention applied by hand.
- Added `.opencode/skills/csharp-developer/references/using-directives.md`, linked from the skill, and recorded the convention in AGENTS.md.
- Reordered using directives across the solution to match.
- Updated `known-issues-and-discrepancies.md` (the format-check paragraph no longer lists using-ordering diagnostics) and `setup-and-commands.md`.

## [2026-09-27] update | Test execution flow

- `JsonService` no longer raises a `TestsLoaded` event and returns only the loaded `List<Test>`; the event was removed from `IJsonService` and `JsonService`.
- `RunService` now orchestrates execution directly: it loads, flattens through `TestsLoadedHandler`, resolves each step's action through `IEventRegistry` to an `ITestEvent`, executes it, and reports the returned `TestExecutionResult` through `ILogger` (`ActionCompleted`/`ActionFailed`). Steps run sequentially and execution no longer stops after the first step.
- `AddServices` now calls `AddEvents`, registering the `ITestEvent` implementations that `EventService` indexes.
- Added a `design-decisions.md` note explaining why loading and execution are separate with no event handoff.
- Rewrote `docs/technical/runtime-pipeline.md`; updated `dependency-injection.md`, the root `README.md`, `AGENTS.md`, and `CHANGELOG.md`.

## [2026-09-23] update | Driver lifecycle wiring

- Added the single `AddAutomation` composition point in `Infrastructure.Extensions`: reads `SpicyTofu:Platform` once, registers exactly one platform, throws on a missing or unknown value.
- `IWebDriver` and `IMobileDriver` now inherit `IAutomationDriver` and `IAsyncDisposable`; the duplicated `StartAsync`/`StopAsync` declarations are gone.
- `RunService` owns the driver lifecycle: `StartAsync` before loading tests, `StopAsync` in a `finally`. Both `Web/Program.cs` and `Mobile/Program.cs` resolve `IRunService` and call `RunAsync()`, and dispose the host with `using IHost host`.
- Mobile now binds `Projects` through `MobileExtensions.AddConfigProperties`.
- Rewrote `docs/technical/dependency-injection.md` (AddAutomation, singleton forwards, host disposal). Updated `automation-driver-contract.md` (inheritance, platform wiring), `runtime-pipeline.md` (start/stop around the load), `web-automation.md` and `mobile-automation.md` (singleton drivers, DI section), and `configuration.md` (mobile Projects binding, Platform read location).
- Updated `docs/wiki/design-decisions.md` (single composition point, driver lifecycle ownership), `platform-notes.md` (entry point flow, mobile Projects), and `known-issues-and-discrepancies.md` (resolver and never-started-hosts entries resolved).
- Updated `AGENTS.md` (resolver references replaced by `AddAutomation`) and `docs/README.md` (TOC summaries and dates).

## [2026-09-23] update | Execution-oriented logging

- Rewrote `docs/technical/logging.md` around the execution-oriented API: `ActionStarted`, `LocatorResolution`, `ActionCompleted`, and `ActionFailed`, with `TestExecutionStep` passed directly and `LocatorCandidate(string Strategy, string Value)` as the locator handoff.
- Updated `known-issues-and-discrepancies.md`: the empty `LocatorStrategy` record and the "unused OutputModeDetector" note are gone, and the logging files are format-clean.

## [2026-09-23] update | Logging in place

- Added `docs/technical/logging.md` covering `ILogger`, `LogEntry`, the `IPrintStrategy` seam, `ConsolePrintStrategy` formatting, and the `Logging` configuration section.
- Updated `known-issues-and-discrepancies.md`: the logging files are format-clean, `OutputModeDetector` is a seam nothing consumes yet, and `ILogger.Error` carries a tolerated `CA1716` warning.

## [2026-09-23] update | Event-driven runtime pipeline

- Updated `known-issues-and-discrepancies.md`: the web host now starts a run through `RunAsync`, `Projects` is bound on the web project only, and the runtime-pipeline files are format-clean.

## [2026-09-20] add | Docs backfill

- Created `docs/technical/` and `docs/wiki/` pages covering architecture, domain layer, configuration, dependency injection, the automation driver contract, web automation, setup commands, design decisions, platform notes, and known issues.
- Created `docs/wiki/index.md` and this log.
- Filled `docs/README.md` with the documentation overview and table of contents.
- Filled `CHANGELOG.md` with the 0.1.0 Unreleased entry.

## [2026-09-20] update | Mobile automation implementation

- Added `docs/technical/mobile-automation.md` covering `MobileHost`, `AppiumServerLauncher`, the Android and iOS device launchers, `MobileDriver`, and `MobileSession`.
- Updated `automation-driver-contract.md` with `IMobileDriver`, `IMobileSession`, and `MobileContextOptions`.
- Updated `dependency-injection.md` with `AddMobileAutomation` and the new `MobileProgram` wiring.
- Updated `configuration.md` and `domain-layer.md` for `AppiumConfig` and the new Appium keys.
- Updated `architecture-overview.md` and `setup-and-commands.md` for the mobile implementation and Appium prerequisites.
- Updated `platform-notes.md` (mobile section rewritten), `design-decisions.md` (implicit environment startup and AppiumConfig naming), and `known-issues-and-discrepancies.md` (resolved placeholder and binding-gap items).

## [2026-09-20] update | Mobile automation review fixes

- Fixed `AppiumServerLauncher` and the Android and iOS device launchers so long-lived child processes do not stall on unread stdout/stderr pipes; redirected output is now drained asynchronously.
- Appium server launch now runs through `cmd.exe /c` on Windows when the configured executable is a shell shim (npm installs `appium` as `.cmd`/`.ps1`, not a real executable).
- Moved the missing-UDID guard in `IosSimulatorLauncher` before the reuse check so an empty `Appium:IosSimulatorUdid` throws instead of silently passing.
- Updated `mobile-automation.md` (Windows shim launch, iOS guard order) and `platform-notes.md` (Windows shim launch).