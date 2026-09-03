using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Test.Agent.Contracts;
using TestFramework.Core;

namespace Test.AgentGateway;

public sealed record GatewayOptions
{
    public required string RepositoryRoot { get; init; }
    public required string EvidenceRoot { get; init; }
    public required IReadOnlyDictionary<string, string> AllowedProjects { get; init; }
    public TimeSpan MaximumTimeout { get; init; } = TimeSpan.FromMinutes(30);
}

public static class ContractJson
{
    public static JsonSerializerOptions Options { get; } = Create();

    private static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}

public sealed class SafeTestGateway : ITestGateway
{
    private static readonly Regex RunIdPattern = new(@"^[a-zA-Z0-9-]{1,80}$", RegexOptions.Compiled);
    private readonly GatewayOptions _options;
    private readonly TrxNormalizer _normalizer;
    private readonly FileEvidenceStore _evidence;

    public SafeTestGateway(GatewayOptions options)
    {
        _options = options;
        _normalizer = new TrxNormalizer();
        _evidence = new FileEvidenceStore(options.EvidenceRoot);
    }

    public async Task<TestRunResult> RunAsync(TestRunRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        var runId = $"{DateTimeOffset.UtcNow:yyyyMMddTHHmmssfffZ}-{Guid.NewGuid():N}";
        var runDirectory = Path.Combine(Path.GetFullPath(_options.EvidenceRoot), runId);
        Directory.CreateDirectory(runDirectory);

        var project = Path.GetFullPath(Path.Combine(_options.RepositoryRoot, _options.AllowedProjects[request.ProjectId]));
        EnsureUnderRoot(project, _options.RepositoryRoot);
        var started = DateTimeOffset.UtcNow;
        var context = new TestExecutionContext
        {
            RunId = runId,
            ProjectId = request.ProjectId,
            RepositoryRoot = Path.GetFullPath(_options.RepositoryRoot),
            EvidenceDirectory = runDirectory,
            StartedAtUtc = started,
            SourceRevision = ReadRevision(_options.RepositoryRoot)
        };
        await WriteJsonAsync(Path.Combine(runDirectory, "context.json"), context, cancellationToken);
        await LogAsync(runDirectory, runId, "run_started", new { request.ProjectId, request.TestIds, request.Categories });

        var trxPath = Path.Combine(runDirectory, "results.trx");
        var stdoutPath = Path.Combine(runDirectory, "stdout.log");
        var stderrPath = Path.Combine(runDirectory, "stderr.log");
        var process = CreateProcess(project, trxPath, request, runId, runDirectory);
        var timedOut = false;
        string stdout;
        string stderr;

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(request.Timeout);
        try
        {
            process.Start();
            var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(timeout.Token);
            stdout = await stdoutTask;
            stderr = await stderrTask;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            TryKill(process);
            throw;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            timedOut = true;
            TryKill(process);
            stdout = await process.StandardOutput.ReadToEndAsync(CancellationToken.None);
            stderr = await process.StandardError.ReadToEndAsync(CancellationToken.None);
        }
        finally
        {
            process.Dispose();
        }

        await File.WriteAllTextAsync(stdoutPath, stdout, cancellationToken);
        await File.WriteAllTextAsync(stderrPath, stderr, cancellationToken);
        var result = _normalizer.Normalize(trxPath, runId, started, DateTimeOffset.UtcNow);
        var warnings = result.Warnings.ToList();
        if (timedOut) warnings.Add($"Execution exceeded timeout of {request.Timeout}.");
        result = result with
        {
            Outcome = timedOut ? TestOutcome.Unknown : result.Outcome,
            Warnings = warnings,
            StandardOutputPath = "stdout.log",
            StandardErrorPath = "stderr.log"
        };
        await WriteJsonAsync(Path.Combine(runDirectory, "test-result.json"), result, cancellationToken);
        await WriteManifestAsync(runDirectory, runId, cancellationToken);
        await LogAsync(runDirectory, runId, "run_completed", new { result.Outcome, TestCount = result.Tests.Count });
        return result;
    }

    public async Task<TestRunResult?> GetResultAsync(string runId, CancellationToken cancellationToken = default)
    {
        var path = SafeRunFile(runId, "test-result.json");
        if (!File.Exists(path)) return null;
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<TestRunResult>(stream, ContractJson.Options, cancellationToken);
    }

    public Task<IReadOnlyList<TestEvidence>> GetEvidenceAsync(
        string runId, CancellationToken cancellationToken = default) => _evidence.ListAsync(runId, cancellationToken);

    private void Validate(TestRunRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.SchemaVersion != ContractVersions.V1)
            throw new ArgumentException($"Unsupported schema version '{request.SchemaVersion}'.");
        if (!_options.AllowedProjects.ContainsKey(request.ProjectId))
            throw new ArgumentException($"ProjectId '{request.ProjectId}' is not allowlisted.");
        if (request.Timeout <= TimeSpan.Zero || request.Timeout > _options.MaximumTimeout)
            throw new ArgumentOutOfRangeException(nameof(request), "Timeout is outside the allowed range.");
        _ = NUnitFilterBuilder.Build(request);
    }

    private static Process CreateProcess(
        string project, string trxPath, TestRunRequest request, string runId, string runDirectory)
    {
        var info = new ProcessStartInfo
        {
            FileName = "dotnet",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        info.Environment["TEST_RUN_ID"] = runId;
        info.Environment["TEST_EVIDENCE_DIR"] = runDirectory;
        info.ArgumentList.Add("test");
        info.ArgumentList.Add(project);
        info.ArgumentList.Add("--no-restore");
        info.ArgumentList.Add("--results-directory");
        info.ArgumentList.Add(runDirectory);
        info.ArgumentList.Add("--logger");
        info.ArgumentList.Add($"trx;LogFileName={Path.GetFileName(trxPath)}");
        var filter = NUnitFilterBuilder.Build(request);
        if (filter is not null)
        {
            info.ArgumentList.Add("--filter");
            info.ArgumentList.Add(filter);
        }

        return new Process { StartInfo = info };
    }

    private string SafeRunFile(string runId, string file)
    {
        if (!RunIdPattern.IsMatch(runId)) throw new ArgumentException("Invalid runId.", nameof(runId));
        return Path.Combine(Path.GetFullPath(_options.EvidenceRoot), runId, file);
    }

    private static void EnsureUnderRoot(string path, string root)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!path.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Allowlisted project resolves outside the repository.");
    }

    private static void TryKill(Process process)
    {
        try { if (!process.HasExited) process.Kill(entireProcessTree: true); }
        catch (InvalidOperationException) { }
    }

    private static string? ReadRevision(string root)
    {
        var fromBuild = Environment.GetEnvironmentVariable("BUILD_SOURCEVERSION");
        if (!string.IsNullOrWhiteSpace(fromBuild)) return fromBuild;
        var head = Path.Combine(root, ".git", "HEAD");
        if (!File.Exists(head)) return null;
        var value = File.ReadAllText(head).Trim();
        if (!value.StartsWith("ref: ", StringComparison.Ordinal)) return value;
        var reference = Path.Combine(root, ".git", value[5..].Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(reference) ? File.ReadAllText(reference).Trim() : value;
    }

    private static async Task WriteJsonAsync<T>(string path, T value, CancellationToken cancellationToken)
    {
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, value, ContractJson.Options, cancellationToken);
    }

    private static async Task LogAsync(string directory, string runId, string eventName, object data)
    {
        var entry = JsonSerializer.Serialize(new
        {
            timestampUtc = DateTimeOffset.UtcNow,
            runId,
            eventName,
            data
        }, ContractJson.Options);
        await File.AppendAllTextAsync(Path.Combine(directory, "run.log.jsonl"), entry + Environment.NewLine);
    }

    private static async Task WriteManifestAsync(string directory, string runId, CancellationToken cancellationToken)
    {
        var files = Directory.EnumerateFiles(directory)
            .Select(path => new TestEvidence
            {
                RunId = runId,
                RelativePath = Path.GetFileName(path),
                Kind = EvidenceKinds.FromPath(path),
                SizeBytes = new FileInfo(path).Length
            })
            .OrderBy(item => item.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        await WriteJsonAsync(Path.Combine(directory, "manifest.json"), files, cancellationToken);
    }
}

public sealed class TrxNormalizer
{
    public TestRunResult Normalize(
        string trxPath, string runId, DateTimeOffset startedAtUtc, DateTimeOffset completedAtUtc)
    {
        if (!File.Exists(trxPath))
            return Empty(runId, startedAtUtc, completedAtUtc, $"TRX file is missing: {Path.GetFileName(trxPath)}.");

        try
        {
            var document = XDocument.Load(trxPath);
            var results = document.Descendants().Where(element => element.Name.LocalName == "UnitTestResult")
                .Select(ParseResult).ToArray();
            var outcome = results.Any(test => test.Outcome == TestOutcome.Failed) ? TestOutcome.Failed
                : results.Length == 0 ? TestOutcome.Unknown
                : results.All(test => test.Outcome == TestOutcome.Skipped) ? TestOutcome.Skipped
                : results.Any(test => test.Outcome == TestOutcome.Passed) ? TestOutcome.Passed
                : TestOutcome.Unknown;
            return new TestRunResult
            {
                RunId = runId,
                Outcome = outcome,
                StartedAtUtc = startedAtUtc,
                CompletedAtUtc = completedAtUtc,
                Tests = results,
                Warnings = results.Length == 0 ? ["TRX contained no test results."] : []
            };
        }
        catch (Exception exception) when (exception is System.Xml.XmlException or IOException or UnauthorizedAccessException)
        {
            return Empty(runId, startedAtUtc, completedAtUtc, $"TRX could not be parsed: {exception.Message}");
        }
    }

    private static TestCaseResult ParseResult(XElement element)
    {
        var name = (string?)element.Attribute("testName") ?? (string?)element.Attribute("testId") ?? "unknown";
        var outcomeText = (string?)element.Attribute("outcome") ?? string.Empty;
        var outcome = outcomeText.ToLowerInvariant() switch
        {
            "passed" => TestOutcome.Passed,
            "failed" => TestOutcome.Failed,
            "notexecuted" or "skipped" => TestOutcome.Skipped,
            _ => TestOutcome.Unknown
        };
        var duration = TimeSpan.TryParse((string?)element.Attribute("duration"), CultureInfo.InvariantCulture, out var value)
            ? value : TimeSpan.Zero;
        var message = element.Descendants().FirstOrDefault(node => node.Name.LocalName == "Message")?.Value;
        var stack = element.Descendants().FirstOrDefault(node => node.Name.LocalName == "StackTrace")?.Value;
        return new TestCaseResult
        {
            TestId = name,
            Outcome = outcome,
            Duration = duration,
            Failure = outcome == TestOutcome.Failed
                ? new TestFailure { TestId = name, Message = message ?? "Test failed without a message.", StackTrace = stack }
                : null
        };
    }

    private static TestRunResult Empty(
        string runId, DateTimeOffset started, DateTimeOffset completed, string warning) => new()
        {
            RunId = runId,
            Outcome = TestOutcome.Unknown,
            StartedAtUtc = started,
            CompletedAtUtc = completed,
            Warnings = [warning]
        };
}

public sealed class FileEvidenceStore(string evidenceRoot) : IEvidenceStore
{
    private static readonly Regex RunIdPattern = new(@"^[a-zA-Z0-9-]{1,80}$", RegexOptions.Compiled);
    private readonly string _root = Path.GetFullPath(evidenceRoot);

    public Task<IReadOnlyList<TestEvidence>> ListAsync(string runId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!RunIdPattern.IsMatch(runId)) throw new ArgumentException("Invalid runId.", nameof(runId));
        var directory = Path.Combine(_root, runId);
        IReadOnlyList<TestEvidence> result = !Directory.Exists(directory)
            ? []
            : Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)
                .Select(path => new TestEvidence
                {
                    RunId = runId,
                    RelativePath = Path.GetRelativePath(directory, path).Replace('\\', '/'),
                    Kind = EvidenceKinds.FromPath(path),
                    SizeBytes = new FileInfo(path).Length
                })
                .OrderBy(item => item.RelativePath, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        return Task.FromResult(result);
    }
}

internal static class EvidenceKinds
{
    public static string FromPath(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".trx" => "test-results",
        ".png" => "screenshot",
        ".zip" => "playwright-trace",
        ".json" => "structured-data",
        ".jsonl" => "structured-log",
        ".log" => "log",
        _ => "artifact"
    };
}

public sealed class DeterministicFailureAnalyzer : IFailureAnalyzer
{
    public Task<FailureAnalysis> AnalyzeAsync(
        TestRunResult result, string testId, IReadOnlyList<TestEvidence> evidence,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var failure = result.Tests.FirstOrDefault(test =>
            string.Equals(test.TestId, testId, StringComparison.OrdinalIgnoreCase))?.Failure;
        var text = $"{failure?.Message} {failure?.StackTrace}".ToLowerInvariant();
        var (classification, confidence, reason) =
            ContainsAny(text, "browser executable", "playwright install", "connection refused", "timed out")
                ? (FailureClassification.Environment, 0.90, "Failure matches a known environment or browser-setup signature.")
            : ContainsAny(text, "assert", "expected", "multiple failures")
                ? (FailureClassification.ProductDefect, 0.65, "Failure contains an assertion mismatch.")
            : ContainsAny(text, "nullreferenceexception", "locator", "strict mode violation", "element not found")
                ? (FailureClassification.TestDefect, 0.70, "Failure matches a test-code or locator signature.")
            : ContainsAny(text, "outofmemory", "testhost", "process exited", "access denied")
                ? (FailureClassification.Infrastructure, 0.80, "Failure matches an infrastructure signature.")
            : (FailureClassification.Unknown, 0.0, "Available evidence is insufficient for deterministic classification.");

        var cited = evidence.Where(item =>
                item.Kind is "screenshot" or "playwright-trace" or "structured-log" or "test-results")
            .Select(item => item.RelativePath).Take(10).ToArray();
        return Task.FromResult(new FailureAnalysis
        {
            RunId = result.RunId,
            TestId = testId,
            Classification = classification,
            Confidence = confidence,
            Reasons = [reason],
            CitedEvidence = cited
        });
    }

    private static bool ContainsAny(string value, params string[] patterns) =>
        patterns.Any(value.Contains);
}
