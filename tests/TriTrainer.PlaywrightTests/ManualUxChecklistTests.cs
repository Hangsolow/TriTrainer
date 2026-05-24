using Microsoft.Playwright;
using TUnit.Aspire;

namespace TriTrainer.PlaywrightTests;

[ClassDataSource<AppFixture>(Shared = SharedType.PerTestSession)]
public class ManualUxChecklistTests(AppFixture fixture)
{
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

        var httpClient = fixture.CreateHttpClient("webfrontend");
        _baseUrl = httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "http://localhost:5000";
    }

    [After(Test)]
    public async Task TeardownBrowserAsync()
    {
        if (_page is not null)
        {
            await _page.CloseAsync();
        }

        if (_context is not null)
        {
            await _context.CloseAsync();
        }

        if (_browser is not null)
        {
            await _browser.CloseAsync();
        }

        _playwright?.Dispose();
    }

    private IPage Page => _page ?? throw new InvalidOperationException("Page not initialized");

    private async Task NavigateToAsync(string path)
    {
        await Page.GotoAsync(
            $"{_baseUrl}/{path.TrimStart('/')}",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 30_000 });
    }

    private static string NormalizePathAndQuery(string url)
    {
        var uri = new Uri(url);
        var value = uri.PathAndQuery.Trim();
        return value.EndsWith('/') && value.Length > 1
            ? value.TrimEnd('/')
            : value;
    }

    private static string ResolveExpectedPathAndQuery(string baseUrl, string href)
    {
        var root = new Uri(baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/");
        var absolute = new Uri(root, href);
        return NormalizePathAndQuery(absolute.ToString());
    }

    private async Task SwitchToMobileContextAsync()
    {
        if (_context is not null)
        {
            await _context.CloseAsync();
        }

        _context = await (_browser ?? throw new InvalidOperationException("Browser not initialized"))
            .NewContextAsync(new BrowserNewContextOptions
            {
                IgnoreHTTPSErrors = true,
                ViewportSize = new ViewportSize { Width = 390, Height = 844 },
                IsMobile = true,
                HasTouch = true,
                DeviceScaleFactor = 3
            });

        _page = await _context.NewPageAsync();
    }

    private static async Task<(double Width, double Height)> GetTapTargetSizeAsync(ILocator locator)
    {
        var box = await locator.BoundingBoxAsync();
        if (box is null)
        {
            return (0, 0);
        }

        return (box.Width, box.Height);
    }

    private async Task<bool> TabUntilFocusedAsync(string selector, int maxTabs = 40)
    {
        for (var i = 0; i < maxTabs; i++)
        {
            await Page.Keyboard.PressAsync("Tab");
            var isFocused = await Page.EvaluateAsync<bool>(
                "sel => document.activeElement?.matches(sel) ?? false",
                selector);

            if (isFocused)
            {
                return true;
            }
        }

        return false;
    }

    [Test]
    [Timeout(120_000)]
    public async Task ManualLike_BackForward_StaysStable_AfterDashboardCtaNavigation()
    {
        const string recommendationSelector = ".tt-reco-card a.btn.btn-sm.btn-outline-primary";
        const string dailyCheckInSelector = "section[aria-label='Daily check-in quick path'] a.btn.btn-primary";

        await NavigateToAsync("/");
        await Assertions.Expect(Page).ToHaveTitleAsync("Dashboard");

        var recommendationCta = Page.Locator(recommendationSelector).First;
        await Assertions.Expect(recommendationCta).ToBeVisibleAsync();
        var recommendationHref = await recommendationCta.GetAttributeAsync("href")
            ?? throw new InvalidOperationException("Recommendation CTA href was null.");
        var recommendationExpectedPath = ResolveExpectedPathAndQuery(_baseUrl, recommendationHref);

        await recommendationCta.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 30_000 });
        var recommendationRoute = NormalizePathAndQuery(Page.Url);
        Console.WriteLine($"Recommendation CTA route observed: {recommendationRoute}");

        await Page.GoBackAsync(new PageGoBackOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 30_000 });
        await Assertions.Expect(Page).ToHaveTitleAsync("Dashboard");

        await Page.GoForwardAsync(new PageGoForwardOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 30_000 });
        await Assert.That(NormalizePathAndQuery(Page.Url)).IsEqualTo(recommendationExpectedPath);

        await NavigateToAsync("/");
        var dailyCheckInCta = Page.Locator(dailyCheckInSelector).First;
        await Assertions.Expect(dailyCheckInCta).ToBeVisibleAsync();
        var dailyCheckInHref = await dailyCheckInCta.GetAttributeAsync("href")
            ?? throw new InvalidOperationException("Daily check-in CTA href was null.");
        var dailyExpectedPath = ResolveExpectedPathAndQuery(_baseUrl, dailyCheckInHref);

        await dailyCheckInCta.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 30_000 });
        var dailyRoute = NormalizePathAndQuery(Page.Url);
        Console.WriteLine($"Daily check-in CTA route observed: {dailyRoute}");

        await Page.GoBackAsync(new PageGoBackOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 30_000 });
        await Assertions.Expect(Page).ToHaveTitleAsync("Dashboard");

        await Page.GoForwardAsync(new PageGoForwardOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 30_000 });
        await Assert.That(NormalizePathAndQuery(Page.Url)).IsEqualTo(dailyExpectedPath);
    }

    [Test]
    [Timeout(120_000)]
    public async Task ManualLike_MobileViewport_CtasAreDiscoverableAndTapNavigates()
    {
        const string recommendationSelector = ".tt-reco-card a.btn.btn-sm.btn-outline-primary";
        const string dailyCheckInSelector = "section[aria-label='Daily check-in quick path'] a.btn.btn-primary";

        await SwitchToMobileContextAsync();
        await NavigateToAsync("/");
        await Assertions.Expect(Page).ToHaveTitleAsync("Dashboard");

        var dailyCheckInCta = Page.Locator(dailyCheckInSelector).First;
        await Assertions.Expect(dailyCheckInCta).ToBeVisibleAsync();
        var (dailyWidth, dailyHeight) = await GetTapTargetSizeAsync(dailyCheckInCta);
        await Assert.That(dailyWidth).IsGreaterThanOrEqualTo(40);
        await Assert.That(dailyHeight).IsGreaterThanOrEqualTo(28);

        var dailyHref = await dailyCheckInCta.GetAttributeAsync("href")
            ?? throw new InvalidOperationException("Daily check-in CTA href was null.");
        var expectedDailyRoute = ResolveExpectedPathAndQuery(_baseUrl, dailyHref);
        await dailyCheckInCta.TapAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 30_000 });
        await Assert.That(NormalizePathAndQuery(Page.Url)).IsEqualTo(expectedDailyRoute);

        await NavigateToAsync("/");
        var recommendationCta = Page.Locator(recommendationSelector).First;
        await Assertions.Expect(recommendationCta).ToBeVisibleAsync();
        var (recommendationWidth, recommendationHeight) = await GetTapTargetSizeAsync(recommendationCta);
        await Assert.That(recommendationWidth).IsGreaterThanOrEqualTo(40);
        await Assert.That(recommendationHeight).IsGreaterThanOrEqualTo(28);

        var recommendationHref = await recommendationCta.GetAttributeAsync("href")
            ?? throw new InvalidOperationException("Recommendation CTA href was null.");
        var expectedRecommendationRoute = ResolveExpectedPathAndQuery(_baseUrl, recommendationHref);
        await recommendationCta.TapAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 30_000 });
        await Assert.That(NormalizePathAndQuery(Page.Url)).IsEqualTo(expectedRecommendationRoute);

        Console.WriteLine(
            $"Mobile CTA tap targets (w x h): Daily={dailyWidth:0.#}x{dailyHeight:0.#}, Recommendation={recommendationWidth:0.#}x{recommendationHeight:0.#}");
    }

    [Test]
    [Timeout(120_000)]
    public async Task ManualLike_Keyboard_TabTraversal_EnterActivation_WorksForKeyCtas()
    {
        const string recommendationSelector = ".tt-reco-card a.btn.btn-sm.btn-outline-primary";
        const string dailyCheckInSelector = "section[aria-label='Daily check-in quick path'] a.btn.btn-primary";

        await NavigateToAsync("/");
        await Assertions.Expect(Page).ToHaveTitleAsync("Dashboard");

        var dailyCheckInCta = Page.Locator(dailyCheckInSelector).First;
        await Assertions.Expect(dailyCheckInCta).ToBeVisibleAsync();

        var reachedDailyViaTab = await TabUntilFocusedAsync(dailyCheckInSelector, maxTabs: 35);
        await Assert.That(reachedDailyViaTab).IsTrue();

        var dailyHref = await dailyCheckInCta.GetAttributeAsync("href")
            ?? throw new InvalidOperationException("Daily check-in CTA href was null.");
        var expectedDailyRoute = ResolveExpectedPathAndQuery(_baseUrl, dailyHref);
        await Page.Keyboard.PressAsync("Enter");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 30_000 });
        await Assert.That(NormalizePathAndQuery(Page.Url)).IsEqualTo(expectedDailyRoute);

        await NavigateToAsync("/");
        var recommendationCta = Page.Locator(recommendationSelector).First;
        await Assertions.Expect(recommendationCta).ToBeVisibleAsync();

        var reachedRecommendationViaTab = await TabUntilFocusedAsync(recommendationSelector, maxTabs: 50);
        await Assert.That(reachedRecommendationViaTab).IsTrue();

        var recommendationHref = await recommendationCta.GetAttributeAsync("href")
            ?? throw new InvalidOperationException("Recommendation CTA href was null.");
        var expectedRecommendationRoute = ResolveExpectedPathAndQuery(_baseUrl, recommendationHref);
        await Page.Keyboard.PressAsync("Enter");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 30_000 });
        await Assert.That(NormalizePathAndQuery(Page.Url)).IsEqualTo(expectedRecommendationRoute);
    }
}