using System.Globalization;
using TimeyWimey.Data;
using TimeyWimey.Model;

namespace TimeyWimey.TimeReports;

public class WeekReportGenerator
{
    private readonly DataPersistence _persistence;

    public WeekReportGenerator(DataPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<ReportPerCodeSystem[]> GenerateForWeekOf(DateOnly date)
    {
        var days = await _persistence.GetDaysForWeek(date);

        var codeSystems = days.SelectMany(d => d.Entries)
            .Select(e => e.Activity)
            .Where(a => a != null)
            .SelectMany(a => a!.TimeCodes)
            .Select(tc => tc.System).Distinct();

        return GeneratePerCodeSystem(codeSystems, days).ToArray();

    }

    private IEnumerable<ReportPerCodeSystem> GeneratePerCodeSystem(IEnumerable<TimeCodeSystem> codeSystems, Day[] days)
    {
        foreach (var system in codeSystems)
        {
            var reportPerCode = GeneratePerCode(system, days).ToArray();
            var dayTotals = Enumerable.Range(0, days.Length)
                .Select(i => reportPerCode.Select(rc => rc.Hours[i]).Sum()).ToArray();
            yield return new ReportPerCodeSystem(system.Name, reportPerCode, days, dayTotals);
        }

    }

    private IEnumerable<ReportPerCode> GeneratePerCode(TimeCodeSystem system, Day[] days)
    {
        foreach (var timeCode in system.TimeCodes)
        {
            var hours = days.Select(day => HoursForCode(day, timeCode)).ToArray();
            if (hours.Any(h => h > 0))
            {
                yield return new ReportPerCode(timeCode.Code, hours, hours.Sum());
            }
        }
    }

    private double HoursForCode(Day day, TimeCode timeCode)
    {
        var hoursPerCode = day.Entries
            .Where(e => e.Activity != null)
            .Where(e => e.Activity!.TimeCodes.Any(tc => tc == timeCode))
            .Sum(e => (e.End - e.Start).TotalHours);

        return hoursPerCode;
    }


    public async Task<DayActivity[]> GetNotesForWeekOf(DateOnly date)
    {
        var days = await _persistence.GetDaysForWeek(date);
        return (from day in days
            from entry in day.Entries
            where !string.IsNullOrWhiteSpace(entry.Notes)
            select new DayActivity(day, entry)).ToArray();
    }

    public async Task<MissingCodeSystem[]> GetEntriesWithMissingCodeSystems(DateOnly date)
    {
        var days = await _persistence.GetDaysForWeek(date);
        var codeSystems = days.SelectMany(e => e.Entries)
            .Select(e => e.Activity)
            .Where(a => a != null)
            .SelectMany(a => a.TimeCodes)
            .Select(tc => tc.System)
            .DistinctBy(s => s.Id).ToArray();
            
        var missing = (from cs in codeSystems
            from d in days
            from e in d.Entries
            where e.Activity != null
            where e.Activity!.TimeCodes.All(tc => tc.SystemId != cs.Id)
            select new MissingCodeSystem(cs.Name, d, e)).Concat(
                from cs in codeSystems
                from d in days
                from e in d.Entries
                where e.Activity == null
                select new MissingCodeSystem(cs.Name, d, e));

        return missing.ToArray();
    }
}

public record ReportPerCodeSystem(string CodeSystem, ReportPerCode[] ReportPerCode, Day[] Days, 
    double[] DayTotals);

public record ReportPerCode(string Code, double[] Hours, double Sum);

public record DayActivity(Day Day, TimeEntry Entry);

public record MissingCodeSystem(string CodeSystem, Day Day, TimeEntry Entry);
