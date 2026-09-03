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
- `run_tests`: arguments are `TestRunRequest`. `projectId` must be `ui-tests`; optional `testIds`, `categories`, and a JSON `timeout` value use the v1 contract.
- `get_test_result`: arguments `{ "runId": string }`.
- `get_test_evidence`: arguments `{ "runId": string }`; returns metadata, never arbitrary file contents.
- `analyze_test_failure`: arguments `{ "runId": string, "testId": string }`.

No fallback command execution, dynamic tool loading, path argument, or filesystem read tool exists.
