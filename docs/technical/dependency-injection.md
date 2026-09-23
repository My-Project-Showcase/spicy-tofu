---
title: Dependency Injection
updated: 2026-09-23
sources:
  - ../../Infrastructure/Extensions/DependencyInjection.cs
  - ../../Web/Extension/WebExtensions.cs
  - ../../Mobile/Extensions/MobileExtensions.cs
  - ../../Web/Program.cs
  - ../../Mobile/Program.cs
---

# Dependency Injection

The web and mobile executables are the composition roots. Both read configuration and register option bindings through DI extension methods.

## Web entry point

`Web/Program.cs` builds the generic host and calls, in order:

1. `AddWebExtensions` (from `WebExtensions`): loads `appsettings.json`, overlays `TOFU_`-prefixed environment variables, binds `SpicyTofuConfig`, `Projects`, `PlaywrightConfig`, and `TestExecution`.
2. `AddInfrastructureDependencies` (from `Infrastructure.Extensions`): calls `AddServices`, which binds the `Logging` section and registers the logging services and the runtime services, all singletons: `ILogger` (`Logger`), `IPrintStrategy` (`ConsolePrintStrategy`), `IJsonService` (`JsonService`), `IRunService` (`RunService`), and `TestsLoadedHandler`.
3. `AddWebAutomation` (from `Infrastructure.Extensions`): the web platform registration.

`Program.cs` resolves `IRunService` from the container and calls `RunAsync()`, which drives the runtime pipeline. See [Runtime Pipeline](./runtime-pipeline.md).

## AddWebAutomation

`AddWebAutomation` lives in `Infrastructure.Extensions.DependencyInjection`. It checks `configuration["SpicyTofu:Platform"]` (case-insensitive) and returns the collection unchanged unless the value equals `Web`. When active it registers:

- `BrowserHost` singleton (the concrete type, as a singleton).
- `IWebDriver` scoped, implemented by `WebDriver`.

`WebDriver` receives `IOptions<TestExecution>` and `IOptions<PlaywrightConfig>` through constructor injection and applies the timeouts at session start. See [Web Automation](./web-automation.md).

## AddMobileAutomation

`AddMobileAutomation` lives in `Infrastructure.Extensions.DependencyInjection`, next to `AddWebAutomation`. It checks `configuration["SpicyTofu:Platform"]` (case-insensitive) and returns the collection unchanged unless the value equals `Mobile`. When active it registers:

- `MobileHost` singleton (the concrete type, as a singleton).
- `IMobileDriver` scoped, implemented by `MobileDriver`.

`MobileDriver` receives `IOptions<TestExecution>` and `IOptions<AppiumConfig>` through constructor injection and applies the command timeout at session start. See [Mobile Automation](./mobile-automation.md).

## Mobile entry point

`Mobile/Program.cs` builds the generic host and calls, in order:

1. `AddMobileDependencies` (from `MobileExtensions`): loads configuration, overlays `TOFU_` variables, and binds `SpicyTofuConfig`, `TestExecution`, and `AppiumConfig`. Note it binds the options twice (once in `AddConfigProperties` called directly, once through `AddEnvCompatibility`, which builds a merged configuration and re-binds), which is redundant but harmless.
2. `AddInfrastructureDependencies` (from `Infrastructure.Extensions`): the same passthrough as the web entry point.
3. `AddMobileAutomation` (from `Infrastructure.Extensions`): the mobile platform registration.

The host is built but never started by `Program.cs`; there is no `Run()` call. Nothing starts the Appium server or an emulator during build or test; the launchers run only on first session start. See [Platform Notes](../wiki/platform-notes.md).

## Wiring summary

| Registration | Definition | Lifetime | Gate |
|---|---|---|---|
| `ILogger` / `Logger` | `AddServices` | singleton | always |
| `IPrintStrategy` / `ConsolePrintStrategy` | `AddServices` | singleton | always |
| `IJsonService` / `JsonService` | `AddServices` | singleton | always |
| `IRunService` / `RunService` | `AddServices` | singleton | always |
| `TestsLoadedHandler` | `AddServices` | singleton | always |
| `BrowserHost` | `AddWebAutomation` | singleton | `SpicyTofu:Platform` = `Web` |
| `IWebDriver` / `WebDriver` | `AddWebAutomation` | scoped | `SpicyTofu:Platform` = `Web` |
| `MobileHost` | `AddMobileAutomation` | singleton | `SpicyTofu:Platform` = `Mobile` |
| `IMobileDriver` / `MobileDriver` | `AddMobileAutomation` | scoped | `SpicyTofu:Platform` = `Mobile` |
| Config option bindings | platform extension | - | always |

## Related pages

- [Configuration](./configuration.md)
- [Automation Driver Contract](./automation-driver-contract.md)
- [Web Automation](./web-automation.md)
- [Mobile Automation](./mobile-automation.md)
- [Domain Layer](./domain-layer.md)
- [Logging](./logging.md)