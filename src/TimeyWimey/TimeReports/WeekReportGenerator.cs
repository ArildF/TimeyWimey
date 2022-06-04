﻿using System.Globalization;
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
            .SelectMany(a => a.TimeCodes)
            .Select(tc => tc.System).Distinct();

        return GeneratePerCodeSystem(codeSystems, days).ToArray();

    }

    private IEnumerable<ReportPerCodeSystem> GeneratePerCodeSystem(IEnumerable<TimeCodeSystem> codeSystems, Day[] days)
    {
        foreach (var system in codeSystems)
        {
            var reportPerCode = GeneratePerCode(system, days).ToArray();
            yield return new ReportPerCodeSystem(system.Name, reportPerCode, days);
        }

    }

    private IEnumerable<ReportPerCode> GeneratePerCode(TimeCodeSystem system, Day[] days)
    {
        foreach (var timeCode in system.TimeCodes)
        {
            var hours = days.Select(day => HoursForCode(day, timeCode)).ToArray();
            if (hours.Any(h => !string.IsNullOrEmpty(h)))
            {
                yield return new ReportPerCode(timeCode.Code, hours);
            }
        }
    }

    private string HoursForCode(Day day, TimeCode timeCode)
    {
        var hoursPerCode = day.Entries
            .Where(e => e.Activity != null)
            .Where(e => e.Activity!.TimeCodes.Any(tc => tc == timeCode))
            .Sum(e => (e.End - e.Start).TotalHours);

        return hoursPerCode > 0 ? hoursPerCode.ToString(CultureInfo.InvariantCulture) : "";
    }



}

public record ReportPerCodeSystem(string CodeSystem, ReportPerCode[] ReportPerCode, Day[] Days);

public record ReportPerCode(string Code, string[] Hours);