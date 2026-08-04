using Microsoft.Data.SqlClient;

namespace E2ETests.Shared.Seeding.Infrastructure;

public static class DatabaseSnapshot
{
    private static string DatabaseName
    {
        get
        {
            var csb = new SqlConnectionStringBuilder(DbContextFactory.ConnectionString);
            return csb.InitialCatalog;
        }
    }

    private static string SnapshotName => $"{DatabaseName}_snap";

    private static string MasterConnectionString
    {
        get
        {
            var csb = new SqlConnectionStringBuilder(DbContextFactory.ConnectionString);
            csb.InitialCatalog = "master";
            return csb.ConnectionString;
        }
    }

    public static async Task CreateAsync()
    {
        await using var conn = new SqlConnection(DbContextFactory.ConnectionString);
        await conn.OpenAsync();

        string logicalName;
        string snapshotPath;

        await using (var cmd = new SqlCommand(
            "SELECT name, physical_name FROM sys.database_files WHERE type_desc = 'ROWS'", conn))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            await reader.ReadAsync();
            logicalName = reader.GetString(0);
            var physicalPath = reader.GetString(1);
            var dir = Path.GetDirectoryName(physicalPath)!.Replace('\\', '/');
            snapshotPath = $"{dir}/{SnapshotName}.ss";
        }

        await using var createCmd = new SqlCommand(
            $"""
            CREATE DATABASE [{SnapshotName}]
            ON (NAME = N'{logicalName}', FILENAME = N'{snapshotPath}')
            AS SNAPSHOT OF [{DatabaseName}]
            """, conn);
        await createCmd.ExecuteNonQueryAsync();
    }

    public static async Task RestoreAsync()
    {
        // Must connect to master — SINGLE_USER kicks all connections to testdb including ours
        await using var conn = new SqlConnection(MasterConnectionString);
        await conn.OpenAsync();

        // TODO: disadvantage of snapshot approach:
        // Note: RESTORE DATABASE requires SET SINGLE_USER WITH ROLLBACK IMMEDIATE, which kills the WebApp's active DB connections and can cause transient 500 errors
        // I found this by spamming F5 in the WebApp while the test suite was running --> the WebApp shows an error and (mostly one) test fails
        // Maybe this could fix those transient errors: add EnableRetryOnFailure() to the WebApp's EF Core config

        await ExecuteAsync(conn, $"ALTER DATABASE [{DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE");
        await ExecuteAsync(conn, $"RESTORE DATABASE [{DatabaseName}] FROM DATABASE_SNAPSHOT = N'{SnapshotName}'");
        await ExecuteAsync(conn, $"ALTER DATABASE [{DatabaseName}] SET MULTI_USER");
        await ExecuteAsync(conn, $"DROP DATABASE [{SnapshotName}]");

        SqlConnection.ClearAllPools();
        await WaitUntilAccessibleAsync();
    }

    private static async Task WaitUntilAccessibleAsync()
    {
        while (true)
        {
            try
            {
                await using var probe = new SqlConnection(DbContextFactory.ConnectionString);
                await probe.OpenAsync();
                return;
            }
            catch (SqlException)
            {
                await Task.Delay(100);
            }
        }
    }

    private static async Task ExecuteAsync(SqlConnection conn, string sql)
    {
        await using var cmd = new SqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
    }
}
