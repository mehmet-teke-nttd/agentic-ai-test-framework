# Architecture

## Release slices

- **P0 — foundation:** .NET 8 solution, central packages, NUnit/ReqnRoll/Playwright sample, and CI.
- **P1 — contracts and discovery:** dependency-free v1 records/interfaces plus predictable Gherkin metadata parsing.
- **P2 — controlled execution:** fixed project allowlist, validated filters, direct process argument lists, cancellation, timeout, TRX, and run-scoped evidence.
- **P3 — agent access and analysis:** five-operation JSON-lines host, normalized result retrieval, evidence inventory, and deterministic failure classification.

## Dependency direction

`Test.Agent.Contracts` has no project or package references. `TestFramework.Core` implements discovery and validation against contracts. `TestFramework.Playwright` depends on Core and Playwright. `Test.AgentGateway` depends on contracts and Core, while the CLI is the composition root. Test projects depend inward on only the components they exercise.

The contracts intentionally contain no Playwright, ReqnRoll, NUnit, process, Azure DevOps, or agent-protocol types. `ITestDiscovery`, `ITestGateway`, `IEvidenceStore`, and `IFailureAnalyzer` are seams for alternate adapters, including a future LLM analyzer.

## Execution flow

1. Validate contract version, project ID, timeout, test IDs, and categories.
2. Resolve an administrator-configured project path and verify it remains under repository root.
3. Create `Evidence/<runId>/`, write context, and start structured logging.
4. launch `dotnet test` directly with `ProcessStartInfo.ArgumentList`; no shell is involved.
5. Apply timeout/cancellation, capture both output streams, and request a TRX in the run directory.
6. Normalize complete, partial, missing, or corrupt TRX into the v1 result contract.
7. Persist result and manifest for later read/analyze operations.

## Failure analysis

The P3 analyzer uses ordered, documented signatures for environment, assertion/product, test/locator, and infrastructure failures. Confidence is conservative. No recognized signature yields `Unknown` with zero confidence. Evidence paths are citations, not hidden model context.

## P5 — Development workflow automation (Cursor SubAgents)

Cursor SubAgents integrate with the development workflow for pre-commit quality checks, codebase exploration, and CI debugging. SubAgents complement Test.AgentGateway:

- **Test.AgentGateway** (runtime) — execute tests, analyze failures, track history
- **Cursor SubAgents** (development) — explore code, review changes, enforce patterns

Available SubAgents: `explore` (code discovery), `bugbot` (quality review), `security-review` (security audit), `ci-investigator` (pipeline debugging), `generalPurpose` (multi-step tasks).

Automated workflows via `.cursor/hooks/hooks.json` provide pre-commit POM reviews, test coverage exploration, and framework compliance verification.

See [SubAgent Integration Guide](subagent-integration-guide.md) and [SubAgent Workflows](subagent-workflows.md) for complete documentation.

## Later phases

Future work can add an MCP adapter, Azure DevOps read adapters, policy-based project registration, richer attachment correlation, and SubAgent orchestration for multi-agent workflows. Repository mutation and work-item mutation remain outside this release.
