using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using TimeyWimey.Model;
using TimeyWimey.Infrastructure;

namespace TimeyWimey.Data;

public class DataPersistence
{
    private readonly IDbContextFactory<WimeyDataContext> _contextFactory;
    private readonly IJSRuntime _js;
    private readonly Calendar _calendar;
    private readonly SchemaMigrations _migrations;
    private readonly ILogger<DataPersistence> _logger;
    private IJSObjectReference? _module;

    public DataPersistence(IDbContextFactory<WimeyDataContext> contextFactory, IJSRuntime js,
        Calendar calendar, SchemaMigrations migrations, ILogger<DataPersistence> logger)
    {
        _contextFactory = contextFactory;
        _js = js;
        _calendar = calendar;
        _migrations = migrations;
        _logger = logger;
    }

    public async Task Save(Day day)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        if (day.Id != 0)
        {
            // this because multiple entries may link to the same TimeActivity id, but different instances
            db.ChangeTracker.TrackGraph(
                day, node =>
                {
                    var keyValue = node.Entry.Property("Id").CurrentValue;
                    var entityType = node.Entry.Metadata;
                    if (keyValue?.Equals(0) ?? false)
                    {
                        node.Entry.State = EntityState.Added;
                        return;
                    }

                    var existingEntity = node.Entry.Context.ChangeTracker.Entries()
                        .FirstOrDefault(
                            e => Equals(e.Metadata, entityType)
                                 && Equals(e.Property("Id").CurrentValue, keyValue));

                    if (existingEntity == null)
                    {
                        _logger.LogDebug($"Tracking {entityType.DisplayName()} entity with key value {keyValue}");

                        node.Entry.State = EntityState.Modified;
                    }
                    else
                    {
                        _logger.LogDebug($"Discarding duplicate {entityType.DisplayName()} entity with key value {keyValue}");
                    }
                });
        }
        else
        {
            await db.Days!.AddAsync(day);
        }

        _logger.LogDebug(() => db.ChangeTracker.DebugView.LongView);
        
        await db.SaveChangesAsync();

        await Sync();
    }

    public async Task Save(TimeActivity activity)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        if (activity.Id != 0)
        {
            db.Activities.Update(activity);
        }
        else
        {
            await db.Activities.AddAsync(activity);
        }
        await db.SaveChangesAsync();

        await Sync();
    }

    private async Task Sync()
    {
        await _module!.InvokeVoidAsync("syncDatabase", false);
    }


    public async Task Initialize()
    {
        _module = await _js.InvokeAsync<IJSObjectReference>("import", "./dbstorage.js");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("browser")))
        {
            await _module.InvokeVoidAsync("mountDatabaseInIndexDB");
        }

        await using var db = await _contextFactory.CreateDbContextAsync();
        if (!await db.Database.CanConnectAsync())
        {
            await db.Database.EnsureCreatedAsync();

            await db.Database.ExecuteSqlRawAsync(@"CREATE TABLE DbVersion 
(
    Version INTEGER PRIMARY_KEY
)"); 
            // update version here when adding migrations
            await db.Database.ExecuteSqlRawAsync("INSERT INTO DbVersion(Version) VALUES(1)");
        }

        await _migrations.MigrateToLatest(db);
        await Sync();
    }

    public async Task<Day[]> GetCurrentWeek()
    {
        var dates = _calendar.GetCurrentWeek();
        await using var db = await _contextFactory.CreateDbContextAsync();
        var days = await db.Days.Where(d => dates.Contains(d.Date))
            .AsNoTrackingWithIdentityResolution()
            .Include(d => d.Entries)
            .ThenInclude(e => e.Activity)
            .ToDictionaryAsync(d => d.Date);

        Day GetOrCreate(DateOnly date) => days.TryGetValue(date, out var res)
            ? res
            : new Day(date);

        var currentWeek = dates.Select(GetOrCreate).ToArray();
        return currentWeek;

    }

    public async Task<TimeActivity[]?> GetActivities()
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Activities.ToArrayAsync();
    }

    public async Task Delete(TimeEntry entry)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        db.Entries.Remove(entry);
        await db.SaveChangesAsync();
    }
}