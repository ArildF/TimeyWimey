using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace TimeyWimey.Data;

public class SchemaMigrations
{

    private readonly Func<DbConnection, ILogger<SchemaMigrations>, Task>[] _migrations = 
    {
        AddNotes,
    };

    private readonly ILogger<SchemaMigrations> _logger;

    public SchemaMigrations(ILogger<SchemaMigrations> logger)
    {
        _logger = logger;
    }

    public async Task MigrateToLatest(WimeyDataContext context)
    {
        _logger.LogInformation("Checking for required data migrations");
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT 1 FROM sqlite_master WHERE type='table' AND name='DbVersion'";
        var res = await cmd.ExecuteScalarAsync();
        if (res == null)
        {
            _logger.LogInformation("Table 'DbVersion' does not exist. Creating.");
            await CreateDbVersionTable(connection);
        }

        await RunMigrations(connection);

        await transaction.CommitAsync();
    }

    private async Task RunMigrations(DbConnection connection)
    {
        var versionCmd = connection.CreateCommand();
        versionCmd.CommandText = "SELECT Version FROM DbVersion";
        object? res = await versionCmd.ExecuteScalarAsync();
        long originalVersion = res switch { long v => v, null => -1, _ => throw new InvalidDataException() };

        _logger.LogInformation($"Current value of DbVersion.Version is {originalVersion}");

        long updateVersion = originalVersion;
        for (; updateVersion < _migrations.Length; updateVersion++)
        {
            var migration = _migrations[updateVersion];
            _logger.LogInformation($"Running migration {updateVersion}, {migration.Method.Name}");
            await migration(connection, _logger);
        }

        if (originalVersion != updateVersion)
        {
            var updateVersionCmd = connection.CreateCommand();
            updateVersionCmd.CommandText = originalVersion == -1
                ? $"INSERT INTO DBVersion(Version) VALUES {updateVersion}"
                : $"UPDATE DBVersion SET Version={updateVersion}";
            await updateVersionCmd.ExecuteNonQueryAsync();

            _logger.LogInformation($"Updated DbVersion.Version to {updateVersion}");
        }
    }

    private async Task CreateDbVersionTable(DbConnection connection)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE DbVersion 
(
    Version INTEGER PRIMARY_KEY
)";
        await cmd.ExecuteNonQueryAsync();
        var insertCmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO DbVersion(Version) VALUES (0)";
        await cmd.ExecuteNonQueryAsync();

        _logger.LogInformation("Created table 'DbVersion'");
    }


    private static async Task AddNotes(DbConnection c, ILogger<SchemaMigrations> logger)
    {
        logger.LogInformation("Adding field Notes(TEXT NULL) to table Entries");
        var cmd = c.CreateCommand();
        cmd.CommandText = "ALTER TABLE Entries ADD COLUMN Notes TEXT NULL";
        await cmd.ExecuteNonQueryAsync();
        logger.LogInformation("Added field Notes(TEXT NULL) to table Entries");
    }
}