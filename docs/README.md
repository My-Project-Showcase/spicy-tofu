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
| [domain-layer.md](./technical/domain-layer.md) | Domain models, the aggregate root base, locator attributes, and the configuration option classes. | 2026-09-20 |
| [configuration.md](./technical/configuration.md) | Configuration sections, which platform binds what, environment variable overlay, and binding gaps. | 2026-09-20 |
| [dependency-injection.md](./technical/dependency-injection.md) | Composition roots, DI extensions, and how `AddWebAutomation` gates the web registrations. | 2026-09-20 |
| [automation-driver-contract.md](./technical/automation-driver-contract.md) | The core automation interfaces: `IAutomationDriver`, `IWebDriver`, `IWebPage`, `IWebSession`, `WebContextOptions`. | 2026-09-20 |
| [web-automation.md](./technical/web-automation.md) | The Playwright implementation: `BrowserHost`, `WebDriver`, `BrowserSession`, `WebPage`, and timeout wiring. | 2026-09-20 |
| [setup-and-commands.md](./technical/setup-and-commands.md) | Prerequisites, build/test/format commands, the Playwright browser install step, and configuration sources. | 2026-09-20 |

### wiki/

| Page | Summary | Updated |
|---|---|---|
| [index.md](./wiki/index.md) | Catalog of every wiki page grouped by category. | 2026-09-20 |
| [log.md](./wiki/log.md) | Append-only history of wiki changes. | 2026-09-20 |
| [design-decisions.md](./wiki/design-decisions.md) | Why the framework is shaped the way it is: the `IWebPage` seam, named sessions, the browser singleton, and platform gating. | 2026-09-20 |
| [platform-notes.md](./wiki/platform-notes.md) | Per-platform knowledge for Playwright (web) and Appium (mobile). | 2026-09-20 |
| [known-issues-and-discrepancies.md](./wiki/known-issues-and-discrepancies.md) | Stale AGENTS.md claims, empty or unused code, hosts that never start, config binding gaps, and naming oddities. | 2026-09-20 |

## Conventions

- Every page in `docs/technical/` and `docs/wiki/` has YAML frontmatter with `title`, `updated` (`YYYY-MM-DD`), and `sources`.
- Links between pages are relative.
- Page filenames are lowercase kebab-case.
- No em dashes in any documentation file.