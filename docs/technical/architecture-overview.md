---
title: Architecture Overview
updated: 2026-09-20
sources:
  - ../../spicy-tofu.sln
  - ../../Domain/Domain.csproj
  - ../../Application/Application.csproj
  - ../../Infrastructure/Infrastructure.csproj
  - ../../Web/Web.csproj
  - ../../Mobile/Mobile.csproj
---

# Architecture Overview

Spicy-Tofu is a cross-platform test automation framework for web (Playwright) and mobile (Appium), written in C# and .NET. The solution is built on Domain Driven Design with a shared core.

## Solution layout

The solution `spicy-tofu.sln` contains five projects, each in a same-named top-level folder.

| Project | Type | References | Role |
|---|---|---|---|
| `Domain` | class library | none | Framework domain models and configuration option classes |
| `Application` | class library | `Domain` | Automation contracts and orchestration seams |
| `Infrastructure` | class library | `Application`, `Domain` | Outward-facing concerns and the Playwright and Appium implementations of the platform contracts |
| `Web` | executable | `Application`, `Domain`, `Infrastructure` | Web composition root |
| `Mobile` | executable | `Application`, `Domain`, `Infrastructure` | Mobile composition root and entry point |

All projects target `net9.0` with `Nullable` and `ImplicitUsings` enabled.

## Layers

Per AGENTS.md the core follows four DDD layers, of which three currently exist as projects:

- **Domain**: the framework's own domain. Test case models, locator attributes, and option classes that mirror `appsettings.json` sections.
- **Application**: use cases and orchestration. Holds the automation driver contracts that platform implementations must satisfy.
- **Infrastructure**: outward-facing concerns. Holds the Playwright host, session, page, and driver implementation, the Appium host, server launcher, device launchers, driver, and session, plus DI extensions.
- **Presentation**: the entry layer that starts a run. In this repo the composition roots are the `Web` and `Mobile` executables.

Dependencies point inward. `Domain` depends on nothing.

## The three tiers

The architecture describes three tiers:

1. **Core**: `Domain`, `Application`, `Infrastructure`.
2. **Web platform**: the web executable plus the Playwright implementation.
3. **Mobile platform**: the mobile executable plus the Appium implementation.

Note that the Playwright implementation lives in `Infrastructure.Automation.Web` and the Appium implementation in `Infrastructure.Automation.Mobile`, not in the `Web` and `Mobile` projects. The `Web` and `Mobile` projects are the composition roots that wire their implementations. See [Design Decisions](../wiki/design-decisions.md) for why platform code sits in Infrastructure.

## Package references

- `Domain`: `Ardalis.SmartEnum` 8.2.0.
- `Application`: `Microsoft.Extensions.DependencyInjection.Abstractions` 10.0.12, `Microsoft.Extensions.Options` 10.0.12.
- `Infrastructure`: `Microsoft.Extensions.Configuration`, `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Hosting`, `Microsoft.Extensions.Options.ConfigurationExtensions` (all 10.0.12), `Microsoft.Playwright` 1.62.0, and `Appium.WebDriver` 8.3.2.
- `Web`: `Microsoft.Extensions.Configuration`, `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Hosting`, `Microsoft.Extensions.Options.ConfigurationExtensions` (all 10.0.12).
- `Mobile`: `Microsoft.Extensions.Configuration`, `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Hosting`, `Microsoft.Extensions.Options.ConfigurationExtensions` (all 10.0.12).

## Application of Playwright and Appium

`Microsoft.Playwright` and `Appium.WebDriver` are referenced by the `Infrastructure` project only. No other assembly references their types. The `Application` contracts expose platform-neutral abstractions (`IWebPage`, `IWebSession`, `IMobileSession`) instead of driver types, which keeps Playwright and Appium out of the core-facing contract.

## Related pages

- [Configuration](./configuration.md)
- [Dependency Injection](./dependency-injection.md)
- [Automation Driver Contract](./automation-driver-contract.md)
- [Web Automation](./web-automation.md)
- [Mobile Automation](./mobile-automation.md)
- [Domain Layer](./domain-layer.md)