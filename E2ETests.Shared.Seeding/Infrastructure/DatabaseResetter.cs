using Npgsql;
using Respawn;

namespace E2ETests.Shared.Seeding.Infrastructure;

public static class DatabaseResetter
{
    private static Respawner? _respawner;
    private static readonly SemaphoreSlim _initLock = new(1, 1);

    public static async Task EnsureInitializedAsync()
    {
        if (_respawner is not null) return;

        await _initLock.WaitAsync();
        try
        {
            if (_respawner is not null) return;

            await using var conn = new NpgsqlConnection(DbContextFactory.ConnectionString);
            await conn.OpenAsync();
            _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"]
            });
        }
        finally
        {
            _initLock.Release();
        }
    }

    public static async Task ResetAsync()
    {
        await EnsureInitializedAsync();

        await using var conn = new NpgsqlConnection(DbContextFactory.ConnectionString);
        await conn.OpenAsync();
        await _respawner!.ResetAsync(conn);

        // TODO: Downsides with this approach:
        // - Respawn deletes all data in the specified tables - since we already have pre-filled data in the database, this data will be deleted as well. In theory, we could re-seed the database after each test but this adds much overhead to each test
        // - If we have multiple test classes running in parallel, they will all reset the database before each test, possibly causing conflicts
    }
}
