using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TimeyWimey.Infrastructure;

namespace TimeyWimey.TimeRegistration;

public class TimeLineCalculator
{
    private readonly IJSRuntime _jsRuntime;

    public TimeLineCalculator(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public double HourVerticalPosition(TimeSpan time) => 100.0 / 24 * time.TotalHours;
    public double HourVerticalPosition(TimeOnly time) => HourVerticalPosition(time.ToTimeSpan());
}