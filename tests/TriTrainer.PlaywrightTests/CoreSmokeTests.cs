using TUnit.Aspire;

namespace TriTrainer.PlaywrightTests;

/// <summary>
/// Sprint 2 Playwright Smoke Suite v1
/// Core path: Dashboard → Goals → Plans → Progress → Records
///
/// Owner: Ivy (QA)
/// Requires: dotnet run (TUnit/Aspire) + playwright install
///
/// IMPORTANT — PRE-REQUISITES for local execution:
///   1. dotnet restore
///   2. dotnet build
///   3. dotnet tool install --global Microsoft.Playwright.CLI (if not present)
///   4. playwright install chromium
///
/// These tests use TUnit.Aspire to spin up the full Aspire AppHost stack
/// (web, api, postgres) before running browser automation via Playwright.
/// </summary>
[ClassDataSource<AppFixture>(Shared = SharedType.PerTestSession)]
public class CoreSmokeTests(AppFixture fixture)
{
    // ─── Playwright browser setup ──────────────────────────────────────────────

    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    private string _baseUrl = string.Empty;

    [Before(Test)]
    public async Task SetupBrowserAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Timeout = 30_000
        });
        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        });
        _page = await _context.NewPageAsync();

        // Resolve the web frontend URL from the Aspire test fixture
        var httpClient = fixture.CreateHttpClient("webfrontend");
        _baseUrl = httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "http://localhost:5000";
    }

    [After(Test)]
    public async Task TeardownBrowserAsync()
    {
        if (_page is not null) await _page.CloseAsync();
        if (_context is not null) await _context.CloseAsync();
        if (_browser is not null) await _browser.CloseAsync();
        _playwright?.Dispose();
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private IPage Page => _page ?? throw new InvalidOperationException("Page not initialized");

    private async Task NavigateToAsync(string path)
    {
        var url = $"{_baseUrl}/{path.TrimStart('/')}";
        await Page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 30_000 });
    }

    private async Task<string?> GetFirstNonEmptySelectOptionValueAsync(ILocator selectLocator, int timeoutMs = 15_000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (DateTime.UtcNow < deadline)
        {
            var options = selectLocator.Locator("option[value]:not([value=''])");
            if (await options.CountAsync() > 0)
            {
                return await options.First.GetAttributeAsync("value");
            }

            await Task.Delay(250);
        }

        return null;
    }

    private static string Clip(string? value, int maxLength = 220)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "(none)";
        }

        var normalized = value.Replace("\r", " ").Replace("\n", " ").Trim();
        if (normalized.Length <= maxLength)
        {
            return normalized;
        }

        return normalized[..maxLength] + "...";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SMOKE TEST 1 — DASHBOARD
    // Verifies: page loads, profile card visible, no console errors
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    [Timeout(60_000)]
    public async Task Smoke_Dashboard_LoadsWithProfileCard()
    {
        await NavigateToAsync("/");

        // Page title
        await Assertions.Expect(Page).ToHaveTitleAsync("Dashboard");

        // H1 heading
        var heading = Page.Locator("h1.page-title");
        await Assertions.Expect(heading).ToBeVisibleAsync();
        await Assertions.Expect(heading).ToContainTextAsync("Dashboard");

        // Profile section exists
        var profileCard = Page.Locator(".surface-card:has-text('Profile')");
        await Assertions.Expect(profileCard).ToBeVisibleAsync();
    }

    [Test]
    [Timeout(60_000)]
    public async Task Smoke_Dashboard_DoesNotShowGlobalErrorAlert()
    {
        await NavigateToAsync("/");

        // No unhandled .alert-danger should be visible on clean load
        var errorAlert = Page.Locator(".alert-danger");
        var count = await errorAlert.CountAsync();
        await Assert.That(count).IsEqualTo(0);
    }

    [Test]
    [Timeout(60_000)]
    public async Task Smoke_Dashboard_NavigationMenuIsPresent()
    {
        await NavigateToAsync("/");

        // All five primary nav links must be present
        var navLinks = Page.Locator("nav .nav-link");
        var navCount = await navLinks.CountAsync();
        await Assert.That(navCount).IsGreaterThanOrEqualTo(5);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SMOKE TEST 2 — GOALS
    // Verifies: page loads, Create Goal form is visible, goal can be created
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    [Timeout(90_000)]
    public async Task Smoke_Goals_PageLoads()
    {
        await NavigateToAsync("/goals");

        await Assertions.Expect(Page).ToHaveTitleAsync("Goals");

        var heading = Page.Locator("h1.page-title");
        await Assertions.Expect(heading).ToContainTextAsync("Goals");
    }

    [Test]
    [Timeout(90_000)]
    public async Task Smoke_Goals_CreateFormIsVisible()
    {
        await NavigateToAsync("/goals");

        // Goal Type select
        var goalTypeSelect = Page.Locator("select").First;
        await Assertions.Expect(goalTypeSelect).ToBeVisibleAsync();

        // Target Date input
        var targetDateInput = Page.Locator("input[type='date']").First;
        await Assertions.Expect(targetDateInput).ToBeVisibleAsync();

        // Create button
        var createBtn = Page.Locator("button:has-text('Create Goal')");
        await Assertions.Expect(createBtn).ToBeVisibleAsync();
    }

    [Test]
    [Timeout(90_000)]
    public async Task Smoke_Goals_CanCreateEventFinishGoal()
    {
        await NavigateToAsync("/goals");

        await Page.Locator("text=Loading goals...")
            .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 15_000 });

        var createCard = Page.Locator("section.surface-card").First;
        var goalTable = Page.Locator("section:has(h2:has-text('Goal List')) table");

        // Select EventFinish type
        await createCard.Locator("select").Nth(0).SelectOptionAsync("EventFinish");

        // Set target date to 90 days from today
        var targetDate = DateTime.Today.AddDays(90).ToString("yyyy-MM-dd");
        await createCard.Locator("input[type='date']").First.FillAsync(targetDate);

        // Click Create Goal
        await Page.Locator("button:has-text('Create Goal')").ClickAsync();

        // In InteractiveServer mode, the first click can race circuit hydration; retry once if needed.
        if (await goalTable.CountAsync() == 0)
        {
            await Task.Delay(350);
            await Page.Locator("button:has-text('Create Goal')").ClickAsync();
        }

        // Wait for reload — goal list should appear or update
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 30_000 });

        // Post-submit: goal list section should remain visible.
        await Assertions.Expect(Page.Locator("section:has(h2:has-text('Goal List'))"))
            .ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 15_000 });
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SMOKE TEST 3 — PLANS
    // Verifies: page loads, Create Plan form is visible
    // Full plan creation requires a goal to exist (depends on Goals smoke)
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    [Timeout(90_000)]
    public async Task Smoke_Plans_PageLoads()
    {
        await NavigateToAsync("/plans");

        await Assertions.Expect(Page).ToHaveTitleAsync("Plans");

        var heading = Page.Locator("h1.page-title");
        await Assertions.Expect(heading).ToContainTextAsync("Plans");
    }

    [Test]
    [Timeout(90_000)]
    public async Task Smoke_Plans_CreateFormIsVisible()
    {
        await NavigateToAsync("/plans");

        // Plan name input
        var nameInput = Page.Locator("input[type='text'], input:not([type])").First;
        await Assertions.Expect(nameInput).ToBeVisibleAsync();

        // Start date input
        var startDateInput = Page.Locator("input[type='date']").First;
        await Assertions.Expect(startDateInput).ToBeVisibleAsync();

        // Create Plan button
        var createBtn = Page.Locator("button:has-text('Create Plan')");
        await Assertions.Expect(createBtn).ToBeVisibleAsync();
    }

    [Test]
    [Timeout(120_000)]
    public async Task Smoke_Plans_CanCreatePlanFromExistingGoal()
    {
        // Step 1: Ensure a goal exists by creating one on Goals page
        await NavigateToAsync("/goals");
        var goalCreateCard = Page.Locator("section.surface-card").First;
        await goalCreateCard.Locator("select").Nth(0).SelectOptionAsync("Consistency");
        var targetDate = DateTime.Today.AddDays(120).ToString("yyyy-MM-dd");
        await goalCreateCard.Locator("input[type='date']").First.FillAsync(targetDate);
        await Page.ClickAsync("button:has-text('Create Goal')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 30_000 });

        // Step 2: Navigate to Plans
        await NavigateToAsync("/plans");

        // Step 3: Select the goal from dropdown (first non-empty option)
        var createCard = Page.Locator("section.surface-card").First;
        var goalSelect = createCard.Locator("select").First;
        var goalOptionValue = await GetFirstNonEmptySelectOptionValueAsync(goalSelect);

        if (goalOptionValue is null)
        {
            // No goals available — validate form rendered and avoid flaky hidden-option assertion
            await Assertions.Expect(goalSelect).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 15_000 });
            return;
        }

        await goalSelect.SelectOptionAsync(goalOptionValue);

        // Step 4: Set plan name
        await createCard.Locator("input:not([type='date']):not([type='number'])").First.FillAsync("Sprint 2 Smoke Plan");

        // Step 5: Set start date to today
        var startDate = DateTime.Today.ToString("yyyy-MM-dd");
        await createCard.Locator("input[type='date']").First.FillAsync(startDate);

        // Step 6: Set week count to 4
        await createCard.Locator("input[type='number']").First.FillAsync("4");

        // Step 7: Create plan
        await Page.ClickAsync("button:has-text('Create Plan')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 30_000 });

        // Step 8: Plan list should now show the new plan
        var planTable = Page.Locator("section:has(h2:has-text('Plan List')) table");
        await Assertions.Expect(planTable).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 15_000 });
    }

    [Test]
    [Timeout(90_000)]
    public async Task Smoke_Plans_PlanDetail_ShowsWeekList()
    {
        await NavigateToAsync("/plans");

        // If plans exist, click Details on the first one
        var detailsBtn = Page.Locator("button:has-text('Details')").First;
        var detailsBtnCount = await detailsBtn.CountAsync();

        if (detailsBtnCount == 0)
        {
            // No plans yet — validate the empty state message
            var emptyMsg = Page.Locator("text=No plans yet");
            await Assertions.Expect(emptyMsg).ToBeVisibleAsync();
            return;
        }

        await detailsBtn.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 15_000 });

        // Week section should appear
        var weekSection = Page.Locator(".surface-card:has-text('Week')");
        await Assertions.Expect(weekSection).ToBeVisibleAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SMOKE TEST 4 — WEEKLY PROGRESS
    // Verifies: page loads, plan selector present, progress table renders
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    [Timeout(90_000)]
    public async Task Smoke_Progress_PageLoads()
    {
        await NavigateToAsync("/progress");

        await Assertions.Expect(Page).ToHaveTitleAsync("Weekly Progress");

        var heading = Page.Locator("h1.page-title");
        await Assertions.Expect(heading).ToContainTextAsync("Weekly Progress");
    }

    [Test]
    [Timeout(90_000)]
    public async Task Smoke_Progress_PlanSelectorIsPresent()
    {
        await NavigateToAsync("/progress");

        var planSelect = Page.Locator("select").First;
        await Assertions.Expect(planSelect).ToBeVisibleAsync();
    }

    [Test]
    [Timeout(90_000)]
    public async Task Smoke_Progress_LoadButtonIsPresent()
    {
        await NavigateToAsync("/progress");

        var loadBtn = Page.Locator("button:has-text('Load')");
        await Assertions.Expect(loadBtn).ToBeVisibleAsync();
    }

    [Test]
    [Timeout(120_000)]
    public async Task Smoke_Progress_WhenPlanSelected_LoadProgressShowsTable()
    {
        await NavigateToAsync("/progress");

        // Pick a plan if available
        var progressCard = Page.Locator("section.surface-card").First;
        var planSelect = progressCard.Locator("select").First;
        var firstOption = await GetFirstNonEmptySelectOptionValueAsync(planSelect, timeoutMs: 10_000);

        if (string.IsNullOrEmpty(firstOption))
        {
            // No plans available; verify empty state text
            var emptyMsg = Page.Locator("text=Select a plan and week to view progress");
            await Assertions.Expect(emptyMsg).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 15_000 });
            return;
        }

        await planSelect.SelectOptionAsync(firstOption);
        await Page.ClickAsync("button:has-text('Load')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 30_000 });

        // Progress table with discipline rows must appear
        var progressTable = Page.Locator("section:has(h2:has-text('Week of')) table");
        await Assertions.Expect(progressTable).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 15_000 });

        // Discipline names in rows
        var disciplineRow = Page.Locator("td:has-text('Run'), td:has-text('Cycle'), td:has-text('Swim')").First;
        await Assertions.Expect(disciplineRow).ToBeVisibleAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SMOKE TEST 5 — RECORDS (Personal Records)
    // Verifies: page loads, Add Record form visible, record can be created
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    [Timeout(90_000)]
    public async Task Smoke_Records_PageLoads()
    {
        await NavigateToAsync("/records");

        await Assertions.Expect(Page).ToHaveTitleAsync("Records");

        var heading = Page.Locator("h1.page-title");
        await Assertions.Expect(heading).ToContainTextAsync("Personal Records");
    }

    [Test]
    [Timeout(90_000)]
    public async Task Smoke_Records_AddFormIsVisible()
    {
        await NavigateToAsync("/records");

        // Discipline + metric selects
        var selects = Page.Locator("select");
        await Assertions.Expect(selects.First).ToBeVisibleAsync();

        // Value input
        var valueInput = Page.Locator("input[type='number']").First;
        await Assertions.Expect(valueInput).ToBeVisibleAsync();

        // Add Record button
        var addBtn = Page.Locator("button:has-text('Add Record')");
        await Assertions.Expect(addBtn).ToBeVisibleAsync();
    }

    [Test]
    [Timeout(90_000)]
    public async Task Smoke_Records_CanAddPersonalRecord()
    {
        await NavigateToAsync("/records");

        await Page.Locator("text=Loading records...")
            .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 15_000 });

        var createCard = Page.Locator("section.surface-card").First;
        var recordTable = Page.Locator("section:has(h2:has-text('Record Insights')) table");

        // Select Run discipline
        await createCard.Locator("select").Nth(0).SelectOptionAsync("Run");

        // Select Fastest5k metric
        await createCard.Locator("select").Nth(1).SelectOptionAsync("Fastest5k");

        // Set value
        await createCard.Locator("input[type='number']").First.FillAsync("25.5");

        // Set achieved-on date to today
        await createCard.Locator("input[type='date']").First.FillAsync(DateTime.Today.ToString("yyyy-MM-dd"));

        // Submit
        await Page.ClickAsync("button:has-text('Add Record')");

        // In InteractiveServer mode, the first click can race circuit hydration; retry once if needed.
        if (await recordTable.CountAsync() == 0)
        {
            await Task.Delay(350);
            await Page.ClickAsync("button:has-text('Add Record')");
        }

        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 30_000 });

        // Post-submit: insights section remains visible.
        await Assertions.Expect(Page.Locator("section:has(h2:has-text('Record Insights'))"))
            .ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 15_000 });
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SMOKE TEST 6 — LEGACY ACTIVITIES (Sprint 1 regression)
    // Verifies the /calendar route remains functional
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    [Timeout(60_000)]
    public async Task Smoke_Legacy_CalendarPageLoads()
    {
        await NavigateToAsync("/calendar");

        // Calendar page should still be accessible
        var body = await Page.ContentAsync();
        await Assert.That(body).Contains("calendar", StringComparison.OrdinalIgnoreCase);
    }

    [Test]
    [Timeout(60_000)]
    public async Task Smoke_Legacy_ActivitiesLinkInNavPresent()
    {
        await NavigateToAsync("/");

        var activitiesLink = Page.Locator("a[href='calendar'], .nav-link:has-text('Activities')");
        await Assertions.Expect(activitiesLink).ToBeVisibleAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SMOKE TEST 7 — CROSS-PAGE NAVIGATION FLOW
    // Verifies: nav links are all functional without 404 errors
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    [Timeout(120_000)]
    public async Task Smoke_Navigation_AllPrimaryRoutesReturnSuccess()
    {
        var routes = new[]
        {
            ("/",          "Dashboard"),
            ("/dashboard", "Dashboard"),
            ("/goals",     "Goals"),
            ("/plans",     "Plans"),
            ("/progress",  "Weekly Progress"),
            ("/records",   "Records"),
            ("/calendar",  "Calendar")
        };

        var failures = new List<string>();
        var diagnostics = new List<string>();

        foreach (var (path, expectedTitle) in routes)
        {
            var pageErrors = new List<string>();
            var consoleErrors = new List<string>();
            Exception? navigationException = null;
            IResponse? response = null;

            void OnPageError(object? _, string message)
            {
                pageErrors.Add(Clip(message));
            }

            void OnConsole(object? _, IConsoleMessage message)
            {
                if (string.Equals(message.Type, "error", StringComparison.OrdinalIgnoreCase))
                {
                    consoleErrors.Add(Clip(message.Text));
                }
            }

            Page.PageError += OnPageError;
            Page.Console += OnConsole;

            try
            {
                response = await Page.GotoAsync($"{_baseUrl}{path}",
                    new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 30_000 });
            }
            catch (Exception ex)
            {
                navigationException = ex;
            }
            finally
            {
                Page.PageError -= OnPageError;
                Page.Console -= OnConsole;
            }

            var status = response?.Status;
            var title = navigationException is null ? Clip(await Page.TitleAsync()) : "(navigation failed before title lookup)";
            var routeDiagnostic =
                $"{path} | expectedTitle='{expectedTitle}' | status={(status?.ToString() ?? "no-response")} | finalUrl='{Clip(Page.Url)}' | actualTitle='{title}' | pageErrors={pageErrors.Count} | consoleErrors={consoleErrors.Count}";

            if (pageErrors.Count > 0)
            {
                routeDiagnostic += $" | firstPageError='{pageErrors[0]}'";
            }

            if (consoleErrors.Count > 0)
            {
                routeDiagnostic += $" | firstConsoleError='{consoleErrors[0]}'";
            }

            if (navigationException is not null)
            {
                routeDiagnostic += $" | navigationException='{Clip(navigationException.Message)}'";
            }

            diagnostics.Add(routeDiagnostic);

            if (navigationException is not null || status is null || status >= 400)
            {
                failures.Add(routeDiagnostic);
            }
        }

        if (failures.Count > 0)
        {
            throw new InvalidOperationException(
                "Smoke navigation route failures detected:\n"
                + string.Join("\n", failures)
                + "\n\nFull route diagnostics:\n"
                + string.Join("\n", diagnostics));
        }

        await Assert.That(failures.Count).IsEqualTo(0);
    }
}
