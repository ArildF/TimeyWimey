using MudBlazor.Utilities;

namespace TimeyWimey.Infrastructure
{
    public class ColorHelper
    {
        public static string CalculateForegroundTextColor(string? backgroundColor)
        {
            try
            {
                var color = backgroundColor != null ? new MudColor(backgroundColor) : new MudColor("#FF00FF");
                return color.L > 0.45 ? "black" : "white";
            }
            catch (Exception)
            {
                return "black";
            }
        }
    }
}
