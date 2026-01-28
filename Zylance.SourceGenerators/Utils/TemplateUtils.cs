using System;
using System.Collections.Generic;
using System.Linq;

namespace Zylance.SourceGenerators.Utils;

public static class TemplateUtils
{
    public static string ForEach<T>(IEnumerable<T> list, Func<T, string> func)
    {
        return string.Join("\n", list.Select(func));
    }
}
