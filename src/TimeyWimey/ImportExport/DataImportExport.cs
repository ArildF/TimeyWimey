using Microsoft.EntityFrameworkCore;
using TimeyWimey.Data;

namespace TimeyWimey.ImportExport;

public class DataImportExport
{
    private readonly IDbContextFactory<ImportExportDataContext> _contextFactory;
    private readonly ILogger<DataImportExport> _logger;
    private readonly DataPersistence _persistence;

    public DataImportExport(IDbContextFactory<ImportExportDataContext> contextFactory,
        ILogger<DataImportExport> logger, DataPersistence persistence)
    {
        _contextFactory = contextFactory;
        _logger = logger;
        _persistence = persistence;
    }

    public async Task<ExportModel> ReadExportModel()
    {
        await using var db = await _contextFactory.CreateDbContextAsync();

        var model = new ExportModel
        {
            Days = await db.Days.ToArrayAsync(),
            Activities = await db.Activities.ToArrayAsync(),
            Entries = await db.Entries.ToArrayAsync(),
            TimeCodes = await db.TimeCodes.ToArrayAsync(),
            CodeSystems = await db.CodeSystems.ToArrayAsync(),
            TimeActivityTimeCode = await db.TimeActivityTimeCode.ToArrayAsync(),

        };
        return model;
    }

    public async Task Import(ExportModel exportModel)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        await db.Database.OpenConnectionAsync();
        await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF;");
        await db.Database.ExecuteSqlRawAsync("PRAGMA ignore_check_constraints=true");

        var tables = new[]
        {
            "Activities", "CodeSystems", "Days", "TimeCodes", "Entries",
            "TimeActivityTimeCode"
        };

        foreach (var table in tables)
        {
            _logger.LogInformation($"Deleting from table {table}");
            var result = await db.Database.ExecuteSqlRawAsync($"DELETE FROM {table}");
            _logger.LogInformation($"Deleted from table {table}, result: {result}");
        }

        await db.TimeCodes.AddRangeAsync(exportModel.TimeCodes);
        await db.Activities.AddRangeAsync(exportModel.Activities);
        await db.CodeSystems.AddRangeAsync(exportModel.CodeSystems);
        await db.Days.AddRangeAsync(exportModel.Days);
        await db.Entries.AddRangeAsync(exportModel.Entries);
        await db.TimeActivityTimeCode.AddRangeAsync(exportModel.TimeActivityTimeCode);

        await db.SaveChangesAsync();

        await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=ON;");
        await db.Database.ExecuteSqlRawAsync("PRAGMA ignore_check_constraints=false");

        await db.Database.CloseConnectionAsync();

        await _persistence.Sync();

    }
}