using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Reflection;
using WebApp.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        // TODO: This kind of cleanup is fragile because:
        // - The employee may already be deleted. (for example in a test that tests deletion)
        // - The entity may be tracked in an old state. (for example in a test that tests updates)
        // - Foreign key dependencies may still exist.
        // - A test might partially modify or move the data.
        // - EF Core may throw concurrency exceptions for expected - but - missing rows.

        return entity;
    }

    public async ValueTask DisposeAsync()
    {
        while (_cleanups.TryPop(out var cleanup))
            await cleanup();

        await _db.DisposeAsync();
    }
}
