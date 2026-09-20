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