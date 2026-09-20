---
title: Dependency Injection
updated: 2026-09-20
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

1. `AddWebExtensions` (from `WebExtensions`): loads `appsettings.json`, overlays `TOFU_`-prefixed environment variables, binds `SpicyTofuConfig`, `TestExecution`, and `PlaywrightConfig`.
2. `AddInfrastructureDependencies` (from `Infrastructure.Extensions`): a passthrough that currently returns `IServiceCollection` unchanged. It exists so the composition root has a single place to pull in infrastructure services later.
3. `AddWebAutomation` (from `Infrastructure.Extensions`): the web platform registration.

The host is built but never started by `Program.cs`; there is no `Run()` call. See [Known Issues and Discrepancies](../wiki/known-issues-and-discrepancies.md).

## AddWebAutomation

`AddWebAutomation` lives in `Infrastructure.Extensions.DependencyInjection`. It checks `configuration["SpicyTofu:Platform"]` (case-insensitive) and returns the collection unchanged unless the value equals `Web`. When active it registers:

- `BrowserHost` singleton (the concrete type, as a singleton).
- `IWebDriver` scoped, implemented by `WebDriver`.

`WebDriver` receives `IOptions<TestExecution>` and `IOptions<PlaywrightConfig>` through constructor injection and applies the timeouts at session start. See [Web Automation](./web-automation.md).

## Mobile entry point

`Mobile/Program.cs` calls `AddMobileDependencies` (from `MobileExtensions`). That extension loads configuration, overlays `TOFU_` variables, and binds `SpicyTofuConfig`, `TestExecution`, and `Appium`. Note it binds the options twice (once in `AddConfigProperties` called directly, once through `AddEnvCompatibility`, which builds a merged configuration and re-binds), which is redundant but harmless. The host is never built in `Mobile/Program.cs`; the builder chain is created without `Build()`. See [Platform Notes](../wiki/platform-notes.md).

## Wiring summary

| Registration | Definition | Lifetime | Gate |
|---|---|---|---|
| `BrowserHost` | `AddWebAutomation` | singleton | `SpicyTofu:Platform` = `Web` |
| `IWebDriver` / `WebDriver` | `AddWebAutomation` | scoped | `SpicyTofu:Platform` = `Web` |
| Config option bindings | platform extension | - | always |

## Related pages

- [Configuration](./configuration.md)
- [Automation Driver Contract](./automation-driver-contract.md)
- [Web Automation](./web-automation.md)
- [Domain Layer](./domain-layer.md)