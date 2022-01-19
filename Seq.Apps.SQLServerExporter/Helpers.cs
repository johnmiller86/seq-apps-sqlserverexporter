using System.Collections.Generic;

namespace Seq.Apps.SQLServerExporter
{
    internal static class Helpers
    {
        internal static string FormatColumnWithBrackets(this string source)
        {
            return $"[{source}]";
        }

        internal static string FormatColumnValueWithSingleQuotes(this string source)
        {
            return $"'{source.Replace("'", "''")}'";
        }

        internal static string JoinEnumerableWithCommas(this IEnumerable<string> source)
        {
            return string.Join(", ", source);
        }
    }
}