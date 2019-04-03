using System;
using System.Collections.Generic;

namespace Daria.Core
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
                action(item);
        }

        public static string Join<T>(this IEnumerable<T> collection, string separator) => string.Join(separator, collection);
    }
}