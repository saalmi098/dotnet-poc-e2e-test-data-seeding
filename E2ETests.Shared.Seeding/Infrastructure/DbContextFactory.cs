using Microsoft.EntityFrameworkCore;
using WebApp.Data;

namespace E2ETests.Shared.Seeding.Infrastructure;

public static class DbContextFactory
{
    public static readonly string ConnectionString =
        Environment.GetEnvironmentVariable("TEST_DB_CONNECTION")
        ?? "Server=localhost,1433;Database=testdb;User Id=sa;Password=Strong!Passw0rd;TrustServerCertificate=True";

    private static readonly DbContextOptions<AppDbContext> _options =
        new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

    public static AppDbContext Create() => new(_options);

    public static async Task EnsureSchemaAsync()
    {
        await using var db = Create();
        await db.Database.EnsureCreatedAsync();
    }
}
