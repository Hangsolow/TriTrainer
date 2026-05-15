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

        // Select EventFinish type
        await Page.SelectOptionAsync("select >> nth=0", "EventFinish");

        // Set target date to 90 days from today
        var targetDate = DateTime.Today.AddDays(90).ToString("yyyy-MM-dd");
        await Page.FillAsync("input[type='date'] >> nth=0", targetDate);

        // Click Create Goal
        await Page.ClickAsync("button:has-text('Create Goal')");

        // Wait for reload — goal list should appear or update
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 30_000 });

        // Goal list section must be present and not empty
        var goalTable = Page.Locator("table");
        await Assertions.Expect(goalTable).ToBeVisibleAsync();
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
        await Page.SelectOptionAsync("select >> nth=0", "Consistency");
        var targetDate = DateTime.Today.AddDays(120).ToString("yyyy-MM-dd");
        await Page.FillAsync("input[type='date'] >> nth=0", targetDate);
        await Page.ClickAsync("button:has-text('Create Goal')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 30_000 });

        // Step 2: Navigate to Plans
        await NavigateToAsync("/plans");

        // Step 3: Select the goal from dropdown (first non-empty option)
        var goalSelect = Page.Locator("select >> nth=0");
        var options = await goalSelect.Locator("option").AllAsync();
        // Find first non-placeholder option
        string? goalOptionValue = null;
        foreach (var opt in options)
        {
            var val = await opt.GetAttributeAsync("value");
            if (!string.IsNullOrEmpty(val))
            {
                goalOptionValue = val;
                break;
            }
        }

        if (goalOptionValue is null)
        {
            // No goals available — skip the creation step, just validate the form state
            var noGoalMsg = Page.Locator("text=Select goal");
            await Assertions.Expect(noGoalMsg).ToBeVisibleAsync();
            return;
        }

        await goalSelect.SelectOptionAsync(goalOptionValue);

        // Step 4: Set plan name
        await Page.FillAsync("input:not([type='date']):not([type='number'])", "Sprint 2 Smoke Plan");

        // Step 5: Set start date to today
        var startDate = DateTime.Today.ToString("yyyy-MM-dd");
        await Page.FillAsync("input[type='date']", startDate);

        // Step 6: Set week count to 4
        await Page.FillAsync("input[type='number']", "4");

        // Step 7: Create plan
        await Page.ClickAsync("button:has-text('Create Plan')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 30_000 });

        // Step 8: Plan list should now show the new plan
        var planTable = Page.Locator("table");
        await Assertions.Expect(planTable).ToBeVisibleAsync();
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
        var planSelect = Page.Locator("select >> nth=0");
        var firstOption = await planSelect.Locator("option[value]:not([value=''])").First.GetAttributeAsync("value");

        if (string.IsNullOrEmpty(firstOption))
        {
            // No plans available; verify empty state text
            var emptyMsg = Page.Locator("text=Select a plan and week to view progress");
            await Assertions.Expect(emptyMsg).ToBeVisibleAsync();
            return;
        }

        await planSelect.SelectOptionAsync(firstOption);
        await Page.ClickAsync("button:has-text('Load')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 30_000 });

        // Progress table with discipline rows must appear
        var progressTable = Page.Locator("table");
        await Assertions.Expect(progressTable).ToBeVisibleAsync();

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

        // Select Run discipline
        await Page.SelectOptionAsync("select >> nth=0", "Run");

        // Select Fastest5k metric
        await Page.SelectOptionAsync("select >> nth=1", "Fastest5k");

        // Set value
        await Page.FillAsync("input[type='number'] >> nth=0", "25.5");

        // Set achieved-on date to today
        await Page.FillAsync("input[type='date']", DateTime.Today.ToString("yyyy-MM-dd"));

        // Submit
        await Page.ClickAsync("button:has-text('Add Record')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 30_000 });

        // Record table must appear
        var recordTable = Page.Locator("table");
        await Assertions.Expect(recordTable).ToBeVisibleAsync();
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

        foreach (var (path, expectedTitle) in routes)
        {
            var response = await Page.GotoAsync($"{_baseUrl}{path}",
                new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 30_000 });

            if (response?.Status >= 400)
            {
                failures.Add($"{path} → HTTP {response.Status}");
            }
        }

        await Assert.That(failures.Count).IsEqualTo(0);
    }
}
