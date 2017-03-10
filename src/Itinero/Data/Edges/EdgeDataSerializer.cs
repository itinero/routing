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