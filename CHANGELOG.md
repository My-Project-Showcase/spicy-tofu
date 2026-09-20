# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - Unreleased

### Added

- Automation driver contract in `Application.Automation`: the `IAutomationDriver` base contract and the web-specific `IWebDriver`, `IWebPage`, `IWebSession`, and `WebContextOptions` interfaces.
- Web automation implementation in `Infrastructure.Automation.Web`: `BrowserHost` (lazy Playwright startup guarded by a semaphore), `WebDriver`, `BrowserSession`, and `WebPage`.
  - Named sessions with a `default` session and a registry backed by a concurrent dictionary.
  - Default and navigation timeouts sourced from the bound `TestExecution` and `PlaywrightConfig` options.
  - Platform-gated registration: `AddWebAutomation` registers `BrowserHost` (singleton) and `IWebDriver` (scoped) only when `SpicyTofu:Platform` is `Web`.
- Configuration keys and option classes: `TestExecution:DefaultTimeoutMs` and `Playwright:NavigationTimeoutMs`, bound through `IOptions`.
- Documentation: `docs/technical/` and `docs/wiki/` pages, the `docs/README.md` table of contents, and the wiki index and log.