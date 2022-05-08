namespace TimeyWimey.TimeRegistration;

public class TimeLineCalculator
{
    public double HourVerticalPosition(TimeSpan time) => 100.0 / 24 * time.TotalHours;
    public double HourVerticalPosition(TimeOnly time) => HourVerticalPosition(time.ToTimeSpan());
}