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

## Configuration

Settings live in `appsettings.json`, split by owner:

- Core sections (`SpicyTofu`, `Projects`, `TestExecution`) are platform-neutral. `SpicyTofu` and `TestExecution` are bound by both platforms; `Projects` is bound by the web project only.
- `Playwright` is read only by the web project. `Appium` is read only by the mobile project.

- MUST NOT add platform-specific keys to core sections. Put them in that platform's section.
- MUST NOT read another platform's section from a platform project.
- MUST NOT branch on `SpicyTofu:Platform` outside the composition root and the resolver.
- MUST NOT commit secrets. Use user secrets or environment variables.

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
5. Write the documentation and update the changelog as described in the Documentation and Changelog sections.

**Modifying existing code**
1. Read the core interface and both platform implementations before changing any of them.
2. Keep both implementations consistent with the interface contract.
3. Update every documentation page the change makes stale, and update the changelog where the Changelog section requires it.

## Documentation

Documentation lives in `docs/`. The agent writes and maintains it. Humans read it and steer it.

Documentation is not a source of truth for behavior. If the docs and the code disagree, the code wins, and the page gets fixed.

### Layout

- `docs/technical/`: how the code works. Interfaces and contracts, classes and their responsibilities per layer, dependency injection wiring, configuration keys and how they bind, and how to build, run, or set up things (for example the Playwright browser install step).
- `docs/wiki/`: the why and the wider knowledge. Design decisions and their reasoning, component library locator notes, per-platform notes (Playwright, Appium), and known issues.
- `docs/README.md`: a brief overview of what the two folders contain and what they are used for, plus a table of contents of every page in them. Agents use it to find context on an implementation without opening every file.
- `docs/wiki/index.md` and `docs/wiki/log.md`: see Wiki special files.

### Reading docs

When you need context on an implementation, read the table of contents in `docs/README.md` first, then open only the pages relevant to the area you are working on.

### When docs must be written

Any task that adds or changes behavior, structure, configuration, or an interface MUST update the documentation in the same task. A task is not complete until its documentation is written.

Changes with no effect on behavior, structure, configuration, or an interface (typo fixes, spelling corrections, formatting) do not need documenting.

### Choosing the folder

- Explains how something works or is wired: `docs/technical/`.
- Explains why it was built that way, or records knowledge about a platform, component library, or issue: `docs/wiki/`.
- The task involves both: write both.
- Unsure which folder fits: stop and ask, and list the options.

Before creating a page, check the table of contents in `docs/README.md`. Update the existing page for that topic instead of creating a duplicate.

### README

`docs/README.md` MUST be updated every time a page in `docs/technical/` or `docs/wiki/` is created, edited, renamed, or deleted. No exceptions.

It contains:

1. An overview of what `docs/technical/` and `docs/wiki/` contain and what they are used for.
2. A table of contents grouped by folder. Each entry has a relative link, a one-line summary, and the page's `updated` date.

### Page rules

- MUST add YAML frontmatter to every page in `docs/technical/` and `docs/wiki/` with `title`, `updated` (`YYYY-MM-DD`), and `sources` (the source files or pages the content is based on).
- MUST use relative links between pages.
- MUST name page files in lowercase kebab-case (for example `browser-host.md`). If a page already exists, keep its name.
- MUST NOT record claims about framework behavior that the code or tests don't support. Mark unverified claims as such.
- MUST NOT document planned or unmerged work as if it exists.
- MUST NOT use em dashes in any documentation file, README, changelog entry, or code comment. Use commas, colons, periods, or parentheses instead.

### Wiki special files

- `docs/wiki/index.md`: a catalog of every wiki page with a link and a one-line summary, grouped by category (architecture, platforms, component libraries, decisions, known issues). Update it on every wiki change.
- `docs/wiki/log.md`: append-only history of wiki changes. Start each entry with `## [YYYY-MM-DD] operation | Title` so it can be scanned with `grep "^## \[" docs/wiki/log.md | tail -5`.

### Wiki operations

**Query**: read `docs/README.md`, open the relevant pages, and answer with references to the pages used. If the answer is worth keeping (a comparison, an analysis, a decision), offer to file it as a new page.

**Lint**: when asked, check `docs/technical/` and `docs/wiki/` for contradictions, stale claims, orphan pages (pages missing from the README table of contents), concepts mentioned without their own page, and missing cross-references. Report findings before making changes.

Query and lint tasks never touch source code.

### When unsure

Do not guess and do not add content that was not asked for. If a documentation decision is not covered here, stop and ask, and list the possible options.

## Changelog

`CHANGELOG.md` at the repo root records notable changes. It follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) and [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

### Structure

The file opens with:

```markdown
# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
```

Versions follow, newest first. A version that has not been released has no date and no compare link:

```markdown
## [X.Y.Z] - Unreleased

### Added

- Description of a big implementation
  - Detail line when needed

### Changed

- Description of a small adjustment

### Fixed

- Description of a small fix
```

A released version has its compare link and release date:

```markdown
## [X.Y.Z](https://github.com/My-Project-Showcase/spicy-tofu/compare/vPREVIOUS...vX.Y.Z) - YYYY-MM-DD
```

### Rules

- MUST update `CHANGELOG.md` only after a task is fully complete (code, build, tests, format check, and documentation). No entries for work in progress.
- The version number lives in `Directory.Build.props`. Only the user changes it. MUST NOT edit or bump it. Every entry goes under the heading for that version.
- If there is no heading for that version, create it as `## [X.Y.Z] - Unreleased` above the previous version. If the heading exists and has a release date, stop and ask. If the version cannot be read from `Directory.Build.props`, stop and ask.
- Use only these sections: `Added`, `Changed`, `Fixed`. Include a section only when it has entries. There is no Contributors section. If a change does not fit these sections, stop and ask.
- Big implementations (a new capability, for example the browser host) go under `Added`.
- Small adjustments and fixes (for example updating parameters in a method for a specific implementation) go under `Changed` (modified behavior) or `Fixed` (corrected defects).
- Changes with no effect on behavior, structure, configuration, or an interface (typo fixes, spelling corrections, formatting) get no entry.
- Only when the user says a version is released: replace `Unreleased` with the release date and add the compare link. If there is no previous version to compare against, stop and ask which link to use.
- MUST NOT use em dashes in changelog entries.

## Commands

```bash
dotnet build spicy-tofu.sln
dotnet test spicy-tofu.sln   # no test projects exist yet; runs as a no-op
dotnet format spicy-tofu.sln --verify-no-changes
# web-only tests: none yet (no test projects exist)
# mobile-only tests: none yet (no test projects; Mobile is a placeholder console app)
```

Build, tests, and the format check must all pass before a task counts as done. The documentation, `docs/README.md`, and `CHANGELOG.md` must also be updated as the Documentation and Changelog sections require.

## Code style

Style is defined by `.editorconfig` and `Directory.Build.props` at the repo root, and both apply to every project.

- MUST NOT edit `.editorconfig` or `Directory.Build.props` to make a violation go away. Fix the code instead.
- MUST NOT run `dotnet format` without `--verify-no-changes` unless asked.
- MUST NOT suppress analyzer warnings with `#pragma` or `[SuppressMessage]` without a comment explaining why.

## Conventions

- Solution and project layout: solution `spicy-tofu.sln` at the repo root; five projects in same-named top-level folders. `Domain`, `Application`, and `Infrastructure` form the core; `Web` and `Mobile` are the platform executables. Both `Web` and `Mobile` reference `Application`, `Domain`, and `Infrastructure`. All projects target `net9.0` with `Nullable` and `ImplicitUsings` enabled.
- Namespace and naming rules: namespaces mirror the project and folder path (`Domain.Runtime.Environment.Configuration`, `Web.Extensions`); use file-scoped namespaces and mark classes `sealed` unless they must be extended. Style and naming are enforced by `.editorconfig` and `Directory.Build.props` (enforced in build): 4-space indentation, Allman braces, `_camelCase` private/internal fields, `s_` static-field prefix, PascalCase constants, C# keywords over BCL types, usings outside the namespace, UTF-8 files.
- Configuration location: configuration in `Web/appsettings.json` and `Mobile/appsettings.json` (copied to output), overlaid by `TOFU_`-prefixed environment variables, bound to the `SpicyTofu`, `Playwright`, `Appium`, `TestExecution`, and `Projects` option classes in `Domain.Runtime.Environment.Configuration`. `Logging` is a core section too, bound by both platforms in `AddServices`; the section is absent from the JSON files, so the `LoggingConfig` defaults apply unless overlaid. `Projects:RootDirectory` is set in `Web/appsettings.json` and points at the test-definition data folder; the mobile project keeps the default `./projects` value and never reads it. No test projects exist yet; when added, record their location here.
- Runtime pipeline: `JsonService` loads the test JSON and raises the `IJsonService.TestsLoaded` application event with the loaded `List<Test>`; `TestsLoadedHandler.Flatten` converts that list into `IEnumerable<TestExecutionStep>`; `RunService.RunAsync` subscribes to the event, converts through the handler, and executes the steps, reporting each step through `ILogger`. The web entry point drives it via `RunAsync`; the mobile entry point does not start a run yet. See `docs/technical/runtime-pipeline.md`.
- Logging: `ILogger` and `IPrintStrategy` are core interfaces in `Application.Logging`; `Logger` (level filtering, timestamps) and `ConsolePrintStrategy` (all presentation, plain text only) live in `Infrastructure.Logging`. `ILogger` exposes execution concepts (`ActionStarted`, `LocatorResolution`, `ActionCompleted`, `ActionFailed`) that take `TestExecutionStep` directly, plus `Debug`, `Info`, `Warning`, `Error`, and `Section`; locator data arrives as `Application.Locators.LocatorCandidate`. Table formatting is private to `ConsolePrintStrategy`, which renders `Interactive` and `Ci` output through `OutputModeDetector`. The logger writes `Console` only through `ConsolePrintStrategy`; consumers take `ILogger` and never touch `Console` or the strategy directly. See `docs/technical/logging.md`.
- Documentation location: `docs/technical/`, `docs/wiki/`, and `docs/README.md` (see Documentation).
- Changelog location: `CHANGELOG.md` at the repo root. The current version is the version number in `Directory.Build.props` (see Changelog).
- Reporting output location: not implemented yet; when reporting is added, record the output location here.
- MUST keep this AGENTS.md up to date whenever commands, conventions, configuration, project structure, or other agent-relevant guidance change. New projects, test projects, reporting, or config locations must be recorded here.