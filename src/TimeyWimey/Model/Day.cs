using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace TimeyWimey.Model;
public class Day
{
    public int WeekNumber => CultureInfo.CurrentCulture.Calendar
        .GetWeekOfYear(
            Date.ToDateTime(TimeOnly.MinValue),
            CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule,
            DayOfWeek.Monday);
    public DateOnly Date { get; set; }
    public DayOfWeek DayOfWeek => Date.DayOfWeek;

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public ObservableCollection<TimeEntry> Entries { get; } = new();

    public Day(DateOnly date)
    {
        Date = date;
    }

    public Day()
    {
        
    }
}