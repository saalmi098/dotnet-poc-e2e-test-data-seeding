using Microsoft.EntityFrameworkCore;
using WebApp.Data;

namespace E2ETests.Infrastructure;

public static class DbContextFactory
{
    private static readonly string ConnectionString =
        Environment.GetEnvironmentVariable("TEST_DB_CONNECTION")
        ?? "Host=localhost;Port=5432;Database=testdb;Username=postgres;Password=postgres";

    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new AppDbContext(options);
    }

    public static async Task EnsureSchemaAsync()
    {
        await using var db = Create();
        await db.Database.EnsureCreatedAsync();
    }
}
