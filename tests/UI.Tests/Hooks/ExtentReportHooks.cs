using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Config;
using Reqnroll;

namespace UI.Tests;

[SetUpFixture]
public sealed class ExtentReportLifecycleHooks
{
    private const int MaxReportsToKeep = 5;
    private const string EvidenceDirectoryEnvironmentVariable = "TEST_EVIDENCE_DIR";
    private static readonly object Sync = new();

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var reportDirectory = GetReportDirectory();
        Directory.CreateDirectory(reportDirectory);

        var timestamp = DateTimeOffset.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        var reportPath = Path.Combine(reportDirectory, $"extent-report-{timestamp}.html");
        var evidenceDirectory = Path.Combine(reportDirectory, "Evidence", timestamp);
        Directory.CreateDirectory(evidenceDirectory);
        Environment.SetEnvironmentVariable(EvidenceDirectoryEnvironmentVariable, evidenceDirectory);

        var spark = new ExtentSparkReporter(reportPath);
        ConfigureReporter(spark);

        lock (Sync)
        {
            ExtentReportContext.ReportDirectory = reportDirectory;
            ExtentReportContext.EvidenceDirectory = evidenceDirectory;
            ExtentReportContext.Extent = new ExtentReports();
            ExtentReportContext.Extent.AttachReporter(spark);
            
            // Add system information
            AddSystemInformation(ExtentReportContext.Extent);
        }

        TestContext.Progress.WriteLine($"[ExtentReport] Writing report to: {reportPath}");
        TestContext.Progress.WriteLine($"[ExtentReport] Evidence directory: {evidenceDirectory}");
    }

    private static void ConfigureReporter(ExtentSparkReporter spark)
    {
        spark.Config.DocumentTitle = "UI Test Execution Report";
        spark.Config.ReportName = "Reqnroll + Playwright Test Results";
        spark.Config.Encoding = Encoding.UTF8.WebName;
        spark.Config.Theme = Theme.Dark;
    }

    private static void AddSystemInformation(ExtentReports extent)
    {
        extent.AddSystemInfo("Environment", "Test");
        extent.AddSystemInfo("Machine", Environment.MachineName);
        extent.AddSystemInfo("OS", Environment.OSVersion.ToString());
        extent.AddSystemInfo(".NET Version", Environment.Version.ToString());
        extent.AddSystemInfo("User", Environment.UserName);
        extent.AddSystemInfo("Execution Time", DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        lock (Sync)
        {
            ExtentReportContext.Extent?.Flush();
            Environment.SetEnvironmentVariable(EvidenceDirectoryEnvironmentVariable, null);
            if (!string.IsNullOrWhiteSpace(ExtentReportContext.ReportDirectory))
            {
                EnforceRetention(ExtentReportContext.ReportDirectory, MaxReportsToKeep);
            }
        }
    }

    private static void EnforceRetention(string reportDirectory, int maxToKeep)
    {
        if (!Directory.Exists(reportDirectory)) return;

        var reportFiles = new DirectoryInfo(reportDirectory)
            .GetFiles("extent-report-*.html", SearchOption.TopDirectoryOnly)
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .ToList();

        foreach (var oldReport in reportFiles.Skip(maxToKeep))
        {
            try
            {
                oldReport.Delete();
            }
            catch
            {
                // Keep execution resilient if cleanup fails.
            }
        }
    }

    private static string GetReportDirectory()
    {
        var overrideDir = Environment.GetEnvironmentVariable("TEST_EXTENT_REPORT_DIR");
        if (!string.IsNullOrWhiteSpace(overrideDir))
        {
            return Path.GetFullPath(overrideDir);
        }

        var current = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "AgenticTestFramework.sln")))
            {
                return Path.Combine(current.FullName, "artifacts", "TestResults", "ExtentReports");
            }

            current = current.Parent;
        }

        return Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "artifacts", "TestResults", "ExtentReports"));
    }
}

[Binding]
public sealed class ExtentReportScenarioHooks(
    ScenarioContext scenarioContext,
    FeatureContext featureContext)
{
    private static readonly AsyncLocal<ExtentTest?> CurrentScenarioTest = new();
    private static readonly AsyncLocal<ExtentTest?> CurrentStepTest = new();
    private static readonly ConcurrentDictionary<string, ExtentTest> FeatureNodes = new();

    [BeforeScenario(Order = 0)]
    public void BeforeScenario()
    {
        if (ExtentReportContext.Extent is null) return;

        // Get or create the feature node (top-level in the report)
        var featureNode = GetOrCreateFeatureNode();

        // Create scenario as a child node of the feature
        var scenarioNode = CreateScenarioNode(featureNode);

        CurrentScenarioTest.Value = scenarioNode;
    }

    private ExtentTest GetOrCreateFeatureNode()
    {
        var featureTitle = featureContext.FeatureInfo.Title;
        
        return FeatureNodes.GetOrAdd(
            featureTitle,
            _ => CreateFeatureNode(featureTitle));
    }

    private ExtentTest CreateFeatureNode(string featureTitle)
    {
        var featureNode = ExtentReportContext.Extent!.CreateTest(featureTitle);
        
        // Add feature-level tags
        foreach (var tag in featureContext.FeatureInfo.Tags)
        {
            featureNode.AssignCategory(tag);
        }

        // Add feature description if available
        if (!string.IsNullOrWhiteSpace(featureContext.FeatureInfo.Description))
        {
            featureNode.Info($"<b>Feature Description:</b><br/>{featureContext.FeatureInfo.Description}");
        }

        return featureNode;
    }

    private ExtentTest CreateScenarioNode(ExtentTest featureNode)
    {
        var scenarioNode = featureNode.CreateNode(scenarioContext.ScenarioInfo.Title);
        
        // Add scenario-level tags
        foreach (var tag in scenarioContext.ScenarioInfo.Tags)
        {
            scenarioNode.AssignCategory(tag);
        }

        // Add scenario description if available
        if (!string.IsNullOrWhiteSpace(scenarioContext.ScenarioInfo.Description))
        {
            scenarioNode.Info($"<i>{scenarioContext.ScenarioInfo.Description}</i>");
        }

        return scenarioNode;
    }

    [AfterScenario(Order = int.MaxValue)]
    public void AfterScenario()
    {
        var scenarioNode = CurrentScenarioTest.Value;
        if (scenarioNode is null) return;

        // Mark scenario status
        if (scenarioContext.TestError is null)
        {
            scenarioNode.Pass("✓ Scenario completed successfully");
        }
        else
        {
            scenarioNode.Fail(scenarioContext.TestError);
            AttachFailureArtifacts(scenarioNode, scenarioContext.ScenarioInfo.Title);
        }

        // Clean up
        CurrentScenarioTest.Value = null;
    }

    [BeforeStep(Order = 0)]
    public void BeforeStep()
    {
        var scenarioNode = CurrentScenarioTest.Value;
        if (scenarioNode is null) return;

        var (keyword, text) = GetCurrentStepDetails();
        var stepDisplayName = $"<b>{keyword}</b> {text}";
        CurrentStepTest.Value = scenarioNode.CreateNode(stepDisplayName);
    }

    [AfterStep(Order = int.MaxValue)]
    public void AfterStep()
    {
        var stepNode = CurrentStepTest.Value;
        if (stepNode is null) return;

        if (scenarioContext.TestError is null)
        {
            stepNode.Pass("✓ Passed");
        }
        else
        {
            stepNode.Fail($"✗ Failed: {scenarioContext.TestError.Message}");
        }

        // Clean up
        CurrentStepTest.Value = null;
    }

    private (string Keyword, string Text) GetCurrentStepDetails()
    {
        try
        {
            var stepInfo = scenarioContext.StepContext.StepInfo;
            var text = stepInfo.Text;
            var keyword = stepInfo.StepDefinitionType.ToString();

            if (!string.IsNullOrWhiteSpace(keyword) && !string.IsNullOrWhiteSpace(text))
            {
                return (keyword, text);
            }
        }
        catch
        {
            // Fallback if step metadata is unavailable
        }

        return ("Step", "Unknown");
    }

    private static void AttachFailureArtifacts(ExtentTest scenarioNode, string scenarioTitle)
    {
        if (string.IsNullOrWhiteSpace(ExtentReportContext.EvidenceDirectory))
        {
            return;
        }

        var safeName = string.Concat(scenarioTitle.Select(character =>
            Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));

        // Attach screenshot
        var screenshotPath = Path.Combine(ExtentReportContext.EvidenceDirectory, $"{safeName}.png");
        if (File.Exists(screenshotPath))
        {
            scenarioNode.AddScreenCaptureFromPath(screenshotPath, "📸 Failure Screenshot");
        }

        // Attach trace
        var tracePath = Path.Combine(ExtentReportContext.EvidenceDirectory, $"{safeName}-trace.zip");
        if (File.Exists(tracePath))
        {
            scenarioNode.Info($"🔍 <b>Trace File:</b> <a href='{tracePath}'>Download Trace</a>");
        }

        // Attach console logs
        var consolePath = Path.Combine(ExtentReportContext.EvidenceDirectory, $"{safeName}-browser-console.json");
        if (File.Exists(consolePath))
        {
            scenarioNode.Info($"📋 <b>Browser Console:</b> <a href='{consolePath}'>View Console Logs</a>");
        }
    }

    /// <summary>
    /// Logs additional information to the current step node.
    /// Call this from your step definitions to add custom logging.
    /// </summary>
    public static void LogToCurrentStep(string message, Status status = Status.Info)
    {
        var stepNode = CurrentStepTest.Value;
        if (stepNode is null) return;

        switch (status)
        {
            case Status.Pass:
                stepNode.Pass(message);
                break;
            case Status.Fail:
                stepNode.Fail(message);
                break;
            case Status.Warning:
                stepNode.Warning(message);
                break;
            case Status.Info:
            default:
                stepNode.Info(message);
                break;
        }
    }

    /// <summary>
    /// Logs additional information to the current scenario node.
    /// Call this from your step definitions to add scenario-level logging.
    /// </summary>
    public static void LogToCurrentScenario(string message, Status status = Status.Info)
    {
        var scenarioNode = CurrentScenarioTest.Value;
        if (scenarioNode is null) return;

        switch (status)
        {
            case Status.Pass:
                scenarioNode.Pass(message);
                break;
            case Status.Fail:
                scenarioNode.Fail(message);
                break;
            case Status.Warning:
                scenarioNode.Warning(message);
                break;
            case Status.Info:
            default:
                scenarioNode.Info(message);
                break;
        }
    }
}

internal static class ExtentReportContext
{
    public static ExtentReports? Extent { get; set; }
    public static string? ReportDirectory { get; set; }
    public static string? EvidenceDirectory { get; set; }
}
