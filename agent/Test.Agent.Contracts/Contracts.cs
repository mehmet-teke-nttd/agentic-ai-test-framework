namespace Test.Agent.Contracts;

public static class ContractVersions
{
    public const string V1 = "1.0";
}

public enum TestOutcome { Passed, Failed, Skipped, Unknown }
public enum FailureClassification { ProductDefect, TestDefect, Environment, Infrastructure, Unknown }
public enum TestLayer { Ui, Api, Integration, Unit, Unknown }
public enum RiskLevel { Low, Medium, High, Critical, Unknown }

public sealed record TestRunRequest
{
    public string SchemaVersion { get; init; } = ContractVersions.V1;
    public required string ProjectId { get; init; }
    public IReadOnlyList<string> TestIds { get; init; } = [];
    public IReadOnlyList<string> Categories { get; init; } = [];
    public TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(10);
}

public sealed record TestExecutionContext
{
    public string SchemaVersion { get; init; } = ContractVersions.V1;
    public required string RunId { get; init; }
    public required string ProjectId { get; init; }
    public required string RepositoryRoot { get; init; }
    public required string EvidenceDirectory { get; init; }
    public DateTimeOffset StartedAtUtc { get; init; }
    public string? SourceRevision { get; init; }
}

public sealed record TestFailure
{
    public string SchemaVersion { get; init; } = ContractVersions.V1;
    public required string TestId { get; init; }
    public required string Message { get; init; }
    public string? StackTrace { get; init; }
    public IReadOnlyList<string> EvidencePaths { get; init; } = [];
}

public sealed record TestCaseResult
{
    public required string TestId { get; init; }
    public TestOutcome Outcome { get; init; }
    public TimeSpan Duration { get; init; }
    public TestFailure? Failure { get; init; }
}

public sealed record TestRunResult
{
    public string SchemaVersion { get; init; } = ContractVersions.V1;
    public required string RunId { get; init; }
    public TestOutcome Outcome { get; init; }
    public DateTimeOffset StartedAtUtc { get; init; }
    public DateTimeOffset CompletedAtUtc { get; init; }
    public IReadOnlyList<TestCaseResult> Tests { get; init; } = [];
    public IReadOnlyList<string> Warnings { get; init; } = [];
    public string? StandardOutputPath { get; init; }
    public string? StandardErrorPath { get; init; }
}

public sealed record TestMetadata
{
    public string SchemaVersion { get; init; } = ContractVersions.V1;
    public required string TestId { get; init; }
    public required string Name { get; init; }
    public required string Feature { get; init; }
    public string? Category { get; init; }
    public RiskLevel Risk { get; init; } = RiskLevel.Unknown;
    public TestLayer Layer { get; init; } = TestLayer.Unknown;
    public string? Requirement { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
    public required string SourcePath { get; init; }
    public int SourceLine { get; init; }
    public IReadOnlyList<string> Warnings { get; init; } = [];
}

public sealed record TestEvidence
{
    public string SchemaVersion { get; init; } = ContractVersions.V1;
    public required string RunId { get; init; }
    public required string RelativePath { get; init; }
    public required string Kind { get; init; }
    public long SizeBytes { get; init; }
    public string? TestId { get; init; }
}

public sealed record FailureAnalysis
{
    public string SchemaVersion { get; init; } = ContractVersions.V1;
    public required string RunId { get; init; }
    public required string TestId { get; init; }
    public FailureClassification Classification { get; init; }
    public double Confidence { get; init; }
    public IReadOnlyList<string> Reasons { get; init; } = [];
    public IReadOnlyList<string> CitedEvidence { get; init; } = [];
}

public interface ITestDiscovery
{
    Task<IReadOnlyList<TestMetadata>> DiscoverAsync(CancellationToken cancellationToken = default);
}

public interface ITestGateway
{
    Task<TestRunResult> RunAsync(TestRunRequest request, CancellationToken cancellationToken = default);
    Task<TestRunResult?> GetResultAsync(string runId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TestEvidence>> GetEvidenceAsync(string runId, CancellationToken cancellationToken = default);
}

public interface IEvidenceStore
{
    Task<IReadOnlyList<TestEvidence>> ListAsync(string runId, CancellationToken cancellationToken = default);
}

public interface IFailureAnalyzer
{
    Task<FailureAnalysis> AnalyzeAsync(
        TestRunResult result,
        string testId,
        IReadOnlyList<TestEvidence> evidence,
        CancellationToken cancellationToken = default);
}
