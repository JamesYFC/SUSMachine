using System.Collections.Generic;

namespace KDMagical.SUSMachine
{
    public static class Extensions
    {
        public static V GetOrCreate<K, V>(this IDictionary<K, V> dict, K key) where V : new()
        {
            if (!dict.TryGetValue(key, out var value))
            {
                value = new V();
                dict.Add(key, value);
            }
            return value;
        }

        public static V GetOrCreate<K, V, N>(this IDictionary<K, V> dict, K key) where N : V, new()
        {
            if (!dict.TryGetValue(key, out var value))
            {
                value = new N();
                dict.Add(key, value);
            }
            return value;
        }
    }
}