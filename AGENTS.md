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

## Project Wiki

Alongside the code, this repo keeps an LLM-maintained wiki: a set of interlinked markdown files that captures design decisions, component library notes, and platform knowledge. The agent writes and maintains it. Humans read it and steer it.

The wiki is documentation, not a source of truth for behavior. If the wiki and the code disagree, the code wins, and the wiki page gets fixed.

### Layers

1. **Raw sources** (`docs/raw/`): design notes, ADRs, component library docs, Playwright and Appium release notes, bug write-ups. Immutable. The agent reads them and never edits them.
2. **Wiki** (`docs/wiki/`): agent-owned pages such as architecture, core interfaces, per-platform notes, per-component-library locator notes, and known issues. The agent creates and updates these.
3. **Schema**: this file. It defines the conventions and workflows below.

Wiki work never touches source code, and code tasks never edit `docs/raw/`.

### Special files

- `docs/wiki/index.md`: a catalog of every page with a link and a one-line summary, grouped by category (architecture, platforms, component libraries, decisions, sources). Update it on every ingest. Read it first when answering a question.
- `docs/wiki/log.md`: append-only history. Start each entry with `## [YYYY-MM-DD] operation | Title` (for example `## [2026-09-19] ingest | Appium 3 release notes`) so it can be scanned with `grep "^## \[" docs/wiki/log.md | tail -5`.

### Operations

**Ingest**: when a new source lands in `docs/raw/`:
1. Read the source and summarize the key takeaways for the user.
2. Add a summary page in the wiki.
3. Update affected pages (interfaces, platform notes, locator notes) and flag anywhere the new source contradicts an existing claim.
4. Update `index.md` and append to `log.md`.

Ingest one source at a time unless told otherwise.

**Query**: read `index.md`, open the relevant pages, and answer with references to the pages used. If the answer is worth keeping (a comparison, an analysis, a decision), offer to file it as a new wiki page.

**Lint**: when asked, check for contradictions, stale claims, orphan pages, concepts mentioned without their own page, and missing cross-references. Report findings before making changes.

### Wiki rules

- MUST NOT edit anything under `docs/raw/`.
- MUST keep `index.md` and `log.md` current with every wiki change.
- MUST use relative links between wiki pages.
- MUST NOT record claims about framework behavior that the code or tests don't support. Mark unverified claims as such.
- Add YAML frontmatter to each wiki page (`title`, `updated`, `sources`) so pages can be queried later.

## Commands

```bash
dotnet build spicy-tofu.sln
dotnet test spicy-tofu.sln   # no test projects exist yet; runs as a no-op
dotnet format spicy-tofu.sln --verify-no-changes
# web-only tests: none yet (no test projects exist)
# mobile-only tests: none yet (no test projects; Mobile is a placeholder console app)
```

Build, tests, and the format check must all pass before a task counts as done.

## Code style

Style is defined by `.editorconfig` and `Directory.Build.props` at the repo root, and both apply to every project.

- MUST NOT edit `.editorconfig` or `Directory.Build.props` to make a violation go away. Fix the code instead.
- MUST NOT run `dotnet format` without `--verify-no-changes` unless asked.
- MUST NOT suppress analyzer warnings with `#pragma` or `[SuppressMessage]` without a comment explaining why.

## Conventions

- Solution and project layout: solution `spicy-tofu.sln` at the repo root; five projects in same-named top-level folders. `Domain`, `Application`, and `Infrastructure` form the core; `Web` and `Mobile` are the platform executables. `Web` references `Application`, `Domain`, and `Infrastructure`; `Mobile` references nothing yet. All projects target `net9.0` with `Nullable` and `ImplicitUsings` enabled.
- Namespace and naming rules: namespaces mirror the project and folder path (`Domain.Runtime.Environment.Configuration`, `Web.Extensions`); use file-scoped namespaces and mark classes `sealed` unless they must be extended. Style and naming are enforced by `.editorconfig` and `Directory.Build.props` (enforced in build): 4-space indentation, Allman braces, `_camelCase` private/internal fields, `s_` static-field prefix, PascalCase constants, C# keywords over BCL types, usings outside the namespace, UTF-8 files.
- Test data and config location: configuration in `Web/appsettings.json` (copied to output), overlaid by `TOFU_`-prefixed environment variables, bound to the `SpicyTofu`, `Playwright`, `TestExecution`, and `Projects` option classes in `Domain.Runtime.Environment.Configuration`. `Projects:RootDirectory` defaults to `./projects` for test-definition data (folder not created yet). No test projects exist yet; when added, record their location here.
- Reporting output location: not implemented yet; when reporting is added, record the output location here.
- MUST keep this AGENTS.md up to date whenever commands, conventions, configuration, project structure, or other agent-relevant guidance change. New projects, test projects, reporting, or config locations must be recorded here.