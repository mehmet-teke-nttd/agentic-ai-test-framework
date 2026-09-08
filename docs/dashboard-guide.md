# 📊 Test Results Dashboard

## Overview

The Test Results Dashboard is a **Blazor Server** web application that provides real-time visualization and analysis of your test results. It automatically reads test execution data from the `Evidence` folder and presents it in an intuitive, interactive interface.

![Dashboard Preview](dashboard-preview.png)

## Features

### 🎯 Dashboard Home
- **Real-time Metrics**: View total runs, pass rates, failure counts, and average durations
- **Recent Activity**: 24-hour and 7-day trends
- **Quick Navigation**: Fast access to detailed views
- **Test Run Overview**: See recent test executions at a glance

### 📋 Test Run History
- **Complete History**: Browse all past test executions
- **Detailed Results**: Drill down into individual test runs
- **Failure Details**: View error messages, stack traces, and evidence
- **Filtering & Sorting**: Find specific test runs quickly

### ⚠️ Flaky Test Detection
- **Automatic Detection**: Identifies tests with inconsistent behavior
- **Classification Analysis**: Shows varying failure types over time
- **Historical Context**: 30-day failure history per test
- **Actionable Recommendations**: Suggestions for improving test stability

### 📈 Test Trends
- **30-Day Analysis**: Daily aggregated test results
- **Pass Rate Trends**: Visualize testing health over time
- **Comparative Analysis**: Day-over-day improvements or regressions
- **Progress Bars**: Easy-to-read visual indicators

### 🔍 LLM Analysis Integration
- Displays AI-powered failure classifications
- Shows confidence levels and reasoning
- Integrates with historical failure tracking
- Provides comparative analysis insights

## Getting Started

### Prerequisites
- .NET 8 SDK
- Windows, Linux, or macOS
- A modern web browser (Chrome, Firefox, Edge, Safari)

### Installation

1. **Navigate to the dashboard directory:**
   ```bash
   cd src/TestDashboard
   ```

2. **Build the project:**
   ```bash
   dotnet build
   ```

3. **Run the dashboard:**
   ```bash
   dotnet run
   ```

4. **Open your browser:**
   Navigate to `https://localhost:5001` (or the URL shown in console)

### Configuration

Edit `appsettings.json` to customize:

```json
{
  "Dashboard": {
    "EvidenceRoot": "../../Evidence",       // Path to test evidence
    "HistoryRoot": "../../Evidence/.history", // Path to failure history
    "RefreshIntervalSeconds": 30,           // Auto-refresh interval
    "MaxTestRuns": 100                      // Max runs to load
  }
}
```

## Usage

### Viewing Test Results

1. **Run your tests** using the agent gateway or directly:
   ```bash
   dotnet test tests/UI.Tests
   ```

2. **The dashboard automatically** picks up new results from the Evidence folder

3. **Navigate** through the dashboard:
   - **Home**: Overview and metrics
   - **Test Runs**: Detailed history
   - **Flaky Tests**: Stability issues
   - **Trends**: Long-term analysis

### Understanding Metrics

#### Pass Rate
```
Pass Rate = (Passed Tests / Total Tests) × 100
```
- **>= 90%**: Excellent (Green)
- **70-89%**: Needs Attention (Yellow)
- **< 70%**: Critical (Red)

#### Flaky Test Detection
A test is considered flaky when it shows:
- Multiple failures in the last 30 days
- At least 2 different failure classifications
- Varying outcomes across test runs

#### Test Trends
- **↑ Green Arrow**: Pass rate improved
- **↓ Red Arrow**: Pass rate declined
- **→ Gray Arrow**: No significant change

## Architecture

### Components

```
TestDashboard/
├── Pages/                 # Razor pages
│   ├── Index.razor       # Dashboard home
│   ├── TestRuns.razor    # Test history & details
│   ├── FlakyTests.razor  # Flaky test detection
│   └── Trends.razor      # Trend analysis
├── Services/             # Business logic
│   └── TestResultsService.cs  # Data reading & analysis
├── Shared/               # Layout components
│   ├── MainLayout.razor
│   └── NavMenu.razor
└── wwwroot/             # Static assets
    └── css/site.css
```

### Data Flow

```
[Evidence Folder]
      ↓
[TestResultsService]
      ↓
[Blazor Components]
      ↓
[User Browser]
```

### Service Layer

**`ITestResultsService`** provides:
- `GetRecentTestRunsAsync()` - Fetch test run summaries
- `GetTestRunResultAsync()` - Get detailed test results
- `GetTestEvidenceAsync()` - Load test evidence (screenshots, logs)
- `GetDashboardMetricsAsync()` - Calculate aggregated metrics
- `GetFlakyTestsAsync()` - Detect unstable tests
- `GetTestTrendsAsync()` - Analyze trends over time

## Advanced Features

### Real-Time Updates

The dashboard can automatically refresh data. To enable polling:

```razor
@code {
    private System.Threading.Timer? _timer;

    protected override void OnInitialized()
    {
        _timer = new Timer(async _ =>
        {
            await LoadData();
            await InvokeAsync(StateHasChanged);
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }
}
```

### Custom Filtering

Extend `TestResultsService` to add filtering:

```csharp
public async Task<List<TestRunSummary>> GetTestRunsByProjectAsync(string projectId)
{
    var allRuns = await GetRecentTestRunsAsync();
    return allRuns.Where(r => r.RunId.Contains(projectId)).ToList();
}
```

### Exporting Data

Add export capabilities:

```csharp
public async Task<byte[]> ExportToCsvAsync()
{
    var runs = await GetRecentTestRunsAsync();
    var csv = new StringBuilder();
    csv.AppendLine("RunId,Outcome,StartedAt,Duration,TotalTests,Passed,Failed");
    
    foreach (var run in runs)
    {
        csv.AppendLine($"{run.RunId},{run.Outcome},{run.StartedAt}," +
                      $"{run.Duration},{run.TotalTests},{run.PassedTests},{run.FailedTests}");
    }
    
    return Encoding.UTF8.GetBytes(csv.ToString());
}
```

## Troubleshooting

### Dashboard Won't Start

**Error:** `Unable to bind to https://localhost:5001`

**Solution:** Port already in use. Change in `Properties/launchSettings.json`:
```json
{
  "applicationUrl": "https://localhost:5002;http://localhost:5003"
}
```

### No Test Data Appears

**Problem:** Dashboard shows "No test runs found"

**Solutions:**
1. Verify `EvidenceRoot` path in `appsettings.json`
2. Check that test runs have completed and generated results
3. Ensure `test-result.json` files exist in evidence folders
4. Run a test to generate sample data

### Flaky Tests Not Detected

**Problem:** Flaky tests page shows "No flaky tests detected"

**Solutions:**
1. Check that `HistoryRoot` path is correct
2. Run tests multiple times to build history
3. Ensure LLM analysis is enabled (generates richer data)
4. History requires at least 3 failures per test

### Performance Issues

**Problem:** Dashboard loads slowly

**Solutions:**
1. Reduce `MaxTestRuns` in configuration
2. Archive old evidence folders
3. Implement pagination in `TestResultsService`
4. Add caching for frequently accessed data

## Integration

### CI/CD Pipeline

Add dashboard deployment to your pipeline:

```yaml
# GitHub Actions example
- name: Deploy Dashboard
  run: |
    cd src/TestDashboard
    dotnet publish -c Release -o ./publish
    # Deploy to your hosting service
```

### Docker Deployment

Create `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/TestDashboard/TestDashboard.csproj", "TestDashboard/"]
RUN dotnet restore "TestDashboard/TestDashboard.csproj"
COPY . .
WORKDIR "/src/TestDashboard"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestDashboard.dll"]
```

Build and run:
```bash
docker build -t test-dashboard .
docker run -p 8080:80 -v ./Evidence:/app/Evidence test-dashboard
```

### Azure Deployment

Deploy to Azure App Service:

```bash
az webapp create --name my-test-dashboard --resource-group myResourceGroup
az webapp deployment source config-local-git --name my-test-dashboard
git remote add azure <deployment-url>
git push azure main
```

## Best Practices

### 1. Regular Cleanup
Archive old evidence to maintain performance:
```bash
# Keep last 30 days
find Evidence -type d -mtime +30 -exec rm -rf {} \;
```

### 2. Evidence Organization
Maintain consistent folder structure:
```
Evidence/
├── <runId>/
│   ├── test-result.json
│   ├── manifest.json
│   └── [screenshots, logs...]
└── .history/
    └── <testId>.json
```

### 3. Monitoring
Set up alerts for critical metrics:
- Pass rate drops below 70%
- Flaky test count increases
- Test duration spikes

### 4. Security
- Use HTTPS in production
- Implement authentication if exposed externally
- Restrict file system access
- Sanitize user inputs

## Customization

### Adding New Pages

Create `Pages/MyPage.razor`:

```razor
@page "/my-page"
@using TestDashboard.Services
@inject ITestResultsService TestResultsService

<h1>My Custom Page</h1>

@code {
    protected override async Task OnInitializedAsync()
    {
        // Your logic here
    }
}
```

Update `NavMenu.razor`:

```razor
<div class="nav-item px-3">
    <NavLink class="nav-link" href="my-page">
        <span class="oi oi-star" aria-hidden="true"></span> My Page
    </NavLink>
</div>
```

### Styling

Modify `wwwroot/css/site.css` or add custom CSS:

```css
.custom-card {
    border-left: 4px solid #0071c1;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}
```

### Adding Charts

Integrate Chart.js or similar libraries:

```html
<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
```

```razor
<canvas id="myChart"></canvas>

@code {
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JSRuntime.InvokeVoidAsync("initChart", "myChart", chartData);
        }
    }
}
```

## Performance Optimization

### Caching
Add memory caching:

```csharp
services.AddMemoryCache();

public class CachedTestResultsService : ITestResultsService
{
    private readonly IMemoryCache _cache;
    private readonly TestResultsService _inner;

    public async Task<DashboardMetrics> GetDashboardMetricsAsync()
    {
        return await _cache.GetOrCreateAsync("metrics", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await _inner.GetDashboardMetricsAsync();
        });
    }
}
```

### Pagination
Implement paging for large datasets:

```csharp
public async Task<PagedResult<TestRunSummary>> GetTestRunsPagedAsync(int page, int pageSize)
{
    var allRuns = await GetRecentTestRunsAsync();
    var pagedRuns = allRuns.Skip((page - 1) * pageSize).Take(pageSize).ToList();
    
    return new PagedResult<TestRunSummary>
    {
        Items = pagedRuns,
        TotalCount = allRuns.Count,
        Page = page,
        PageSize = pageSize
    };
}
```

## FAQ

**Q: Can I host this on IIS?**
A: Yes, publish the app and configure IIS to host .NET 8 applications.

**Q: Does it support multiple projects?**
A: Yes, the service reads all test runs from the Evidence folder regardless of project.

**Q: Can I customize the refresh interval?**
A: Yes, modify `RefreshIntervalSeconds` in `appsettings.json`.

**Q: How do I add authentication?**
A: Integrate ASP.NET Core Identity or Azure AD. See [Microsoft docs](https://learn.microsoft.com/en-us/aspnet/core/security/).

**Q: Can I export reports?**
A: Not built-in, but you can extend the service to generate CSV/PDF exports.

## Support & Contributing

- **Issues**: Report bugs in the project repository
- **Feature Requests**: Open a discussion or issue
- **Documentation**: Help improve these docs with PRs

## Related Documentation

- [Agent Protocol](../docs/agent-protocol.md) - Gateway API
- [LLM Analysis Guide](../docs/llm-failure-analysis-guide.md) - AI-powered analysis
- [Test Layers Guide](../docs/test-layers-guide.md) - Testing strategy
- [Architecture](../docs/architecture.md) - System design

---

**Dashboard Version:** 1.0.0  
**Last Updated:** September 2026  
**Framework:** .NET 8, Blazor Server
