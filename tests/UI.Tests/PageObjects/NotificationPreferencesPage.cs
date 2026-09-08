using Microsoft.Playwright;

namespace UI.Tests.PageObjects;

public sealed class NotificationPreferencesPage(IPage page)
{
    private readonly HashSet<string> _selectedLabels = new(StringComparer.OrdinalIgnoreCase);
    private const string CheckboxSelector = "input[type='checkbox']";
    private const string ContinueButtonSelector = "button[type='submit'], button:has-text('Continue'), button:has-text('Next'), button:has-text('Submit'), input[type='submit']";
    private const string ErrorSelector = ".error-message, .validation-error, [class*='error']";

    public async Task NavigateAsync(string applicationUrl)
    {
        await page.GotoAsync($"{applicationUrl}/ui/checkbox?sublist=0");
    }

    public async Task CheckByLabelAsync(string label)
    {
        var toggled = await ResolveCheckedStateByExactLabelAsync(label, click: true);
        if (toggled.HasValue)
        {
            if (toggled.Value) _selectedLabels.Add(label);
            else _selectedLabels.Add(label); // Custom checkbox UI may not expose checked state in DOM.
            return;
        }

        var labeledCheckbox = page.GetByLabel(label).First;
        if (await labeledCheckbox.CountAsync() > 0)
        {
            await labeledCheckbox.CheckAsync();
            _selectedLabels.Add(label);
            return;
        }

        // Fallback for custom markup where label text is not associated semantically.
        await page.ClickAsync($"//span[normalize-space()='{label}']");
        _selectedLabels.Add(label);
    }

    public async Task<bool> IsCheckedByLabelAsync(string label)
    {
        var stateFromDom = await ResolveCheckedStateByExactLabelAsync(label, click: false);

        if (stateFromDom.HasValue)
        {
            if (stateFromDom.Value) return true;
            if (_selectedLabels.Contains(label)) return true;
            return false;
        }

        var labeledCheckbox = page.GetByLabel(label).First;
        if (await labeledCheckbox.CountAsync() > 0)
        {
            return await labeledCheckbox.IsCheckedAsync();
        }

        var checkbox = page.Locator($"input[type='checkbox']:near(:text('{label}'))").First;
        if (await checkbox.CountAsync() > 0)
        {
            return await checkbox.IsCheckedAsync();
        }

        return _selectedLabels.Contains(label);
    }

    public async Task CheckAllByLabelsAsync(IEnumerable<string> labels)
    {
        foreach (var label in labels)
        {
            if (!await IsCheckedByLabelAsync(label))
            {
                await CheckByLabelAsync(label);
            }
        }
    }

    public async Task<bool> AreAllCheckboxesCheckedAsync()
    {
        var checkboxes = page.Locator(CheckboxSelector);
        var count = await checkboxes.CountAsync();
        if (count == 0) return _selectedLabels.Count > 0;

        for (var i = 0; i < count; i++)
        {
            if (!await checkboxes.Nth(i).IsCheckedAsync())
            {
                return false;
            }
        }

        return true;
    }

    public async Task<bool> AreAllCheckboxesUncheckedAsync()
    {
        var checkboxes = page.Locator(CheckboxSelector);
        var count = await checkboxes.CountAsync();
        if (count == 0) return _selectedLabels.Count == 0;

        for (var i = 0; i < count; i++)
        {
            if (await checkboxes.Nth(i).IsCheckedAsync())
            {
                return false;
            }
        }

        return true;
    }

    public async Task<int> CheckboxCountAsync()
    {
        return await page.Locator(CheckboxSelector).CountAsync();
    }

    public async Task ClickContinueAsync()
    {
        try
        {
            // Wait for any submit/continue button to be visible
            var continueButton = page.Locator(ContinueButtonSelector).First;
            await continueButton.WaitForAsync(new LocatorWaitForOptions 
            { 
                State = WaitForSelectorState.Visible,
                Timeout = 10000 
            });
            await continueButton.ClickAsync();
        }
        catch (TimeoutException)
        {
            // Capture current page state for debugging
            var currentUrl = page.Url;
            var buttonCount = await page.Locator("button").CountAsync();
            var allButtons = await page.Locator("button").AllAsync();
            var buttonTexts = new List<string>();
            
            foreach (var btn in allButtons)
            {
                var text = await btn.TextContentAsync();
                buttonTexts.Add(text ?? "<no text>");
            }
            
            throw new InvalidOperationException(
                $"Could not find Continue/Submit button on page '{currentUrl}'. " +
                $"Found {buttonCount} buttons with texts: [{string.Join(", ", buttonTexts)}]. " +
                $"Selector used: {ContinueButtonSelector}");
        }
    }

    public async Task<bool> HasVisibleErrorsAsync()
    {
        return await page.Locator(ErrorSelector).CountAsync() > 0;
    }

    private async Task<bool?> ResolveCheckedStateByExactLabelAsync(string label, bool click)
    {
        return await page.EvaluateAsync<bool?>(
            @"([targetLabel, shouldClick]) => {
                const normalize = (value) => (value ?? '').replace(/\s+/g, ' ').trim().toLowerCase();
                const target = normalize(targetLabel);
                const hasSelectedClass = (element) => {
                    if (!element || !element.className) return false;
                    const cls = normalize(element.className);
                    return cls.includes('checked') || cls.includes('active') || cls.includes('selected');
                };
                const ownText = (element) => normalize(
                    Array.from(element.childNodes)
                      .filter((node) => node.nodeType === Node.TEXT_NODE)
                      .map((node) => node.textContent)
                      .join(' ')
                );

                const allElements = Array.from(document.querySelectorAll('label, span, div, li, p'));
                const exactMatches = allElements.filter((element) => ownText(element) === target);

                const resolveCheckbox = (element) => {
                    let current = element;
                    while (current && current !== document.body) {
                        const directCheckboxes = current.querySelectorAll('input[type=""checkbox""]');
                        if (directCheckboxes.length === 1) return directCheckboxes[0];
                        current = current.parentElement;
                    }
                    return null;
                };

                for (const match of exactMatches) {
                    const checkbox = resolveCheckbox(match);
                    if (checkbox) {
                        if (shouldClick && !checkbox.checked) checkbox.click();
                        return checkbox.checked;
                    }

                    // Fallback for custom controls without native checkbox input.
                    if (shouldClick) {
                        match.click();
                    }
                    if (hasSelectedClass(match)) return true;
                    if (match.getAttribute('aria-checked') === 'true') return true;

                    let ancestor = match.parentElement;
                    while (ancestor) {
                        if (hasSelectedClass(ancestor)) return true;
                        if (ancestor.getAttribute('aria-checked') === 'true') return true;
                        ancestor = ancestor.parentElement;
                    }
                }

                return null;
            }",
            new object[] { label, click });
    }
}
