using Microsoft.Playwright;
using Xunit;

namespace E2ETests.Infrastructure;

public abstract class PlaywrightTestBase : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;

    protected IBrowserContext BrowserContext { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;
    protected SeedingContext Seed { get; private set; } = null!;

    protected virtual string BaseUrl =>
        Environment.GetEnvironmentVariable("TEST_BASE_URL")
        ?? "https://localhost:5000";

    public virtual async ValueTask InitializeAsync()
    {
        await DbContextFactory.EnsureSchemaAsync();

        Seed = new SeedingContext();

        var headed = Environment.GetEnvironmentVariable("PLAYWRIGHT_HEADED") == "true";

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = !headed,
            SlowMo = headed ? 500 : 0   // slow down actions when watching
        });
        BrowserContext = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = BaseUrl
        });
        Page = await BrowserContext.NewPageAsync();
    }

    public virtual async ValueTask DisposeAsync()
    {
        await Seed.DisposeAsync();
        await BrowserContext.DisposeAsync();
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }
}
