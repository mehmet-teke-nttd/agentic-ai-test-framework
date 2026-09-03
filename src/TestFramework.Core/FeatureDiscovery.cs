using System.Text.RegularExpressions;
using Test.Agent.Contracts;

namespace TestFramework.Core;

public sealed class FeatureFileDiscovery(string repositoryRoot) : ITestDiscovery
{
    private static readonly Regex TagRegex = new(@"@(?<tag>[^\s@]+)", RegexOptions.Compiled);
    private readonly string _repositoryRoot = Path.GetFullPath(repositoryRoot);

    public async Task<IReadOnlyList<TestMetadata>> DiscoverAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<TestMetadata>();
        foreach (var file in Directory.EnumerateFiles(_repositoryRoot, "*.feature", SearchOption.AllDirectories)
                     .Where(path => !HasIgnoredSegment(path))
                     .Order(StringComparer.OrdinalIgnoreCase))
        {
            results.AddRange(await ParseAsync(file, cancellationToken));
        }

        return results;
    }

    private async Task<IReadOnlyList<TestMetadata>> ParseAsync(string path, CancellationToken cancellationToken)
    {
        var lines = await File.ReadAllLinesAsync(path, cancellationToken);
        var featureName = Path.GetFileNameWithoutExtension(path);
        var featureTags = new List<string>();
        var pendingTags = new List<string>();
        var results = new List<TestMetadata>();
        var featureSeen = false;

        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index].Trim();
            if (line.StartsWith('@'))
            {
                pendingTags.AddRange(TagRegex.Matches(line).Select(match => match.Groups["tag"].Value));
                continue;
            }

            if (line.StartsWith("Feature:", StringComparison.OrdinalIgnoreCase))
            {
                featureName = ValueAfterColon(line) ?? featureName;
                featureTags.AddRange(pendingTags);
                pendingTags.Clear();
                featureSeen = true;
                continue;
            }

            if (!line.StartsWith("Scenario:", StringComparison.OrdinalIgnoreCase) &&
                !line.StartsWith("Scenario Outline:", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var scenarioName = ValueAfterColon(line) ?? $"Unnamed scenario at line {index + 1}";
            var tags = featureTags.Concat(pendingTags).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            pendingTags.Clear();
            results.Add(CreateMetadata(path, index + 1, featureName, scenarioName, tags, featureSeen));
        }

        return results;
    }

    private TestMetadata CreateMetadata(
        string path, int line, string featureName, string scenarioName, string[] tags, bool featureSeen)
    {
        var warnings = new List<string>();
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var tag in tags)
        {
            var separator = tag.IndexOf(':');
            if (separator < 1)
            {
                continue;
            }

            var key = tag[..separator];
            var value = tag[(separator + 1)..];
            if (string.IsNullOrWhiteSpace(value))
            {
                warnings.Add($"Metadata tag '{tag}' has no value.");
            }
            else if (values.ContainsKey(key))
            {
                warnings.Add($"Scenario metadata '{key}' overrides an earlier value.");
                values[key] = value;
            }
            else
            {
                values.Add(key, value);
            }
        }

        if (!featureSeen) warnings.Add("Feature declaration is missing; filename used.");
        if (!values.ContainsKey("Category")) warnings.Add("Category metadata is missing.");
        if (!values.ContainsKey("Risk")) warnings.Add("Risk metadata is missing.");
        if (!values.ContainsKey("Layer")) warnings.Add("Layer metadata is missing.");
        if (!values.ContainsKey("Requirement")) warnings.Add("Requirement metadata is missing.");

        var risk = ParseEnum<RiskLevel>(values, "Risk", warnings);
        var layer = ParseEnum<TestLayer>(values, "Layer", warnings);
        var relativePath = Path.GetRelativePath(_repositoryRoot, path).Replace('\\', '/');

        return new TestMetadata
        {
            TestId = $"{featureName}::{scenarioName}",
            Name = scenarioName,
            Feature = values.GetValueOrDefault("Feature", featureName),
            Category = values.GetValueOrDefault("Category"),
            Risk = risk,
            Layer = layer,
            Requirement = values.GetValueOrDefault("Requirement"),
            Tags = tags,
            SourcePath = relativePath,
            SourceLine = line,
            Warnings = warnings
        };
    }

    private static T ParseEnum<T>(
        IReadOnlyDictionary<string, string> values, string key, ICollection<string> warnings) where T : struct, Enum
    {
        if (!values.TryGetValue(key, out var value))
        {
            return Enum.TryParse<T>("Unknown", out var unknown) ? unknown : default;
        }

        if (Enum.TryParse<T>(value, true, out var parsed))
        {
            return parsed;
        }

        warnings.Add($"Metadata '{key}' has unsupported value '{value}'.");
        return Enum.TryParse<T>("Unknown", out var fallback) ? fallback : default;
    }

    private static string? ValueAfterColon(string line)
    {
        var index = line.IndexOf(':');
        var value = index < 0 ? string.Empty : line[(index + 1)..].Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static bool HasIgnoredSegment(string path) =>
        path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(segment => segment is "bin" or "obj" or ".git");
}

public static class NUnitFilterBuilder
{
    private static readonly Regex SafeValue = new(@"^[\w .:/()\-]+$", RegexOptions.Compiled);

    public static string? Build(TestRunRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateValues(request.TestIds, nameof(request.TestIds));
        ValidateValues(request.Categories, nameof(request.Categories));

        var clauses = new List<string>();
        if (request.TestIds.Count > 0)
        {
            clauses.Add($"({string.Join("|", request.TestIds.Select(id => $"FullyQualifiedName={id}"))})");
        }

        if (request.Categories.Count > 0)
        {
            clauses.Add($"({string.Join("|", request.Categories.Select(category => $"TestCategory={category}"))})");
        }

        return clauses.Count == 0 ? null : string.Join("&", clauses);
    }

    private static void ValidateValues(IEnumerable<string> values, string parameter)
    {
        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 200 || !SafeValue.IsMatch(value))
            {
                throw new ArgumentException($"Unsafe or invalid filter value '{value}'.", parameter);
            }
        }
    }
}
