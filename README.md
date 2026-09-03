# Agentic AI Test Framework

First usable release (P0-P3) of a .NET 8 test framework for controlled, agent-assisted testing. It combines NUnit, ReqnRoll, and Playwright behind a stable gateway. The release grants only **READ, RUN, ANALYZE, and RECOMMEND** capabilities.

## Projects

- `agent/Test.Agent.Contracts` — dependency-free, transport-independent v1 contracts and interfaces.
- `src/TestFramework.Core` — `.feature` discovery and safe NUnit filter construction.
- `src/TestFramework.Playwright` — browser session and failure evidence capture.
- `agent/Test.AgentGateway` — allowlisted execution, TRX normalization, evidence store, and deterministic analysis.
- `agent/Test.AgentGateway.Cli` — narrow JSON-lines stdio agent tool host.
- `tests/UI.Tests` — ReqnRoll + Playwright sample using a configurable application URL.
- `tests/TestFramework.UnitTests` — discovery, validation, normalization, evidence, and analysis tests.

See [architecture](docs/architecture.md), [security model](docs/security.md), and [agent protocol](docs/agent-protocol.md).

## Prerequisites and verification

The SDK is pinned to .NET `8.0.424`. Any compatible .NET 8 patch can be selected by updating `global.json`.

```powershell
dotnet restore
dotnet build --no-restore
pwsh tests/UI.Tests/bin/Debug/net8.0/playwright.ps1 install chromium
dotnet test --no-build --logger "trx;LogFileName=results.trx"
dotnet format --verify-no-changes --no-restore
```

If Chromium is not installed, the sample browser scenario is explicitly skipped; unit and gateway seam tests still run.

## Application URL

Set `Application.BaseUrl` in `tests/UI.Tests/appsettings.json`. The committed default is a self-contained `data:` URL, so the sample works without a hosted application. To target another environment without changing the file, set the `TEST_APP_BASE_URL` environment variable:

```powershell
$env:TEST_APP_BASE_URL = "https://qa.example.com"
dotnet test tests/UI.Tests/UI.Tests.csproj --no-build
```

The environment variable takes precedence over `appsettings.json`. Only absolute HTTP, HTTPS, and `data:` URLs are accepted. The sample greeting assertion expects the configured page to contain `#greeting` with text `Agentic test framework`; replace the sample scenario with application-specific steps when pointing at a real system.

## Metadata convention

Metadata is declared with Gherkin tags at feature or scenario scope:

```gherkin
@Category:Smoke @Feature:Checkout @Risk:High @Layer:Ui @Requirement:REQ-42
Feature: Checkout
```

Supported keys are `Category`, `Feature`, `Risk`, `Layer`, and `Requirement`. All tags are retained. Missing keys and duplicate, empty, or unsupported values produce discovery warnings. Unknown `Risk` or `Layer` values normalize to `Unknown`; they do not crash discovery.

## Evidence

Each gateway run gets an unguessable `RunId` and writes only beneath `Evidence/<runId>/`:

- `context.json` — run, project, path, time, and source revision context
- `run.log.jsonl` — structured, RunId-correlated lifecycle records
- `stdout.log` / `stderr.log`
- `results.trx` and normalized `test-result.json`
- `manifest.json`
- failed-scenario screenshot, Playwright trace, and browser-console JSON when available

The evidence root is configurable through `GatewayOptions`.

## Agent host

Start the stdio host from the repository:

```powershell
dotnet run --project agent/Test.AgentGateway.Cli
```

Send one JSON object per line. The only available tools are `discover_tests`, `run_tests`, `get_test_result`, `get_test_evidence`, and `analyze_test_failure`. `run_tests` accepts the allowlist ID `ui-tests`; it never accepts a command or project path.

An MCP package is intentionally not required in P3. The JSON-lines host has the same narrow, versioned request/response semantics without coupling contracts to a changing SDK. It can be adapted to MCP later without changing the domain interfaces.
