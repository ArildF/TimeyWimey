namespace TimeyWimey.Model;

public class Calendar
{
    public DateOnly[] GetCurrentWeek()
    {
        int diff = DateTime.Now.DayOfWeek == DayOfWeek.Sunday ? 6
            : DateTime.Now.DayOfWeek - DayOfWeek.Monday;
        var firstOfWeek = DateOnly.FromDateTime(DateTime.Now).AddDays(-diff);


        return Enumerable.Range(0, 7)
            .Select(days => firstOfWeek.AddDays(days))
            .ToArray();
        ;
    }
}