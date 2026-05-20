using WebApp.Data;

namespace E2ETests.Shared.Seeding.Infrastructure;

public sealed class SeedingContext : IAsyncDisposable
{
    private readonly AppDbContext _db;
    private readonly Stack<Func<Task>> _cleanups = new();

    public SeedingContext() => _db = DbContextFactory.Create();

    public async Task<T> SeedAsync<T>(T entity) where T : class
    {
        _db.Set<T>().Add(entity);
        await _db.SaveChangesAsync();

        // capture ref for cleanup; stack = reverse-insert order
        _cleanups.Push(async () =>
        {
            _db.Set<T>().Remove(entity);
            await _db.SaveChangesAsync();
        });

        return entity;
    }

    public async ValueTask DisposeAsync()
    {
        while (_cleanups.TryPop(out var cleanup))
            await cleanup();

        await _db.DisposeAsync();
    }
}
