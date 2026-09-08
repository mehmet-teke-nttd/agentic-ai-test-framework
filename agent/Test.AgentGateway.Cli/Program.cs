using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Test.Agent.Contracts;
using Test.AgentGateway;
using TestFramework.Core;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var root = FindRepositoryRoot(AppContext.BaseDirectory);
var options = new GatewayOptions
{
    RepositoryRoot = root,
    EvidenceRoot = Path.Combine(root, "Evidence"),
    AllowedProjects = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["ui-tests"] = Path.Combine("tests", "UI.Tests", "UI.Tests.csproj"),
        ["api-tests"] = Path.Combine("tests", "API.Tests", "API.Tests.csproj"),
        ["integration-tests"] = Path.Combine("tests", "Integration.Tests", "Integration.Tests.csproj")
    }
};
var gateway = new SafeTestGateway(options);
var discovery = new FeatureFileDiscovery(root);
var historyTracker = new HistoricalFailureTracker(options.EvidenceRoot);

// Configure failure analyzer based on settings
IFailureAnalyzer analyzer;
var llmEnabled = configuration.GetValue<bool>("LlmAnalyzer:Enabled");
if (llmEnabled)
{
    var llmOptions = new LlmFailureAnalyzerOptions
    {
        Provider = configuration["LlmAnalyzer:Provider"] ?? "OpenAI",
        ApiKey = Environment.GetEnvironmentVariable("LLM_API_KEY")
            ?? configuration["LlmAnalyzer:ApiKey"]
            ?? throw new InvalidOperationException("LLM API key not configured. Set LLM_API_KEY environment variable or LlmAnalyzer:ApiKey in appsettings.json"),
        Endpoint = configuration["AzureOpenAI:Endpoint"],
        Model = configuration["LlmAnalyzer:Model"] ?? "gpt-4o-mini",
        MaxTokens = configuration.GetValue<int>("LlmAnalyzer:MaxTokens", 2000),
        Temperature = configuration.GetValue<double>("LlmAnalyzer:Temperature", 0.3),
        Timeout = TimeSpan.TryParse(configuration["LlmAnalyzer:Timeout"], out var timeout)
            ? timeout
            : TimeSpan.FromSeconds(30),
        IncludeScreenshots = configuration.GetValue<bool>("LlmAnalyzer:IncludeScreenshots", true),
        IncludeStackTraces = configuration.GetValue<bool>("LlmAnalyzer:IncludeStackTraces", true),
        IncludeHistoricalData = configuration.GetValue<bool>("LlmAnalyzer:IncludeHistoricalData", false)
    };
    
    analyzer = new LlmFailureAnalyzer(llmOptions, new DeterministicFailureAnalyzer());
    Console.Error.WriteLine($"[INFO] LLM Failure Analyzer enabled: {llmOptions.Provider} ({llmOptions.Model})");
}
else
{
    analyzer = new DeterministicFailureAnalyzer();
    Console.Error.WriteLine("[INFO] Using deterministic failure analyzer");
}

var wireJson = new JsonSerializerOptions(ContractJson.Options) { WriteIndented = false };

while (await Console.In.ReadLineAsync() is { } line)
{
    ToolResponse response;
    try
    {
        var request = JsonSerializer.Deserialize<ToolRequest>(line, ContractJson.Options)
            ?? throw new ArgumentException("Request cannot be null.");
        if (request.SchemaVersion != ContractVersions.V1)
            throw new ArgumentException($"Unsupported schema version '{request.SchemaVersion}'.");
        response = await DispatchAsync(request);
    }
    catch (Exception exception) when (exception is JsonException or ArgumentException or InvalidOperationException)
    {
        response = new ToolResponse(null, false, null, new ToolError("invalid_request", exception.Message));
    }
    Console.WriteLine(JsonSerializer.Serialize(response, wireJson));
}

async Task<ToolResponse> DispatchAsync(ToolRequest request)
{
    object? result = request.Tool switch
    {
        "discover_tests" => await DiscoverAsync(request.Arguments),
        "run_tests" => await gateway.RunAsync(Deserialize<TestRunRequest>(request.Arguments)),
        "get_test_result" => await gateway.GetResultAsync(RequiredString(request.Arguments, "runId")),
        "get_test_evidence" => await gateway.GetEvidenceAsync(RequiredString(request.Arguments, "runId")),
        "analyze_test_failure" => await AnalyzeAsync(request.Arguments),
        "compare_analyzers" => await CompareAnalyzersAsync(request.Arguments),
        "get_failure_history" => await GetFailureHistoryAsync(request.Arguments),
        "analyze_failure_patterns" => await AnalyzeFailurePatternsAsync(request.Arguments),
        _ => throw new ArgumentException(
            "Unknown tool. Allowed tools: discover_tests, run_tests, get_test_result, get_test_evidence, analyze_test_failure, compare_analyzers, get_failure_history, analyze_failure_patterns.")
    };
    return new ToolResponse(request.Id, true, result, null);
}

async Task<IReadOnlyList<TestMetadata>> DiscoverAsync(JsonElement arguments)
{
    var all = await discovery.DiscoverAsync();
    if (!arguments.TryGetProperty("query", out var queryElement) ||
        string.IsNullOrWhiteSpace(queryElement.GetString()))
    {
        return all;
    }
    var query = queryElement.GetString()!;
    return all.Where(test =>
        test.TestId.Contains(query, StringComparison.OrdinalIgnoreCase) ||
        test.Tags.Any(tag => tag.Contains(query, StringComparison.OrdinalIgnoreCase))).ToArray();
}

async Task<FailureAnalysis> AnalyzeAsync(JsonElement arguments)
{
    var runId = RequiredString(arguments, "runId");
    var testId = RequiredString(arguments, "testId");
    var result = await gateway.GetResultAsync(runId)
        ?? throw new ArgumentException($"Run '{runId}' was not found.");
    var evidence = await gateway.GetEvidenceAsync(runId);
    var analysis = await analyzer.AnalyzeAsync(result, testId, evidence);
    
    // Record in history if it's a failure
    if (result.Outcome == TestOutcome.Failed)
    {
        await historyTracker.RecordFailureAsync(testId, analysis, result);
    }
    
    return analysis;
}

async Task<ComparativeAnalysisResult> CompareAnalyzersAsync(JsonElement arguments)
{
    var runId = RequiredString(arguments, "runId");
    var testId = RequiredString(arguments, "testId");
    var result = await gateway.GetResultAsync(runId)
        ?? throw new ArgumentException($"Run '{runId}' was not found.");
    var evidence = await gateway.GetEvidenceAsync(runId);

    // Run both analyzers
    var deterministicAnalyzer = new DeterministicFailureAnalyzer();
    var deterministicAnalysis = await deterministicAnalyzer.AnalyzeAsync(result, testId, evidence);

    FailureAnalysis llmAnalysis;
    if (llmEnabled)
    {
        // If LLM is enabled, use it; otherwise create a placeholder
        llmAnalysis = await analyzer.AnalyzeAsync(result, testId, evidence);
    }
    else
    {
        llmAnalysis = new FailureAnalysis
        {
            RunId = runId,
            TestId = testId,
            Classification = FailureClassification.Unknown,
            Confidence = 0.0,
            Reasons = ["LLM analyzer not enabled. Enable in appsettings.json and provide API key."],
            CitedEvidence = []
        };
    }

    return FailureAnalysisComparator.Compare(deterministicAnalysis, llmAnalysis, runId, testId);
}

async Task<List<HistoricalFailureEntry>> GetFailureHistoryAsync(JsonElement arguments)
{
    var testId = RequiredString(arguments, "testId");
    return await historyTracker.LoadHistoryAsync(testId);
}

async Task<FailurePatternAnalysis> AnalyzeFailurePatternsAsync(JsonElement arguments)
{
    var testId = RequiredString(arguments, "testId");
    return await historyTracker.AnalyzePatternsAsync(testId);
}

static T Deserialize<T>(JsonElement element) =>
    element.Deserialize<T>(ContractJson.Options) ?? throw new ArgumentException("Arguments are required.");

static string RequiredString(JsonElement element, string name)
{
    if (!element.TryGetProperty(name, out var property) || string.IsNullOrWhiteSpace(property.GetString()))
        throw new ArgumentException($"Argument '{name}' is required.");
    return property.GetString()!;
}

static string FindRepositoryRoot(string start)
{
    var directory = new DirectoryInfo(start);
    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "AgenticTestFramework.sln")))
            return directory.FullName;
        directory = directory.Parent;
    }
    throw new InvalidOperationException("Could not locate repository root.");
}

internal sealed record ToolRequest(
    string SchemaVersion,
    string? Id,
    string Tool,
    JsonElement Arguments);

internal sealed record ToolResponse(
    string? Id,
    bool Success,
    object? Result,
    ToolError? Error)
{
    public string SchemaVersion { get; init; } = ContractVersions.V1;
}

internal sealed record ToolError(string Code, string Message);
