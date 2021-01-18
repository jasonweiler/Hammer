using System;
using System.Collections.Generic;

namespace Maul.Extensions
{
    public static class StringExtensions
    {
        public static bool IEquals(this string valueA, string valueB)
        {
            return string.Equals(valueA, valueB, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IContains(this string valueA, string valueB)
        {
            return valueA.IndexOf(valueB, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string JoinString(this string separator, IEnumerable<string> strings)
        {
            return string.Join(separator, strings);
        }
    }
}
