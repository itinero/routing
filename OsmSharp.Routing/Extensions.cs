// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using OsmSharp.Collections.Tags;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics.Contracts;

namespace OsmSharp.Routing
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
        /// Writes a tags collection to the given stream and prefixed with it's size.
        /// </summary>
        public static long WriteWithSize(this TagsCollectionBase tags, System.IO.Stream stream)
        {
            var serializer = new OsmSharp.Collections.Tags.Serializer.TagsCollectionSerializer();
            var position = stream.Position;
            serializer.SerializeWithSize(tags, stream);
            return stream.Position - position;
        }

        /// <summary>
        /// Reads a tags collection.
        /// </summary>
        public static TagsCollectionBase ReadWithSizeTagsCollection(this System.IO.Stream stream)
        {
            var serializer = new OsmSharp.Collections.Tags.Serializer.TagsCollectionSerializer();
            return serializer.DeserializeWithSize(stream);
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
            var size = System.BitConverter.ToInt64(longBytes, 0);
            var data = new byte[size];
            stream.Read(data, 0, (int)size);

            return System.Text.UnicodeEncoding.Unicode.GetString(data);
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

        ///// <summary>
        ///// Sets the position within the given stream. 
        ///// </summary>
        //public static long SeekBegin(this Stream stream, long offset)
        //{
        //    if (offset <= int.MaxValue)
        //    {
        //        return stream.Seek((int)offset, SeekOrigin.Begin);
        //    }
        //    stream.Seek(0, SeekOrigin.Begin);
        //    while (offset > int.MaxValue)
        //    {
        //        stream.Seek(int.MaxValue, SeekOrigin.Current);
        //        offset -= int.MaxValue;
        //    }
        //    return stream.Seek((int)offset, SeekOrigin.Current);
        //}
    }
}