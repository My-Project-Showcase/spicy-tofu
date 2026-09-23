---
title: Dependency Injection
updated: 2026-09-23
sources:
  - ../../Infrastructure/Extensions/DependencyInjection.cs
  - ../../Application/Automation/IAutomationDriver.cs
  - ../../Web/Extension/WebExtensions.cs
  - ../../Mobile/Extensions/MobileExtensions.cs
  - ../../Web/Program.cs
  - ../../Mobile/Program.cs
---

# Dependency Injection

The web and mobile executables are the composition roots. Both read configuration and register option bindings through DI extension methods, and both select their automation platform through the single `AddAutomation` composition point.

## Web entry point

`Web/Program.cs` builds the generic host and calls, in order:

1. `AddWebExtensions` (from `WebExtensions`): loads `appsettings.json`, overlays `TOFU_`-prefixed environment variables, binds `SpicyTofuConfig`, `Projects`, `PlaywrightConfig`, and `TestExecution`.
2. `AddInfrastructureDependencies` (from `Infrastructure.Extensions`): calls `AddServices`, which binds the `Logging` section and registers the logging services and the runtime services, all singletons: `ILogger` (`Logger`), `IPrintStrategy` (`ConsolePrintStrategy`), `IJsonService` (`JsonService`), `IRunService` (`RunService`), and `TestsLoadedHandler`.
3. `AddAutomation` (from `Infrastructure.Extensions`): the single platform-selection point.

`Program.cs` owns the host with `using IHost host = ...` so the host and its singleton services are disposed when the run ends (see [Driver Lifecycle and Host Disposal](#driver-lifecycle-and-host-disposal)). After the host is built it resolves `IRunService` from the container and calls `RunAsync()`, which drives the runtime pipeline. See [Runtime Pipeline](./runtime-pipeline.md).

## AddAutomation

`AddAutomation` lives in `Infrastructure.Extensions.DependencyInjection`. It is the only place in the codebase that reads `SpicyTofu:Platform` and the only place that branches on platform. It reads the value once during service registration and registers exactly one platform:

- When the value is `Web` (case-insensitive): registers `BrowserHost` (singleton), `WebDriver` (singleton), and forwards `IWebDriver` and `IAutomationDriver` to that same `WebDriver` instance.
- When the value is `Mobile` (case-insensitive): registers `MobileHost` (singleton), `MobileDriver` (singleton), and forwards `IMobileDriver` and `IAutomationDriver` to that same `MobileDriver` instance.
- When the value is missing, empty, or anything else: throws `InvalidOperationException` with a message naming the invalid value. A bad platform fails at composition time, before any test execution starts, and never falls back to a default platform.

Each forward uses `sp => sp.GetRequiredService<WebDriver>()` (or `MobileDriver`), so the concrete driver, its platform interface, and `IAutomationDriver` all resolve to the same singleton instance. A second `AddSingleton<Interface, Implementation>` registration would create a second instance with its own session registry, so forwards are used instead.

Drivers are singletons, not scoped as before. `RunService` is a singleton resolved from the root provider, and `Host.CreateDefaultBuilder` enables `ValidateScopes` in the `Development` environment; a scoped driver under a singleton consumer would throw at runtime.

See [Automation Driver Contract](./automation-driver-contract.md) for the interface relationship, and [Configuration](./configuration.md) for the `SpicyTofu` section.

## Mobile entry point

`Mobile/Program.cs` builds the generic host and calls, in order:

1. `AddMobileDependencies` (from `MobileExtensions`): loads configuration, overlays `TOFU_` variables, and binds `SpicyTofuConfig`, `Projects`, `TestExecution`, and `AppiumConfig`. Note it binds the options twice (once in `AddConfigProperties` called directly, once through `AddEnvCompatibility`, which builds a merged configuration and re-binds), which is redundant but harmless.
2. `AddInfrastructureDependencies` (from `Infrastructure.Extensions`): the same passthrough as the web entry point.
3. `AddAutomation` (from `Infrastructure.Extensions`): the same single platform-selection point as web.

Like the web entry point, the mobile program owns the host with `using IHost host = ...`, then resolves `IRunService` and calls `RunAsync()`. The only difference between the two executables is which `IAutomationDriver` `AddAutomation` registers, which is driven by their `SpicyTofu:Platform` values. Selection and execution flow are shared.

## Driver lifecycle and host disposal

Driver start/stop and host disposal are separate responsibilities.

- `RunService` owns the automation lifecycle inside `RunAsync`: it calls `IAutomationDriver.StartAsync()` before loading tests and `StopAsync()` in a `finally` block. See [Runtime Pipeline](./runtime-pipeline.md).
- The `Program.cs` files own application disposal through `using IHost host = ...`. `IHost` implements only `IDisposable` (not `IAsyncDisposable`), so the host is disposed synchronously; the service provider's sync dispose still runs `DisposeAsync` on the `IAsyncDisposable` singletons (`BrowserHost`, `MobileHost`, and the driver forwarding targets). This closes the browser, Appium server, and device processes that the run started.

`Program.cs` never calls `StartAsync` or `StopAsync`, and never constructs a driver. That is `RunService`'s job.

## Wiring summary

| Registration | Definition | Lifetime | Gate |
|---|---|---|---|
| `ILogger` / `Logger` | `AddServices` | singleton | always |
| `IPrintStrategy` / `ConsolePrintStrategy` | `AddServices` | singleton | always |
| `IJsonService` / `JsonService` | `AddServices` | singleton | always |
| `IRunService` / `RunService` | `AddServices` | singleton | always |
| `TestsLoadedHandler` | `AddServices` | singleton | always |
| `BrowserHost` | `AddAutomation` | singleton | `SpicyTofu:Platform` = `Web` |
| `WebDriver` | `AddAutomation` | singleton | `SpicyTofu:Platform` = `Web` |
| `IWebDriver` (forward to `WebDriver`) | `AddAutomation` | singleton | `SpicyTofu:Platform` = `Web` |
| `IAutomationDriver` (forward to `WebDriver`) | `AddAutomation` | singleton | `SpicyTofu:Platform` = `Web` |
| `MobileHost` | `AddAutomation` | singleton | `SpicyTofu:Platform` = `Mobile` |
| `MobileDriver` | `AddAutomation` | singleton | `SpicyTofu:Platform` = `Mobile` |
| `IMobileDriver` (forward to `MobileDriver`) | `AddAutomation` | singleton | `SpicyTofu:Platform` = `Mobile` |
| `IAutomationDriver` (forward to `MobileDriver`) | `AddAutomation` | singleton | `SpicyTofu:Platform` = `Mobile` |
| Config option bindings | platform extension | - | always |

Only one `IAutomationDriver` registration exists per process, matching the selected platform.

## Related pages

- [Configuration](./configuration.md)
- [Automation Driver Contract](./automation-driver-contract.md)
- [Web Automation](./web-automation.md)
- [Mobile Automation](./mobile-automation.md)
- [Domain Layer](./domain-layer.md)
- [Logging](./logging.md)