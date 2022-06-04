namespace TimeyWimey.Model;

public class Calendar
{
    public DateOnly[] GetWeek(DateOnly date)
    {
        int diff = date.DayOfWeek == DayOfWeek.Sunday ? 6
            : date.DayOfWeek - DayOfWeek.Monday;
        var firstOfWeek = date.AddDays(-diff);


        return Enumerable.Range(0, 7)
            .Select(days => firstOfWeek.AddDays(days))
            .ToArray();
        ;
    }
}