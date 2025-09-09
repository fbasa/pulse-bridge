using Microsoft.Data.SqlClient;

public static class DatabaseBootstrap
{
    public static async Task EnsureDatabaseExistsAsync(string targetConnStr, CancellationToken ct = default)
    {
        string dbName = GetDbName(targetConnStr);

        // Connect to master so we can check/create the DB
        var serverConn = new SqlConnectionStringBuilder(targetConnStr) { InitialCatalog = "master" }.ConnectionString;

        await using var conn = new SqlConnection(serverConn);
        await conn.OpenAsync(ct);

        // App-level lock avoids races in multi-instance startups
        await using (var lockCmd = new SqlCommand(
          "EXEC sp_getapplock @Resource=@r, @LockMode='Exclusive', @LockOwner='Session', @LockTimeout=15000;",
          conn))
        {
            lockCmd.Parameters.AddWithValue("@r", "EnsureDb_" + dbName);
            await lockCmd.ExecuteNonQueryAsync(ct);
        }

        // Create DB if it doesn't exist
        var existsCmd = new SqlCommand("SELECT COUNT(1) FROM sys.databases WHERE name = @name", conn);
        existsCmd.Parameters.AddWithValue("@name", dbName);
        var exists = (int)(await existsCmd.ExecuteScalarAsync(ct))! > 0;

        if (!exists)
        {
            var createSql = $"CREATE DATABASE {QuoteName(dbName)};";
            await using (var createCmd = new SqlCommand(createSql, conn))
                await createCmd.ExecuteNonQueryAsync(ct);

            // Wait until ONLINE (usually quick, but be safe)
            while (true)
            {
                var stateCmd = new SqlCommand(
                    "SELECT state_desc FROM sys.databases WHERE name = @name", conn);
                stateCmd.Parameters.AddWithValue("@name", dbName);
                var state = (string)(await stateCmd.ExecuteScalarAsync(ct))!;
                if (string.Equals(state, "ONLINE", StringComparison.OrdinalIgnoreCase)) break;
                await Task.Delay(500, ct);
            }
        }

        // (Optional) set defaults you like
        // await new SqlCommand($"ALTER DATABASE {QuoteName(dbName)} SET READ_COMMITTED_SNAPSHOT ON;", conn)
        //     .ExecuteNonQueryAsync(ct);

        // Release applock
        await using (var rel = new SqlCommand(
            "EXEC sp_releaseapplock @Resource=@r, @LockOwner='Session';",
            conn))
        {
            rel.Parameters.AddWithValue("@r", "EnsureDb_" + dbName);
            await rel.ExecuteNonQueryAsync(ct);
        }

        static string QuoteName(string name) => $"[{name.Replace("]", "]]")}]";
    }

    public static string GetDbName(string targetConnStr)
    {
        var target = new SqlConnectionStringBuilder(targetConnStr);
        var dbName = target.InitialCatalog;
        if (string.IsNullOrWhiteSpace(dbName))
            throw new InvalidOperationException("Quartz connection string must include Initial Catalog / Database.");
        return dbName;
    }
}
