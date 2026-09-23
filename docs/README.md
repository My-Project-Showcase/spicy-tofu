# Documentation

This folder holds the project documentation, split into two areas.

- `docs/technical/`: how the code works. Interfaces and contracts, classes and their responsibilities per layer, dependency injection wiring, configuration keys and how they bind, and how to build, run, or set up things.
- `docs/wiki/`: the why and the wider knowledge. Design decisions and their reasoning, per-platform notes, and known issues.

These pages are maintained by the agent and steered by humans. If a page and the code disagree, the code wins and the page gets fixed.

## Table of Contents

### technical/

| Page | Summary | Updated |
|---|---|---|
| [architecture-overview.md](./technical/architecture-overview.md) | Solution layout, the three core layers and two executables, project references, dependency direction, and package references. | 2026-09-20 |
| [domain-layer.md](./technical/domain-layer.md) | Domain models, the aggregate root base, locator attributes, the execution step record, and the configuration option classes. | 2026-09-23 |
| [configuration.md](./technical/configuration.md) | Configuration sections, which platform binds what, environment variable overlay, and binding gaps. | 2026-09-23 |
| [dependency-injection.md](./technical/dependency-injection.md) | Composition roots, DI extensions, runtime services, the single `AddAutomation` platform-selection point, driver registration, and host disposal. | 2026-09-23 |
| [runtime-pipeline.md](./technical/runtime-pipeline.md) | How test case JSON becomes execution-ready steps and how the driver is started and stopped around the load: the `TestsLoaded` event, `TestsLoadedHandler`, `TestExecutionStep`, and the `RunService` subscription. | 2026-09-23 |
| [logging.md](./technical/logging.md) | The execution-oriented `ILogger` facade, `LogEntry`/`LogKind`, `LocatorCandidate`, the `IPrintStrategy` seam, and CI/Interactive rendering. | 2026-09-23 |
| [automation-driver-contract.md](./technical/automation-driver-contract.md) | The core automation interfaces: `IAutomationDriver` (lifecycle base), `IWebDriver`, `IWebPage`, `IWebSession`, `WebContextOptions`, `IMobileDriver`, `IMobileSession`, `MobileContextOptions`, and platform wiring. | 2026-09-23 |
| [web-automation.md](./technical/web-automation.md) | The Playwright implementation: `BrowserHost`, `WebDriver`, `BrowserSession`, `WebPage`, and timeout wiring. | 2026-09-23 |
| [mobile-automation.md](./technical/mobile-automation.md) | The Appium implementation: `MobileHost`, `AppiumServerLauncher`, the Android and iOS device launchers, `MobileDriver`, and `MobileSession`. | 2026-09-23 |
| [setup-and-commands.md](./technical/setup-and-commands.md) | Prerequisites, build/test/format commands, the Playwright browser install step, Appium and mobile device setup, and configuration sources. | 2026-09-20 |

### wiki/

| Page | Summary | Updated |
|---|---|---|
| [index.md](./wiki/index.md) | Catalog of every wiki page grouped by category. | 2026-09-20 |
| [log.md](./wiki/log.md) | Append-only history of wiki changes. | 2026-09-20 |
| [design-decisions.md](./wiki/design-decisions.md) | Why the framework is shaped the way it is: the `IWebPage` seam, named sessions, the browser singleton, the single `AddAutomation` composition point, driver lifecycle ownership, implicit mobile environment startup with reuse semantics, and the `AppiumConfig` naming. | 2026-09-23 |
| [platform-notes.md](./wiki/platform-notes.md) | Per-platform knowledge for Playwright (web) and Appium (mobile). | 2026-09-23 |
| [known-issues-and-discrepancies.md](./wiki/known-issues-and-discrepancies.md) | Stale AGENTS.md claims, empty or unused code, driver startup notes, config binding gaps, and naming oddities. | 2026-09-23 |

## Conventions

- Every page in `docs/technical/` and `docs/wiki/` has YAML frontmatter with `title`, `updated` (`YYYY-MM-DD`), and `sources`.
- Links between pages are relative.
- Page filenames are lowercase kebab-case.
- No em dashes in any documentation file.