//// OsmSharp - OpenStreetMap (OSM) SDK
//// Copyright (C) 2015 Abelshausen Ben
//// 
//// This file is part of OsmSharp.
//// 
//// OsmSharp is free software: you can redistribute it and/or modify
//// it under the terms of the GNU General Public License as published by
//// the Free Software Foundation, either version 2 of the License, or
//// (at your option) any later version.
//// 
//// OsmSharp is distributed in the hope that it will be useful,
//// but WITHOUT ANY WARRANTY; without even the implied warranty of
//// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//// GNU General Public License for more details.
//// 
//// You should have received a copy of the GNU General Public License
//// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

//using OsmSharp.Collections.Arrays;
//using System;

//namespace OsmSharp.Routing.Data.Contracted
//{
//    /// <summary>
//    /// Represents the data on a contracted edge.
//    /// </summary>
//    public struct ContractedEdgeData : Graphs.Directed.IEdgeData, Graphs.Directed.IMappedEdgeData<ContractedEdgeData>
//    {
//        /// <summary>
//        /// Bitmask holding status info [forwardMove(1), backwardMove(2), forwardTags(4), contracted(8), shapeInBox(16)]
//        /// </summary>
//        private byte _meta;

//        /// <summary>
//        /// Contains a value that either represents the contracted vertex of the tags id.
//        /// </summary>
//        private uint _value;

//        /// <summary>
//        /// Creates a new contracted edge.
//        /// </summary>
//        public ContractedEdgeData(uint tagsId, bool tagsForward, bool canMoveforward, bool canMoveBackward, float weight)
//            : this()
//        {
//            _meta = 0;

//            this.CanMoveBackward = canMoveBackward;
//            this.CanMoveForward = canMoveforward;
//            this.Forward = tagsForward;
//            this.Weight = weight;
//        }

//        /// <summary>
//        /// Creates a new contracted edge.
//        /// </summary>
//        public ContractedEdgeData(uint tagsId, bool tagsForward, bool canMoveforward, bool canMoveBackward, float weight, bool shapeInBox)
//            : this()
//        {
//            _meta = 0;

//            this.CanMoveBackward = canMoveBackward;
//            this.CanMoveForward = canMoveforward;
//            this.Forward = tagsForward;
//            this.Weight = weight;
//        }

//        /// <summary>
//        /// Creates a new contracted edge.
//        /// </summary>
//        public ContractedEdgeData(uint contractedId, bool canMoveforward, bool canMoveBackward, float weight)
//            : this()
//        {
//            _meta = 0;

//            this.CanMoveBackward = canMoveBackward;
//            this.CanMoveForward = canMoveforward;
//            this.ContractedId = contractedId;
//            this.Weight = weight;
//        }

//        /// <summary>
//        /// Creates a new contracted edge.
//        /// </summary>
//        public ContractedEdgeData(uint contractedId, bool canMoveforward, bool canMoveBackward, float weight, bool shapeInBox)
//            : this()
//        {
//            _meta = 0;

//            this.CanMoveBackward = canMoveBackward;
//            this.CanMoveForward = canMoveforward;
//            this.ContractedId = contractedId;
//            this.Weight = weight;
//        }

//        /// <summary>
//        /// Creates a new contracted edge using raw data.
//        /// </summary>
//        private ContractedEdgeData(uint value, float weight, byte meta)
//            : this()
//        {
//            _meta = meta;
//            _value = value;

//            this.Weight = weight;
//        }

//        /// <summary>
//        /// Gets the raw meta data.
//        /// </summary>
//        public byte Meta
//        {
//            get
//            {
//                return _meta;
//            }
//        }

//        /// <summary>
//        /// Gets the raw value.
//        /// </summary>
//        public uint Value
//        {
//            get
//            {
//                return _value;
//            }
//        }

//        /// <summary>
//        /// Holds the weight.
//        /// </summary>
//        public float Weight { get; private set; }

//        /// <summary>
//        /// Returns true if you can move forward along this edge.
//        /// </summary>
//        public bool CanMoveForward
//        {
//            get
//            {
//                return (_meta & (1 << 0)) != 0;
//            }
//            private set
//            {
//                if (value)
//                {
//                    _meta = (byte)(_meta | 1);
//                }
//                else
//                {
//                    _meta = (byte)(_meta & (255 - 1));
//                }
//            }
//        }

//        /// <summary>
//        /// Returns true if you can move backward along this edge.
//        /// </summary>
//        public bool CanMoveBackward
//        {
//            get
//            {
//                return (_meta & (1 << 1)) != 0;
//            }
//            private set
//            {
//                if (value)
//                {
//                    _meta = (byte)(_meta | 2);
//                }
//                else
//                {
//                    _meta = (byte)(_meta & (255 - 2));
//                }
//            }
//        }

//        /// <summary>
//        /// Holds the forward contracted id.
//        /// </summary>
//        public uint ContractedId
//        {
//            get
//            {
//                if (!this.RepresentsNeighbourRelations)
//                {
//                    return _value;
//                }
//                return uint.MaxValue;
//            }
//            private set
//            {
//                // set contracted.
//                _meta = (byte)(_meta | 8);
//                _value = value;
//            }
//        }

//        /// <summary>
//        /// Returns true when this edge is not contracted and represents an normal neighbour relation.
//        /// </summary>
//        public bool RepresentsNeighbourRelations
//        {
//            get
//            {
//                return !this.IsContracted;
//            }
//        }

//        /// <summary>
//        /// Returns true if this edge is a contracted edge.
//        /// </summary>
//        public bool IsContracted
//        {
//            get
//            {
//                return (_meta & (1 << 3)) != 0;
//            }
//        }

//        /// <summary>
//        /// Flag indicating if the tags are forward relative to this edge or not.
//        /// </summary>
//        public bool Forward
//        {
//            get
//            {
//                return (_meta & (1 << 2)) != 0;
//            }
//            private set
//            {
//                if (value)
//                {
//                    _meta = (byte)(_meta | 4);
//                }
//                else
//                {
//                    _meta = (byte)(_meta & (255 - 4));
//                }
//            }
//        }

//        /// <summary>
//        /// Returns true if the given edge equals this edge.
//        /// </summary>
//        /// <returns></returns>
//        public bool Equals(Graphs.Directed.IEdgeData other)
//        {
//            var otherEdge = (ContractedEdgeData)other;
//            return otherEdge._value == this._value &&
//                otherEdge._meta == this._meta &&
//                otherEdge.Weight == this.Weight;
//        }

//        /// <summary>
//        /// Holds the size this edge has when converted to uints.
//        /// </summary>
//        public static int SizeUints = 3;

//        /// <summary>
//        /// A delegate to map an edge onto uints.
//        /// </summary>
//        public static MappedHugeArray<ContractedEdgeData, uint>.MapFrom MapFromDelegate = (array, idx) =>
//            {
//                return new ContractedEdgeData(
//                    array[idx],
//                    BitConverter.ToSingle(BitConverter.GetBytes(array[idx + 1]), 0),
//                    (byte)array[idx + 2]);
//            };

//        /// <summary>
//        /// A delegate to map an edge onto uints.
//        /// </summary>
//        public static MappedHugeArray<ContractedEdgeData, uint>.MapTo MapToDelegate = (array, idx, value) =>
//            {
//                array[idx] = value.Value;
//                array[idx + 1] = BitConverter.ToUInt32(BitConverter.GetBytes(value.Weight), 0);
//                array[idx + 2] = value.Meta;
//            };

//        MappedHugeArray<ContractedEdgeData, uint>.MapFrom Graphs.Directed.IMappedEdgeData<ContractedEdgeData>.MapFromDelegate
//        {
//            get { return ContractedEdgeData.MapFromDelegate; }
//        }

//        MappedHugeArray<ContractedEdgeData, uint>.MapTo Graphs.Directed.IMappedEdgeData<ContractedEdgeData>.MapToDelegate
//        {
//            get { return ContractedEdgeData.MapToDelegate; }
//        }

//        int Graphs.Directed.IMappedEdgeData<ContractedEdgeData>.MappedSize
//        {
//            get { return ContractedEdgeData.SizeUints; }
//        }
//    }
//}