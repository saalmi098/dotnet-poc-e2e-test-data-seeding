using Microsoft.Playwright;
using Xunit;

namespace E2ETests.ApproachC.Testcontainers.Infrastructure;

[Collection("E2E")]
public abstract class PlaywrightTestBase : IAsyncLifetime
{
    private readonly E2EFixture _fixture;
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private DatabaseSnapshot _snapshot = null!;

    protected IBrowserContext BrowserContext { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;
    protected SqlSeedingContext Seed { get; private set; } = null!;

    protected PlaywrightTestBase(E2EFixture fixture)
    {
        _fixture = fixture;
    }

    public virtual async ValueTask InitializeAsync()
    {
        _snapshot = new DatabaseSnapshot(_fixture.ConnectionString);
        await _snapshot.CreateAsync();

        Seed = new SqlSeedingContext(_fixture.ConnectionString);

        var headed = Environment.GetEnvironmentVariable("PLAYWRIGHT_HEADED") == "true";
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = !headed,
            SlowMo = headed ? 500 : 0
        });
        BrowserContext = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = _fixture.BaseUrl
        });
        Page = await BrowserContext.NewPageAsync();
    }

    public virtual async ValueTask DisposeAsync()
    {
        await Seed.DisposeAsync();
        await BrowserContext.DisposeAsync();
        await _browser.DisposeAsync();
        _playwright.Dispose();
        await _snapshot.RestoreAsync();
    }
}
