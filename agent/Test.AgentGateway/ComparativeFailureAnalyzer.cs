using Test.Agent.Contracts;

namespace Test.AgentGateway;

public sealed class ComparativeFailureAnalyzer : IFailureAnalyzer
{
    private readonly IFailureAnalyzer _primaryAnalyzer;
    private readonly IFailureAnalyzer _secondaryAnalyzer;

    public ComparativeFailureAnalyzer(
        IFailureAnalyzer primaryAnalyzer,
        IFailureAnalyzer secondaryAnalyzer)
    {
        _primaryAnalyzer = primaryAnalyzer;
        _secondaryAnalyzer = secondaryAnalyzer;
    }

    public async Task<FailureAnalysis> AnalyzeAsync(
        TestRunResult result,
        string testId,
        IReadOnlyList<TestEvidence> evidence,
        CancellationToken cancellationToken = default)
    {
        // Run both analyzers in parallel
        var primaryTask = _primaryAnalyzer.AnalyzeAsync(result, testId, evidence, cancellationToken);
        var secondaryTask = _secondaryAnalyzer.AnalyzeAsync(result, testId, evidence, cancellationToken);

        await Task.WhenAll(primaryTask, secondaryTask);

        var primary = await primaryTask;
        var secondary = await secondaryTask;

        // Return the analysis with higher confidence
        // If confidence is similar, prefer the primary analyzer
        if (Math.Abs(primary.Confidence - secondary.Confidence) < 0.1)
        {
            return primary;
        }

        return primary.Confidence > secondary.Confidence ? primary : secondary;
    }
}

public sealed record ComparativeAnalysisResult
{
    public string SchemaVersion { get; init; } = ContractVersions.V1;
    public required string RunId { get; init; }
    public required string TestId { get; init; }
    public required FailureAnalysis DeterministicAnalysis { get; init; }
    public required FailureAnalysis LlmAnalysis { get; init; }
    public required AnalysisComparison Comparison { get; init; }
}

public sealed record AnalysisComparison
{
    public bool ClassificationMatches { get; init; }
    public double ConfidenceDifference { get; init; }
    public IReadOnlyList<string> CommonReasons { get; init; } = [];
    public IReadOnlyList<string> OnlyInDeterministic { get; init; } = [];
    public IReadOnlyList<string> OnlyInLlm { get; init; } = [];
    public string Recommendation { get; init; } = "";
}

public static class FailureAnalysisComparator
{
    public static ComparativeAnalysisResult Compare(
        FailureAnalysis deterministic,
        FailureAnalysis llm,
        string runId,
        string testId)
    {
        var classificationMatches = deterministic.Classification == llm.Classification;
        var confidenceDiff = Math.Abs(deterministic.Confidence - llm.Confidence);

        var deterministicReasons = new HashSet<string>(
            deterministic.Reasons,
            StringComparer.OrdinalIgnoreCase);
        var llmReasons = new HashSet<string>(
            llm.Reasons,
            StringComparer.OrdinalIgnoreCase);

        var commonReasons = deterministicReasons
            .Intersect(llmReasons, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var onlyInDeterministic = deterministicReasons
            .Except(llmReasons, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var onlyInLlm = llmReasons
            .Except(deterministicReasons, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var recommendation = GenerateRecommendation(
            classificationMatches,
            confidenceDiff,
            deterministic,
            llm);

        return new ComparativeAnalysisResult
        {
            RunId = runId,
            TestId = testId,
            DeterministicAnalysis = deterministic,
            LlmAnalysis = llm,
            Comparison = new AnalysisComparison
            {
                ClassificationMatches = classificationMatches,
                ConfidenceDifference = confidenceDiff,
                CommonReasons = commonReasons,
                OnlyInDeterministic = onlyInDeterministic,
                OnlyInLlm = onlyInLlm,
                Recommendation = recommendation
            }
        };
    }

    private static string GenerateRecommendation(
        bool classificationMatches,
        double confidenceDiff,
        FailureAnalysis deterministic,
        FailureAnalysis llm)
    {
        if (classificationMatches && confidenceDiff < 0.2)
        {
            return "Both analyzers agree with similar confidence. High certainty in classification.";
        }

        if (classificationMatches && confidenceDiff >= 0.2)
        {
            var higher = deterministic.Confidence > llm.Confidence ? "Deterministic" : "LLM";
            return $"Both agree on classification, but {higher} analyzer has significantly higher confidence. Consider reviewing the evidence.";
        }

        if (!classificationMatches && deterministic.Confidence > 0.7)
        {
            return "Analyzers disagree. Deterministic analyzer has high confidence based on known patterns. Review LLM analysis for additional insights.";
        }

        if (!classificationMatches && llm.Confidence > 0.7)
        {
            return "Analyzers disagree. LLM identified context that deterministic patterns missed. Review LLM recommendations carefully.";
        }

        return "Analyzers disagree with moderate confidence. Manual review recommended to determine root cause.";
    }
}
