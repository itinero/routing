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

using System;

namespace OsmSharp.Routing.Data.Contracted
{
    /// <summary>
    /// Parses edge data.
    /// </summary>
    public static class ContractedEdgeDataSerializer
    {
        /// <summary>
        /// Holds the maxium distance that can be stored on an edge.
        /// </summary>
        public const float MAX_DISTANCE = ((uint.MaxValue >> 2) / 10.0f);

        /// <summary>
        /// Parses the profile id.
        /// </summary>
        /// <returns></returns>
        public static void Deserialize(uint data0, uint data1, out float weight, out bool? direction,
            out uint contractedId)
        {
            var dirFlags = (data0 & ((uint)3 << 30)) >> 30;
            direction = null;
            if(dirFlags == 1)
            {
                direction = true;
            }
            else if(dirFlags == 2)
            {
                direction = false;
            }
            weight = (data0 >> 2) / 10f;
            contractedId = data1;
        }

        /// <summary>
        /// Returns the size of a the data in uint's when serialized.
        /// </summary>
        public static int Size
        {
            get { return 2; }
        }

        /// <summary>
        /// Deserializes edges data.
        /// </summary>
        /// <returns></returns>
        public static ContractedEdgeData Deserialize(uint[] data)
        {
            float weight;
            bool? direction;
            uint contractedId;
            ContractedEdgeDataSerializer.Deserialize(data[0], data[1], 
                out weight, out direction, out contractedId);

            return new ContractedEdgeData()
            {
                ContractedId = contractedId,
                Weight = weight,
                Direction = direction
            };
        }

        /// <summary>
        /// Serializes edge data.
        /// </summary>
        /// <returns></returns>
        public static uint[] Serialize(float weight, bool? direction, uint contractedId)
        {
            if (weight > MAX_DISTANCE)
            {
                throw new ArgumentOutOfRangeException("Cannot store distance on edge, too big.");
            }
            if (weight < 0)
            {
                throw new ArgumentOutOfRangeException("Cannot store distance on edge, too small.");
            }

            var dirFlags = 0;
            if(direction.HasValue && direction.Value)
            {
                dirFlags = 1;
            }
            else if(direction.HasValue && !direction.Value)
            {
                dirFlags = 2;
            }

            var data0 = (uint)(dirFlags) << 30;
            data0 = data0 + (uint)(weight / 10F);
            weight = (data0 >> 2) / 10f;
            var data1 = contractedId;

            return new uint[] { data0, data1 };
        }

        /// <summary>
        /// Serializes edge data.
        /// </summary>
        /// <returns></returns>
        public static uint[] Serialize(ContractedEdgeData data)
        {
            return ContractedEdgeDataSerializer.Serialize(data.Weight, data.Direction, data.ContractedId);
        }
    }
}