# AGENTS.md

This file governs how agents work in Spicy-Tofu, a cross-platform test automation framework for web (Playwright) and mobile (Appium), written in C#/.NET and built on Domain Driven Design with a shared core.

## Architecture

The solution has three tiers:

1. **Core**: architecture, data ingestion, test case models, reporting, and the interfaces every platform implements.
2. **Web project**: Playwright implementation of the core interfaces.
3. **Mobile project**: Appium implementation of the core interfaces.

Web and mobile are separate assemblies. They never reference each other, and the core never references either of them.

The core follows four DDD layers:

- **Domain**: the framework's own domain (TestCase, TestStep, Action, Assertion, and so on). It does not model the business domain of the app under test.
- **Application**: use cases and orchestration.
- **Infrastructure**: data ingestion, reporting, and other outward-facing concerns.
- **Presentation**: the entry layer that starts a run.

Dependencies point inward. Domain depends on nothing.

## Platform wiring

Platform implementations are discovered and wired by the reflection-based resolver. To add platform behavior, implement a core interface in the platform assembly and let the resolver find it.

## Constraints

- MUST put anything identical on both platforms in the core.
- MUST keep Playwright and Appium APIs inside their own platform assemblies.
- MUST NOT add platform checks (`if (isMobile)`, platform enums, and similar) to core code. If behavior differs, add an interface to the core and implement it per platform.
- MUST NOT reference Playwright or Appium types from Domain or Application.
- MUST NOT bypass the resolver by instantiating platform classes directly from core code.
- MUST NOT add a package reference from the core to a platform assembly, directly or transitively.

## Component library locators

Each component library has its own locator class holding that library's locators as one group. Classes are resolved through the locator registry. Do not add a shared catch-all locator builder, and do not split a library's class further by component type.

## Workflows

**Adding a capability**
1. Decide whether it is platform-neutral. If so, model it in the core.
2. Define the interface in the core first.
3. Implement it in the web assembly, the mobile assembly, or both.
4. Confirm the resolver picks up the new implementation.

**Modifying existing code**
1. Read the core interface and both platform implementations before changing any of them.
2. Keep both implementations consistent with the interface contract.

## Commands

```bash
dotnet build Spicy-Tofu.sln
dotnet test Spicy-Tofu.sln
# [add: run web tests only]
# [add: run mobile tests only, plus emulator/device requirements]
# [add: format/analyzer command, if any]
```

Build and run the tests before treating a task as done.

## Conventions

- Solution and project layout: [fill in]
- Namespace and naming rules: [fill in]
- Test data and config location: [fill in]
- Reporting output location: [fill in]