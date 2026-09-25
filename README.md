# Spicy-Tofu

Spicy-Tofu is a cross-platform test automation framework for web and mobile, written in C# on .NET 9. Test cases are defined in JSON, run through a single shared core, and driven by [Playwright](https://playwright.dev/) on web or [Appium](https://appium.io/) on mobile.

**Status:** pre-alpha. The project is at version 0.1.0 and has not been released. Configuration, composition, test loading, logging, and the automation driver lifecycle work end to end. The path that turns a loaded step into an actual browser or device interaction does not exist yet. See [Current Status](#current-status).

## Overview

Spicy-Tofu aims to give web and mobile test automation one execution model instead of two. A test is a JSON document rather than a compiled class, so a test author can describe intent without writing C#. The same test hierarchy, the same loader, the same run service, and the same logger are used on both platforms. Only the automation driver underneath differs.

The framework is built around four ideas:

- **One core, two platform implementations.** The test model, the driver contract, the runtime pipeline, and the logging pipeline are platform-neutral. Playwright and Appium are confined to one project.
- **Platform selection is a composition concern.** The platform is chosen once, when the dependency injection container is built. Nothing downstream branches on it.
- **JSON-driven definitions.** Test cases live in JSON files, not in code, so they can be authored, reviewed, and versioned separately from the framework.
- **Explicit driver lifecycle.** The run service starts the driver before loading tests and stops it in a `finally` block, so resources are released whether the run succeeds or throws.

The project is pre-alpha, so treat the model and the interfaces as the design surface that is settling, and the execution engine as work still to come.

## Current Status

### Implemented

- A five-project .NET 9 solution with dependencies pointing inward and `Domain` depending on nothing.
- JSON test definitions loaded from `Projects:RootDirectory` and deserialized into the `Test` model with case-insensitive property matching.
- Flattening of the `Test` / `Workflow` / `TestSteps` hierarchy into `TestExecutionStep` records.
- An execution-oriented logging pipeline: an `ILogger` facade, a `LogEntry` handoff, and an `IPrintStrategy` rendering seam with separate Interactive and CI layouts.
- An `IAutomationDriver` lifecycle owned by `RunService`, with start before the load and stop in a `finally` block.
- Composition-time platform selection in `AddAutomation`, which fails fast on a missing or unknown `SpicyTofu:Platform`.
- Web automation: a lazy Playwright browser host, named browser sessions, and per-context default and navigation timeouts.
- Mobile automation: an Appium server launcher, Android emulator and iOS simulator launchers, and named Appium sessions.
- Per-platform configuration binding plus a `TOFU_`-prefixed environment variable overlay.

### Not yet implemented

- **Executing steps.** `RunService` reports each loaded step through `ILogger.ActionStarted`. Nothing dispatches that step to a page or a screen.
- **Interaction surface.** `IWebPage` is an empty marker interface, `IMobileSession` exposes only `Name`, and there are no click, fill, navigate, or read operations anywhere in the contracts.
- **Locator resolution.** The `LocatorAttribute` SmartEnum and the `ILogger.LocatorResolution` rendering path exist, but no code turns a step's `Attribute` and `Target` into a platform locator.
- **Assertions.** There is no assertion model, and nothing evaluates an expected result.
- **Result reporting.** Runs produce console text only. There are no result records, no pass or fail state, and no report output.
- **Parallelism and retries.** `TestExecution:Parallel`, `Workers`, and `Retries` are bound to options but never read by the framework.
- **Automated tests.** No test projects exist, so `dotnet test` is a no-op.
- **Mobile interaction.** Appium sessions are created and torn down correctly, but nothing drives the application under test.

## How Spicy-Tofu Works

A run is a single linear flow. The diagram below is the actual current behaviour, including where it stops.

```text
appsettings.json + TOFU_ environment variables
                 │
                 ▼
      Host composition
      (Web/Program.cs or Mobile/Program.cs)
                 │
                 ├── AddWebExtensions / AddMobileDependencies
                 │       bind the configuration sections this platform owns
                 │
                 ├── AddInfrastructureDependencies
                 │       logger, JSON loader, run service (all singletons)
                 │
                 └── AddAutomation
                         reads SpicyTofu:Platform exactly once
                         │
                         ▼
              IAutomationDriver
              (WebDriver or MobileDriver)
                         │
                         ▼
                RunService.RunAsync()
                         │
                         ▼
              driver.StartAsync()
              starts the "default" session
                         │
                         ▼
              IJsonService.LoadJson()
              reads every *.json under Projects:RootDirectory
                         │
                         ▼
                  TestsLoaded event
                  carries List<Test>
                         │
                         ▼
             TestsLoadedHandler.Flatten()
             produces one TestExecutionStep per step
                         │
                         ▼
             ILogger.ActionStarted(step)
             reports the step
                         │
                         ▼
        [ not yet implemented ]
        dispatch the step to the driver
                         │
                         ▼
               driver.StopAsync()
               in a finally block
```

What each stage is for:

- **Configuration** supplies the test-definition location, timeouts, and the platform choice.
- **Host composition** builds the dependency injection container. The two entry points, `Web/Program.cs` and `Mobile/Program.cs`, are nearly identical; they differ only in which configuration sections they bind and what value `SpicyTofu:Platform` holds.
- **`AddAutomation`** reads the platform once and registers exactly one implementation of `IAutomationDriver`.
- **`RunService`** drives the run and owns the driver lifecycle. It depends on `IAutomationDriver` and never learns which platform it got.
- **`IJsonService`** reads and deserializes the JSON files, then raises `TestsLoaded` with the loaded tests. It does not flatten or execute.
- **`TestsLoadedHandler`** flattens the nested test hierarchy into a flat sequence of `TestExecutionStep` records.
- **Logging** reports progress as plain text through the `ILogger` and `IPrintStrategy` pair.

The load and the driver lifecycle are coupled deliberately: the driver starts first, and the stop call sits in a `finally` block so it runs on every path, including failures and cancellation.

For the full detail, see [Runtime Pipeline](docs/technical/runtime-pipeline.md) and [Dependency Injection](docs/technical/dependency-injection.md).

## Test Definition Model

Tests are a three-level hierarchy:

```text
Test
 └── Workflow
      └── TestSteps
```

| Level | Type | Properties |
|---|---|---|
| Test | `Domain.Entities.TestCases.Test` | `Id`, `Name`, `Workflows` |
| Workflow | `Domain.Entities.TestCases.Workflow` | `Id`, `Name`, `Steps` |
| Step | `Domain.Entities.TestCases.TestSteps` | `Type`, `Attribute`, `Target`, `Value` |

A step is a flat action description:

- **`Type`** is the action to perform, for example `Click` or `Fill`.
- **`Attribute`** is the locator strategy used to find the element, for example `Id`, `XPath`, or `AccessibilityId`.
- **`Target`** identifies the element the locator applies to.
- **`Value`** carries the data for the action.

`Type` and `Attribute` are free-form strings today. The set of recognised action types, and the mapping from an attribute to a concrete Playwright or Appium locator, are not yet defined in code, so the values below are illustrative rather than a specified vocabulary. `Domain.Entities.Enums.LocatorAttribute` lists the intended attribute values (`Id`, `Name`, `Class`, `Css`, `XPath`, `Text`, `AccessibilityId`) and flags which ones are mobile-supported, but nothing consumes it yet.

A representative file, illustrative only, since the repository does not yet ship a sample test definition:

```json
{
  "Id": "TC-001",
  "Name": "Valid login",
  "Workflows": [
    {
      "Id": "WF-001",
      "Name": "Login flow",
      "Steps": [
        {
          "Type": "Fill",
          "Attribute": "Id",
          "Target": "username",
          "Value": "alice"
        },
        {
          "Type": "Click",
          "Attribute": "Id",
          "Target": "login-submit",
          "Value": ""
        }
      ]
    }
  ]
}
```

Property matching is case-insensitive, so `Id` and `id` are equivalent. Each `*.json` file under `Projects:RootDirectory` deserializes into one `Test`, and a file that fails to parse is reported and skipped without stopping the load.

At runtime the nested hierarchy is flattened by `TestsLoadedHandler` into a `TestExecutionStep`, a record that keeps the owning `Test` and `Workflow` alongside the step so log output can name all three.

For the full model, see [Domain Layer](docs/technical/domain-layer.md).

## Repository Architecture

The solution follows Domain Driven Design with a shared core and two platform executables.

| Project | Type | References | Responsibility |
|---|---|---|---|
| `Domain` | class library | none | The framework's own domain: the test case model, locator attributes, execution records, and the configuration option classes |
| `Application` | class library | `Domain` | Use-case seams: the automation driver contracts, the logging facade, and the runtime service interfaces |
| `Infrastructure` | class library | `Application`, `Domain` | Outward-facing concerns: the Playwright and Appium implementations, dependency injection, the JSON loader, the run service, and console logging |
| `Web` | executable | all three core projects | The web composition root and entry point |
| `Mobile` | executable | all three core projects | The mobile composition root and entry point |

Two points are worth calling out because they are easy to get wrong:

- **Dependencies point inward.** `Domain` depends on nothing, `Application` depends only on `Domain`, and the executables sit outermost. Platform details never leak toward the core.
- **The platform implementations live in `Infrastructure`, not in `Web` and `Mobile`.** Both `Microsoft.Playwright` and `Appium.WebDriver` are referenced by `Infrastructure` alone. The `Web` and `Mobile` projects are thin composition roots, which is why they are almost identical.

The four DDD layers are `Domain`, `Application`, `Infrastructure`, and `Presentation`. There is no separate `Presentation` project: the `Web` and `Mobile` executables are the presentation layer.

For the full layout, including package references, see [Architecture Overview](docs/technical/architecture-overview.md).

## Automation Architecture

The automation contract lives in `Application.Automation` and contains no Playwright or Appium types.

```text
IAutomationDriver
├── IWebDriver
└── IMobileDriver
```

- **`IAutomationDriver`** is the base contract every platform implements. It is deliberately minimal: `StartAsync` and `StopAsync`. It is the only automation surface `RunService` depends on.
- **`IWebDriver`** adds `Page`, `StartSessionAsync(name, options)`, and `GetSession(name)`, and is implemented by `WebDriver` on top of Playwright.
- **`IMobileDriver`** adds `StartSessionAsync(name, options)` and `GetSession(name)`, and is implemented by `MobileDriver` on top of Appium.

Both platform drivers also implement `IAsyncDisposable`. Sessions are named, and `StartAsync` opens one called `default` on both platforms, so a run can hold more than one browser context or Appium session at a time.

The important consequence is that `RunService` takes an `IAutomationDriver` through constructor injection and calls only the base members. It has no idea whether it is driving Chromium or an Android emulator, and it never needs to know. Any future step executor inherits the same property.

The contracts currently stop at lifecycle and session management. The interaction surface that would sit on `IWebPage` and `IMobileSession` is not defined yet.

For the full contract, see [Automation Driver Contract](docs/technical/automation-driver-contract.md), [Web Automation](docs/technical/web-automation.md), and [Mobile Automation](docs/technical/mobile-automation.md).

## Platform Selection

`SpicyTofu:Platform` determines which automation implementation is composed at startup.

```json
{
  "SpicyTofu": {
    "Environment": "Development",
    "Platform": "Web"
  }
}
```

`AddAutomation` in `Infrastructure.Extensions.DependencyInjection` is the only place in the codebase that reads this value and the only place that branches on platform.

```text
Platform = Web
      │
      ▼
  WebDriver
      │
      ▼
IAutomationDriver

Platform = Mobile
      │
      ▼
 MobileDriver
      │
      ▼
IAutomationDriver
```

The driver is registered as a singleton, and both its platform interface and `IAutomationDriver` forward to that same instance, so a run never ends up with two drivers holding separate session registries.

A missing or unknown value throws an `InvalidOperationException` during composition, before any test loading begins. There is no fallback to a default platform and no reflection-based resolver.

**Platform selection is a composition concern, not an execution concern.** There is deliberately no `if (isMobile)` branch anywhere below the composition root, and no platform enum in `Domain` or `Application`.

For the wiring detail, see [Dependency Injection](docs/technical/dependency-injection.md).

## Configuration

Settings live in `Web/appsettings.json` and `Mobile/appsettings.json`, both copied to the build output. Sections are split by owner, and each platform only binds its own.

| Section | Bound by | Controls |
|---|---|---|
| `SpicyTofu` | both | `Environment` label and the `Platform` selection |
| `Projects` | both | `RootDirectory`, the folder scanned for test definition JSON |
| `TestExecution` | both | `DefaultTimeoutMs`; `Parallel`, `Workers`, and `Retries` are bound but unused |
| `Logging` | both | Log threshold, output mode, timestamps, and wrap widths |
| `Playwright` | web only | Browser choice, headless mode, navigation timeout |
| `Appium` | mobile only | Server URL, platform and device, app path, SDK paths, and launch options |

`Logging` is not present in either `appsettings.json`, so the defaults on the `LoggingConfig` class apply unless overlaid.

A representative web configuration, with placeholders in place of machine-specific values:

```json
{
  "SpicyTofu": {
    "Environment": "Development",
    "Platform": "Web"
  },
  "Projects": {
    "RootDirectory": "<path to the folder holding your test JSON files>"
  },
  "TestExecution": {
    "Workers": 1,
    "Retries": 0,
    "DefaultTimeoutMs": 30000
  },
  "Playwright": {
    "Browser": "Chromium",
    "Headless": false,
    "NavigationTimeoutMs": 30000
  }
}
```

The mobile file has the same core sections with `Platform` set to `Mobile`, and replaces `Playwright` with an `Appium` section:

```json
{
  "Appium": {
    "ServerUrl": "http://127.0.0.1:4723",
    "PlatformName": "Android",
    "AutomationName": "UiAutomator2",
    "DeviceName": "<your device or emulator name>",
    "PlatformVersion": "<your OS version>",
    "App": "<path to the app under test>",
    "AvdName": "<the AVD to boot, or empty if already running>",
    "AndroidSdkPath": "<Android SDK path, or empty to use ANDROID_HOME>",
    "IosSimulatorUdid": "<simulator UDID, macOS only>",
    "AppiumServerExecutable": "appium",
    "NoReset": false,
    "NewCommandTimeoutSec": 120
  }
}
```

`Projects:RootDirectory` is the path the loader reads, so it must be set for a run to find anything. If the directory does not exist, the load reports a warning and no event fires.

### Environment variable overlay

Both platforms overlay `TOFU_`-prefixed environment variables on top of the JSON, with `__` standing in for `:`. This is the supported way to keep secrets and machine-specific paths out of the committed files.

```bash
TOFU__SpicyTofu__Platform=Mobile
TOFU__Projects__RootDirectory=C:\qa\spicy-tofu-tests
TOFU__Appium__ServerUrl=http://127.0.0.1:4725
```

Do not commit credentials. Use environment variables or user secrets.

For every key, its default, and which assembly binds it, see [Configuration](docs/technical/configuration.md).

## Project Structure

```text
spicy-tofu/
├── Domain/                 test case model, locator attributes, configuration option classes
├── Application/            automation contracts, logging facade, runtime service interfaces
├── Infrastructure/         Playwright and Appium implementations, DI, JSON loading, console logging
├── Web/                    web composition root and entry point, web appsettings.json
├── Mobile/                 mobile composition root and entry point, mobile appsettings.json
├── docs/
│   ├── technical/          how the code works
│   └── wiki/               why it is built this way, platform notes, known issues
├── AGENTS.md               repository conventions for agents and contributors
├── CHANGELOG.md            release history
├── Directory.Build.Props   shared analysis, style enforcement, and version
├── spicy-tofu.sln          the solution
└── README.md               this file
```

The five projects sit at the top level in same-named folders. The Playwright and Appium implementations live under `Infrastructure/Automation/`, split into `Web` and `Mobile` subfolders, not under the `Web` and `Mobile` projects.

## Getting Started

### Prerequisites

- The .NET 9 SDK. Every project targets `net9.0`.
- For web: Playwright browsers, installed once per machine in the step below.
- For mobile: an Appium server executable on `PATH` (default `appium`, installed separately with `npm install -g appium`), plus either the Android SDK with `adb` and `emulator` and a configured AVD, or on macOS, Xcode with `simctl`.

Mobile runs are not self-contained. The launchers will start a server and a device for you, but they cannot install the tooling those processes depend on.

### Build

```bash
dotnet build spicy-tofu.sln
```

### Install Playwright browsers (web only)

No build target installs browsers, so this step is manual. The helper script is produced by the `Microsoft.Playwright` package and appears in the build output:

```bash
./Web/bin/Debug/net9.0/playwright.ps1 install chromium
```

On Windows PowerShell, run it as `.\Web\bin\Debug\net9.0\playwright.ps1 install chromium`. Substitute `firefox` or `webkit` to match `Playwright:Browser`.

### Run

Both platforms are ordinary console executables, so the standard .NET SDK invocation applies:

```bash
dotnet run --project Web
dotnet run --project Mobile
```

Before either command is useful, set `Projects:RootDirectory` to a folder containing test definition JSON, and make sure the relevant platform section is configured. Note that a run currently loads and reports steps without acting on them, so a successful exit does not mean any test passed.

### Test

```bash
dotnet test spicy-tofu.sln
```

This is currently a no-op, because the solution contains no test projects.

For the full setup matrix, see [Setup and Commands](docs/technical/setup-and-commands.md).

## Development Workflow

```bash
dotnet build spicy-tofu.sln                              # build every project
dotnet test spicy-tofu.sln                               # runs as a no-op today
dotnet format spicy-tofu.sln --verify-no-changes         # style check
```

`Directory.Build.Props` sets the analysis level to `latest-recommended` and enforces code style during the build, so `dotnet build` is also a style gate. `TreatWarningsAsErrors` applies to Release builds only.

`dotnet format` currently reports violations in files written earlier in the project's history. The check is expected to pass before a change is considered complete, but it is not green today. See [Known Limitations](#known-limitations).

[AGENTS.md](AGENTS.md) holds the repository conventions: architecture rules, configuration ownership, documentation and changelog obligations, and the commands above.

## Documentation

```text
README.md            What is Spicy-Tofu and how does it work?  (you are here)
docs/technical/      How exactly is this part implemented?
docs/wiki/           Why was it designed this way, and what is known to be broken?
```

The split is deliberate. This README is the overview and navigation layer. `docs/technical/` is the implementation detail layer. `docs/wiki/` is the decision, history, and knowledge layer.

Start with the [documentation index](docs/README.md) for the full table of contents.

### Architecture

- [Architecture Overview](docs/technical/architecture-overview.md) - solution layout, layers, project references, package references.
- [Domain Layer](docs/technical/domain-layer.md) - the test case model, locator attributes, execution records, and option classes.
- [Design Decisions](docs/wiki/design-decisions.md) - why the framework is shaped the way it is.

### Composition and configuration

- [Dependency Injection](docs/technical/dependency-injection.md) - both entry points, `AddServices`, `AddAutomation`, and the full registration table.
- [Configuration](docs/technical/configuration.md) - every section, every key, defaults, and which platform binds what.
- [Setup and Commands](docs/technical/setup-and-commands.md) - prerequisites, browser install, and Appium and device setup.

### Runtime

- [Runtime Pipeline](docs/technical/runtime-pipeline.md) - from JSON files to `TestExecutionStep` records, and the driver lifecycle around the load.
- [Logging](docs/technical/logging.md) - the `ILogger` API, `LogEntry`, the `IPrintStrategy` seam, and the Interactive and CI layouts.

### Automation

- [Automation Driver Contract](docs/technical/automation-driver-contract.md) - `IAutomationDriver`, `IWebDriver`, and `IMobileDriver`.
- [Web Automation](docs/technical/web-automation.md) - `BrowserHost`, `WebDriver`, and named browser sessions.
- [Mobile Automation](docs/technical/mobile-automation.md) - `MobileHost`, the Appium server launcher, the device launchers, and `MobileDriver`.
- [Platform Notes](docs/wiki/platform-notes.md) - per-platform knowledge, browser install, and reuse semantics.

### Reference

- [Wiki Index](docs/wiki/index.md) - the catalog of every wiki page.
- [Known Issues and Discrepancies](docs/wiki/known-issues-and-discrepancies.md) - the detailed list of gaps, naming oddities, and tolerated warnings.
- [CHANGELOG.md](CHANGELOG.md) - what has been added and changed.

## Design Principles

- **Platform-agnostic execution.** The run service, the loader, and the logger are shared. Platform code stays behind an interface.
- **Composition-time platform selection.** The platform is resolved once, when the container is built, and nowhere else.
- **Inward dependencies.** `Domain` depends on nothing, and no core project references a platform package.
- **JSON-driven test definitions.** Tests are data, so they can be authored and versioned independently of the framework.
- **An explicit driver lifecycle.** `RunService` starts the driver before the load and stops it in a `finally` block. The entry points never construct a driver.
- **No speculative abstractions.** Hosts such as `BrowserHost` and `MobileHost` are registered as concrete types because only one implementation exists. An interface would be added when a second one does.
- **No runtime platform branching.** No platform enum in the core, and no `if (isMobile)` below the composition root.

The reasoning behind each of these is recorded in [Design Decisions](docs/wiki/design-decisions.md).

## Known Limitations

- **Steps are not executed.** A run loads and reports steps, then exits. Nothing interacts with a browser or a device.
- **No assertions or result reporting.** There is no pass or fail state, and no report output beyond console text.
- **No test projects.** `dotnet test` is a no-op, and the framework itself is untested.
- **The format check is not green.** `dotnet format spicy-tofu.sln --verify-no-changes` reports violations in older files.
- **Compilation warnings in Debug builds.** `CS0108` and `CS8618` warnings come from the domain model. They are not errors because `TreatWarningsAsErrors` applies to Release only.
- **Mobile tooling is a manual prerequisite.** Appium, the Android SDK, and any AVD must already be installed and configured.
- **No LICENSE file.** The repository does not currently state reuse terms.
- **`Web/appsettings.json` ships a machine-specific `Projects:RootDirectory`.** Replace it locally or override it with a `TOFU_` environment variable.

The full list, including naming oddities such as the `AggregrateRoot` type name and the `TestSteps` class in `TestStep.cs`, is in [Known Issues and Discrepancies](docs/wiki/known-issues-and-discrepancies.md).
