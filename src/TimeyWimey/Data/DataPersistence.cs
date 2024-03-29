﻿using System.Runtime.InteropServices;
using System.Text.Json;
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

    public const string DatabaseFilePath = "/database/app.db";
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
                    // don't persist beyond the Day => TimeEntry level
                    if (!(node.Entry.Entity switch { Day _ => true, TimeEntry => true, TimeActivity => true, _ => false }))
                    {
                        return;
                    }

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
            var existingActivity = await db.Activities.Include(a => a.TimeCodes)
                .SingleAsync(a => a.Id == activity.Id);
            db.Entry(existingActivity).CurrentValues.SetValues(activity);

            var toRemove = (from code in existingActivity.TimeCodes
                where activity.TimeCodes.All(c => c.Id != code.Id)
                select code).ToArray();

            foreach (var code in toRemove)
            {
                existingActivity.TimeCodes.Remove(code);
            }

            foreach (var code in activity.TimeCodes)
            {
                if (existingActivity.TimeCodes.All(c => c.Id != code.Id))
                {
                    var c = await db.TimeCodes.FindAsync(code.Id);
                    existingActivity.TimeCodes.Add(c!);
                }
            }

            db.Activities.Update(existingActivity);
        }
        else
        {
            db.Activities.Update(activity);
        }

        _logger.LogDebug(db.ChangeTracker.DebugView.LongView);
        await db.SaveChangesAsync();

        await Sync();
    }

    public async Task Sync()
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
            await db.Database.ExecuteSqlRawAsync("INSERT INTO DbVersion(Version) VALUES(2)");

            var systems = new[]
            {
                new TimeCodeSystem
                {
                    Name = "CTR",
                    Description = "The least worst",
                    TimeCodes = new[]
                    {
                        new TimeCode
                        {
                            Code = "SSA-V",
                            Description = "Something",
                        },
                        new TimeCode
                        {
                            Code = "SSA-L",
                            Description = "Something",
                        },
                        new TimeCode
                        {
                            Code = "SSA-T",
                            Description = "Something",
                        },
                    }
                },
                new TimeCodeSystem()
                {
                    Name = "Clarity",
                    Description = "The most worst",
                    TimeCodes = new[]
                    {
                        new TimeCode
                        {
                            Code = "SSA-V#76",
                            Description = "Something",
                        },
                        new TimeCode
                        {
                            Code = "SSA-V#54",
                            Description = "Something",
                        },
                    }

                }
            };

            db.CodeSystems.AddRange(systems);
            await db.SaveChangesAsync();
        }

        await _migrations.MigrateToLatest(db);
        await Sync();
    }

    public async Task<Day[]> GetDaysForWeek(DateOnly date)
    {
        var dates = _calendar.GetWeek(date);
        await using var db = await _contextFactory.CreateDbContextAsync();
        var days = await db.Days.Where(d => dates.Contains(d.Date))
            .AsNoTrackingWithIdentityResolution()
            .Include(d => d.Entries)
            .ThenInclude(e => e.Activity)
            .ThenInclude(a => a!.TimeCodes)
            .ThenInclude(tc => tc.System)
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
        return await db.Activities.OrderByDescending(a => a.LastUsed).ToArrayAsync();
    }

    public async Task<TimeActivity[]?> GetActivitiesFull()
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Activities
            .Include(a => a.TimeCodes)
            .ThenInclude(tc => tc.System)
            .OrderByDescending(a => a.Name).ToArrayAsync();
    }

    public async Task Delete(TimeEntry entry)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        db.Entries.Remove(entry);
        await db.SaveChangesAsync();
        await Sync();
    }

    public async Task<List<TimeCode>> GetTimeCodes()
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.TimeCodes
            .Include(t => t.System)
            .ToListAsync();
    }

    public async Task NewTimeCodeSystem(TimeCodeSystem system)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        await db.CodeSystems.AddAsync(system);
        await db.SaveChangesAsync();
        await Sync();
    }

    public async Task<List<TimeCodeSystem>> GetTimeCodeSystems()
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.CodeSystems
            .Include(cs => cs.TimeCodes)
            .ToListAsync();
    }

    public async Task Save(TimeCode code)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        db.TimeCodes.Update(code);

        await db.SaveChangesAsync();
        await Sync();

    }

    public async Task Delete(TimeCode code)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        db.TimeCodes.Remove(code);

        await db.SaveChangesAsync();
        await Sync();
    }

    public async Task<byte[]> GetDatabaseFileContents()
    {
        return await _module!.InvokeAsync<byte[]>("readDatabaseFile", DatabaseFilePath);
    }

    public async Task<JsonElement> StatDatabaseFile()
    {
        return await _module!.InvokeAsync<JsonElement>("statDatabaseFile", DatabaseFilePath);
    }
}