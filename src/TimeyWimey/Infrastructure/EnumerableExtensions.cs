using System.Diagnostics.CodeAnalysis;

namespace TimeyWimey.Infrastructure;

public static class EnumerableExtensions
{
    
    [return: NotNull]
    public static  IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? self)
    {
        return self ?? Array.Empty<T>();
    }

}