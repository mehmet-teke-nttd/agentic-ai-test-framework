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

## Tool surface

- `discover_tests`: arguments `{ "query"?: string }`; searches IDs and tags.
- `run_tests`: arguments are `TestRunRequest`. `projectId` must be one of: `ui-tests`, `api-tests`, or `integration-tests`; optional `testIds`, `categories`, and a JSON `timeout` value use the v1 contract.
- `get_test_result`: arguments `{ "runId": string }`.
- `get_test_evidence`: arguments `{ "runId": string }`; returns metadata, never arbitrary file contents.
- `analyze_test_failure`: arguments `{ "runId": string, "testId": string }`; uses configured analyzer (Deterministic or LLM).
- `compare_analyzers`: arguments `{ "runId": string, "testId": string }`; runs both Deterministic and LLM analyzers for comparison. ⭐ NEW
- `get_failure_history`: arguments `{ "testId": string }`; retrieves historical failure records. ⭐ NEW
- `analyze_failure_patterns`: arguments `{ "testId": string }`; analyzes patterns and detects flaky tests. ⭐ NEW

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
