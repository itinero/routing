// Itinero - Routing for .NET
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using System;

namespace Itinero.Data.Edges
{
    /// <summary>
    /// Parses edge data.
    /// </summary>
    public static class EdgeDataSerializer
    {
        /// <summary>
        /// Holds the maximum profile count.
        /// </summary>
        public const ushort MAX_PROFILE_COUNT = (ushort)(1 << 14);

        /// <summary>
        /// Holds the maxium distance that can be stored on an edge.
        /// </summary>
        public const float MAX_DISTANCE = (((uint.MaxValue - 1) >> 14) / 10.0f);

        /// <summary>
        /// Parses the profile id.
        /// </summary>
        /// <returns></returns>
        public static void Deserialize(uint value, out float distance, out ushort profile)
        {
            distance = (value >> 14) / 10f;
            profile = (ushort)(value & (uint)((1 << 14) - 1));
        }

        /// <summary>
        /// Returns the size of a the data in uint's when serialized.
        /// </summary>
        public static int Size
        {
            get { return 1; }
        }

        /// <summary>
        /// Deserializes edges data.
        /// </summary>
        /// <returns></returns>
        public static EdgeData Deserialize(uint[] data)
        {
            float distance;
            ushort profile;
            EdgeDataSerializer.Deserialize(data[0], out distance, out profile);

            return new EdgeData()
            {
                Profile = profile,
                Distance = distance
            };
        }

        /// <summary>
        /// Serializes edge data.
        /// </summary>
        /// <returns></returns>
        public static uint[] Serialize(float distance, ushort profile)
        {
            if (distance > MAX_DISTANCE)
            {
                throw new ArgumentOutOfRangeException("Cannot store distance on edge, too big.");
            }
            if (distance < 0)
            {
                throw new ArgumentOutOfRangeException("Cannot store distance on edge, too small.");
            }
            if (profile >= MAX_PROFILE_COUNT)
            {
                throw new ArgumentOutOfRangeException("Cannot store profile id on edge, too big.");
            }

            var serDistance = (uint)(distance * 10) << 14;
            uint value = profile + serDistance;
            return new uint[] { value };
        }

        /// <summary>
        /// Serializes edge data.
        /// </summary>
        /// <returns></returns>
        public static uint[] Serialize(EdgeData data)
        {
            return EdgeDataSerializer.Serialize(data.Distance, data.Profile);
        }
    }
}