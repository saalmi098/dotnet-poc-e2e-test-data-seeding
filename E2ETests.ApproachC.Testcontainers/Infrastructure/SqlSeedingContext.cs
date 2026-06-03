using Microsoft.EntityFrameworkCore;
using WebApp.Data;

namespace E2ETests.ApproachC.Testcontainers.Infrastructure;

public sealed class SqlSeedingContext : IAsyncDisposable
{
    private readonly AppDbContext _db;

    public SqlSeedingContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        _db = new AppDbContext(options);
    }

    public async Task<T> SeedAsync<T>(T entity) where T : class
    {
        _db.Set<T>().Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async ValueTask DisposeAsync() => await _db.DisposeAsync();
}
