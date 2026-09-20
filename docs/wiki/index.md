# Wiki

This is the index of every page in the wiki. It is grouped by category. The wiki records the why and the wider knowledge: design decisions and their reasoning, per-platform notes, and known issues.

## Architecture

- [Design Decisions](./design-decisions.md): why `IWebPage` keeps Playwright out of the core, why sessions are named, why the browser host is a semaphore-guarded singleton, how platform-gated registration works, why mobile starts the Appium server and device implicitly with a reuse-and-own lifecycle, and why the mobile config class is `AppiumConfig`.

## Platforms

- [Platform Notes](./platform-notes.md): per-platform knowledge for Playwright (web) and Appium (mobile), including browser install, config binding, server and device launchers, and reuse semantics.

## Known Issues

- [Known Issues and Discrepancies](./known-issues-and-discrepancies.md): stale AGENTS.md claims, empty or unused code, hosts that never start, configuration binding gaps, naming oddities, and the format check result.

## Related

- [docs/README.md](../README.md): the documentation overview and full table of contents.