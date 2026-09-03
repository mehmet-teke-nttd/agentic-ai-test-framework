using System.Text.Json;
using Test.Agent.Contracts;
using Test.AgentGateway;
using TestFramework.Core;

var root = FindRepositoryRoot(AppContext.BaseDirectory);
var options = new GatewayOptions
{
    RepositoryRoot = root,
    EvidenceRoot = Path.Combine(root, "Evidence"),
    AllowedProjects = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["ui-tests"] = Path.Combine("tests", "UI.Tests", "UI.Tests.csproj")
    }
};
var gateway = new SafeTestGateway(options);
var discovery = new FeatureFileDiscovery(root);
var analyzer = new DeterministicFailureAnalyzer();
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
        _ => throw new ArgumentException(
            "Unknown tool. Allowed tools: discover_tests, run_tests, get_test_result, get_test_evidence, analyze_test_failure.")
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
    return await analyzer.AnalyzeAsync(result, testId, evidence);
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
