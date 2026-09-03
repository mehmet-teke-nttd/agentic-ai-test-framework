using Test.Agent.Contracts;
using Test.AgentGateway;
using TestFramework.Core;
using TestFramework.Playwright;

namespace TestFramework.UnitTests;

public sealed class FrameworkTests
{
    private string _temporaryDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), "agentic-framework-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_temporaryDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_temporaryDirectory)) Directory.Delete(_temporaryDirectory, recursive: true);
    }

    [Test]
    public async Task Discovery_parses_metadata_and_reports_missing_values()
    {
        await File.WriteAllTextAsync(Path.Combine(_temporaryDirectory, "sample.feature"), """
            @Category:Smoke @Risk:High @Layer:Ui @Requirement:REQ-42 @Feature:Checkout
            Feature: Buying
              Scenario: Complete order
                Given something

              @Risk:Impossible
              Scenario: Malformed metadata
                Given something else
            """);

        var tests = await new FeatureFileDiscovery(_temporaryDirectory).DiscoverAsync();

        Assert.Multiple(() =>
        {
            Assert.That(tests, Has.Count.EqualTo(2));
            Assert.That(tests[0].Category, Is.EqualTo("Smoke"));
            Assert.That(tests[0].Risk, Is.EqualTo(RiskLevel.High));
            Assert.That(tests[0].Layer, Is.EqualTo(TestLayer.Ui));
            Assert.That(tests[0].Requirement, Is.EqualTo("REQ-42"));
            Assert.That(tests[0].Feature, Is.EqualTo("Checkout"));
            Assert.That(tests[1].Risk, Is.EqualTo(RiskLevel.Unknown));
            Assert.That(tests[1].Warnings, Has.Some.Contains("unsupported value"));
        });
    }

    [Test]
    public void Filter_builder_constructs_combined_filter()
    {
        var request = new TestRunRequest
        {
            ProjectId = "ui-tests",
            TestIds = ["Feature::Scenario"],
            Categories = ["Smoke"]
        };

        Assert.That(
            NUnitFilterBuilder.Build(request),
            Is.EqualTo("(FullyQualifiedName=Feature::Scenario)&(TestCategory=Smoke)"));
    }

    [TestCase("Smoke; dotnet evil")]
    [TestCase("")]
    [TestCase("name\"")]
    public void Filter_builder_rejects_unsafe_values(string value)
    {
        var request = new TestRunRequest { ProjectId = "ui-tests", Categories = [value] };
        Assert.That(() => NUnitFilterBuilder.Build(request), Throws.ArgumentException);
    }

    [Test]
    public void Application_url_resolver_prefers_environment_then_configuration()
    {
        const string configured = "https://qa.example.test";
        const string environment = "https://staging.example.test";

        Assert.Multiple(() =>
        {
            Assert.That(ApplicationUrlResolver.Resolve(configured), Is.EqualTo(configured));
            Assert.That(ApplicationUrlResolver.Resolve(configured, environment), Is.EqualTo(environment));
            Assert.That(ApplicationUrlResolver.Resolve(null), Is.EqualTo(ApplicationUrlResolver.DefaultBaseUrl));
        });
    }

    [TestCase("relative/path")]
    [TestCase("ftp://example.test")]
    [TestCase("javascript:alert(1)")]
    public void Application_url_resolver_rejects_unsupported_urls(string value)
    {
        Assert.That(
            () => ApplicationUrlResolver.Resolve(value),
            Throws.InvalidOperationException.With.Message.Contains("absolute HTTP, HTTPS, or data URL"));
    }

    [Test]
    public void Trx_normalizer_maps_pass_fail_and_skip()
    {
        var path = WriteTrx("""
            <TestRun>
              <Results>
                <UnitTestResult testName="passes" outcome="Passed" duration="00:00:00.100" />
                <UnitTestResult testName="fails" outcome="Failed" duration="00:00:00.200">
                  <Output><ErrorInfo><Message>Expected true</Message><StackTrace>at test</StackTrace></ErrorInfo></Output>
                </UnitTestResult>
                <UnitTestResult testName="skips" outcome="NotExecuted" />
              </Results>
            </TestRun>
            """);

        var result = new TrxNormalizer().Normalize(path, "run-1", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        Assert.Multiple(() =>
        {
            Assert.That(result.Outcome, Is.EqualTo(TestOutcome.Failed));
            Assert.That(result.Tests.Select(test => test.Outcome),
                Is.EqualTo(new[] { TestOutcome.Passed, TestOutcome.Failed, TestOutcome.Skipped }));
            Assert.That(result.Tests[1].Failure?.Message, Is.EqualTo("Expected true"));
        });
    }

    [Test]
    public void Trx_normalizer_returns_unknown_for_missing_and_corrupt_files()
    {
        var normalizer = new TrxNormalizer();
        var missing = normalizer.Normalize(
            Path.Combine(_temporaryDirectory, "missing.trx"), "missing", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        var corruptPath = WriteTrx("<not-xml");
        var corrupt = normalizer.Normalize(corruptPath, "corrupt", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        Assert.Multiple(() =>
        {
            Assert.That(missing.Outcome, Is.EqualTo(TestOutcome.Unknown));
            Assert.That(missing.Warnings.Single(), Does.Contain("missing"));
            Assert.That(corrupt.Outcome, Is.EqualTo(TestOutcome.Unknown));
            Assert.That(corrupt.Warnings.Single(), Does.Contain("could not be parsed"));
        });
    }

    [Test]
    public async Task Evidence_store_lists_only_files_for_valid_run()
    {
        var runDirectory = Path.Combine(_temporaryDirectory, "run-123");
        Directory.CreateDirectory(runDirectory);
        await File.WriteAllTextAsync(Path.Combine(runDirectory, "shot.png"), "image");
        var store = new FileEvidenceStore(_temporaryDirectory);

        var evidence = await store.ListAsync("run-123");

        Assert.Multiple(() =>
        {
            Assert.That(evidence, Has.Count.EqualTo(1));
            Assert.That(evidence[0].Kind, Is.EqualTo("screenshot"));
            Assert.That(async () => await store.ListAsync("../escape"), Throws.ArgumentException);
        });
    }

    [TestCase("Expected: 2 But was: 1", FailureClassification.ProductDefect)]
    [TestCase("Browser executable doesn't exist; playwright install", FailureClassification.Environment)]
    [TestCase("Locator strict mode violation", FailureClassification.TestDefect)]
    [TestCase("testhost process exited", FailureClassification.Infrastructure)]
    [TestCase("unrecognized failure", FailureClassification.Unknown)]
    public async Task Analyzer_classifies_known_signatures(string message, FailureClassification expected)
    {
        var run = FailedRun(message);
        var analysis = await new DeterministicFailureAnalyzer().AnalyzeAsync(run, "test", []);
        Assert.That(analysis.Classification, Is.EqualTo(expected));
        if (expected == FailureClassification.Unknown) Assert.That(analysis.Confidence, Is.Zero);
    }

    private string WriteTrx(string contents)
    {
        var path = Path.Combine(_temporaryDirectory, $"{Guid.NewGuid():N}.trx");
        File.WriteAllText(path, contents);
        return path;
    }

    private static TestRunResult FailedRun(string message) => new()
    {
        RunId = "run",
        Outcome = TestOutcome.Failed,
        StartedAtUtc = DateTimeOffset.UtcNow,
        CompletedAtUtc = DateTimeOffset.UtcNow,
        Tests =
        [
            new TestCaseResult
            {
                TestId = "test",
                Outcome = TestOutcome.Failed,
                Failure = new TestFailure { TestId = "test", Message = message }
            }
        ]
    };
}
