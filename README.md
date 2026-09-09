# Agentic AI Test Framework

First usable release (P0-P3) + **LLM-powered failure analysis (P4)** of a .NET 8 test framework for controlled, agent-assisted testing. It combines NUnit, ReqnRoll, and Playwright behind a stable gateway. The release grants **READ, RUN, ANALYZE, and RECOMMEND** capabilities, now enhanced with AI-powered root cause analysis.

## 🎯 Key Features

- ✅ **Complete Test Pyramid**: UI, API, and Integration test layers
- 🧠 **LLM-Powered Analysis**: AI-driven failure root cause identification
- 📊 **Historical Tracking**: Automatic pattern detection and flaky test identification
- 🔄 **Dual Analyzers**: Deterministic + LLM with comparative analysis
- 🎯 **Actionable Insights**: Fix recommendations, not just classifications
- 🌐 **Multi-Provider**: OpenAI, Azure OpenAI, Anthropic support
- 🤖 **SubAgent Integration**: Cursor SubAgents for code review, exploration, and CI debugging 🚀 NEW!

## Projects

- `agent/Test.Agent.Contracts` — dependency-free, transport-independent v1 contracts and interfaces.
- `src/TestFramework.Core` — `.feature` discovery and safe NUnit filter construction.
- `src/TestFramework.Playwright` — browser session and failure evidence capture.
- `agent/Test.AgentGateway` — allowlisted execution, TRX normalization, evidence store, and deterministic analysis.
- `agent/Test.AgentGateway.Cli` — narrow JSON-lines stdio agent tool host.
- `tests/UI.Tests` — ReqnRoll + Playwright sample using a configurable application URL (Layer: UI).
- `tests/API.Tests` — ReqnRoll + HttpClient for REST API testing (Layer: API).
- `tests/Integration.Tests` — Database and service integration tests (Layer: Integration).
- `tests/TestFramework.UnitTests` — discovery, validation, normalization, evidence, and analysis tests.

See [architecture](docs/architecture.md), [security model](docs/security.md), [agent protocol](docs/agent-protocol.md), and [test layers guide](docs/test-layers-guide.md).

## 🎨 Page Object Model Pattern

**This framework strictly follows the Page Object Model (POM) pattern for UI tests.**

- ✅ All UI interactions encapsulated in Page Objects (`tests/UI.Tests/PageObjects/`)
- ✅ Step definitions contain NO direct Playwright calls
- ✅ Selectors centralized and maintainable
- ✅ Clear separation between test logic and UI interaction

**For AI Assistants:**
- **[AGENTS.md](.cursor/rules/AGENTS.md)** - Mandatory guidelines for AI code generation
- **[RULE.md](.cursor/rules/RULE.md)** - Page Object Model implementation rules
- **[REFACTORING_COMPLETE.md](REFACTORING_COMPLETE.md)** - Real refactoring example

## 📚 Documentation

### Core Framework
- **[Architecture](docs/architecture.md)** - System design and dependency flow
- **[Test Layers Guide](docs/test-layers-guide.md)** - Complete guide to UI/API/Integration testing
- **[Agent Protocol](docs/agent-protocol.md)** - Tool reference and JSON-lines protocol
- **[Security Model](docs/security.md)** - Trust boundaries and controls

### AI-Powered Features 🧠
- **[LLM Failure Analysis Guide](docs/llm-failure-analysis-guide.md)** - AI-powered failure analysis
- **[LLM Analysis Demo](docs/LLM_ANALYSIS_DEMO.md)** - Live examples and workflows
- **[LLM Implementation](docs/IMPLEMENTATION_SUMMARY_LLM.md)** - AI analysis details

### Development Workflow 🚀 NEW!
- **[SubAgent Integration Guide](docs/subagent-integration-guide.md)** - Cursor SubAgent integration
- **[SubAgent Workflows](docs/subagent-workflows.md)** - Practical workflow examples
- **[SubAgent Quick Reference](docs/SUBAGENT_QUICKREF.md)** - Fast command lookup

### Implementation Details
- **[API/Integration Implementation](docs/IMPLEMENTATION_SUMMARY.md)** - Test pyramid details

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

## API Test Configuration

Set `Api.BaseUrl` in `tests/API.Tests/appsettings.json` or use the `TEST_API_BASE_URL` environment variable:

```powershell
$env:TEST_API_BASE_URL = "https://api.example.com"
dotnet test tests/API.Tests/API.Tests.csproj --no-build
```

The default configuration points to `https://jsonplaceholder.typicode.com` for demonstration purposes.

## Integration Test Configuration

Configure database connection and service URLs in `tests/Integration.Tests/appsettings.json`:

```json
{
  "Database": {
    "ConnectionString": "Server=localhost;Database=TestDB;..."
  },
  "Services": {
    "AuthServiceUrl": "http://localhost:5001",
    "OrderServiceUrl": "http://localhost:5002"
  }
}
```

Or use environment variables:
- `TEST_DB_CONNECTION_STRING` for database connection
- Service URLs are read from configuration

## Running Tests by Layer

Run tests by specific layer using the `@Layer` tag:

```powershell
# UI tests only
dotnet test --filter "TestCategory=Ui"

# API tests only
dotnet test --filter "TestCategory=Api"

# Integration tests only
dotnet test --filter "TestCategory=Integration"

# All smoke tests across all layers
dotnet test --filter "TestCategory=Smoke"

# High risk tests only
dotnet test --filter "FullyQualifiedName~Risk:High"
```

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

Send one JSON object per line. The available tools are `discover_tests`, `run_tests`, `get_test_result`, `get_test_evidence`, `analyze_test_failure`, `compare_analyzers`, `get_failure_history`, and `analyze_failure_patterns`. `run_tests` accepts the allowlist IDs: `ui-tests`, `api-tests`, or `integration-tests`; it never accepts a command or project path.

### LLM-Powered Failure Analysis 🧠 NEW!

Enable AI-powered intelligent failure analysis:

1. **Set API Key:**
```powershell
$env:LLM_API_KEY = "your-openai-key"
```

2. **Enable in Configuration:**
Edit `agent/Test.AgentGateway.Cli/appsettings.json`:
```json
{
  "LlmAnalyzer": {
    "Enabled": true,
    "Provider": "OpenAI",
    "Model": "gpt-4o-mini"
  }
}
```

3. **Analyze Failures:**
```json
{"tool":"analyze_test_failure","arguments":{"runId":"<runId>","testId":"<testId>"}}
{"tool":"compare_analyzers","arguments":{"runId":"<runId>","testId":"<testId>"}}
{"tool":"analyze_failure_patterns","arguments":{"testId":"<testId>"}}
```

**Features:**
- 🎯 Deep root cause analysis with AI
- 💡 Actionable fix recommendations
- 📈 Historical pattern detection
- 🔍 Flaky test identification
- 🔄 Comparative analysis (Deterministic vs LLM)

See **[LLM Failure Analysis Guide](docs/llm-failure-analysis-guide.md)** for complete documentation.

### Example Usage

```json
{"schemaVersion":"1.0","id":"1","tool":"discover_tests","arguments":{}}
{"schemaVersion":"1.0","id":"2","tool":"run_tests","arguments":{"projectId":"api-tests","categories":["Smoke"],"timeout":"00:05:00"}}
{"schemaVersion":"1.0","id":"3","tool":"analyze_test_failure","arguments":{"runId":"<runId>","testId":"<testId>"}}
{"schemaVersion":"1.0","id":"4","tool":"compare_analyzers","arguments":{"runId":"<runId>","testId":"<testId>"}}
```

An MCP package is intentionally not required in P3. The JSON-lines host has the same narrow, versioned request/response semantics without coupling contracts to a changing SDK. It can be adapted to MCP later without changing the domain interfaces.
