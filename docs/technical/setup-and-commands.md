---
title: Setup and Commands
updated: 2026-09-28
sources:
  - ../../AGENTS.md
  - ../../Directory.Build.Props
  - ../../spicy-tofu.sln
  - ../../Domain/Domain.csproj
  - ../../Application/Application.csproj
  - ../../Infrastructure/Infrastructure.csproj
  - ../../Web/Web.csproj
  - ../../Mobile/Mobile.csproj
  - ../../Tests/Tests.csproj
  - ../../Web/appsettings.json
  - ../../Mobile/appsettings.json
---

# Setup and Commands

## Prerequisites

- .NET SDK 9.0 (`net9.0` target).
- For the web platform, Playwright browsers. The Playwright package places a `playwright.ps1` helper in the web project's output directory; it exists at `Web/bin/Debug/net9.0/playwright.ps1` after a build.
- For the mobile platform, an Appium server executable. The default `AppiumServerExecutable` value is `appium`, resolved on `PATH`. `Appium.WebDriver` (8.3.2) brings the client; the server is a separate install (`npm install -g appium`). The machine running the tests also needs the Android SDK (`adb`, `emulator`) and an AVD configured under `Appium:AvdName`, or, on macOS, Xcode's `simctl` with an `Appium:IosSimulatorUdid`.

## Playwright browser install

```powershell
.\Web\bin\Debug\net9.0\playwright.ps1 install chromium
```

The helper script is produced by the `Microsoft.Playwright` NuGet package (1.62.0) and lives in the build output next to the web executable. This step is manual; no build target installs browsers automatically.

## Appium and mobile device setup

The mobile launchers expect the tooling to exist on the host:

- The Appium server is started as a process whose executable comes from `AppiumServerExecutable` (default `appium`). It is found on `PATH`; no package installs it.
- The Android emulator is started through the `emulator` tool from `Appium:AndroidSdkPath`, or failing that from the `ANDROID_HOME` environment variable, or from `PATH`. `adb` is resolved the same way. The AVD to boot comes from `Appium:AvdName`.
- The iOS simulator is started through `xcrun simctl` and is only supported on macOS. See [Mobile Automation](./mobile-automation.md).

Nothing here is installed by build or test; the server and the device are only started by the launchers on first session start at runtime.

## Build

```bash
dotnet build spicy-tofu.sln
```

The solution contains seven projects: `Domain`, `Application`, `Infrastructure`, `SharedKernel`, `Web`, `Mobile`, `Tests`. All target `net9.0`.

## Test

```bash
dotnet test spicy-tofu.sln
```

The `Tests` project runs under xUnit and covers the core runtime services (`TestsLoadedHandler`, `EventService`, `RunService`, `JsonService`) and `SampleElementRepository`, using fakes for the driver, event registry, and logger.

## Format check

```bash
dotnet format spicy-tofu.sln --verify-no-changes
```

Style is defined by `.editorconfig` and `Directory.Build.props` and enforced in build (`EnforceCodeStyleInBuild`), with analyzers at `latest-recommended`. `TreatWarningsAsErrors` applies to Release builds only. Using-directive order is a documented convention, not enforced by `dotnet format`: external (`System.*` first, then Microsoft and third-party), then `Application`, `Domain`, `Infrastructure`, `SharedKernel`, `Web`, `Mobile`. See AGENTS.md. The format check passes.

## Configuration sources

- `appsettings.json` in `Web/` and `Mobile/` (both copied to output with `CopyToOutputDirectory=PreserveNewest`). Both entry points set the host content root to `AppContext.BaseDirectory`, so the output copy is loaded no matter which directory the process is started from (Rider, `dotnet run`, or the built `.exe` directly).
- `TOFU_`-prefixed environment variables overlay the JSON.
- `Directory.Build.props` sets analysis level, code style enforcement, and Release-only warnings-as-errors. It also carries the `Version` property (`0.1.0`), which is the single source of the version used by `CHANGELOG.md`.

## Configuration binding by platform

| Section | Bound by web | Bound by mobile |
|---|---|---|
| `SpicyTofu` | yes | yes |
| `TestExecution` | yes | yes |
| `Projects` | yes | yes |
| `Playwright` | yes | no |
| `Appium` | no | yes |
| `Logging` | yes, in `AddServices` | yes, in `AddServices` |

See [Configuration](./configuration.md).

## Related pages

- [Configuration](./configuration.md)
- [Architecture Overview](./architecture-overview.md)
- [Known Issues and Discrepancies](../wiki/known-issues-and-discrepancies.md)