namespace Zylance.SourceGenerators.Utils;

/// <summary>
///     Utility helpers used by source generator templates.
/// </summary>
public static class TemplateUtils
{
    /// <summary>
    ///     Applies <paramref name="func" /> to each element in
    ///     <paramref name="list" /> and concatenates the results with newlines.
    /// </summary>
    /// <param name="list">Sequence of items to transform.</param>
    /// <param name="func">Mapping function from an item to its string representation.</param>
    /// <returns>Joined string of transformed items separated by newline characters.</returns>
    public static string ForEach<T>(IEnumerable<T> list, Func<T, string> func)
    {
        return string.Join("\n", list.Select(func));
    }
}
