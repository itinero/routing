/*
 *  Licensed to SharpSoftware under one or more contributor
 *  license agreements. See the NOTICE file distributed with this work for 
 *  additional information regarding copyright ownership.
 * 
 *  SharpSoftware licenses this file to you under the Apache License, 
 *  Version 2.0 (the "License"); you may not use this file except in 
 *  compliance with the License. You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using Itinero.Attributes;
using Reminiscence.Arrays;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Itinero
{
    /// <summary>
    /// Contains general extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets the value associated with the specified key or returns the default.
        /// </summary>
        public static TValue TryGetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            if(dictionary.TryGetValue(key, out value))
            {
                return value;
            }
            return default(TValue);
        }

        /// <summary>
        /// Gets the next power of two.
        /// </summary>
        public static int NextPowerOfTwo(int i)
        {
            if(i == 0)
            {
                return 0;
            }
            else if(i == 1)
            {
                return 1;
            }
            else if(i == 2)
            {
                return 2;
            }
            else if (!((i & (i - 1)) == 0))
            {
                i |= i >> 1;
                i |= i >> 2;
                i |= i >> 4;
                i |= i >> 8;
                i |= i >> 16;
                i++;
            }
            return i;
        }

        /// <summary>
        /// Writes the given value with size prefix.
        /// </summary>
        public static long WriteWithSize(this System.IO.Stream stream, string value)
        {
            var bytes = System.Text.UnicodeEncoding.Unicode.GetBytes(value);
            return stream.WriteWithSize(bytes);
        }

        /// <summary>
        /// Writes the given value with size prefix.
        /// </summary>
        public static long WriteWithSize(this System.IO.Stream stream, string[] values)
        {
            var memoryStream = new MemoryStream();
            for (var i = 0; i < values.Length; i++)
            {
                memoryStream.WriteWithSize(values[i]);
            }
            return stream.WriteWithSize(memoryStream.ToArray());
        }

        /// <summary>
        /// Writes the given value with size prefix.
        /// </summary>
        public static long WriteWithSize(this System.IO.Stream stream, byte[] value)
        {
            stream.Write(System.BitConverter.GetBytes((long)value.Length), 0, 8);
            stream.Write(value, 0, value.Length);
            return value.Length + 8;
        }

        /// <summary>
        /// Writes an attributes collection to the given stream and prefixed with it's size.
        /// </summary>
        public static long WriteWithSize(this IAttributeCollection attributes, System.IO.Stream stream)
        {
            return attributes.SerializeWithSize(stream);
        }

        /// <summary>
        /// Reads an attributes collection.
        /// </summary>
        public static IAttributeCollection ReadWithSizeAttributesCollection(this System.IO.Stream stream)
        {
            return IAttributeCollectionExtension.DeserializeWithSize(stream);
        }

        /// <summary>
        /// Reads a string array.
        /// </summary>
        public static string[] ReadWithSizeStringArray(this System.IO.Stream stream)
        {
            var longBytes = new byte[8];
            stream.Read(longBytes, 0, 8);
            var position = stream.Position;
            var size = System.BitConverter.ToInt64(longBytes, 0);

            var strings = new List<string>();
            while (stream.Position < position + size)
            {
                strings.Add(stream.ReadWithSizeString());
            }
            return strings.ToArray();
        }

        /// <summary>
        /// Reads a string.
        /// </summary>
        public static string ReadWithSizeString(this System.IO.Stream stream)
        {
            var longBytes = new byte[8];
            stream.Read(longBytes, 0, 8);
            var size = BitConverter.ToInt64(longBytes, 0);
            var data = new byte[size];
            stream.Read(data, 0, (int)size);

            return System.Text.UnicodeEncoding.Unicode.GetString(data, 0, data.Length);
        }

        /// <summary>
        /// Reads a string.
        /// </summary>
        public static string ReadToEnd(this System.IO.Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }

        /// <summary>
        /// Sets the position within the given stream. 
        /// </summary>
        public static long SeekBegin(this BinaryWriter stream, long offset)
        {
            if(offset <= int.MaxValue)
            {
                return stream.Seek((int)offset, SeekOrigin.Begin);
            }
            stream.Seek(0, SeekOrigin.Begin);
            while(offset > int.MaxValue)
            {
                stream.Seek(int.MaxValue, SeekOrigin.Current);
                offset -= int.MaxValue;
            }
            return stream.Seek((int)offset, SeekOrigin.Current);
        }
        
        /// <summary>
        /// Returns a string representing the object in a culture invariant way.
        /// </summary>
        public static string ToInvariantString(this object obj)
        {
            return obj is IConvertible ? ((IConvertible)obj).ToString(CultureInfo.InvariantCulture)
                : obj is IFormattable ? ((IFormattable)obj).ToString(null, CultureInfo.InvariantCulture)
                : obj.ToString();
        }

        /// <summary>
        /// Returns a string for the given array even when null.
        /// </summary>
        public static string ToStringSafe<T>(this T[] array)
        {
            if (array == null)
            {
                return "null";
            }
            var stringBuilder = new StringBuilder();
            stringBuilder.Append('[');
            for(var i = 0; i < array.Length; i++)
            {
                if (i > 0)
                {
                    stringBuilder.Append(',');
                }
                stringBuilder.Append(array[i].ToInvariantString());
            }
            stringBuilder.Append(']');
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Shrinks and copies the given list, removes elements with indices in the toRemove set.
        /// </summary>
        public static List<T> ShrinkAndCopyList<T>(this List<T> list, HashSet<int> toRemove)
        {
            var shrinked = new List<T>(System.Math.Max(list.Count - toRemove.Count, 0));
            for (var i = 0; i < list.Count; i++)
            {
                if (!toRemove.Contains(i))
                {
                    shrinked.Add(list[i]);
                }
            }
            return shrinked;
        }

        /// <summary>
        /// Shrinks and copies the given array, removes elements with indices in the toRemove set.
        /// </summary>
        public static T[] ShrinkAndCopyArray<T>(this T[] list, HashSet<int> toRemove)
        {
            return (new List<T>(list)).ShrinkAndCopyList(toRemove).ToArray();
        }

        /// <summary>
        /// Shrinks and copies the matrix and removes rows/columns with indices in the toRemove set.
        /// </summary>
        public static T[][] SchrinkAndCopyMatrix<T>(this T[][] matrix, HashSet<int> toRemove)
        {
            var schrunk = new T[matrix.Length - toRemove.Count][];
            var schrunkX = 0;
            for (var x = 0; x < matrix.Length; x++)
            {
                if (!toRemove.Contains(x))
                { // keep this element.
                    var schrunkY = 0;
                    schrunk[schrunkX] = new T[matrix.Length - toRemove.Count];
                    for (var y = 0; y < matrix[x].Length; y++)
                    {
                        if (!toRemove.Contains(y))
                        { // keep this element.
                            schrunk[schrunkX][schrunkY] = matrix[x][y];
                            schrunkY++;
                        }
                    }
                    schrunkX++;
                }
            }
            return schrunk;
        }

        /// <summary>
        /// Moves this enumerator until a given condition is met.
        /// </summary>
        public static bool MoveNextUntil<T>(this IEnumerator<T> enumerator, Func<T, bool> stopHere)
        {
            while (enumerator.MoveNext())
            {
                if (stopHere(enumerator.Current))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Copies elements to the given target.
        /// </summary>
        public static void CopyTo<T>(this T[] array, T[] target, int index, int sourceIndex, int count)
        {
            for(var i = 0; i < count; i++)
            {
                target[index + i] = array[sourceIndex + i];
            }
        }

        /// <summary>
        /// Creates a new sub-array, containing the element starting at index, with the given length.
        /// </summary>
        public static T[] SubArray<T>(this T[] array, int index, int length)
        {
            var sub = new T[length];
            for(var i = 0; i < length; i++)
            {
                sub[i] = array[i + index];
            }
            return sub;
        }

        /// <summary>
        /// Creates a new array appending the given array.
        /// </summary>
        public static T[] Append<T>(this T[] array, T[] other)
        {
            var result = new T[array.Length + other.Length];
            for(var i = 0; i < array.Length; i++)
            {
                result[i] = array[i];
            }
            for (var i = 0; i < other.Length; i++)
            {
                result[array.Length + i] = other[i];
            }
            return result;
        }

        /// <summary>
        /// Creates a new array appending the given element.
        /// </summary>
        public static T[] Append<T>(this T[] array, T other)
        {
            var result = new T[array.Length + 1];
            for (var i = 0; i < array.Length; i++)
            {
                result[i] = array[i];
            }
            result[array.Length] = other;
            return result;
        }

        /// <summary>
        /// Creates a new array appending the given array but only the first 'count' elements.
        /// </summary>
        public static T[] Append<T>(this T[] array, T[] other, int count)
        {
            var result = new T[array.Length + other.Length];
            for (var i = 0; i < array.Length; i++)
            {
                result[i] = array[i];
            }
            for (var i = 0; i < count; i++)
            {
                result[array.Length + i] = other[i];
            }
            return result;
        }

        /// <summary>
        /// Reverse the elements in the given array.
        /// </summary>
        public static void Reverse<T>(this T[] array)
        {
            System.Array.Reverse(array);
        }
        
        /// <summary>
        /// Returns a copy of the given array with the elements reversed.
        /// </summary>
        public static T[] ReverseAndCopy<T>(this T[] array)
        {
            var reversed = new T[array.Length];
            for(var i = 0; i < array.Length; i++)
            {
                reversed[array.Length - 1 - i] = array[i];
            }
            return reversed;
        }

        /// <summary>
        /// Ensures that this <see cref="ArrayBase{T}"/> has room for at least
        /// the given number of elements, resizing if not.
        /// </summary>
        /// <typeparam name="T">
        /// The type of element stored in this array.
        /// </typeparam>
        /// <param name="array">
        /// This array.
        /// </param>
        /// <param name="minimumSize">
        /// The minimum number of elements that this array must fit.
        /// </param>
        public static void EnsureMinimumSize<T>(this ArrayBase<T> array, long minimumSize)
        {
            if (array.Length < minimumSize)
            {
                IncreaseMinimumSize(array, minimumSize, fillEnd: false, fillValueIfNeeded: default(T));
            }
        }

        /// <summary>
        /// Ensures that this <see cref="ArrayBase{T}"/> has room for at least
        /// the given number of elements, resizing and filling the empty space
        /// with the given value if not.
        /// </summary>
        /// <typeparam name="T">
        /// The type of element stored in this array.
        /// </typeparam>
        /// <param name="array">
        /// This array.
        /// </param>
        /// <param name="minimumSize">
        /// The minimum number of elements that this array must fit.
        /// </param>
        /// <param name="fillValue">
        /// The value to use to fill in the empty spaces if we have to resize.
        /// </param>
        public static void EnsureMinimumSize<T>(this ArrayBase<T> array, long minimumSize, T fillValue)
        {
            if (array.Length < minimumSize)
            {
                IncreaseMinimumSize(array, minimumSize, fillEnd: true, fillValueIfNeeded: fillValue);
            }
        }

        private static void IncreaseMinimumSize<T>(ArrayBase<T> array, long minimumSize, bool fillEnd, T fillValueIfNeeded)
        {
            long oldSize = array.Length;

            // fast-forward, perhaps, through the first several resizes.
            // Math.Max also ensures that we can resize from 0.
            long size = Math.Max(1024, oldSize * 2);
            while (size < minimumSize)
            {
                size *= 2;
            }

            array.Resize(size);
            if (!fillEnd)
            {
                return;
            }

            for (long i = oldSize; i < size; i++)
            {
                array[i] = fillValueIfNeeded;
            }
        }
    }

    /// <summary>
    /// An implementation of the EqualityComparer that allows the use of delegates.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DelegateEqualityComparer<T> : IEqualityComparer<T>
    {
        /// <summary>
        /// A delegate to calculate the hashcode.
        /// </summary>
        public delegate int GetHashCodeDelegate(T obj);

        /// <summary>
        /// A delegate to compare two objects.
        /// </summary>
        public delegate bool EqualsDelegate(T x, T y);

        /// <summary>
        /// Creates a new equality comparer.
        /// </summary>
        public DelegateEqualityComparer(GetHashCodeDelegate hashCodeDelegate, EqualsDelegate equalsDelegate)
        {
            if (hashCodeDelegate == null) { throw new ArgumentNullException("hashCodeDelegate"); }
            if (equalsDelegate == null) { throw new ArgumentNullException("equalsDelegate"); }

            _equalsDelegate = equalsDelegate;
            _hashCodeDelegate = hashCodeDelegate;
        }

        /// <summary>
        /// Holds the equals delegate.
        /// </summary>
        private EqualsDelegate _equalsDelegate;

        /// <summary>
        /// Returns true if the two given objects are considered equal.
        /// </summary>
        public bool Equals(T x, T y)
        {
            return _equalsDelegate.Invoke(x, y);
        }

        /// <summary>
        /// Holds the hashcode delegate.
        /// </summary>
        private GetHashCodeDelegate _hashCodeDelegate;

        /// <summary>
        /// Calculates the hashcode for the given object.
        /// </summary>
        public int GetHashCode(T obj)
        {
            return _hashCodeDelegate.Invoke(obj);
        }
    }
}