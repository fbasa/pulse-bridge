using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

public interface IQuartzSchemaBootstrapper
{
    Task EnsureCreatedAsync(CancellationToken ct);
}

public sealed class QuartzSchemaBootstrapper : IQuartzSchemaBootstrapper
{
    private readonly IConfiguration _config;

    public QuartzSchemaBootstrapper(IConfiguration config) => _config = config;

    public async Task EnsureCreatedAsync(CancellationToken ct)
    {
        var connStr = _config.GetConnectionString("QuartzNet")
                     ?? throw new InvalidOperationException("Missing Quartz connection string.");

        string dbName = DatabaseBootstrap.GetDbName(connStr);

        await using var conn = new SqlConnection(connStr);
        await conn.OpenAsync(ct);

        // Avoid races when multiple instances start at once (K8s/scale-out)
        await using (var lockCmd = new SqlCommand(
          "EXEC sp_getapplock @Resource=@r, @LockMode='Exclusive', @LockOwner='Session', @LockTimeout=15000;",
          conn))
        {
            lockCmd.Parameters.AddWithValue("@r", "EnsureDb_" + dbName);
            await lockCmd.ExecuteNonQueryAsync(ct);
        }

        // If any QRTZ_ table exists, assume schema is already present
        const string existsSql = "SELECT COUNT(*) FROM sys.tables WHERE name LIKE 'QRTZ[_]%';";
        var count = (int)(await new SqlCommand(existsSql, conn).ExecuteScalarAsync(ct))!;
        if (count > 0) return;

        // Load embedded DDL
        var ddl = ReadEmbedded("QuartzSchema.sql");

        // Optional: adjust prefix if you changed it in configuration
        // ddl = ddl.Replace("QRTZ_", _config["Quartz:TablePrefix"] ?? "QRTZ_");

        // Split on GO (batch terminator) and execute each batch
        var batches = Regex.Split(ddl, @"^\s*GO\s*?$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        foreach (var batch in batches)
        {
            var sql = batch.Trim();
            if (string.IsNullOrWhiteSpace(sql)) continue;

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            await cmd.ExecuteNonQueryAsync(ct);
        }

        // (Optional) release applock explicitly
        await using (var rel = new SqlCommand(
           "EXEC sp_releaseapplock @Resource=@r, @LockOwner='Session';",
           conn))
        {
            rel.Parameters.AddWithValue("@r", "EnsureDb_" + dbName);
            await rel.ExecuteNonQueryAsync(ct);
        }

        static string ReadEmbedded(string nameSuffix)
        {
            var asm = Assembly.GetExecutingAssembly();
            //var resourceName = asm.GetManifestResourceNames()
            //    .First(n => n.EndsWith(nameSuffix, StringComparison.OrdinalIgnoreCase));

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,nameSuffix);
            //using var stream = asm.GetManifestResourceStream(path)!;
            using var reader = new StreamReader(path);
            return reader.ReadToEnd();
        }
    }
}
