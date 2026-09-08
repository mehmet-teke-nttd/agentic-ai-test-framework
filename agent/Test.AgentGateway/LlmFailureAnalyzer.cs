using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Test.Agent.Contracts;

namespace Test.AgentGateway;

public sealed class LlmFailureAnalyzerOptions
{
    public required string Provider { get; init; } // "OpenAI", "AzureOpenAI", "Anthropic"
    public required string ApiKey { get; init; }
    public string? Endpoint { get; init; } // For Azure OpenAI
    public string Model { get; init; } = "gpt-4o-mini";
    public int MaxTokens { get; init; } = 2000;
    public double Temperature { get; init; } = 0.3;
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
    public bool IncludeScreenshots { get; init; } = true;
    public bool IncludeStackTraces { get; init; } = true;
    public bool IncludeHistoricalData { get; init; } = false;
}

public sealed class LlmFailureAnalyzer : IFailureAnalyzer
{
    private readonly LlmFailureAnalyzerOptions _options;
    private readonly HttpClient _httpClient;
    private readonly IFailureAnalyzer _fallbackAnalyzer;
    private readonly string _endpoint;

    public LlmFailureAnalyzer(
        LlmFailureAnalyzerOptions options,
        IFailureAnalyzer? fallbackAnalyzer = null)
    {
        _options = options;
        _fallbackAnalyzer = fallbackAnalyzer ?? new DeterministicFailureAnalyzer();
        _httpClient = new HttpClient { Timeout = options.Timeout };
        
        _endpoint = _options.Provider.ToLowerInvariant() switch
        {
            "openai" => "https://api.openai.com/v1/chat/completions",
            "azureopenai" => _options.Endpoint 
                ?? throw new InvalidOperationException("Endpoint required for Azure OpenAI"),
            "anthropic" => "https://api.anthropic.com/v1/messages",
            _ => throw new ArgumentException($"Unsupported provider: {_options.Provider}")
        };
    }

    public async Task<FailureAnalysis> AnalyzeAsync(
        TestRunResult result,
        string testId,
        IReadOnlyList<TestEvidence> evidence,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // If LLM analysis fails, fall back to deterministic analyzer
            var failure = result.Tests.FirstOrDefault(t =>
                string.Equals(t.TestId, testId, StringComparison.OrdinalIgnoreCase))?.Failure;

            if (failure == null)
            {
                return new FailureAnalysis
                {
                    RunId = result.RunId,
                    TestId = testId,
                    Classification = FailureClassification.Unknown,
                    Confidence = 0.0,
                    Reasons = ["Test did not fail or was not found in results."],
                    CitedEvidence = []
                };
            }

            var prompt = BuildAnalysisPrompt(result, testId, failure, evidence);
            var llmResponse = await CallLlmAsync(prompt, cancellationToken);
            var analysis = ParseLlmResponse(llmResponse, result.RunId, testId, evidence);

            return analysis;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            // Fall back to deterministic analyzer on LLM failure
            return await _fallbackAnalyzer.AnalyzeAsync(result, testId, evidence, cancellationToken);
        }
    }

    private string BuildAnalysisPrompt(
        TestRunResult result,
        string testId,
        TestFailure failure,
        IReadOnlyList<TestEvidence> evidence)
    {
        var prompt = new StringBuilder();
        
        prompt.AppendLine("You are an expert software test failure analyzer. Analyze this test failure and provide:");
        prompt.AppendLine("1. Classification (ProductDefect, TestDefect, Environment, Infrastructure, or Unknown)");
        prompt.AppendLine("2. Confidence level (0.0 to 1.0)");
        prompt.AppendLine("3. Multiple specific reasons for the failure");
        prompt.AppendLine("4. Actionable recommendations to fix the issue");
        prompt.AppendLine();
        prompt.AppendLine("## Test Information");
        prompt.AppendLine($"Test ID: {testId}");
        prompt.AppendLine($"Run ID: {result.RunId}");
        prompt.AppendLine($"Outcome: {result.Outcome}");
        prompt.AppendLine($"Started: {result.StartedAtUtc:u}");
        prompt.AppendLine($"Completed: {result.CompletedAtUtc:u}");
        prompt.AppendLine();
        
        prompt.AppendLine("## Failure Details");
        prompt.AppendLine($"Message: {failure.Message}");
        prompt.AppendLine();
        
        if (_options.IncludeStackTraces && !string.IsNullOrWhiteSpace(failure.StackTrace))
        {
            prompt.AppendLine("## Stack Trace");
            prompt.AppendLine("```");
            prompt.AppendLine(TruncateIfNeeded(failure.StackTrace, 2000));
            prompt.AppendLine("```");
            prompt.AppendLine();
        }

        var screenshots = evidence.Where(e => e.Kind == "screenshot").ToArray();
        var traces = evidence.Where(e => e.Kind == "playwright-trace").ToArray();
        var logs = evidence.Where(e => e.Kind is "log" or "structured-log").ToArray();

        if (_options.IncludeScreenshots && screenshots.Length > 0)
        {
            prompt.AppendLine("## Available Evidence");
            prompt.AppendLine($"- Screenshots: {screenshots.Length} file(s)");
            foreach (var screenshot in screenshots.Take(3))
            {
                prompt.AppendLine($"  - {screenshot.RelativePath}");
            }
        }

        if (traces.Length > 0)
        {
            prompt.AppendLine($"- Playwright Traces: {traces.Length} file(s)");
        }

        if (logs.Length > 0)
        {
            prompt.AppendLine($"- Log Files: {logs.Length} file(s)");
        }

        prompt.AppendLine();
        prompt.AppendLine("## Classification Guidelines");
        prompt.AppendLine("- **ProductDefect**: Assertion failures, incorrect application behavior, business logic errors");
        prompt.AppendLine("- **TestDefect**: Test code issues, flaky tests, incorrect selectors, missing waits");
        prompt.AppendLine("- **Environment**: Missing dependencies, network issues, browser not installed, service unavailable");
        prompt.AppendLine("- **Infrastructure**: Timeouts, out of memory, test host crashes, resource exhaustion");
        prompt.AppendLine();
        
        prompt.AppendLine("## Response Format");
        prompt.AppendLine("Respond ONLY with valid JSON in this exact format:");
        prompt.AppendLine("```json");
        prompt.AppendLine("{");
        prompt.AppendLine("  \"classification\": \"ProductDefect|TestDefect|Environment|Infrastructure|Unknown\",");
        prompt.AppendLine("  \"confidence\": 0.85,");
        prompt.AppendLine("  \"reasons\": [");
        prompt.AppendLine("    \"Primary reason for failure\",");
        prompt.AppendLine("    \"Secondary contributing factor\",");
        prompt.AppendLine("    \"Additional context\"");
        prompt.AppendLine("  ],");
        prompt.AppendLine("  \"recommendations\": [");
        prompt.AppendLine("    \"Specific action to fix the issue\",");
        prompt.AppendLine("    \"Prevention strategy\"");
        prompt.AppendLine("  ]");
        prompt.AppendLine("}");
        prompt.AppendLine("```");

        return prompt.ToString();
    }

    private async Task<string> CallLlmAsync(string prompt, CancellationToken cancellationToken)
    {
        var requestBody = _options.Provider.ToLowerInvariant() switch
        {
            "openai" or "azureopenai" => BuildOpenAiRequest(prompt),
            "anthropic" => BuildAnthropicRequest(prompt),
            _ => throw new NotSupportedException($"Provider {_options.Provider} not supported")
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _endpoint)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json")
        };

        // Add authentication headers
        if (_options.Provider.ToLowerInvariant() == "anthropic")
        {
            request.Headers.Add("x-api-key", _options.ApiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");
        }
        else
        {
            request.Headers.Add("Authorization", $"Bearer {_options.ApiKey}");
        }

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return ExtractResponseText(responseContent, _options.Provider);
    }

    private object BuildOpenAiRequest(string prompt)
    {
        return new
        {
            model = _options.Model,
            messages = new[]
            {
                new { role = "system", content = "You are an expert test failure analyzer. Respond only with valid JSON." },
                new { role = "user", content = prompt }
            },
            temperature = _options.Temperature,
            max_tokens = _options.MaxTokens
        };
    }

    private object BuildAnthropicRequest(string prompt)
    {
        return new
        {
            model = _options.Model,
            max_tokens = _options.MaxTokens,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = _options.Temperature
        };
    }

    private string ExtractResponseText(string responseContent, string provider)
    {
        using var doc = JsonDocument.Parse(responseContent);
        
        return provider.ToLowerInvariant() switch
        {
            "openai" or "azureopenai" => doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty,
            
            "anthropic" => doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty,
            
            _ => throw new NotSupportedException($"Provider {provider} not supported")
        };
    }

    private FailureAnalysis ParseLlmResponse(
        string llmResponse,
        string runId,
        string testId,
        IReadOnlyList<TestEvidence> evidence)
    {
        try
        {
            // Extract JSON from markdown code blocks if present
            var jsonContent = llmResponse;
            if (llmResponse.Contains("```json"))
            {
                var start = llmResponse.IndexOf("```json") + 7;
                var end = llmResponse.IndexOf("```", start);
                jsonContent = llmResponse[start..end].Trim();
            }
            else if (llmResponse.Contains("```"))
            {
                var start = llmResponse.IndexOf("```") + 3;
                var end = llmResponse.IndexOf("```", start);
                jsonContent = llmResponse[start..end].Trim();
            }

            var response = JsonSerializer.Deserialize<LlmAnalysisResponse>(jsonContent,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                });

            if (response == null)
            {
                throw new JsonException("Failed to deserialize LLM response");
            }

            // Cite relevant evidence
            var citedEvidence = evidence
                .Where(e => e.Kind is "screenshot" or "playwright-trace" or "structured-log" or "test-results")
                .Select(e => e.RelativePath)
                .Take(10)
                .ToArray();

            return new FailureAnalysis
            {
                RunId = runId,
                TestId = testId,
                Classification = response.Classification,
                Confidence = Math.Clamp(response.Confidence, 0.0, 1.0),
                Reasons = response.Reasons.Concat(response.Recommendations.Select(r => $"[Recommendation] {r}")).ToArray(),
                CitedEvidence = citedEvidence
            };
        }
        catch (JsonException)
        {
            // If JSON parsing fails, create a generic analysis
            return new FailureAnalysis
            {
                RunId = runId,
                TestId = testId,
                Classification = FailureClassification.Unknown,
                Confidence = 0.5,
                Reasons = ["LLM provided analysis in unexpected format", llmResponse.Take(500).ToString() ?? "No response"],
                CitedEvidence = evidence.Select(e => e.RelativePath).Take(5).ToArray()
            };
        }
    }

    private static string TruncateIfNeeded(string text, int maxLength)
    {
        if (text.Length <= maxLength) return text;
        return text[..maxLength] + "\n... (truncated)";
    }

    private sealed class LlmAnalysisResponse
    {
        [JsonPropertyName("classification")]
        public FailureClassification Classification { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("reasons")]
        public List<string> Reasons { get; set; } = [];

        [JsonPropertyName("recommendations")]
        public List<string> Recommendations { get; set; } = [];
    }
}
