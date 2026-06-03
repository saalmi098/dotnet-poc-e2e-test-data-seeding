using WebApp.Data;

namespace E2ETests.Shared.Seeding.Infrastructure;

public sealed class SeedingContext : IAsyncDisposable
{
    private readonly AppDbContext _db;

    public SeedingContext() => _db = DbContextFactory.Create();

    public async Task<T> SeedAsync<T>(T entity) where T : class
    {
        _db.Set<T>().Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async ValueTask DisposeAsync()
    {
        await _db.DisposeAsync();
        await DatabaseResetter.ResetAsync();
    }
}
