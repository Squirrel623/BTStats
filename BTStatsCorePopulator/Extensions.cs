using System;
using System.Collections.Generic;
using System.Text;

namespace BTStatsCorePopulator
{
    public static class Extensions
    {
        public static void AddAndIncrement<T>(this IDictionary<T, int> dict, T key)
        {
            if (!dict.ContainsKey(key))
            {
                dict[key] = 0;
            }
            dict[key]++;
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> func)
        {
            foreach(var item in collection)
            {
                func(item);
            }
        }
    }
}
