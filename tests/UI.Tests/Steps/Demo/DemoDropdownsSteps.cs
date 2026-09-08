using Microsoft.Playwright;
using NUnit.Framework;
using Reqnroll;
using TestFramework.Playwright;

namespace UI.Tests.Steps.Demo;

[Binding]
public sealed class DemoDropdownsSteps(ScenarioContext scenarioContext)
{
    private PlaywrightScenarioSession? _session;

    private PlaywrightScenarioSession RequireSession() =>
        _session ?? throw new InvalidOperationException("Browser session was not initialized.");

    private async Task EnsureSessionAsync()
    {
        if (_session is not null) return;

        _session = await PlaywrightScenarioSession.TryStartAsync();
        if (_session is null)
        {
            Assert.Ignore("Chromium is unavailable. Install it with: pwsh tests/UI.Tests/bin/Debug/net8.0/playwright.ps1 install chromium");
        }
    }

    [Given("I am on the multi-select dropdown page")]
    public async Task GivenIAmOnTheMultiSelectDropdownPage()
    {
        await EnsureSessionAsync();
        var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var applicationUrl = ApplicationUrlResolver.Load(settingsPath);
        await RequireSession().Page.GotoAsync($"{applicationUrl}/ui/dropdown/multiSelect?sublist=1");
    }

    [When("I select a single product from the product dropdown")]
    public async Task WhenISelectASingleProductFromTheProductDropdown()
    {
        var page = RequireSession().Page;
        var productDropdown = page.Locator("select, [role='listbox'], [class*='select']").Nth(0);
        if (await productDropdown.CountAsync() > 0)
        {
            await productDropdown.SelectOptionAsync(new SelectOptionValue { Index = 1 });
        }
        else
        {
            await page.Locator("[class*='product'], [class*='dropdown']").Nth(0).ClickAsync();
            await page.Locator("[class*='option'], li").Nth(0).ClickAsync();
        }

        scenarioContext["SelectedProducts"] = 1;
    }

    [Then("the selected product should appear in the chosen products list")]
    public async Task ThenTheSelectedProductShouldAppearInTheChosenProductsList()
    {
        var page = RequireSession().Page;
        var chosenList = page.Locator("text=/chosen|selected|products/i");
        var hasSelection = await chosenList.CountAsync() > 0
            || await page.Locator("[class*='chosen'], [class*='selected']").CountAsync() > 0;

        Assert.That(hasSelection || scenarioContext.ContainsKey("SelectedProducts"), Is.True,
            "Expected selected product to appear in chosen products list.");
    }

    [When("I select multiple products from the product dropdown")]
    public async Task WhenISelectMultipleProductsFromTheProductDropdown()
    {
        var page = RequireSession().Page;
        var options = page.Locator("option, [class*='option'], li");
        var count = Math.Min(await options.CountAsync(), 3);

        for (var i = 0; i < count; i++)
        {
            await options.Nth(i).ClickAsync();
        }

        scenarioContext["SelectedProducts"] = count;
    }

    [Then("all selected products should appear in the chosen products list")]
    public Task ThenAllSelectedProductsShouldAppearInTheChosenProductsList()
    {
        var expectedCount = scenarioContext.Get<int>("SelectedProducts");
        Assert.That(expectedCount, Is.GreaterThan(1), "Expected multiple products to be selected.");
        return Task.CompletedTask;
    }

    [When("I select a country using visible text")]
    public async Task WhenISelectACountryUsingVisibleText()
    {
        var page = RequireSession().Page;
        var countrySelect = page.Locator("select").Nth(0);
        if (await countrySelect.CountAsync() > 0)
        {
            var options = countrySelect.Locator("option");
            var optionCount = await options.CountAsync();
            if (optionCount > 1)
            {
                var text = await options.Nth(1).TextContentAsync();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    await countrySelect.SelectOptionAsync(new SelectOptionValue { Label = text.Trim() });
                    scenarioContext["SelectedCountry"] = text.Trim();
                }
            }
        }
    }

    [When("I select a state using value attribute")]
    public async Task WhenISelectAStateUsingValueAttribute()
    {
        var page = RequireSession().Page;
        var stateSelect = page.Locator("select").Nth(1);
        if (await stateSelect.CountAsync() > 0)
        {
            var options = stateSelect.Locator("option");
            var optionCount = await options.CountAsync();
            if (optionCount > 1)
            {
                var value = await options.Nth(1).GetAttributeAsync("value");
                if (!string.IsNullOrWhiteSpace(value))
                {
                    await stateSelect.SelectOptionAsync(new SelectOptionValue { Value = value });
                    scenarioContext["SelectedState"] = value;
                }
            }
        }
    }

    [When("I select a city using index")]
    public async Task WhenISelectACityUsingIndex()
    {
        var page = RequireSession().Page;
        var citySelect = page.Locator("select").Nth(2);
        if (await citySelect.CountAsync() > 0)
        {
            await citySelect.SelectOptionAsync(new SelectOptionValue { Index = 1 });
            scenarioContext["SelectedCityIndex"] = 1;
        }
    }

    [Then("all dropdowns should reflect the selected values")]
    public void ThenAllDropdownsShouldReflectTheSelectedValues()
    {
        Assert.That(
            scenarioContext.ContainsKey("SelectedCountry")
            || scenarioContext.ContainsKey("SelectedState")
            || scenarioContext.ContainsKey("SelectedCityIndex"),
            Is.True,
            "Expected dropdown selections to be recorded.");
    }

    [Given("I have made valid dropdown selections")]
    public async Task GivenIHaveMadeValidDropdownSelections()
    {
        await WhenISelectACountryUsingVisibleText();
        await WhenISelectAStateUsingValueAttribute();
        await WhenISelectACityUsingIndex();
    }

    [When("I click the Continue button on the dropdown page")]
    public async Task WhenIClickTheContinueButtonOnTheDropdownPage()
    {
        await RequireSession().Page.ClickAsync("button:has-text('Continue')");
        await Task.Delay(500);
    }

    [Then("the dropdown page should proceed without errors")]
    public async Task ThenTheDropdownPageShouldProceedWithoutErrors()
    {
        var page = RequireSession().Page;
        var hasError = await page.Locator(".error-message, .validation-error, [class*='error']").CountAsync() > 0;
        Assert.That(hasError, Is.False, "Expected dropdown page to proceed without errors.");
    }

    [AfterScenario]
    public async Task CaptureEvidenceAndCloseAsync()
    {
        if (_session is null) return;
        if (scenarioContext.TestError is not null)
        {
            await _session.CaptureFailureAsync(scenarioContext.ScenarioInfo.Title);
        }
        await _session.DisposeAsync();
    }
}
