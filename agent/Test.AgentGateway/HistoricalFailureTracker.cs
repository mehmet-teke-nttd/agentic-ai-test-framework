using System.Text.Json;
using Test.Agent.Contracts;

namespace Test.AgentGateway;

public sealed class HistoricalFailureTracker
{
    private readonly string _historyDirectory;
    private readonly int _maxHistoryEntries;

    public HistoricalFailureTracker(string evidenceRoot, int maxHistoryEntries = 100)
    {
        _historyDirectory = Path.Combine(evidenceRoot, ".history");
        _maxHistoryEntries = maxHistoryEntries;
        Directory.CreateDirectory(_historyDirectory);
    }

    public async Task RecordFailureAsync(
        string testId,
        FailureAnalysis analysis,
        TestRunResult result,
        CancellationToken cancellationToken = default)
    {
        var entry = new HistoricalFailureEntry
        {
            TestId = testId,
            RunId = analysis.RunId,
            Timestamp = DateTimeOffset.UtcNow,
            Classification = analysis.Classification,
            Confidence = analysis.Confidence,
            Reasons = analysis.Reasons.ToList(),
            Duration = result.Tests
                .FirstOrDefault(t => t.TestId == testId)?.Duration ?? TimeSpan.Zero,
            Outcome = result.Outcome
        };

        var historyFile = GetHistoryFilePath(testId);
        var entries = await LoadHistoryAsync(testId, cancellationToken);
        
        entries.Add(entry);

        // Keep only the most recent entries
        if (entries.Count > _maxHistoryEntries)
        {
            entries = entries.OrderByDescending(e => e.Timestamp)
                .Take(_maxHistoryEntries)
                .ToList();
        }

        await File.WriteAllTextAsync(
            historyFile,
            JsonSerializer.Serialize(entries, ContractJson.Options),
            cancellationToken);
    }

    public async Task<List<HistoricalFailureEntry>> LoadHistoryAsync(
        string testId,
        CancellationToken cancellationToken = default)
    {
        var historyFile = GetHistoryFilePath(testId);
        
        if (!File.Exists(historyFile))
        {
            return [];
        }

        try
        {
            var json = await File.ReadAllTextAsync(historyFile, cancellationToken);
            return JsonSerializer.Deserialize<List<HistoricalFailureEntry>>(json, ContractJson.Options)
                ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public async Task<FailurePatternAnalysis> AnalyzePatternsAsync(
        string testId,
        CancellationToken cancellationToken = default)
    {
        var history = await LoadHistoryAsync(testId, cancellationToken);
        
        if (history.Count == 0)
        {
            return new FailurePatternAnalysis
            {
                TestId = testId,
                TotalFailures = 0,
                IsFlaky = false,
                CommonClassification = FailureClassification.Unknown,
                Insights = ["No historical data available for this test."]
            };
        }

        var recentFailures = history
            .Where(e => e.Timestamp > DateTimeOffset.UtcNow.AddDays(-30))
            .OrderByDescending(e => e.Timestamp)
            .Take(20)
            .ToList();

        var classificationCounts = recentFailures
            .GroupBy(e => e.Classification)
            .ToDictionary(g => g.Key, g => g.Count());

        var mostCommon = classificationCounts
            .OrderByDescending(kvp => kvp.Value)
            .FirstOrDefault();

        // Check for flakiness: if test alternates between pass and fail
        var isFlaky = DetectFlakiness(recentFailures);

        var insights = GenerateInsights(recentFailures, mostCommon, isFlaky);

        return new FailurePatternAnalysis
        {
            TestId = testId,
            TotalFailures = history.Count,
            RecentFailures = recentFailures.Count,
            IsFlaky = isFlaky,
            CommonClassification = mostCommon.Key,
            ClassificationDistribution = classificationCounts,
            AverageConfidence = recentFailures.Average(e => e.Confidence),
            Insights = insights
        };
    }

    private bool DetectFlakiness(List<HistoricalFailureEntry> recentFailures)
    {
        // If less than 3 failures, can't determine flakiness
        if (recentFailures.Count < 3) return false;

        // Check if classifications vary significantly
        var uniqueClassifications = recentFailures
            .Select(e => e.Classification)
            .Distinct()
            .Count();

        return uniqueClassifications >= 2;
    }

    private List<string> GenerateInsights(
        List<HistoricalFailureEntry> recentFailures,
        KeyValuePair<FailureClassification, int> mostCommon,
        bool isFlaky)
    {
        var insights = new List<string>();

        if (recentFailures.Count == 0)
        {
            insights.Add("No recent failures recorded.");
            return insights;
        }

        insights.Add($"Test has failed {recentFailures.Count} times in the last 30 days.");

        if (isFlaky)
        {
            insights.Add("⚠️ Test appears to be FLAKY - failure classification varies across runs.");
            insights.Add("Recommendation: Investigate test stability and add proper waits/retries.");
        }
        else if (mostCommon.Value >= recentFailures.Count * 0.8)
        {
            insights.Add($"✓ Consistent failure pattern detected: {mostCommon.Key} ({mostCommon.Value}/{recentFailures.Count} times).");
            insights.Add(GetRecommendationForClassification(mostCommon.Key));
        }

        var avgConfidence = recentFailures.Average(e => e.Confidence);
        if (avgConfidence < 0.5)
        {
            insights.Add($"⚠️ Low average confidence ({avgConfidence:P0}) - failures may be complex or novel.");
        }

        var recentTrend = AnalyzeTrend(recentFailures);
        if (recentTrend != null)
        {
            insights.Add(recentTrend);
        }

        return insights;
    }

    private string? AnalyzeTrend(List<HistoricalFailureEntry> recentFailures)
    {
        if (recentFailures.Count < 5) return null;

        var recent5 = recentFailures.Take(5).ToList();
        var older5 = recentFailures.Skip(5).Take(5).ToList();

        if (older5.Count == 0) return null;

        var recentAvgConfidence = recent5.Average(e => e.Confidence);
        var olderAvgConfidence = older5.Average(e => e.Confidence);

        if (recentAvgConfidence > olderAvgConfidence + 0.15)
        {
            return "📈 Trend: Analysis confidence improving over time.";
        }
        else if (recentAvgConfidence < olderAvgConfidence - 0.15)
        {
            return "📉 Trend: Analysis confidence declining - test may be degrading.";
        }

        return null;
    }

    private string GetRecommendationForClassification(FailureClassification classification)
    {
        return classification switch
        {
            FailureClassification.ProductDefect => "Recommendation: File a bug report and investigate application code.",
            FailureClassification.TestDefect => "Recommendation: Review and fix test code, selectors, or assertions.",
            FailureClassification.Environment => "Recommendation: Check environment setup, dependencies, and configuration.",
            FailureClassification.Infrastructure => "Recommendation: Investigate resource limits, timeouts, and infrastructure health.",
            _ => "Recommendation: Further investigation needed to determine root cause."
        };
    }

    private string GetHistoryFilePath(string testId)
    {
        // Sanitize test ID for filename
        var sanitized = string.Join("_", testId.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_historyDirectory, $"{sanitized}.json");
    }
}

public sealed record HistoricalFailureEntry
{
    public required string TestId { get; init; }
    public required string RunId { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public FailureClassification Classification { get; init; }
    public double Confidence { get; init; }
    public List<string> Reasons { get; init; } = [];
    public TimeSpan Duration { get; init; }
    public TestOutcome Outcome { get; init; }
}

public sealed record FailurePatternAnalysis
{
    public required string TestId { get; init; }
    public int TotalFailures { get; init; }
    public int RecentFailures { get; init; }
    public bool IsFlaky { get; init; }
    public FailureClassification CommonClassification { get; init; }
    public Dictionary<FailureClassification, int> ClassificationDistribution { get; init; } = [];
    public double AverageConfidence { get; init; }
    public List<string> Insights { get; init; } = [];
}
