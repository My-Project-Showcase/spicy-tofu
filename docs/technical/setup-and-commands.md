---
title: Setup and Commands
updated: 2026-09-20
sources:
  - ../../AGENTS.md
  - ../../Directory.Build.Props
  - ../../spicy-tofu.sln
  - ../../Domain/Domain.csproj
  - ../../Application/Application.csproj
  - ../../Infrastructure/Infrastructure.csproj
  - ../../Web/Web.csproj
  - ../../Mobile/Mobile.csproj
  - ../../Web/appsettings.json
  - ../../Mobile/appsettings.json
---

# Setup and Commands

## Prerequisites

- .NET SDK 9.0 (`net9.0` target).
- For the web platform, Playwright browsers. The Playwright package places a `playwright.ps1` helper in the web project's output directory; it exists at `Web/bin/Debug/net9.0/playwright.ps1` after a build.

## Playwright browser install

```powershell
.\Web\bin\Debug\net9.0\playwright.ps1 install chromium
```

The helper script is produced by the `Microsoft.Playwright` NuGet package (1.62.0) and lives in the build output next to the web executable. This step is manual; no build target installs browsers automatically.

## Build

```bash
dotnet build spicy-tofu.sln
```

The solution contains five projects: `Domain`, `Application`, `Infrastructure`, `Web`, `Mobile`. All target `net9.0`.

## Test

```bash
dotnet test spicy-tofu.sln
```

No test projects exist yet, so this runs as a no-op.

## Format check

```bash
dotnet format spicy-tofu.sln --verify-no-changes
```

Style is defined by `.editorconfig` and `Directory.Build.props` and enforced in build (`EnforceCodeStyleInBuild`), with analyzers at `latest-recommended`. `TreatWarningsAsErrors` applies to Release builds only. The format check currently reports violations; see [Known Issues and Discrepancies](../wiki/known-issues-and-discrepancies.md).

## Configuration sources

- `appsettings.json` in `Web/` and `Mobile/` (both copied to output with `CopyToOutputDirectory=PreserveNewest`).
- `TOFU_`-prefixed environment variables overlay the JSON.
- `Directory.Build.props` sets analysis level, code style enforcement, and Release-only warnings-as-errors. It contains no version property.

## Configuration binding by platform

| Section | Bound by web | Bound by mobile |
|---|---|---|
| `SpicyTofu` | yes | yes |
| `TestExecution` | yes | yes |
| `Projects` | no | no |
| `Playwright` | yes | no |
| `Appium` | no | yes |

See [Configuration](./configuration.md).

## Related pages

- [Configuration](./configuration.md)
- [Architecture Overview](./architecture-overview.md)
- [Known Issues and Discrepancies](../wiki/known-issues-and-discrepancies.md)