using Microsoft.EntityFrameworkCore;

namespace TimeyWimey.ImportExport;

public class DataImportExport
{
    private readonly IDbContextFactory<ImportExportDataContext> _contextFactory;

    public DataImportExport(IDbContextFactory<ImportExportDataContext> contextFactory)
    {
        _contextFactory = contextFactory;
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
}