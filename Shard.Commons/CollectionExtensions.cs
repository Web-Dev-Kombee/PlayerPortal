using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shard.Commons
{
    /// <summary>
    /// Provides collection-related convenience extensions
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Safely gets a value from a dictionary
        /// </summary>
        /// <typeparam name="TKey">The type of key to lookup</typeparam>
        /// <typeparam name="TValue">The type of the value to retrieve</typeparam>
        /// <param name="dict">The collection to check</param>
        /// <param name="key">The key to lookup in the provided collection</param>
        /// <returns>The matching value if the key was found, otherwise the default value of TValue if not.</returns>
        public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            return GetValue(dict, key, default(TValue));
        }

        /// <summary>
        /// Safely gets a value from a dictionary
        /// </summary>
        /// <typeparam name="TKey">The type of key to lookup</typeparam>
        /// <typeparam name="TValue">The type of the value to retrieve</typeparam>
        /// <param name="dict">The collection to check</param>
        /// <param name="key">The key to lookup in the provided collection</param>
        /// <param name="defaultVal">The default value to return if the key was not found</param>
        /// <returns>The matching value if the key was found, otherwise the default value if not.</returns>
        public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultVal)
        {
            TValue val;
            if (dict == null || key == null || !dict.TryGetValue(key, out val)) return defaultVal;
            return val;
        }

        /// <summary>
        /// Safely gets a value from a dictionary
        /// </summary>
        /// <typeparam name="TKey">The type of key to lookup</typeparam>
        /// <typeparam name="TValue">The type of the value to retrieve</typeparam>
        /// <param name="dict">The collection to check</param>
        /// <param name="key">The key to lookup in the provided collection</param>
        /// <returns>The matching value if the key was found, otherwise the default value of TValue if not.</returns>
        public static TValue GetValue<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict, TKey key)
        {
            return GetValue(dict, key, default(TValue));
        }

        /// <summary>
        /// Safely gets a value from a dictionary
        /// </summary>
        /// <typeparam name="TKey">The type of key to lookup</typeparam>
        /// <typeparam name="TValue">The type of the value to retrieve</typeparam>
        /// <param name="dict">The collection to check</param>
        /// <param name="key">The key to lookup in the provided collection</param>
        /// <param name="defaultVal">The default value to return if the key was not found</param>
        /// <returns>The matching value if the key was found, otherwise the default value if not.</returns>
        public static TValue GetValue<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict, TKey key, TValue defaultVal)
        {
            TValue val;
            if (dict == null || key == null || !dict.TryGetValue(key, out val)) return defaultVal;
            return val;
        }

        /// <summary>
        /// Safely gets a value from a dictionary
        /// </summary>
        /// <typeparam name="TKey">The type of key to lookup</typeparam>
        /// <typeparam name="TValue">The type of the value to retrieve</typeparam>
        /// <param name="dict">The collection to check</param>
        /// <param name="key">The key to lookup in the provided collection</param>
        /// <returns>The matching value if the key was found, otherwise the default value of TValue if not.</returns>
        public static TValue GetValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) //Dictionary<> is supported to prevent ambiguity with IDictionary and IReadOnlyDictionary
        {
            return GetValue(dict, key, default(TValue));
        }

        /// <summary>
        /// Safely gets a value from a dictionary
        /// </summary>
        /// <typeparam name="TKey">The type of key to lookup</typeparam>
        /// <typeparam name="TValue">The type of the value to retrieve</typeparam>
        /// <param name="dict">The collection to check</param>
        /// <param name="key">The key to lookup in the provided collection</param>
        /// <param name="defaultVal">The default value to return if the key was not found</param>
        /// <returns>The matching value if the key was found, otherwise the default value if not.</returns>
        public static TValue GetValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultVal)
        {
            TValue val;
            if (dict == null || key == null || !dict.TryGetValue(key, out val)) return defaultVal;
            return val;
        }

        /// <summary>
        /// Safely gets a value from a lookup
        /// </summary>
        /// <typeparam name="TKey">The type of key to lookup</typeparam>
        /// <typeparam name="TValue">The type of the value to retrieve</typeparam>
        /// <param name="dict">The collection to check</param>
        /// <param name="key">The key to lookup in the provided collection</param>
        /// <returns>The matching value if the key was found, otherwise the default value of TValue if not.</returns>
        public static TValue GetValue<TKey, TValue>(this ILookup<TKey, TValue> dict, TKey key)
        {
            return GetValue(dict, key, default(TValue));
        }

        /// <summary>
        /// Safely gets a value from a lookup
        /// </summary>
        /// <typeparam name="TKey">The type of key to lookup</typeparam>
        /// <typeparam name="TValue">The type of the value to retrieve</typeparam>
        /// <param name="dict">The collection to check</param>
        /// <param name="key">The key to lookup in the provided collection</param>
        /// <param name="defaultVal">The default value to return if the key was not found</param>
        /// <returns>The matching value if the key was found, otherwise the default value if not.</returns>
        public static TValue GetValue<TKey, TValue>(this ILookup<TKey, TValue> dict, TKey key, TValue defaultVal)
        {
            if (dict == null || key == null || !dict.Contains(key)) return defaultVal;
            return dict[key].FirstOrDefault();
        }

        /// <summary>
        /// Safely gets a value list from a lookup
        /// </summary>
        /// <typeparam name="TKey">The type of key to lookup</typeparam>
        /// <typeparam name="TValue">The type of the value to retrieve</typeparam>
        /// <param name="dict">The collection to check</param>
        /// <param name="key">The key to lookup in the provided collection</param>
        /// <returns>The matching value list if the key was found, otherwise null if not.</returns>
        public static IEnumerable<TValue> GetValues<TKey, TValue>(this ILookup<TKey, TValue> dict, TKey key)
        {
            if (dict == null || key == null || !dict.Contains(key)) return null;
            return dict[key];
        }

        /// <summary>
        /// Safely gets a value from a producer-consumer concurrent collection
        /// </summary>
        /// <typeparam name="T">The type of value to retrieve</typeparam>
        /// <param name="queue">The queue to retrieve from</param>
        /// <returns>The dequeued element, or the default value of T if there was none</returns>
        public static T GetValue<T>(this IProducerConsumerCollection<T> queue)
        {
            T value;
            if (queue.TryTake(out value)) return value;
            else return default(T);
        }

        /// <summary>
        /// Safely gets a value from a producer-consumer concurrent collection
        /// </summary>
        /// <typeparam name="T">The type of value to retrieve</typeparam>
        /// <param name="queue">The queue to retrieve from</param>
        /// <param name="defaultVal">The default value to use if there are no elements to dequeue</param>
        /// <returns>The dequeued element, or the default value if there was none</returns>
        public static T GetValue<T>(this IProducerConsumerCollection<T> queue, T defaultVal)
        {
            T value;
            if (queue.TryTake(out value)) return value;
            else return defaultVal;
        }

        /// <summary>
        /// Retrieves a value if present, or creates one and returns it if not
        /// </summary>
        /// <typeparam name="TKey">The type of key to lookup</typeparam>
        /// <typeparam name="TValue">The type of the value to retrieve</typeparam>
        /// <param name="dict">The collection to check</param>
        /// <param name="key">The key to lookup in the provided collection</param>
        /// <param name="valueFactory">The method that will be called to create a new value, if one was not found</param>
        /// <returns>The matching or newly created value.</returns>
        public static TValue GetOrCreateValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> valueFactory)
        {
            if (dict == null) throw new NullReferenceException("The referenced dictionary is null");
            if (dict.ContainsKey(key)) return dict[key];
            else return dict[key] = valueFactory();
        }

        /// <summary>
        /// Joins an enumerable collection into a string using the default .ToString representation of an object (or "" if null), using the specified joiner
        /// </summary>
        /// <typeparam name="T">The type of object in the collection</typeparam>
        /// <param name="list">The collection to iterate</param>
        /// <param name="joiner">The string sequence to insert between items</param>
        /// <returns>A string containing the joined objects</returns>
        public static string Join<T>(this IEnumerable<T> list, string joiner)
        {
            if (list == null) return null;
            StringBuilder sb = new StringBuilder();
            foreach (var i in list) sb.Append((sb.Length > 0 ? joiner : "") + (i == null ? "" : i.ToString()));
            return sb.ToString();
        }

        /// <summary>
        /// Executes an action for each element in a collection.
        /// </summary>
        /// <typeparam name="T">The type of element in the collection</typeparam>
        /// <param name="list">The collection of elements</param>
        /// <param name="action">The action to call on the collection</param>
        /// <returns>The original collection provided in list (for chainability)</returns>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            foreach (var item in list)
                action(item);
            return list;
        }

        /// <summary>
        /// Compares selected portions of two arrays for equality in their elements
        /// </summary>
        /// <typeparam name="T">The type of element</typeparam>
        /// <param name="left">The array to compare</param>
        /// <param name="leftIdx">The index in the array to start comparing from</param>
        /// <param name="right">The array to compare to</param>
        /// <param name="rightIdx">The index in the array to start comparing to</param>
        /// <param name="count">The number of elements to consider</param>
        /// <returns>true if the same, false if null or otherwise mismatched</returns>
        public static bool CompareTo<T>(this T[] left, int leftIdx, T[] right, int rightIdx, int count)
            where T : IEquatable<T>
        {
            if (left == null
                || right == null
                || leftIdx + count > left.Length
                || rightIdx + count > right.Length) return false;

            for (var i = 0; i < count; i++)
                if (!left[leftIdx + i].Equals(right[rightIdx + i])) return false;
            return true;
        }

        /// <summary>
        /// Compares two arrays for equality in their elements
        /// </summary>
        /// <typeparam name="T">The type of element</typeparam>
        /// <param name="left">The array to compare</param>
        /// <param name="right">The array to compare to</param>
        /// <returns>true if the same, false if null or otherwise mismatched</returns>
        public static bool CompareTo<T>(this T[] left, T[] right)
            where T : IEquatable<T>
        {
            if (left == null
                || right == null
                || left.Length != right.Length)
                return false;
            return CompareTo(left, 0, right, 0, left.Length);
        }

        /// <summary>
        /// Builds a dictionary from a given collection, allowing for duplicate keys in the collection
        /// </summary>
        /// <typeparam name="TSource">The type of items in the collection</typeparam>
        /// <typeparam name="TKey">The type to use as the dictionary key</typeparam>
        /// <param name="source">The collection to use</param>
        /// <param name="keySelector">The method to select the key property from source</param>
        /// <param name="duplicates">The method of duplicate resolution to use</param>
        /// <param name="onDuplicate">An optional action to call when a duplicate is detected</param>
        /// <returns>The constructed dictionary</returns>
        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Duplicates duplicates, Action<TKey, TSource> onDuplicate = null)
        {
            return source.ToDictionary(keySelector, v => v, duplicates, onDuplicate);
        }

        /// <summary>
        /// Builds a dictionary from a given collection, allowing for duplicate keys in the collection
        /// </summary>
        /// <typeparam name="TSource">The type of items in the collection</typeparam>
        /// <typeparam name="TKey">The type to use as the dictionary key</typeparam>
        /// <typeparam name="TElement">The type to use as the dictionary element</typeparam>
        /// <param name="source">The collection to use</param>
        /// <param name="keySelector">The method to select the key property from source</param>
        /// <param name="elementSelector">The method to select or project the element from the source</param>
        /// <param name="duplicates">The method of duplicate resolution to use</param>
        /// <param name="onDuplicate">An optional action to call when a duplicate is detected</param>
        /// <returns>The constructed dictionary</returns>
        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Duplicates duplicates, Action<TKey, TSource> onDuplicate = null)
        {
            var dict = new Dictionary<TKey, TElement>();
            foreach (var item in source)
            {
                var key = keySelector(item);
                var isDup = dict.ContainsKey(key);
                if (!isDup || duplicates == Duplicates.LastInWins) dict[key] = elementSelector(item);
                if (isDup) onDuplicate?.Invoke(key, item);
            }
            return dict;
        }

        /// <summary>
        /// Builds a dictionary from a given collection, allowing for duplicate keys in the collection with a specified comparer
        /// </summary>
        /// <typeparam name="TSource">The type of items in the collection</typeparam>
        /// <typeparam name="TKey">The type to use as the dictionary key</typeparam>
        /// <typeparam name="TElement">The type to use as the dictionary element</typeparam>
        /// <param name="source">The collection to use</param>
        /// <param name="keySelector">The method to select the key property from source</param>
        /// <param name="elementSelector">The method to select or project the element from the source</param>
        /// <param name="comparer">The comparer to use when creating the dictionary</param>
        /// <param name="duplicates">The method of duplicate resolution to use</param>
        /// <param name="onDuplicate">An optional action to call when a duplicate is detected</param>
        /// <returns>The constructed dictionary</returns>
        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer, Duplicates duplicates, Action<TKey, TSource> onDuplicate = null)
        {
            var dict = new Dictionary<TKey, TElement>(comparer);
            foreach (var item in source)
            {
                var key = keySelector(item);
                var isDup = dict.ContainsKey(key);
                if (!isDup || duplicates == Duplicates.LastInWins) dict[key] = elementSelector(item);
                if (isDup) onDuplicate?.Invoke(key, item);
            }
            return dict;
        }

    }
}
