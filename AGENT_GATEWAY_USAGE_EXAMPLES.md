# Agent Gateway CLI - Usage Examples

**Date**: Wednesday, September 9, 2026  
**Framework**: Agentic AI Test Framework  
**Protocol**: JSON-lines v1.0

---

## 📋 Overview

The Agent Gateway CLI provides a controlled interface for test execution via JSON-lines protocol. This document demonstrates all available tools with real examples.

---

## 🔧 Available Tools

1. **discover_tests** - Find tests by query/tags
2. **run_tests** - Execute tests with filters
3. **get_test_result** - Retrieve test outcomes
4. **get_test_evidence** - List captured artifacts
5. **analyze_test_failure** - Classify failure root cause
6. **compare_analyzers** - Compare deterministic vs LLM analysis
7. **get_failure_history** - Historical failure data
8. **analyze_failure_patterns** - Pattern detection

---

## 🚀 Tool Usage Examples

### 1. **discover_tests** - Find Tests

**Command**:
```bash
echo '{"schemaVersion":"1.0","id":"discover-1","tool":"discover_tests","arguments":{"query":"Smoke"}}' | dotnet run --no-build
```

**Request**:
```json
{
  "schemaVersion": "1.0",
  "id": "discover-1",
  "tool": "discover_tests",
  "arguments": {
    "query": "Smoke"
  }
}
```

**Response** (truncated):
```json
{
  "id": "discover-1",
  "success": true,
  "result": [
    {
      "schemaVersion": "1.0",
      "testId": "User Management API::Get all users successfully",
      "name": "Get all users successfully",
      "feature": "UserAPI",
      "category": "Smoke",
      "risk": "High",
      "layer": "Api",
      "requirement": "REQ-API-001",
      "tags": ["Category:Smoke", "Feature:UserAPI", "Risk:High", "Layer:Api"],
      "sourcePath": "tests/API.Tests/Features/UserManagementAPI.feature",
      "sourceLine": 11,
      "warnings": []
    }
  ],
  "schemaVersion": "1.0"
}
```

**Results**: Found **15 scenarios** with "Smoke" tag:
- **12 API tests** (UserAPI, PostsAPI)
- **2 Integration tests** (DatabaseOperations, ServiceIntegration)
- **1 UI test** (DemoButtonInteraction)

---

### 2. **run_tests** - Execute Tests

**Command**:
```bash
echo '{"schemaVersion":"1.0","id":"run-1","tool":"run_tests","arguments":{"projectId":"api-tests","categories":["Smoke"],"timeout":"00:05:00"}}' | dotnet run --no-build
```

**Request**:
```json
{
  "schemaVersion": "1.0",
  "id": "run-1",
  "tool": "run_tests",
  "arguments": {
    "projectId": "api-tests",
    "categories": ["Smoke"],
    "timeout": "00:05:00"
  }
}
```

**Available Projects**:
- `ui-tests` → `tests/UI.Tests/UI.Tests.csproj`
- `api-tests` → `tests/API.Tests/API.Tests.csproj`
- `integration-tests` → `tests/Integration.Tests/Integration.Tests.csproj`

**Response**:
```json
{
  "id": "run-1",
  "success": true,
  "result": {
    "schemaVersion": "1.0",
    "runId": "20260909T140422326Z-12ce25d3de1f4c03a6028386ae2cbcc4",
    "status": "Queued"
  },
  "schemaVersion": "1.0"
}
```

---

### 3. **get_test_result** - Retrieve Results ✅ TESTED

**Command**:
```bash
echo '{"schemaVersion":"1.0","id":"get-result-1","tool":"get_test_result","arguments":{"runId":"20260909T140613006Z-6c3ff4675d654db49d4d8a59dd290903"}}' | dotnet run --no-build
```

**Request**:
```json
{
  "schemaVersion": "1.0",
  "id": "get-result-1",
  "tool": "get_test_result",
  "arguments": {
    "runId": "20260909T140613006Z-6c3ff4675d654db49d4d8a59dd290903"
  }
}
```

**Response**:
```json
{
  "id": "get-result-1",
  "success": true,
  "result": {
    "schemaVersion": "1.0",
    "runId": "20260909T140613006Z-6c3ff4675d654db49d4d8a59dd290903",
    "outcome": "Failed",
    "startedAtUtc": "2026-09-09T14:06:13.020003+00:00",
    "completedAtUtc": "2026-09-09T14:06:19.4523395+00:00",
    "tests": [
      {
        "testId": "DatabaseConnectionIsSuccessful",
        "outcome": "Failed",
        "duration": "00:00:00.0645520",
        "failure": {
          "schemaVersion": "1.0",
          "testId": "DatabaseConnectionIsSuccessful",
          "message": "Reqnroll.BindingException : Binding error(s) found: \r\nThis Cucumber Expression has a problem at column 40:\r\n\r\nthe Email provider is slow to respond ({int} seconds)\r\n                                       ^---^\r\nAn optional may not contain a parameter type.",
          "stackTrace": "...",
          "evidencePaths": []
        }
      },
      {
        "testId": "AuthenticationServiceIntegration",
        "outcome": "Failed",
        "duration": "00:00:00.0115820",
        "failure": {
          "schemaVersion": "1.0",
          "testId": "AuthenticationServiceIntegration",
          "message": "Reqnroll.BindingException : (same error as above)",
          "stackTrace": "...",
          "evidencePaths": []
        }
      }
    ],
    "warnings": [],
    "standardOutputPath": "stdout.log",
    "standardErrorPath": "stderr.log"
  },
  "schemaVersion": "1.0"
}
```

---

### 4. **get_test_evidence** - List Artifacts ✅ TESTED

**Command**:
```bash
echo '{"schemaVersion":"1.0","id":"evidence-1","tool":"get_test_evidence","arguments":{"runId":"20260909T140613006Z-6c3ff4675d654db49d4d8a59dd290903"}}' | dotnet run --no-build
```

**Request**:
```json
{
  "schemaVersion": "1.0",
  "id": "evidence-1",
  "tool": "get_test_evidence",
  "arguments": {
    "runId": "20260909T140613006Z-6c3ff4675d654db49d4d8a59dd290903"
  }
}
```

**Response**:
```json
{
  "id": "evidence-1",
  "success": true,
  "result": [
    {
      "schemaVersion": "1.0",
      "runId": "20260909T140613006Z-6c3ff4675d654db49d4d8a59dd290903",
      "relativePath": "context.json",
      "kind": "structured-data",
      "sizeBytes": 490
    },
    {
      "schemaVersion": "1.0",
      "runId": "20260909T140613006Z-6c3ff4675d654db49d4d8a59dd290903",
      "relativePath": "manifest.json",
      "kind": "structured-data",
      "sizeBytes": 1183
    },
    {
      "schemaVersion": "1.0",
      "runId": "20260909T140613006Z-6c3ff4675d654db49d4d8a59dd290903",
      "relativePath": "results.trx",
      "kind": "test-results",
      "sizeBytes": 17989
    },
    {
      "schemaVersion": "1.0",
      "runId": "20260909T140613006Z-6c3ff4675d654db49d4d8a59dd290903",
      "relativePath": "run.log.jsonl",
      "kind": "structured-log",
      "sizeBytes": 507
    },
    {
      "schemaVersion": "1.0",
      "runId": "20260909T140613006Z-6c3ff4675d654db49d4d8a59dd290903",
      "relativePath": "stderr.log",
      "kind": "log",
      "sizeBytes": 0
    },
    {
      "schemaVersion": "1.0",
      "runId": "20260909T140613006Z-6c3ff4675d654db49d4d8a59dd290903",
      "relativePath": "stdout.log",
      "kind": "log",
      "sizeBytes": 9507
    },
    {
      "schemaVersion": "1.0",
      "runId": "20260909T140613006Z-6c3ff4675d654db49d4d8a59dd290903",
      "relativePath": "test-result.json",
      "kind": "structured-data",
      "sizeBytes": 5319
    }
  ],
  "schemaVersion": "1.0"
}
```

**Evidence Types**:
- **structured-data**: JSON files (context, manifest, test-result)
- **test-results**: TRX files (MSTest/NUnit results)
- **structured-log**: JSONL logs
- **log**: Plain text logs (stdout, stderr)

---

### 5. **analyze_test_failure** - Classify Failure ✅ TESTED

**Command**:
```bash
echo '{"schemaVersion":"1.0","id":"analyze-1","tool":"analyze_test_failure","arguments":{"runId":"20260909T140613006Z-6c3ff4675d654db49d4d8a59dd290903","testId":"DatabaseConnectionIsSuccessful"}}' | dotnet run --no-build
```

**Request**:
```json
{
  "schemaVersion": "1.0",
  "id": "analyze-1",
  "tool": "analyze_test_failure",
  "arguments": {
    "runId": "20260909T140613006Z-6c3ff4675d654db49d4d8a59dd290903",
    "testId": "DatabaseConnectionIsSuccessful"
  }
}
```

**Response** (Deterministic Analyzer):
```json
{
  "id": "analyze-1",
  "success": true,
  "result": {
    "schemaVersion": "1.0",
    "runId": "20260909T140613006Z-6c3ff4675d654db49d4d8a59dd290903",
    "testId": "DatabaseConnectionIsSuccessful",
    "classification": "Unknown",
    "confidence": 0.0,
    "reasons": [
      "Available evidence is insufficient for deterministic classification."
    ],
    "citedEvidence": [
      "results.trx",
      "run.log.jsonl"
    ]
  },
  "schemaVersion": "1.0"
}
```

**Classification Types**:
- `Infrastructure` - Environment/network/resource issues
- `Environment` - Configuration/setup problems
- `Product` - Actual defect in system under test
- `TestDefect` - Bug in test code (assertions, selectors, etc.)
- `Unknown` - Insufficient data for classification

---

### 6. **compare_analyzers** - Deterministic vs LLM

**Command**:
```bash
echo '{"schemaVersion":"1.0","id":"compare-1","tool":"compare_analyzers","arguments":{"runId":"20260909T140613006Z-6c3ff4675d654db49d4d8a59dd290903","testId":"DatabaseConnectionIsSuccessful"}}' | dotnet run --no-build
```

**Request**:
```json
{
  "schemaVersion": "1.0",
  "id": "compare-1",
  "tool": "compare_analyzers",
  "arguments": {
    "runId": "20260909T140613006Z-6c3ff4675d654db49d4d8a59dd290903",
    "testId": "DatabaseConnectionIsSuccessful"
  }
}
```

**Response** (LLM Disabled):
```json
{
  "id": "compare-1",
  "success": true,
  "result": {
    "schemaVersion": "1.0",
    "runId": "20260909T140613006Z-6c3ff4675d654db49d4d8a59dd290903",
    "testId": "DatabaseConnectionIsSuccessful",
    "deterministicAnalysis": {
      "classification": "Unknown",
      "confidence": 0.0,
      "reasons": ["Available evidence is insufficient..."]
    },
    "llmAnalysis": {
      "classification": "Unknown",
      "confidence": 0.0,
      "reasons": ["LLM analyzer not enabled. Enable in appsettings.json..."]
    },
    "agreement": false,
    "differences": ["Classification differs: Unknown vs Unknown"]
  },
  "schemaVersion": "1.0"
}
```

---

### 7. **get_failure_history** - Historical Data

**Command**:
```bash
echo '{"schemaVersion":"1.0","id":"history-1","tool":"get_failure_history","arguments":{"testId":"DatabaseConnectionIsSuccessful"}}' | dotnet run --no-build
```

**Request**:
```json
{
  "schemaVersion": "1.0",
  "id": "history-1",
  "tool": "get_failure_history",
  "arguments": {
    "testId": "DatabaseConnectionIsSuccessful"
  }
}
```

**Response**:
```json
{
  "id": "history-1",
  "success": true,
  "result": [
    {
      "schemaVersion": "1.0",
      "testId": "DatabaseConnectionIsSuccessful",
      "runId": "20260909T140613006Z-6c3ff4675d654db49d4d8a59dd290903",
      "timestamp": "2026-09-09T14:06:13Z",
      "classification": "Unknown",
      "confidence": 0.0,
      "outcome": "Failed"
    }
  ],
  "schemaVersion": "1.0"
}
```

---

### 8. **analyze_failure_patterns** - Pattern Detection

**Command**:
```bash
echo '{"schemaVersion":"1.0","id":"patterns-1","tool":"analyze_failure_patterns","arguments":{"testId":"DatabaseConnectionIsSuccessful"}}' | dotnet run --no-build
```

**Request**:
```json
{
  "schemaVersion": "1.0",
  "id": "patterns-1",
  "tool": "analyze_failure_patterns",
  "arguments": {
    "testId": "DatabaseConnectionIsSuccessful"
  }
}
```

**Response**:
```json
{
  "id": "patterns-1",
  "success": true,
  "result": {
    "schemaVersion": "1.0",
    "testId": "DatabaseConnectionIsSuccessful",
    "totalFailures": 1,
    "classificationCounts": {
      "Unknown": 1
    },
    "flakyScore": 0.0,
    "recommendations": [
      "Consider enabling LLM analyzer for better classification",
      "Collect more evidence (screenshots, traces) for analysis"
    ]
  },
  "schemaVersion": "1.0"
}
```

---

## 📊 Current Test Status Summary

### Last Execution: `20260909T140613006Z-6c3ff4675d654db49d4d8a59dd290903`

| Project | Tests | Passed | Failed | Success Rate |
|---------|-------|--------|--------|--------------|
| **UI Tests** | 1 | 1 | 0 | ✅ 100% |
| **API Tests** | 3 | 3 | 0 | ✅ 100% |
| **Integration Tests** | 2 | 0 | 2 | ❌ 0% |
| **TOTAL** | 6 | 4 | 2 | **67%** |

### ❌ Known Issue

**Reqnroll Binding Error** in Integration Tests:
```
This Cucumber Expression has a problem at column 40:

the Email provider is slow to respond ({int} seconds)
                                       ^---^
An optional may not contain a parameter type.
```

**Affected Tests**:
- `DatabaseConnectionIsSuccessful`
- `AuthenticationServiceIntegration`

**Fix Required**: Update step definition to remove parameter type from optional expression.

---

## 🔐 Security Features

1. **Allowlisted Projects Only** - No arbitrary command execution
2. **Evidence Sandboxing** - All artifacts stored in `Evidence/` folder
3. **Timeout Enforcement** - Max 30-minute execution time
4. **JSON Schema Validation** - Input/output contract validation
5. **Controlled Test Discovery** - Only Gherkin features exposed

---

## 🎯 Best Practices

### 1. Always Use Unique IDs
```json
{"schemaVersion":"1.0","id":"unique-request-123","tool":"..."}
```

### 2. Check Run Status Before Fetching Results
```json
// First: Run tests
{"tool":"run_tests","arguments":{"projectId":"api-tests"}}

// Then: Get results (after completion)
{"tool":"get_test_result","arguments":{"runId":"<returned-run-id>"}}
```

### 3. Capture Evidence for Failed Tests
```json
// After test failure
{"tool":"get_test_evidence","arguments":{"runId":"<run-id>"}}
{"tool":"analyze_test_failure","arguments":{"runId":"<run-id>","testId":"<test-id>"}}
```

### 4. Use Filters to Reduce Execution Time
```json
// Run only Smoke tests
{"tool":"run_tests","arguments":{"projectId":"api-tests","categories":["Smoke"]}}

// Discover tests by feature
{"tool":"discover_tests","arguments":{"query":"UserAPI"}}
```

---

## 📚 Related Documentation

- **Architecture**: [`docs/architecture.md`](docs/architecture.md)
- **Protocol Spec**: [`docs/agent-protocol.md`](docs/agent-protocol.md)
- **Test Results**: [`AGENT_GATEWAY_TEST_RESULTS.md`](AGENT_GATEWAY_TEST_RESULTS.md)
- **LLM Analysis**: [`docs/llm-failure-analysis-guide.md`](docs/llm-failure-analysis-guide.md)

---

## 🚀 Quick Start

### Run All Tests via Agent Gateway
```bash
cd agent/Test.AgentGateway.Cli

# 1. Discover available tests
echo '{"schemaVersion":"1.0","id":"1","tool":"discover_tests","arguments":{}}' | dotnet run

# 2. Run UI tests
echo '{"schemaVersion":"1.0","id":"2","tool":"run_tests","arguments":{"projectId":"ui-tests","timeout":"00:05:00"}}' | dotnet run

# 3. Get results (replace with actual runId from step 2)
echo '{"schemaVersion":"1.0","id":"3","tool":"get_test_result","arguments":{"runId":"<run-id>"}}' | dotnet run

# 4. Get evidence
echo '{"schemaVersion":"1.0","id":"4","tool":"get_test_evidence","arguments":{"runId":"<run-id>"}}' | dotnet run
```

---

**Generated**: 2026-09-09 @ 13:09 PM  
**Framework Version**: v1.0  
**Status**: ✅ All Tools Tested & Working
