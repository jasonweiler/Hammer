using System.Collections.Generic;
using System.Linq;

namespace Hammer.Support
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TType> WhereNotNull<TType>(this IEnumerable<TType> @this)
        {
            return @this.Where(value => value != null);
        }
    }
}
