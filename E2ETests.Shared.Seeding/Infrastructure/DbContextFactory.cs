using Microsoft.EntityFrameworkCore;
using WebApp.Data;

namespace E2ETests.Shared.Seeding.Infrastructure;

public static class DbContextFactory
{
    private static readonly string ConnectionString =
        Environment.GetEnvironmentVariable("TEST_DB_CONNECTION")
        ?? "Host=localhost;Port=5432;Database=testdb;Username=postgres;Password=postgres";

    private static readonly DbContextOptions<AppDbContext> _options =
        new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

    public static AppDbContext Create() => new(_options);

    public static async Task EnsureSchemaAsync()
    {
        await using var db = Create();
        await db.Database.EnsureCreatedAsync();
    }
}