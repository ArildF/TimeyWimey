namespace TimeyWimey.Infrastructure;

public static class EnumerableExtensions
{
    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? self)
    {
        return self ?? Array.Empty<T>();
    }

}