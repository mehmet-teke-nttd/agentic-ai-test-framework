using Microsoft.Playwright;

namespace UI.Tests.PageObjects;

/// <summary>
/// Page Object for the DemoApps Button Interaction feature
/// Encapsulates all interactions with button elements
/// </summary>
public sealed class DemoButtonInteractionPage(IPage page)
{
    // Selectors
    private const string UITestingConceptsLinkSelector = "text=UI Testing Concepts";
    private const string ButtonMenuItemSelector = "//section[@class='poppins text-[14px]'] [text()='Button']";
    private const string YesButtonSelector = "button:has-text('Yes')";
    private const string ConfirmationTextSelector = ".confirmation-message, [class*='result'], [id*='result']";
    
    // Navigation
    public async Task NavigateToHomepageAsync(string applicationUrl)
    {
        await page.GotoAsync(applicationUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task NavigateToButtonTestingPageAsync(string applicationUrl)
    {
        await page.GotoAsync($"{applicationUrl}ui-testing-concepts/button");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task NavigateToUITestingConceptsAsync()
    {
        await page.ClickAsync(UITestingConceptsLinkSelector);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task SelectButtonFromMenuAsync()
    {
        await page.ClickAsync(ButtonMenuItemSelector);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    // Interactions
    public async Task ClickYesButtonAsync()
    {
        var yesButton = page.Locator(YesButtonSelector).First;
        await yesButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await yesButton.ClickAsync();
    }

    // Assertions
    public Task<bool> IsButtonPageDisplayedAsync()
    {
        var url = page.Url;
        return Task.FromResult(url.Contains("button", StringComparison.OrdinalIgnoreCase));
    }

    public async Task<string?> GetConfirmationTextAsync()
    {
        try
        {
            // Try multiple common patterns for result/confirmation text
            var locators = new[]
            {
                page.Locator("text=/You selected.*Yes/i").First,
                page.Locator(ConfirmationTextSelector).First,
                page.Locator("[class*='confirmation']").First
            };

            foreach (var locator in locators)
            {
                try
                {
                    await locator.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 2000 });
                    return await locator.TextContentAsync();
                }
                catch
                {
                    // Try next locator
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Could not find confirmation text on page '{page.Url}'", ex);
        }
    }

    public async Task VerifyConfirmationTextAsync(string expectedText)
    {
        var confirmationText = await GetConfirmationTextAsync();
        
        if (string.IsNullOrEmpty(confirmationText))
        {
            throw new InvalidOperationException(
                $"Confirmation text not found on page '{page.Url}'");
        }

        if (!confirmationText.Contains(expectedText, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Expected confirmation text to contain '{expectedText}' but found '{confirmationText}'");
        }
    }
}
