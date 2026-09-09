# Agent JSON-lines protocol v1

Each stdin line is one request; each stdout line is one response. `schemaVersion` must be `"1.0"`.

```json
{"schemaVersion":"1.0","id":"1","tool":"discover_tests","arguments":{"query":"Smoke"}}
```

Successful response:

```json
{"schemaVersion":"1.0","id":"1","success":true,"result":[]}
```

Error response:

```json
{"schemaVersion":"1.0","id":"1","success":false,"error":{"code":"invalid_request","message":"..."}}
```

Unknown tools, missing required arguments, invalid `runId` format, and unsupported schema versions all return `invalid_request`. Structured errors do not include stack traces.

## Tool surface

The host exposes eight tools. There is no command execution, dynamic tool loading, path argument, or filesystem-read tool.

Typical workflow: `discover_tests` → `run_tests` → `get_test_result` / `get_test_evidence` → `analyze_test_failure` → optional `compare_analyzers`, `get_failure_history`, `analyze_failure_patterns`.

### `discover_tests`

Finds Gherkin scenarios by scanning `*.feature` files under the repository (skipping `bin`, `obj`, and `.git`). Each scenario becomes `TestMetadata` with `testId` `{FeatureName}::{ScenarioName}`.

**Arguments**

| Field | Required | Description |
|---|---|---|
| `query` | no | Case-insensitive substring match against `testId` and tags. Omit or leave empty to return every discovered scenario. |

**Request**

```json
{"schemaVersion":"1.0","id":"1","tool":"discover_tests","arguments":{"query":"Smoke"}}
```

**Result:** array of `TestMetadata`.

```json
[
  {
    "schemaVersion": "1.0",
    "testId": "Demo Button Interaction::Clicking a button updates the page",
    "name": "Clicking a button updates the page",
    "feature": "DemoButtonInteraction",
    "category": "Smoke",
    "risk": "Medium",
    "layer": "Ui",
    "requirement": null,
    "tags": ["Category:Smoke", "Feature:DemoButtonInteraction", "Risk:Medium", "Layer:Ui"],
    "sourcePath": "tests/UI.Tests/Features/Demo/DemoButtonInteraction.feature",
    "sourceLine": 8,
    "warnings": []
  }
]
```

Missing `Category`, `Risk`, `Layer`, or `Requirement` tags produce discovery warnings; they do not fail the call. Unknown `Risk` or `Layer` values normalize to `Unknown`.

---

### `run_tests`

Runs an allowlisted test project via `dotnet test` (no shell). The host maps `projectId` to a project path under the repository root; callers never pass a path or command.

**Arguments** (`TestRunRequest`)

| Field | Required | Description |
|---|---|---|
| `projectId` | yes | One of `ui-tests`, `api-tests`, `integration-tests`. |
| `testIds` | no | Fully qualified NUnit names. Combined with `OR`. Empty means all tests in the project. |
| `categories` | no | NUnit `TestCategory` values (from Gherkin tags such as `Smoke`). Combined with `OR`. |
| `timeout` | no | JSON duration (for example `"00:05:00"`). Default `00:10:00`. Must be greater than zero and at most `00:30:00`. |
| `schemaVersion` | no | Must be `"1.0"` if sent. Default `"1.0"`. |

If both `testIds` and `categories` are set, they are combined with `AND`. Filter values must be 1–200 characters and match `[\w .:/()\-]`.

The gateway generates `runId` as `{utcTimestamp}-{guid}` and writes evidence under `Evidence/<runId>/`. Environment variables `TEST_RUN_ID` and `TEST_EVIDENCE_DIR` are set on the test process.

**Request**

```json
{"schemaVersion":"1.0","id":"2","tool":"run_tests","arguments":{"projectId":"ui-tests","categories":["Smoke"],"timeout":"00:05:00"}}
```

**Result:** `TestRunResult`.

```json
{
  "schemaVersion": "1.0",
  "runId": "20260909T133000123Z-abc123def456",
  "outcome": "Failed",
  "startedAtUtc": "2026-09-09T13:30:00.123Z",
  "completedAtUtc": "2026-09-09T13:30:12.456Z",
  "tests": [
    {
      "testId": "DemoButtonInteraction.ClickingAButtonUpdatesThePage",
      "outcome": "Failed",
      "duration": "00:00:05.123",
      "failure": {
        "testId": "DemoButtonInteraction.ClickingAButtonUpdatesThePage",
        "message": "Expected text to contain 'Done'",
        "stackTrace": "at DemoButtonInteractionSteps.ThenThePageShowsConfirmation..."
      }
    }
  ],
  "warnings": [],
  "standardOutputPath": "stdout.log",
  "standardErrorPath": "stderr.log"
}
```

`outcome` is `Passed`, `Failed`, `Skipped`, or `Unknown` (including timeout). Keep `runId` for later result, evidence, and analysis calls.

---

### `get_test_result`

Reads the normalized result for a previous run from `Evidence/<runId>/test-result.json`. Does not re-run tests.

**Arguments**

| Field | Required | Description |
|---|---|---|
| `runId` | yes | Gateway-generated id. Must match `^[a-zA-Z0-9-]{1,80}$`. |

**Request**

```json
{"schemaVersion":"1.0","id":"3","tool":"get_test_result","arguments":{"runId":"20260909T133000123Z-abc123def456"}}
```

**Result:** the same `TestRunResult` as `run_tests`, or `null` if that run does not exist.

---

### `get_test_evidence`

Returns an inventory of files under `Evidence/<runId>/`. Returns metadata only (`relativePath`, `kind`, `sizeBytes`) — never file contents.

**Arguments**

| Field | Required | Description |
|---|---|---|
| `runId` | yes | Gateway-generated id. Same validation as `get_test_result`. |

**Request**

```json
{"schemaVersion":"1.0","id":"4","tool":"get_test_evidence","arguments":{"runId":"20260909T133000123Z-abc123def456"}}
```

**Result:** array of `TestEvidence`.

```json
[
  { "runId": "20260909T133000123Z-abc123def456", "relativePath": "results.trx", "kind": "test-results", "sizeBytes": 4096 },
  { "runId": "20260909T133000123Z-abc123def456", "relativePath": "screenshot.png", "kind": "screenshot", "sizeBytes": 88210 },
  { "runId": "20260909T133000123Z-abc123def456", "relativePath": "trace.zip", "kind": "playwright-trace", "sizeBytes": 120448 }
]
```

`kind` is derived from extension: `.trx` → `test-results`, `.png` → `screenshot`, `.zip` → `playwright-trace`, `.json` → `structured-data`, `.jsonl` → `structured-log`, `.log` → `log`, anything else → `artifact`.

Typical run artifacts: `context.json`, `run.log.jsonl`, `stdout.log`, `stderr.log`, `results.trx`, `test-result.json`, `manifest.json`, plus UI failure screenshot, Playwright trace, and browser-console JSON when captured.

---

### `analyze_test_failure`

Classifies one failed test in a run. Uses the **configured** analyzer: LLM when `LlmAnalyzer:Enabled` is true, otherwise the deterministic signature analyzer.

On a failed run, the analysis is also appended to `Evidence/.history/<testId>.json` (up to 100 entries) for later history and pattern tools.

**Arguments**

| Field | Required | Description |
|---|---|---|
| `runId` | yes | Run that produced the failure. |
| `testId` | yes | Test id from the run result. |

Returns `invalid_request` if the run is missing.

**Request**

```json
{"schemaVersion":"1.0","id":"5","tool":"analyze_test_failure","arguments":{"runId":"20260909T133000123Z-abc123def456","testId":"DemoButtonInteraction.ClickingAButtonUpdatesThePage"}}
```

**Result:** `FailureAnalysis`.

```json
{
  "schemaVersion": "1.0",
  "runId": "20260909T133000123Z-abc123def456",
  "testId": "DemoButtonInteraction.ClickingAButtonUpdatesThePage",
  "classification": "TestDefect",
  "confidence": 0.75,
  "reasons": ["Failure matches a test-code or locator signature."],
  "citedEvidence": ["screenshot.png", "trace.zip"]
}
```

`classification` is `ProductDefect`, `TestDefect`, `Environment`, `Infrastructure`, or `Unknown`. Confidence is conservative; no matching signature yields `Unknown` with `0.0`. `citedEvidence` lists relative paths, not hidden model context.

---

### `compare_analyzers`

Runs **both** analyzers on the same failure and returns a side-by-side comparison. Always runs the deterministic analyzer. If LLM is disabled, `llmAnalysis` is `Unknown` with a reason that LLM is not enabled.

**Arguments**

| Field | Required | Description |
|---|---|---|
| `runId` | yes | Run that produced the failure. |
| `testId` | yes | Test id from the run result. |

**Request**

```json
{"schemaVersion":"1.0","id":"6","tool":"compare_analyzers","arguments":{"runId":"20260909T133000123Z-abc123def456","testId":"DemoButtonInteraction.ClickingAButtonUpdatesThePage"}}
```

**Result:** `ComparativeAnalysisResult`.

```json
{
  "schemaVersion": "1.0",
  "runId": "20260909T133000123Z-abc123def456",
  "testId": "DemoButtonInteraction.ClickingAButtonUpdatesThePage",
  "deterministicAnalysis": {
    "classification": "TestDefect",
    "confidence": 0.70,
    "reasons": ["Failure matches a test-code or locator signature."]
  },
  "llmAnalysis": {
    "classification": "TestDefect",
    "confidence": 0.85,
    "reasons": ["Selector matches multiple elements", "[Recommendation] Use a data-testid"]
  },
  "comparison": {
    "classificationMatches": true,
    "confidenceDifference": 0.15,
    "commonReasons": [],
    "onlyInDeterministic": ["Failure matches a test-code or locator signature."],
    "onlyInLlm": ["Selector matches multiple elements", "[Recommendation] Use a data-testid"],
    "recommendation": "Both agree on classification, but LLM analyzer has significantly higher confidence. Consider reviewing the evidence."
  }
}
```

Use this when you want a second opinion. It does not write failure history; call `analyze_test_failure` for that.

---

### `get_failure_history`

Returns recorded analyses for one test. History is written by `analyze_test_failure` on failed runs, stored under `Evidence/.history/`.

**Arguments**

| Field | Required | Description |
|---|---|---|
| `testId` | yes | Test id to look up. |

**Request**

```json
{"schemaVersion":"1.0","id":"7","tool":"get_failure_history","arguments":{"testId":"DemoButtonInteraction.ClickingAButtonUpdatesThePage"}}
```

**Result:** array of `HistoricalFailureEntry` (empty if none).

```json
[
  {
    "testId": "DemoButtonInteraction.ClickingAButtonUpdatesThePage",
    "runId": "20260909T133000123Z-abc123def456",
    "timestamp": "2026-09-09T13:30:20Z",
    "classification": "TestDefect",
    "confidence": 0.75,
    "reasons": ["Failure matches a test-code or locator signature."],
    "duration": "00:00:05.123",
    "outcome": "Failed"
  }
]
```

---

### `analyze_failure_patterns`

Aggregates history for one test (last 30 days, up to 20 recent entries) and reports flakiness, dominant classification, and insights.

A test is treated as flaky when there are at least three recent failures with two or more distinct classifications.

**Arguments**

| Field | Required | Description |
|---|---|---|
| `testId` | yes | Test id to analyze. |

**Request**

```json
{"schemaVersion":"1.0","id":"8","tool":"analyze_failure_patterns","arguments":{"testId":"DemoButtonInteraction.ClickingAButtonUpdatesThePage"}}
```

**Result:** `FailurePatternAnalysis`.

```json
{
  "testId": "DemoButtonInteraction.ClickingAButtonUpdatesThePage",
  "totalFailures": 15,
  "recentFailures": 8,
  "isFlaky": true,
  "commonClassification": "TestDefect",
  "classificationDistribution": { "TestDefect": 12, "Environment": 3 },
  "averageConfidence": 0.73,
  "insights": [
    "Test has failed 8 times in the last 30 days.",
    "⚠️ Test appears to be FLAKY - failure classification varies across runs.",
    "Recommendation: Investigate test stability and add proper waits/retries."
  ]
}
```

If there is no history, `totalFailures` is `0`, `isFlaky` is `false`, and `insights` explains that no data is available. Call `analyze_test_failure` on failed runs first so history exists.

## Available Test Projects

- **ui-tests**: Browser-based UI tests using Playwright (Layer: UI)
- **api-tests**: REST API tests using HttpClient (Layer: API)
- **integration-tests**: Database and service integration tests (Layer: Integration)

## LLM Failure Analysis

The gateway supports AI-powered failure analysis using Large Language Models. Configure in `appsettings.json`:

```json
{
  "LlmAnalyzer": {
    "Enabled": true,
    "Provider": "OpenAI",
    "Model": "gpt-4o-mini",
    "ApiKey": "your-api-key"
  }
}
```

Set `LLM_API_KEY` environment variable to override. See [LLM Failure Analysis Guide](llm-failure-analysis-guide.md) for details.

No fallback command execution, dynamic tool loading, path argument, or filesystem read tool exists.
