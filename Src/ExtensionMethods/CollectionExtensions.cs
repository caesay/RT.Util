﻿using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.Collections;

namespace RT.Util.ExtensionMethods
{
    /// <summary>
    /// Provides extension methods on various collection types or interfaces in the System.Collections.Generic namespace such as <see cref="Dictionary&lt;K,V&gt;"/> and on arrays.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Adds an element to a List&lt;V&gt; stored in the current IDictionary&lt;K, List&lt;V&gt;&gt;.
        /// If the specified key does not exist in the current IDictionary, a new List is created.
        /// </summary>
        /// <typeparam name="K">Type of the key of the IDictionary.</typeparam>
        /// <typeparam name="V">Type of the values in the Lists.</typeparam>
        /// <param name="dic">IDictionary to operate on.</param>
        /// <param name="key">Key at which the list is located in the IDictionary.</param>
        /// <param name="value">Value to add to the List located at the specified Key.</param>
        public static void AddSafe<K, V>(this IDictionary<K, List<V>> dic, K key, V value)
        {
            if (dic == null)
                throw new ArgumentNullException("dic");
            if (key == null)
                throw new ArgumentNullException("key", "Null values cannot be used for keys in dictionaries.");
            if (!dic.ContainsKey(key))
                dic[key] = new List<V>();
            dic[key].Add(value);
        }

        /// <summary>
        /// Adds an element to a two-level Dictionary&lt;,&gt;.
        /// If the specified key does not exist in the outer Dictionary, a new Dictionary is created.
        /// </summary>
        /// <typeparam name="K1">Type of the key of the outer Dictionary.</typeparam>
        /// <typeparam name="K2">Type of the key of the inner Dictionary.</typeparam>
        /// <typeparam name="V">Type of the values in the inner Dictionary.</typeparam>
        /// <param name="dic">Dictionary to operate on.</param>
        /// <param name="key1">Key at which the inner Dictionary is located in the outer Dictionary.</param>
        /// <param name="key2">Key at which the value is located in the inner Dictionary.</param>
        /// <param name="value">Value to add to the inner Dictionary.</param>
        public static void AddSafe<K1, K2, V>(this IDictionary<K1, Dictionary<K2, V>> dic, K1 key1, K2 key2, V value)
        {
            if (dic == null)
                throw new ArgumentNullException("dic");
            if (key1 == null)
                throw new ArgumentNullException("key1", "Null values cannot be used for keys in dictionaries.");
            if (key2 == null)
                throw new ArgumentNullException("key2", "Null values cannot be used for keys in dictionaries.");
            if (!dic.ContainsKey(key1))
                dic[key1] = new Dictionary<K2, V>();
            dic[key1][key2] = value;
        }

        /// <summary>
        /// Adds an element to a List&lt;V&gt; stored in a two-level Dictionary&lt;,&gt;.
        /// If the specified key does not exist in the current Dictionary, a new List is created.
        /// </summary>
        /// <typeparam name="K1">Type of the key of the first-level Dictionary.</typeparam>
        /// <typeparam name="K2">Type of the key of the second-level Dictionary.</typeparam>
        /// <typeparam name="V">Type of the values in the Lists.</typeparam>
        /// <param name="dic">Dictionary to operate on.</param>
        /// <param name="key1">Key at which the second-level Dictionary is located in the first-level Dictionary.</param>
        /// <param name="key2">Key at which the list is located in the second-level Dictionary.</param>
        /// <param name="value">Value to add to the List located at the specified Keys.</param>
        public static void AddSafe<K1, K2, V>(this IDictionary<K1, Dictionary<K2, List<V>>> dic, K1 key1, K2 key2, V value)
        {
            if (dic == null)
                throw new ArgumentNullException("dic");
            if (key1 == null)
                throw new ArgumentNullException("key1", "Null values cannot be used for keys in dictionaries.");
            if (key2 == null)
                throw new ArgumentNullException("key2", "Null values cannot be used for keys in dictionaries.");
            if (!dic.ContainsKey(key1))
                dic[key1] = new Dictionary<K2, List<V>>();
            if (!dic[key1].ContainsKey(key2))
                dic[key1][key2] = new List<V>();
            dic[key1][key2].Add(value);
        }

        /// <summary>
        /// Increments an integer in an <see cref="IDictionary&lt;K, V&gt;"/> by 1. If the specified key does
        /// not exist in the current dictionary, the value 1 is inserted.
        /// </summary>
        /// <typeparam name="K">Type of the key of the dictionary.</typeparam>
        /// <param name="dic">Dictionary to operate on.</param>
        /// <param name="key">Key at which the list is located in the dictionary.</param>
        public static void IncSafe<K>(this IDictionary<K, int> dic, K key)
        {
            if (dic == null)
                throw new ArgumentNullException("dic");
            if (key == null)
                throw new ArgumentNullException("key", "Null values cannot be used for keys in dictionaries.");
            if (!dic.ContainsKey(key))
                dic[key] = 1;
            else
                dic[key] = dic[key] + 1;
        }

        /// <summary>
        /// Increments an integer in an <see cref="IDictionary&lt;K, V&gt;"/> by the specified amount.
        /// If the specified key does not exist in the current dictionary, the value <paramref name="amount"/> is inserted.
        /// </summary>
        /// <typeparam name="K">Type of the key of the dictionary.</typeparam>
        /// <param name="dic">Dictionary to operate on.</param>
        /// <param name="key">Key at which the list is located in the dictionary.</param>
        /// <param name="amount">The amount by which to increment the integer.</param>
        public static void IncSafe<K>(this IDictionary<K, int> dic, K key, int amount)
        {
            if (dic == null)
                throw new ArgumentNullException("dic");
            if (key == null)
                throw new ArgumentNullException("key", "Null values cannot be used for keys in dictionaries.");
            if (!dic.ContainsKey(key))
                dic[key] = amount;
            else
                dic[key] = dic[key] + amount;
        }

        /// <summary>
        /// Brings the elements of the given list into a random order
        /// </summary>
        /// <typeparam name="T">Type of elements in the list.</typeparam>
        /// <param name="list">List to shuffle.</param>
        /// <returns>The list operated on.</returns>
        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            for (int j = list.Count; j >= 1; j--)
            {
                int item = Rnd.Next(0, j);
                if (item < j - 1)
                {
                    var t = list[item];
                    list[item] = list[j - 1];
                    list[j - 1] = t;
                }
            }
            return list;
        }

        /// <summary>
        /// Compares two dictionaries for equality, member-wise. Two dictionaries are equal if
        /// they contain all the same key-value pairs.
        /// </summary>
        public static bool DictionaryEqual<TK, TV>(this IDictionary<TK, TV> dictA, IDictionary<TK, TV> dictB)
            where TV : IEquatable<TV>
        {
            if (dictA == null)
                throw new ArgumentNullException("dictA");
            if (dictB == null)
                throw new ArgumentNullException("dictB");
            if (dictA.Count != dictB.Count)
                return false;
            foreach (var key in dictA.Keys)
            {
                if (!dictB.ContainsKey(key))
                    return false;
                if (!dictA[key].Equals(dictB[key]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Performs a binary search for the specified key on a <see cref="SortedList&lt;TK,TV&gt;"/>. When no
        /// match exists, returns the nearest indices for interpolation/extrapolation purposes.
        /// </summary>
        /// <remarks>
        /// If an exact match exists, index1 == index2 == the index of the match. If an exact match
        /// is not found, index1 &lt; index2. If the key is less than every key in the list, index1
        /// is int.MinValue and index2 is 0. If it's greater than every key, index1 = last item
        /// index and index2 = int.MaxValue. Otherwise index1 and index2 are the indices of the items
        /// that would surround the key were it present in the list.
        /// </remarks>
        /// <param name="list">List to operate on.</param>
        /// <param name="key">The key to look for.</param>
        /// <param name="index1">Receives the value of the first index (see remarks).</param>
        /// <param name="index2">Receives the value of the second index (see remarks).</param>
        public static void BinarySearch<TK, TV>(this SortedList<TK, TV> list, TK key, out int index1, out int index2)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (key == null)
                throw new ArgumentNullException("key", "Null values cannot be used for keys in SortedList.");

            var keys = list.Keys;
            var comparer = Comparer<TK>.Default;

            int imin = 0;
            int imax = (0 + keys.Count) - 1;
            while (imin <= imax)
            {
                int inew = imin + ((imax - imin) >> 1);

                int cmp_res;
                try { cmp_res = comparer.Compare(keys[inew], key); }
                catch (Exception exception) { throw new InvalidOperationException("SortedList.BinarySearch could not compare keys due to a comparer exception.", exception); }

                if (cmp_res == 0)
                {
                    index1 = index2 = inew;
                    return;
                }
                else if (cmp_res < 0)
                {
                    imin = inew + 1;
                }
                else
                {
                    imax = inew - 1;
                }
            }

            index1 = imax; // we know that imax + 1 == imin
            index2 = imin;
            if (imax < 0)
                index1 = int.MinValue;
            if (imin >= keys.Count)
                index2 = int.MaxValue;
        }

        /// <summary>
        /// Gets a value from a dictionary by key. If the key does not exist in the dictionary,
        /// the default value is returned instead.
        /// </summary>
        /// <param name="dict">Dictionary to operate on.</param>
        /// <param name="key">Key to look up.</param>
        /// <param name="defaultVal">Value to return if key is not contained in the dictionary.</param>
        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultVal)
        {
            if (dict == null)
                throw new ArgumentNullException("dict");
            if (key == null)
                throw new ArgumentNullException("key", "Null values cannot be used for keys in dictionaries.");
            TValue value;
            if (dict.TryGetValue(key, out value))
                return value;
            else
                return defaultVal;
        }

        /// <summary>
        /// Similar to <see cref="string.Substring(int)"/>, only for arrays. Returns a new array containing
        /// all items from the specified <paramref name="startIndex"/> onwards.
        /// </summary>
        public static T[] Subarray<T>(this T[] array, int startIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (startIndex < 0 || startIndex > array.Length)
                throw new ArgumentOutOfRangeException("startIndex", "startIndex cannot be negative or extend beyond the end of the array.");
            int length = array.Length - startIndex;
            T[] result = new T[length];
            Array.Copy(array, startIndex, result, 0, length);
            return result;
        }

        /// <summary>
        /// Similar to <see cref="string.Substring(int,int)"/>, only for arrays. Returns a new array containing
        /// <paramref name="length"/> items from the specified <paramref name="startIndex"/> onwards.
        /// </summary>
        public static T[] Subarray<T>(this T[] array, int startIndex, int length)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex", "startIndex cannot be negative.");
            if (length < 0 || startIndex + length > array.Length)
                throw new ArgumentOutOfRangeException("length", "length cannot be negative or extend beyond the end of the array.");
            T[] result = new T[length];
            Array.Copy(array, startIndex, result, 0, length);
            return result;
        }

        /// <summary>
        /// Determines whether the two arrays contain the same content in the specified location.
        /// </summary>
        public static bool SubarrayEquals<T>(this T[] sourceArray, int sourceStartIndex, T[] otherArray, int otherStartIndex, int length) where T : IEquatable<T>
        {
            if (sourceArray == null)
                throw new ArgumentNullException("sourceArray");
            if (otherArray == null)
                throw new ArgumentNullException("otherArray");
            if (sourceStartIndex < 0 || length < 0 || otherStartIndex < 0 || sourceStartIndex + length > sourceArray.Length || otherStartIndex + length > otherArray.Length)
                throw new ArgumentOutOfRangeException();
            for (int i = 0; i < length; i++)
            {
                if (!sourceArray[sourceStartIndex + i].Equals(otherArray[otherStartIndex + i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Searches the current array for a specified subarray and returns the index of the first occurrence, or -1 if not found.
        /// </summary>
        /// <param name="sourceArray">Array in which to search for the subarray.</param>
        /// <param name="findWhat">Subarray to search for.</param>
        /// <param name="startIndex">Index in <paramref name="sourceArray"/> at which to start searching.</param>
        /// <param name="sourceLength">Maximum length of the source array to search. The greatest index that can be returned is this minus the length of <paramref name="findWhat"/>.</param>
        public static int IndexOfSubarray<T>(this T[] sourceArray, T[] findWhat, int startIndex, int sourceLength) where T : IEquatable<T>
        {
            if (sourceArray == null)
                throw new ArgumentNullException("sourceArray");
            if (findWhat == null)
                throw new ArgumentNullException("findWhat");
            if (startIndex < 0 || startIndex > sourceArray.Length)
                throw new ArgumentOutOfRangeException();
            var maxIndex = sourceLength - findWhat.Length;
            for (int i = startIndex; i <= maxIndex; i++)
            {
                if (sourceArray.SubarrayEquals(i, findWhat, 0, findWhat.Length))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Creates and returns a read-only wrapper around this collection.
        /// Note: a new wrapper is created on every call. Consider caching it.
        /// </summary>
        public static ReadOnlyCollection<T> AsReadOnly<T>(this ICollection<T> coll)
        {
            if (coll == null)
                throw new ArgumentNullException("coll");
            return new ReadOnlyCollection<T>(coll);
        }

        /// <summary>
        /// Gets a read-only wrapper around this collection. If <paramref name="cache"/> is already
        /// a wrapper for this collection returns that, otherwise creates a new wrapper, stores it
        /// in <paramref name="cache"/>, and returns that.
        /// </summary>
        public static ReadOnlyCollection<T> AsReadOnly<T>(this ICollection<T> coll, ref ReadOnlyCollection<T> cache)
        {
            if (coll == null)
                throw new ArgumentNullException("coll");
            if (cache == null || !cache.IsWrapperFor(coll))
                cache = new ReadOnlyCollection<T>(coll);
            return cache;
        }

        /// <summary>
        /// Creates and returns a read-only wrapper around this dictionary.
        /// Note: a new wrapper is created on every call. Consider caching it.
        /// </summary>
        public static ReadOnlyDictionary<TK, TV> AsReadOnly<TK, TV>(this IDictionary<TK, TV> dict)
        {
            if (dict == null)
                throw new ArgumentNullException("dict");
            return new ReadOnlyDictionary<TK, TV>(dict);
        }

        /// <summary>
        /// Gets a read-only wrapper around this dictionary. If <paramref name="cache"/> is already
        /// a wrapper for this dictionary returns that, otherwise creates a new wrapper, stores it
        /// in <paramref name="cache"/>, and returns that.
        /// </summary>
        public static ReadOnlyDictionary<TK, TV> AsReadOnly<TK, TV>(this IDictionary<TK, TV> dict, ref ReadOnlyDictionary<TK, TV> cache)
        {
            if (dict == null)
                throw new ArgumentNullException("dict");
            if (cache == null || !cache.IsWrapperFor(dict))
                cache = new ReadOnlyDictionary<TK, TV>(dict);
            return cache;
        }

        /// <summary>
        /// Creates a new dictionary containing the union of the key/value pairs contained in the specified dictionaries.
        /// Keys in <paramref name="second"/> overwrite keys in <paramref name="first"/>.
        /// </summary>
        public static IDictionary<TKey, TValue> CopyMerge<TKey, TValue>(this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second)
        {
            if (first == null)
                throw new ArgumentNullException("first");
            if (second == null)
                throw new ArgumentNullException("second");
            Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>(first);
            foreach (var kvp in second)
                dict.Add(kvp.Key, kvp.Value);
            return dict;
        }

        /// <summary>
        /// Generates a representation of the specified byte array as hexadecimal numbers (“hexdump”).
        /// </summary>
        public static string ToHex(this byte[] byteArray)
        {
            if (byteArray == null)
                throw new ArgumentNullException("byteArray");

            char[] charArr = new char[byteArray.Length * 2];
            var j = 0;
            for (int i = 0; i < byteArray.Length; i++)
            {
                byte b = (byte) (byteArray[i] >> 4);
                charArr[j] = (char) (b < 10 ? '0' + b : 'W' + b);   // 'a'-10 = 'W'
                j++;
                b = (byte) (byteArray[i] & 0xf);
                charArr[j] = (char) (b < 10 ? '0' + b : 'W' + b);
                j++;
            }
            return new string(charArr);
        }

        /// <summary>Removes all entries from a dictionary that satisfy a specified predicate.</summary>
        /// <typeparam name="TKey">Type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TVal">Type of the values in the dictionary.</typeparam>
        /// <param name="dict">Dictionary to operate on.</param>
        /// <param name="predicate">Specifies a predicate that determines which entries should be removed from the dictionary.</param>
        public static void RemoveAll<TKey, TVal>(this IDictionary<TKey, TVal> dict, Predicate<KeyValuePair<TKey, TVal>> predicate)
        {
            foreach (var kvp in dict.Where(kvp => predicate(kvp)).ToArray())
                dict.Remove(kvp.Key);
        }

        /// <summary>Removes all entries from a dictionary whose keys satisfy a specified predicate.</summary>
        /// <typeparam name="TKey">Type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TVal">Type of the values in the dictionary.</typeparam>
        /// <param name="dict">Dictionary to operate on.</param>
        /// <param name="predicate">Specifies a predicate that determines which entries should be removed from the dictionary.</param>
        public static void RemoveAllByKey<TKey, TVal>(this IDictionary<TKey, TVal> dict, Predicate<TKey> predicate)
        {
            foreach (var kvp in dict.Where(kvp => predicate(kvp.Key)).ToArray())
                dict.Remove(kvp.Key);
        }

        /// <summary>Removes all entries from a dictionary whose values satisfy a specified predicate.</summary>
        /// <typeparam name="TKey">Type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TVal">Type of the values in the dictionary.</typeparam>
        /// <param name="dict">Dictionary to operate on.</param>
        /// <param name="predicate">Specifies a predicate that determines which entries should be removed from the dictionary.</param>
        public static void RemoveAllByValue<TKey, TVal>(this IDictionary<TKey, TVal> dict, Predicate<TVal> predicate)
        {
            foreach (var kvp in dict.Where(kvp => predicate(kvp.Value)).ToArray())
                dict.Remove(kvp.Key);
        }

        /// <summary>Enqueues several values into a <see cref="Queue&lt;T&gt;"/>.</summary>
        /// <typeparam name="T">Type of the elements in the queue.</typeparam>
        /// <param name="queue">Queue to insert items into.</param>
        /// <param name="values">Values to enqueue.</param>
        public static void EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> values)
        {
            foreach (var value in values)
                queue.Enqueue(value);
        }
    }
}
