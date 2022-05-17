using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using TimeyWimey.Model;

namespace TimeyWimey.Data;

public class DataPersistence
{
    private readonly IDbContextFactory<WimeyDataContext> _contextFactory;
    private readonly IJSRuntime _js;
    private readonly Calendar _calendar;
    private IJSObjectReference? _module;

    public DataPersistence(IDbContextFactory<WimeyDataContext> contextFactory, IJSRuntime js,
        Calendar calendar)
    {
        _contextFactory = contextFactory;
        _js = js;
        _calendar = calendar;
    }

    public async Task Save(Day day)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        if (day.Id != 0)
        {
            db.Days.Update(day);
        }
        else
        {
            await db.Days.AddAsync(day);
        }
        await db.SaveChangesAsync();

        await Sync();
    }

    private async Task Sync()
    {
        await _module.InvokeVoidAsync("syncDatabase", false);
    }


    public async Task Initialize()
    {
        _module = await _js.InvokeAsync<IJSObjectReference>("import", "./dbstorage.js");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("browser")))
        {
            await _module.InvokeVoidAsync("mountDatabaseInIndexDB");
        }

        await using var db = await _contextFactory.CreateDbContextAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task<Day[]> GetCurrentWeek()
    {
        var dates = _calendar.GetCurrentWeek();
        await using var db = await _contextFactory.CreateDbContextAsync();
        var days = await db.Days.Where(d => dates.Contains(d.Date))
            .Include(d => d.Entries)
            .ToDictionaryAsync(d => d.Date);

        Day GetOrCreate(DateOnly date) => days.TryGetValue(date, out var res)
            ? res
            : new Day(date);

        var currentWeek = dates.Select(GetOrCreate).ToArray();
        return currentWeek;

    }
}