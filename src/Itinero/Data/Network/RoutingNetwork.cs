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

using Itinero.LocalGeo;
using Itinero.Graphs.Geometric;
using Itinero.Graphs.Geometric.Shapes;
using Reminiscence.Arrays;
using Reminiscence.IO;
using Reminiscence.IO.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using Itinero.Data.Network.Edges;

namespace Itinero.Data.Network
{
    /// <summary>
    /// Represents a routing network.
    /// </summary>
    public class RoutingNetwork
    {
        private readonly float _maxEdgeDistance;
        private readonly GeometricGraph _graph;
        private readonly ArrayBase<uint> _edgeData;
        private readonly int _edgeDataSize = 2;

        /// <summary>
        /// Creates a new routing network.
        /// </summary>
        public RoutingNetwork(float maxEdgeDistance = Constants.DefaultMaxEdgeDistance)
        {
            _graph = new GeometricGraph(1);
            _edgeData = Context.ArrayFactory.CreateMemoryBackedArray<uint>(_edgeDataSize * _graph.EdgeCount);
            _maxEdgeDistance = maxEdgeDistance;
        }
        

        /// <summary>
        /// Creates a new routing network.
        /// </summary>
        public RoutingNetwork(MemoryMap map, 
            float maxEdgeDistance = Constants.DefaultMaxEdgeDistance)
        {
            _graph = new GeometricGraph(map, 1);
            _edgeData = Context.ArrayFactory.CreateMemoryBackedArray<uint>(_edgeDataSize * _graph.EdgeCount);
            _maxEdgeDistance = maxEdgeDistance;
        }

        /// <summary>
        /// Creates a new routing network.
        /// </summary>
        public RoutingNetwork(MemoryMap map, RoutingNetworkProfile profile, 
            float maxEdgeDistance = Constants.DefaultMaxEdgeDistance)
        {
            _maxEdgeDistance = maxEdgeDistance;
            if (profile == null)
            {
                _graph = new GeometricGraph(map, 1);
                _edgeData = new Array<uint>(map, _edgeDataSize * _graph.EdgeCount);
            }
            else
            {
                _graph = new GeometricGraph(map, profile.GeometricGraphProfile, 1);
                _edgeData = new Array<uint>(map, _edgeDataSize * _graph.EdgeCount, profile.EdgeDataProfile);
            }
        }

        /// <summary>
        /// Creates a new routing network.
        /// </summary>
        public RoutingNetwork(GeometricGraph graph, 
            float maxEdgeDistance = Constants.DefaultMaxEdgeDistance)
        {
            _graph = graph;
            _edgeData = Context.ArrayFactory.CreateMemoryBackedArray<uint>(_edgeDataSize * graph.EdgeCount);
            _maxEdgeDistance = maxEdgeDistance;
        }

        /// <summary>
        /// Creates a new routing network.
        /// </summary>
        public RoutingNetwork(MemoryMap map, GeometricGraph graph, 
            float maxEdgeDistance = Constants.DefaultMaxEdgeDistance)
        {
            _graph = graph;
            _edgeData = Context.ArrayFactory.CreateMemoryBackedArray<uint>(_edgeDataSize * graph.EdgeCount);
            _maxEdgeDistance = maxEdgeDistance;
        }

        /// <summary>
        /// Creates a new routing network from existing data.
        /// </summary>
        private RoutingNetwork(GeometricGraph graph, ArrayBase<uint> edgeData, 
            float maxEdgeDistance = Constants.DefaultMaxEdgeDistance)
        {
            _graph = graph;
            _edgeData = edgeData;
            _maxEdgeDistance = maxEdgeDistance;
        }

        /// <summary>
        /// Returns the maximum edge distance.
        /// </summary>
        public float MaxEdgeDistance
        {
            get
            {
                return _maxEdgeDistance;
            }
        }

        /// <summary>
        /// Returns the geometric graph.
        /// </summary>
        public GeometricGraph GeometricGraph
        {
            get
            {
                return _graph;
            }
        }
        
        /// <summary>
        /// Returns true if this network is simple (max one edge between any two vertices).
        /// </summary>
        /// <returns></returns>
        public bool IsSimple
        {
            get
            {
                return _graph.IsSimple;
            }
        }

        /// <summary>
        /// Adds a new vertex.
        /// </summary>
        public void AddVertex(uint vertex, float latitude, float longitude)
        {
            _graph.AddVertex(vertex, latitude, longitude);
        }

        /// <summary>
        /// Adds a new vertex.
        /// </summary>
        public void AddVertex(uint vertex, float latitude, float longitude, short? elevation)
        {
            _graph.AddVertex(vertex, latitude, longitude, elevation);
        }

        /// <summary>
        /// Gets the vertex.
        /// </summary>
        public Coordinate GetVertex(uint vertex)
        {
            return _graph.GetVertex(vertex);
        }

        /// <summary>
        /// Gets the vertex.
        /// </summary>
        /// <returns></returns>
        public bool GetVertex(uint vertex, out float latitude, out float longitude)
        {
            return _graph.GetVertex(vertex, out latitude, out longitude);
        }

        /// <summary>
        /// Gets the vertex.
        /// </summary>
        /// <returns></returns>
        public bool RemoveVertex(uint vertex)
        {
            return _graph.RemoveVertex(vertex);
        }
        
        /// <summary>
        /// Adds a new edge.
        /// </summary>
        /// <returns></returns>
        public uint AddEdge(uint vertex1, uint vertex2, EdgeData data, ShapeBase shape)
        {
            if(data.Distance > _maxEdgeDistance) { throw new ArgumentException("data.Distance too big for this network."); }

            var edgeId = _graph.AddEdge(vertex1, vertex2,
                Data.Edges.EdgeDataSerializer.Serialize(
                    data.Distance, data.Profile), shape);
            _edgeData.EnsureMinimumSize(edgeId + 1);
            _edgeData[edgeId] = data.MetaId;
            return edgeId;
        }

        /// <summary>
        /// Updates the data associated with the given edge.
        /// </summary>
        public void UpdateEdgeData(uint edgeId, EdgeData data)
        {
            if (edgeId >= _edgeData.Length)
            {
                throw new ArgumentException("Edge is not part of this network.");
            }
            _graph.UpdateEdgeData(edgeId, Data.Edges.EdgeDataSerializer.Serialize(
                    data.Distance, data.Profile));
            _edgeData[edgeId] = data.MetaId;
        }

        /// <summary>
        /// Gets the edge with the given id.
        /// </summary>
        /// <returns></returns>
        public RoutingEdge GetEdge(uint edgeId)
        {
            var edge = _graph.GetEdge(edgeId);

            var baseEdgeData = Data.Edges.EdgeDataSerializer.Deserialize(
                edge.Data);
            var edgeData = new EdgeData()
            {
                MetaId = _edgeData[edgeId],
                Distance = baseEdgeData.Distance,
                Profile = baseEdgeData.Profile
            };
            return new RoutingEdge(edge.Id, edge.From, edge.To, edgeData, edge.DataInverted, edge.Shape);
        }

        /// <summary>
        /// Removes all edges from/to the given vertex.
        /// </summary>
        /// <returns></returns>
        public int RemoveEdges(uint vertex)
        {
            return _graph.RemoveEdges(vertex);
        }

        /// <summary>
        /// Removes the given edge.
        /// </summary>
        /// <returns></returns>
        public bool RemoveEdge(uint edgeId)
        {
            return _graph.RemoveEdge(edgeId);
        }

        /// <summary>
        /// Removes the given edge.
        /// </summary>
        /// <returns></returns>
        public int RemoveEdges(uint vertex1, uint vertex2)
        {
            return _graph.RemoveEdges(vertex1, vertex2);
        }

        /// <summary>
        /// Switches the two vertices.
        /// </summary>
        public void Switch(uint vertex1, uint vertex2)
        {
            // switch vertices, edges do not change id's.
            _graph.Switch(vertex1, vertex2);
        }

        /// <summary>
        /// Gets an empty edge enumerator.
        /// </summary>
        /// <returns></returns>
        public EdgeEnumerator GetEdgeEnumerator()
        {
            return new EdgeEnumerator(this, _graph.GetEdgeEnumerator());
        }

        /// <summary>
        /// Gets an edge enumerator for the given vertex.
        /// </summary>
        /// <returns></returns>
        public EdgeEnumerator GetEdgeEnumerator(uint vertex)
        {
            return new EdgeEnumerator(this, _graph.GetEdgeEnumerator(vertex));
        }

        /// <summary>
        /// Relocates data internally in the most compact way possible.
        /// </summary>
        public void Compress()
        {
            _graph.Compress((originalId, newId) =>
                {
                    _edgeData[newId] = _edgeData[originalId];
                });
            _edgeData.Resize(_graph.EdgeCount);
        }

        /// <summary>
        /// Relocates data internally in the most compact way possible.
        /// </summary>
        public void Compress(Action<uint, uint> updateEdgeId)
        {
            _graph.Compress((originalId, newId) =>
                {
                    _edgeData[newId] = _edgeData[originalId];

                    if (updateEdgeId != null)
                    {
                        updateEdgeId(originalId, newId);
                    }
                });
            if (_edgeData.Length > _graph.EdgeCount)
            {
                _edgeData.Resize(_graph.EdgeCount);
            }
        }

        /// <summary>
        /// Resizes the internal data structures to their smallest size possible.
        /// </summary>
        public void Trim()
        {
            _graph.Trim();
            _edgeData.Resize(_graph.EdgeCount);
        }

        /// <summary>
        /// Returns the number of vertices in this graph.
        /// </summary>
        public uint VertexCount
        {
            get
            {
                return _graph.VertexCount;
            }
        }

        /// <summary>
        /// Returns the number of edges in this graph.
        /// </summary>
        public long EdgeCount
        {
            get { return _graph.EdgeCount; }
        }
        
        /// <summary>
        /// Disposes.
        /// </summary>
        public void Dispose()
        {
            _graph.Dispose();
            _edgeData.Dispose();
        }

        /// <summary>
        /// An edge enumerator.
        /// </summary>
        public class EdgeEnumerator : IEnumerable<RoutingEdge>, IEnumerator<RoutingEdge>
        {
            private readonly RoutingNetwork _network;
            private readonly GeometricGraph.EdgeEnumerator _enumerator;

            internal EdgeEnumerator(RoutingNetwork network, GeometricGraph.EdgeEnumerator enumerator)
            {
                _network = network;
                _enumerator = enumerator;
            }

            /// <summary>
            /// Returns true if there is at least one edge.
            /// </summary>
            public bool HasData
            {
                get
                {
                    return _enumerator.HasData;
                }
            }

            /// <summary>
            /// Returns the id of the current edge.
            /// </summary>
            public uint Id
            {
                get
                {
                    return _enumerator.Id;
                }
            }

            /// <summary>
            /// Returns the vertex at the beginning.
            /// </summary>
            public uint From
            {
                get
                {
                    return _enumerator.From;
                }
            }

            /// <summary>
            /// Returns the vertex at the end.
            /// </summary>
            public uint To
            {
                get
                {
                    return _enumerator.To;
                }
            }

            /// <summary>
            /// Returns the edge data.
            /// </summary>
            public EdgeData Data
            {
                get
                {
                    var baseEdgeData = Itinero.Data.Edges.EdgeDataSerializer.Deserialize(
                        _enumerator.Data);
                    return new EdgeData()
                    {
                        MetaId = _network._edgeData[_enumerator.Id],
                        Distance = baseEdgeData.Distance,
                        Profile = baseEdgeData.Profile
                    };
                }
            }

            /// <summary>
            /// Returns true if the edge data is inverted by default.
            /// </summary>
            public bool DataInverted
            {
                get
                {
                    return _enumerator.DataInverted;
                }
            }

            /// <summary>
            /// Gets the shape.
            /// </summary>
            public ShapeBase Shape
            {
                get
                {
                    return  _enumerator.Shape;
                }
            }

            /// <summary>
            /// Moves to the given vertex.
            /// </summary>
            /// <returns></returns>
            public bool MoveTo(uint vertex)
            {
                return _enumerator.MoveTo(vertex);
            }

            /// <summary>
            /// Moves to the given edge.
            /// </summary>
            /// <returns></returns>
            public void MoveToEdge(uint edgeId)
            {
                _enumerator.MoveToEdge(edgeId);
            }

            /// <summary>
            /// Returns the current edge.
            /// </summary>
            public RoutingEdge Current
            {
                get { return new RoutingEdge(this); }
            }

            /// <summary>
            /// Returns the current edge.
            /// </summary>
            object System.Collections.IEnumerator.Current
            {
                get { return this.Current; }
            }

            /// <summary>
            /// Moves to the next edge.
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            /// <summary>
            /// Resets this enumerator.
            /// </summary>
            public void Reset()
            {
                _enumerator.Reset();
            }

            /// <summary>
            /// Disposes.
            /// </summary>
            public void Dispose()
            {
                _enumerator.Dispose();
            }

            /// <summary>
            /// Gets the enumerator.
            /// </summary>
            /// <returns></returns>
            public IEnumerator<RoutingEdge> GetEnumerator()
            {
                this.Reset();
                return this;
            }

            /// <summary>
            /// Gets the enumerator.
            /// </summary>
            /// <returns></returns>
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        /// <summary>
        /// Serializes to a stream.
        /// </summary>
        public long Serialize(System.IO.Stream stream)
        {
            return this.Serialize(stream, null);
        } 

        /// <summary>
        /// Serializes to a stream.
        /// </summary>
        public long Serialize(System.IO.Stream stream, Action<uint, uint> updateEdgeId)
        {
            this.Compress(updateEdgeId);

            // serialize geometric graph.
            long size = 1;
            // write the version #.
            // v1->v2: Added maxEdgeDistance.
            stream.WriteByte(2);

            // write maxEdgeDistance.
            var bytes = BitConverter.GetBytes(_maxEdgeDistance);
            stream.Write(bytes, 0, 4);
            size += 4;

            // write graph.
            size += _graph.Serialize(stream);

            // serialize edge data.
            _edgeData.CopyTo(stream);
            size += _edgeData.Length * 4;

            return size;
        }

        /// <summary>
        /// Deserializes from a stream.
        /// </summary>
        public static RoutingNetwork Deserialize(Stream stream, RoutingNetworkProfile profile)
        {
            var version = stream.ReadByte();
            if (version > 2)
            {
                throw new Exception(string.Format("Cannot deserialize routing network: Invalid version #: {0}, upgrade Itinero.", version));
            }

            var position = stream.Position;
            var initialPosition = stream.Position;

            // read maxEdgeDistance if version # = 2.
            var maxEdgeDistance = Constants.DefaultMaxEdgeDistance;
            if(version == 2)
            {
                var bytes = new byte[4];
                stream.Read(bytes, 0, 4);
                maxEdgeDistance = BitConverter.ToSingle(bytes, 0);
            }

            // deserialize graph.
            var graph = GeometricGraph.Deserialize(stream, profile == null ? null : profile.GeometricGraphProfile);
            var size = stream.Position - position;

            var edgeLength = graph.EdgeCount;
            var edgeSize = 1;

            ArrayBase<uint> edgeData;
            if (profile == null)
            { // just create arrays and read the data.
                edgeData = Context.ArrayFactory.CreateMemoryBackedArray<uint>(edgeLength * edgeSize);
                edgeData.CopyFrom(stream);
                size += edgeLength * edgeSize * 4;
            }
            else
            { // create accessors over the exact part of the stream that represents vertices/edges.
                position = stream.Position;
                var map = new MemoryMapStream(new CappedStream(stream, position,
                    edgeLength * edgeSize * 4));
                edgeData = new Array<uint>(map.CreateUInt32(edgeLength * edgeSize), profile.EdgeDataProfile);
                size += edgeLength * edgeSize * 4;
            }

            // make stream is positioned correctly.
            stream.Seek(initialPosition + size, System.IO.SeekOrigin.Begin);

            return new RoutingNetwork(graph, edgeData, maxEdgeDistance);
        }
    }
}