using Microsoft.Data.SqlClient;

namespace E2ETests.ApproachC.Testcontainers.Infrastructure;

public sealed class DatabaseSnapshot
{
    private readonly string _connectionString;
    private readonly string _databaseName;
    private readonly string _snapshotName;
    private readonly string _masterConnectionString;

    public DatabaseSnapshot(string connectionString)
    {
        _connectionString = connectionString;
        var csb = new SqlConnectionStringBuilder(connectionString);
        _databaseName = csb.InitialCatalog;
        _snapshotName = $"{_databaseName}_snap";
        csb.InitialCatalog = "master";
        _masterConnectionString = csb.ConnectionString;
    }

    public async Task CreateAsync()
    {
        await using var conn = new SqlConnection(_connectionString);
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
            snapshotPath = $"{dir}/{_snapshotName}.ss";
        }

        await using var createCmd = new SqlCommand(
            $"""
            CREATE DATABASE [{_snapshotName}]
            ON (NAME = N'{logicalName}', FILENAME = N'{snapshotPath}')
            AS SNAPSHOT OF [{_databaseName}]
            """, conn);
        await createCmd.ExecuteNonQueryAsync();
    }

    public async Task RestoreAsync()
    {
        // Connect to master — SINGLE_USER kicks all connections to the target DB
        await using var conn = new SqlConnection(_masterConnectionString);
        await conn.OpenAsync();

        await ExecuteAsync(conn, $"ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE");
        await ExecuteAsync(conn, $"RESTORE DATABASE [{_databaseName}] FROM DATABASE_SNAPSHOT = N'{_snapshotName}'");
        await ExecuteAsync(conn, $"ALTER DATABASE [{_databaseName}] SET MULTI_USER");
        await ExecuteAsync(conn, $"DROP DATABASE [{_snapshotName}]");

        SqlConnection.ClearAllPools();
        await WaitUntilAccessibleAsync();
    }

    private async Task WaitUntilAccessibleAsync()
    {
        while (true)
        {
            try
            {
                await using var probe = new SqlConnection(_connectionString);
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
