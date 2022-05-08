using System.Collections.ObjectModel;
using System.Globalization;

namespace TimeyWimey.Model;
public class Day
{
    public int WeekNumber => CultureInfo.CurrentCulture.Calendar
        .GetWeekOfYear(
            Date.ToDateTime(TimeOnly.MinValue),
            CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule,
            DayOfWeek.Monday);
    public DateOnly Date { get; }
    public DayOfWeek DayOfWeek => Date.DayOfWeek;

    public ObservableCollection<TimeEntry> Entries { get; } = new();

    public Day(DateOnly date)
    {
        Date = date;
    }
}