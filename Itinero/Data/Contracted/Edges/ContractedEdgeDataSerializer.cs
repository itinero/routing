// Itinero - OpenStreetMap (OSM) SDK
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

using Itinero.Graphs.Directed;
using System;

namespace Itinero.Data.Contracted.Edges
{
    /// <summary>
    /// Parses edge data.
    /// </summary>
    public static class ContractedEdgeDataSerializer
    {
        /// <summary>
        /// Holds the precision-factor.
        /// </summary>
        public const int PRECISION_FACTOR = 100;

        /// <summary>
        /// Holds the maxium distance that can be stored on an edge.
        /// </summary>
        public static float MAX_DISTANCE = 4294967000 / 4 / PRECISION_FACTOR;

        /// <summary>
        /// Deserializes edges data.
        /// </summary>
        /// <returns></returns>
        public static ContractedEdgeData Deserialize(uint data, uint metaData)
        {
            float weight;
            bool? direction;
            uint contractedId;
            ContractedEdgeDataSerializer.Deserialize(data, metaData,
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
            weight = ((data0 - dirFlags) / 4.0f) / (float)PRECISION_FACTOR;
            contractedId = data1;
        }

        /// <summary>
        /// Parses the edge data.
        /// </summary>
        public static void DeserializeDynamic(uint[] data, out float weight, out bool? direction, out float distance, out float time)
        {
            ContractedEdgeDataSerializer.Deserialize(data[0], out weight, out direction);
            distance = data[1] / 10.0f;
            time = data[2];
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
            weight = ((data0 - dirFlags) / 4.0f) / (float)PRECISION_FACTOR;
        }

        /// <summary>
        /// Deserializes the agugmented data.
        /// </summary>
        public static void DeserializeMetaAgumented(uint[] data, out uint contractedId, out float distance, out float time)
        {
            contractedId = data[0];
            distance = data[1] / 10.0f;
            time = data[2];
        }

        /// <summary>
        /// Parses the edge data.
        /// </summary>
        /// <returns></returns>
        public static Func<uint[], float> DeserializeWeightFunc = (data) =>
            {
                return ContractedEdgeDataSerializer.DeserializeWeight(data[0]);
            };

        /// <summary>
        /// Parses the edge data.
        /// </summary>
        /// <returns></returns>
        public static float DeserializeWeight(uint data)
        {
            float weight;
            bool? direction;
            ContractedEdgeDataSerializer.Deserialize(data, out weight, out direction);
            return weight;
        }

        /// <summary>
        /// Returns true if the data represents the same direction.
        /// </summary>
        /// <returns></returns>
        public static bool HasDirection(uint data, bool? direction)
        {
            float weight;
            bool? currentDirection;
            ContractedEdgeDataSerializer.Deserialize(data, out weight, out currentDirection);
            return currentDirection == direction;
        }

        /// <summary>
        /// Returns the size of a the meta data in uint's when serialized.
        /// </summary>
        public static int MetaSize
        {
            get { return 1; }
        }

        /// <summary>
        /// Returns the size of a the meta data in uint's when serialized and including augmented weights.
        /// </summary>
        public static int MetaAugmentedSize
        {
            get { return 3; }
        }

        /// <summary>
        /// Returns the size of the fixed component of the data in the dynamic graph.
        /// </summary>
        public static int DynamicFixedSize
        {
            get { return 1; }
        }

        /// <summary>
        /// Returns the size of the fixed component of the data in the dynamic graph including augmented weights.
        /// </summary>
        public static int DynamicAugmentedFixedSize
        {
            get { return 3; }
        }

        /// <summary>
        /// Returns the size of a the data in uint's when serialized.
        /// </summary>
        public static int Size
        {
            get { return 1; }
        }

        /// <summary>
        /// Serializes edge data.
        /// </summary>
        /// <returns></returns>
        public static uint Serialize(float weight, bool? direction)
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
            data0 = data0 + ((uint)(weight * PRECISION_FACTOR) * 4);
            return data0;
        }

        /// <summary>
        /// Serializes augmented edge data.
        /// </summary>
        /// <returns></returns>
        public static uint[] SerializeDynamicAugmented(float weight, bool? direction, float distance, float time)
        { // precision of 0.1m and 1 second.
            return new uint[] {
                Serialize(weight, direction),
                (uint)distance * 10,
                (uint)time
            };
        }

        /// <summary>
        /// Serializes augmented edge data.
        /// </summary>
        /// <returns></returns>
        public static uint[] SerializeMetaAugmented(uint contractedId, float distance, float time)
        { // precision of 0.1m and 1 second.
            return new uint[] {
                contractedId,
                (uint)distance * 10,
                (uint)time
            };
        }

        /// <summary>
        /// Serializes edge data.
        /// </summary>
        /// <returns></returns>
        public static uint[] SerializeMeta(ContractedEdgeData data)
        {
            return new uint[]
            {
                ContractedEdgeDataSerializer.Serialize(data.Weight, data.Direction),
                data.ContractedId
            };
        }

        /// <summary>
        /// Gets contracted edge data.
        /// </summary>
        /// <returns></returns>
        public static ContractedEdgeData GetContractedEdgeData(this MetaEdge edge)
        {
            float weight;
            bool? direction;
            uint contractedId;
            ContractedEdgeDataSerializer.Deserialize(edge.Data[0], edge.MetaData[0],
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
        public static uint GetContractedId(this MetaEdge edge)
        {
            return edge.MetaData[0];
        }
    }
}