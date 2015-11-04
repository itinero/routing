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

using OsmSharp.Collections.Coordinates.Collections;
using OsmSharp.Math.Geo.Simple;
using OsmSharp.Routing.Graphs.Geometric;
using OsmSharp.Routing.Graphs.Geometric.Shapes;
using OsmSharp.Routing.Network.Data;
using Reminiscence.Arrays;
using Reminiscence.IO;
using Reminiscence.IO.Streams;
using System.Collections.Generic;

namespace OsmSharp.Routing.Network
{
    /// <summary>
    /// Represents a routing network.
    /// </summary>
    public class RoutingNetwork
    {
        private readonly GeometricGraph _graph;
        private readonly ArrayBase<uint> _edgeData;
        private readonly int _edgeDataSize = 2;
        private const int BLOCK_SIZE = 1000;

        /// <summary>
        /// Creates a new routing network.
        /// </summary>
        public RoutingNetwork(GeometricGraph graph)
        {
            _graph = graph;
            _edgeData = new MemoryArray<uint>(_edgeDataSize * graph.EdgeCount);
        }

        /// <summary>
        /// Creates a new routing network from existing data.
        /// </summary>
        private RoutingNetwork(GeometricGraph graph, ArrayBase<uint> edgeData)
        {
            _graph = graph;
            _edgeData = edgeData;
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
        /// Increase edge data size to fit at least the given edge.
        /// </summary>
        private void IncreaseSizeEdgeData(uint edgeId)
        {
            var size = _edgeData.Length;
            while(edgeId >= size)
            {
                size += BLOCK_SIZE;
            }
            _edgeData.Resize(size);
        }

        /// <summary>
        /// Adds a new vertex.
        /// </summary>
        public void AddVertex(uint vertex, float latitude, float longitude)
        {
            _graph.AddVertex(vertex, latitude, longitude);
        }

        /// <summary>
        /// Gets the vertex.
        /// </summary>
        /// <returns></returns>
        public GeoCoordinateSimple GetVertex(uint vertex)
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
            var edgeId = _graph.AddEdge(vertex1, vertex2, 
                OsmSharp.Routing.Data.EdgeDataSerializer.Serialize(
                    data.Distance, data.Profile), shape);
            if(edgeId >= _edgeData.Length)
            {
                this.IncreaseSizeEdgeData(edgeId);
            }
            _edgeData[edgeId] = data.MetaId;
            return edgeId;
        }

        /// <summary>
        /// Gets the edge with the given id.
        /// </summary>
        /// <returns></returns>
        public RoutingEdge GetEdge(uint edgeId)
        {
            var edge = _graph.GetEdge(edgeId);

            var baseEdgeData = OsmSharp.Routing.Data.EdgeDataSerializer.Deserialize(
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
        public bool RemoveEdge(uint vertex1, uint vertex2)
        {
            return _graph.RemoveEdge(vertex1, vertex2);
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
                    var baseEdgeData = OsmSharp.Routing.Data.EdgeDataSerializer.Deserialize(
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
            this.Compress();

            var position = stream.Position; // store current offset.

            // serialize geometric graph.
            var size = _graph.Serialize(stream);

            // serialize edge data.
            _edgeData.CopyTo(stream);
            size += _edgeData.Length * 4;

            return size;
        }

        /// <summary>
        /// Deserializes from a stream.
        /// </summary>
        public static RoutingNetwork Deserialize(System.IO.Stream stream, RoutingNetworkProfile profile)
        {
            var position = stream.Position;
            var initialPosition = stream.Position;
            var graph = GeometricGraph.Deserialize(stream, profile == null ? null : profile.GeometricGraphProfile);
            var size = stream.Position - position;

            var edgeLength = graph.EdgeCount;
            var edgeSize = 1;

            ArrayBase<uint> edgeData;
            if (profile == null)
            { // just create arrays and read the data.
                edgeData = new MemoryArray<uint>(edgeLength * edgeSize);
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

            return new RoutingNetwork(graph, edgeData);
        }
    }
}