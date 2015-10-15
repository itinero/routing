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

using OsmSharp.Routing.Graphs.Directed;
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
        public const float MAX_DISTANCE = 4294967000 / 4 / 10;

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
        /// Parses the edge data.
        /// </summary>
        /// <returns></returns>
        public static void Deserialize(uint data0, uint data1, out float weight, out bool? direction,
            out uint contractedId)
        {
            var dirFlags = (data0 & 3);
            direction = null;
            if(dirFlags == 1)
            {
                direction = true;
            }
            else if(dirFlags == 2)
            {
                direction = false;
            }
            weight = ((data0 - dirFlags) / 4.0f) / 10.0f;
            contractedId = data1;
        }

        /// <summary>
        /// Parses the edge data.
        /// </summary>
        /// <returns></returns>
        public static void Deserialize(uint data0, out float weight, out bool? direction)
        {
            var dirFlags = (data0 & 3);
            direction = null;
            if (dirFlags == 1)
            {
                direction = true;
            }
            else if (dirFlags == 2)
            {
                direction = false;
            }
            weight = ((data0 - dirFlags) / 4.0f) / 10.0f;
        }

        /// <summary>
        /// Parses the edge data.
        /// </summary>
        /// <returns></returns>
        public static Func<uint[], float> DeserializeWeightFunc = (data) =>
            {
                return ContractedEdgeDataSerializer.DeserializeWeight(data);
            };

        /// <summary>
        /// Parses the edge data.
        /// </summary>
        /// <returns></returns>
        public static float DeserializeWeight(uint[] data)
        {
            float weight;
            bool? direction;
            ContractedEdgeDataSerializer.Deserialize(data[0], out weight, out direction);
            return weight;
        }

        /// <summary>
        /// Returns true if the data represents the same direction.
        /// </summary>
        /// <returns></returns>
        public static bool HasDirection(uint[] data, bool? direction)
        {
            float weight;
            bool? currentDirection;
            ContractedEdgeDataSerializer.Deserialize(data[0], out weight, out currentDirection);
            return currentDirection == direction;
        }

        /// <summary>
        /// Returns the size of a the data in uint's when serialized.
        /// </summary>
        public static int Size
        {
            get { return 2; }
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

            var data0 = (uint)dirFlags;
            data0 = data0 + ((uint)(weight * 10) * 4);
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

        /// <summary>
        /// Gets contracted edge data.
        /// </summary>
        /// <returns></returns>
        public static ContractedEdgeData GetContractedEdgeData(this Edge edge)
        {
            float weight;
            bool? direction;
            uint contractedId;
            ContractedEdgeDataSerializer.Deserialize(edge.Data[0], edge.Data[1],
                out weight, out direction, out contractedId);
            return new ContractedEdgeData()
            {
                ContractedId = contractedId,
                Direction = direction,
                Weight = weight
            };
        }

        /// <summary>
        /// Gets contracted id.
        /// </summary>
        /// <returns></returns>
        public static uint GetContractedId(this Edge edge)
        {
            return edge.Data[1];
        }
    }
}