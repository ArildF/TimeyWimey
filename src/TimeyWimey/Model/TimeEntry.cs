namespace TimeyWimey.Model;

public class TimeEntry
{
    public TimeOnly Start { get; set; }
    public TimeOnly End { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
}