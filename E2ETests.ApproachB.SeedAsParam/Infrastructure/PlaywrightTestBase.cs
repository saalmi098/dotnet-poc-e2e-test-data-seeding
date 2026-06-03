using E2ETests.Shared.Seeding.Infrastructure;
using Microsoft.Playwright;
using Xunit;
using static System.Net.Mime.MediaTypeNames;

namespace E2ETests.ApproachB.SeedAsParam.Infrastructure;

public abstract class PlaywrightTestBase : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;

    protected IBrowserContext BrowserContext { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;

    protected virtual string BaseUrl =>
        Environment.GetEnvironmentVariable("TEST_BASE_URL")
        ?? "https://localhost:5000";

    public virtual async ValueTask InitializeAsync()
    {
        var headed = Environment.GetEnvironmentVariable("PLAYWRIGHT_HEADED") == "true";

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = !headed,
            SlowMo = headed ? 500 : 0
        });
        BrowserContext = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = BaseUrl
        });
        Page = await BrowserContext.NewPageAsync();
    }

    public virtual async ValueTask DisposeAsync()
    {
        await BrowserContext.DisposeAsync();
        await _browser.DisposeAsync();
        _playwright.Dispose();

        // Restore must live in IAsyncLifetime.DisposeAsync(): xUnit guarantees it runs before the next test's InitializeAsync(); DisposalTracker does not have this guarantee
        await DatabaseSnapshot.RestoreAsync();
    }
}
