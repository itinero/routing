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

using Itinero.LocalGeo;
using Itinero.Graphs.Geometric.Shapes;
using Reminiscence.Arrays;
using Reminiscence.IO;
using Reminiscence.IO.Streams;
using System;
using System.Collections.Generic;

namespace Itinero.Graphs.Geometric
{
    /// <summary>
    /// A geometric graph.
    /// </summary>
    public class GeometricGraph
    {
        private const float NO_COORDINATE = float.MaxValue;
        private const int BLOCKSIZE = 1024;

        private readonly Graph _graph;
        private readonly ArrayBase<float> _coordinates;
        private readonly ShapesArray _shapes;

        /// <summary>
        /// Creates a new geometric graph.
        /// </summary>
        public GeometricGraph(int edgeDataSize)
            : this(edgeDataSize, BLOCKSIZE)
        {

        }

        /// <summary>
        /// Creates a new geometric graph.
        /// </summary>
        public GeometricGraph(int edgeDataSize, int size)
        {
            _graph = new Graph(edgeDataSize, size);
            _coordinates = new MemoryArray<float>(size * 2);
            for (var i = 0; i < _coordinates.Length; i++)
            {
                _coordinates[i] = NO_COORDINATE;
            }
            _shapes = new ShapesArray(size);
        }

        /// <summary>
        /// Creates a new geometric graph.
        /// </summary>
        public GeometricGraph(MemoryMap map, int edgeDataSize)
            : this(map, edgeDataSize, BLOCKSIZE)
        {

        }

        /// <summary>
        /// Creates a new geometric graph.
        /// </summary>
        public GeometricGraph(MemoryMap map, int edgeDataSize, int size)
        {
            _graph = new Graph(map, edgeDataSize, size);
            _coordinates = new Array<float>(map, size * 2);
            for (var i = 0; i < _coordinates.Length; i++)
            {
                _coordinates[i] = NO_COORDINATE;
            }
            _shapes = new ShapesArray(map, size);
        }

        /// <summary>
        /// Creates a new geometric graph.
        /// </summary>
        public GeometricGraph(MemoryMap map, GeometricGraphProfile profile, int edgeDataSize)
            : this(map, profile, edgeDataSize, BLOCKSIZE)
        {

        }

        /// <summary>
        /// Creates a new geometric graph.
        /// </summary>
        public GeometricGraph(MemoryMap map, GeometricGraphProfile profile, int edgeDataSize, int size)
        {
            if (profile == null)
            {
                _graph = new Graph(map, edgeDataSize, size);
                _coordinates = new Array<float>(map, size * 2);
                for (var i = 0; i < _coordinates.Length; i++)
                {
                    _coordinates[i] = NO_COORDINATE;
                }
                _shapes = new ShapesArray(map, size);
            }
            else
            {
                _graph = new Graph(map, profile.GraphProfile, edgeDataSize, size);
                _coordinates = new Array<float>(map, size * 2, profile.CoordinatesProfile);
                for (var i = 0; i < _coordinates.Length; i++)
                {
                    _coordinates[i] = NO_COORDINATE;
                }
                _shapes = new ShapesArray(map, size);
            }
        }

        /// <summary>
        /// Creates a new geometric graph.
        /// </summary>
        private GeometricGraph(Graph graph, ArrayBase<float> coordinates,
            ShapesArray shapes)
        {
            _graph = graph;
            _coordinates = coordinates;
            _shapes = shapes;
        }

        /// <summary>
        /// Gets the basic graph.
        /// </summary>
        public Graph Graph
        {
            get
            {
                return _graph;
            }
        }

        /// <summary>
        /// Gets the given vertex.
        /// </summary>
        public Coordinate GetVertex(uint vertex)
        {
            if (vertex >= _coordinates.Length) { throw new ArgumentException(string.Format("Vertex {0} does not exist.", vertex)); }
            if (_coordinates[vertex].Equals(NO_COORDINATE)) { throw new ArgumentException(string.Format("Vertex {0} does not exist.", vertex)); }

            return new Coordinate()
            {
                Latitude = _coordinates[vertex * 2],
                Longitude = _coordinates[vertex * 2 + 1]
            };
        }

        /// <summary>
        /// Gets the given vertex.
        /// </summary>
        public bool GetVertex(uint vertex, out float latitude, out float longitude)
        {
            if (vertex * 2 + 1 < _coordinates.Length)
            {
                latitude = _coordinates[vertex * 2];
                longitude = _coordinates[vertex * 2 + 1];
                if (!latitude.Equals(NO_COORDINATE))
                {
                    return true;
                }
            }
            latitude = 0;
            longitude = 0;
            return false;
        }

        /// <summary>
        /// Removes the given vertex.
        /// </summary>
        public bool RemoveVertex(uint vertex)
        {
            if (_graph.RemoveVertex(vertex))
            {
                _coordinates[vertex * 2] = NO_COORDINATE;
                _coordinates[vertex * 2 + 1] = NO_COORDINATE;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds the given vertex.
        /// </summary>
        public void AddVertex(uint vertex, float latitude, float longitude)
        {
            _graph.AddVertex(vertex);

            if (vertex * 2 + 1 >= _coordinates.Length)
            { // increase coordinates length.
                var newBlocks = 1;
                while (vertex * 2 + 1 >= _coordinates.Length + (newBlocks * BLOCKSIZE * 2))
                { // increase more.
                    newBlocks++;
                }

                var oldLength = _coordinates.Length;
                _coordinates.Resize(_coordinates.Length + (newBlocks * BLOCKSIZE * 2));
                for (var i = oldLength; i < _coordinates.Length; i++)
                {
                    _coordinates[i] = NO_COORDINATE;
                }
            }
            _coordinates[vertex * 2] = latitude;
            _coordinates[vertex * 2 + 1] = longitude;
        }

        /// <summary>
        /// Adds a new edge.
        /// </summary>
        /// <returns></returns>
        public uint AddEdge(uint vertex1, uint vertex2, uint[] data, ShapeBase shape)
        {
            var edgeId = _graph.AddEdge(vertex1, vertex2, data);

            if (edgeId >= _shapes.Length)
            { // increase coordinates length.
                var newBlocks = 1;
                while(edgeId >= _shapes.Length + (BLOCKSIZE * newBlocks))
                {
                    newBlocks++;
                }
                _shapes.Resize(_shapes.Length + (newBlocks * BLOCKSIZE));
            }

            _shapes[edgeId] = shape;
            return edgeId;
        }

        /// <summary>
        /// Updates the data associated with this edge.
        /// </summary>
        public void UpdateEdgeData(uint edgeId, uint[] data)
        {
            _graph.UpdateEdgeData(edgeId, data);
        }

        /// <summary>
        /// Gets the edge with the given id.
        /// </summary>
        /// <returns></returns>
        public GeometricEdge GetEdge(uint edgeId)
        {
            var edge = _graph.GetEdge(edgeId);

            return new GeometricEdge(edge.Id, edge.From, edge.To, edge.Data, edge.DataInverted, _shapes[edgeId]);
        }

        /// <summary>
        /// Removes all edges from/to the given vertex.
        /// </summary>
        /// <returns></returns>
        public int RemoveEdges(uint vertex)
        {
            var removed = 0;
            var edges = this.GetEdgeEnumerator(vertex);
            while (edges.MoveNext())
            {
                _shapes[edges.Id] = null;
                if (this.RemoveEdge(edges.Id))
                {
                    removed++;
                }
            }
            return removed;
        }

        /// <summary>
        /// Removes the edge with the given id.
        /// </summary>
        /// <returns></returns>
        public bool RemoveEdge(uint edgeId)
        {
            if (_graph.RemoveEdge(edgeId))
            {
                _shapes[edgeId] = null;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the given edge.
        /// </summary>
        /// <returns></returns>
        public bool RemoveEdge(uint vertex1, uint vertex2)
        {
            var edge = this.GetEdgeEnumerator(vertex1);
            while (edge.MoveNext())
            {
                if (edge.To == vertex2)
                {
                    return this.RemoveEdge(edge.Id);
                }
            }
            return false;
        }

        /// <summary>
        /// Switches the two vertices.
        /// </summary>
        public void Switch(uint vertex1, uint vertex2)
        {
            // switch vertices, edges do not change id's.
            _graph.Switch(vertex1, vertex2);

            // switch coordinates.
            var tempLatitude = _coordinates[vertex1 * 2];
            var tempLongitude = _coordinates[vertex1 * 2 + 1];
            _coordinates[vertex1 * 2] = _coordinates[vertex2 * 2];
            _coordinates[vertex1 * 2 + 1] = _coordinates[vertex2 * 2 + 1];
            _coordinates[vertex2 * 2] = tempLatitude;
            _coordinates[vertex2 * 2 + 1] = tempLongitude;
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
        /// <param name="updateEdgeId">The edge id's may change. This action can be used to hook into every change.</param>
        public void Compress(Action<uint, uint> updateEdgeId)
        {
            _graph.Compress((originalId, newId) =>
            {
                updateEdgeId(originalId, newId);
                _shapes.Switch(originalId, newId);
            });
            _shapes.Resize(_graph.EdgeCount);
            _coordinates.Resize(_graph.VertexCapacity * 2);
        }

        /// <summary>
        /// Relocates data internally in the most compact way possible.
        /// </summary>
        public void Compress()
        {
            _graph.Compress((originalId, newId) =>
                {
                    _shapes.Switch(originalId, newId);
                });
            _shapes.Resize(_graph.EdgeCount);
            _coordinates.Resize(_graph.VertexCapacity * 2);
        }

        /// <summary>
        /// Resizes the internal data structures to their smallest size possible.
        /// </summary>
        public void Trim()
        {
            _graph.Trim();

            _coordinates.Resize(_graph.VertexCount);
            _shapes.Resize(_graph.EdgeCount);
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
        /// Returns the size in bytes as if serialized.
        /// </summary>
        /// <returns></returns>
        public long SizeInBytes
        {
            get
            {
                return 1 + _graph.SizeInBytes +
                    _coordinates.Length * 4 +
                    _shapes.SizeInBytes;
            }
        }

        /// <summary>
        /// Gets the shape for the given edge.
        /// </summary>
        public ShapeBase GetShape(uint edge)
        {
            return _shapes[edge];
        }

        /// <summary>
        /// An edge enumerator.
        /// </summary>
        public class EdgeEnumerator : IEnumerable<GeometricEdge>, IEnumerator<GeometricEdge>
        {
            private readonly GeometricGraph _graph;
            private readonly Graph.EdgeEnumerator _enumerator;

            internal EdgeEnumerator(GeometricGraph graph, Graph.EdgeEnumerator enumerator)
            {
                _graph = graph;
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
            public uint[] Data
            {
                get
                {
                    return _enumerator.Data;
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
                    return _graph._shapes[_enumerator.Id];
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
            public void MoveToEdge(uint edge)
            {
                _enumerator.MoveToEdge(edge);
            }

            /// <summary>
            /// Returns the current edge.
            /// </summary>
            public GeometricEdge Current
            {
                get { return new GeometricEdge(this); }
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
            public IEnumerator<GeometricEdge> GetEnumerator()
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
        /// Disposes.
        /// </summary>
        public void Dispose()
        {
            _graph.Dispose();
        }

        /// <summary>
        /// Serializes this graph to disk.
        /// </summary>
        public long Serialize(System.IO.Stream stream)
        {
            // compress first.
            this.Compress();

            // serialize the base graph & make sure to seek to right after.
            long size = 1;
            stream.WriteByte(1);
            size += _graph.Serialize(stream);

            // serialize the coordinates.
            size += _coordinates.CopyTo(stream);

            // and serialize the shapes.
            size += _shapes.CopyTo(stream);

            return size;
        }

        /// <summary>
        /// Deserializes a graph from the stream.
        /// </summary>
        public static GeometricGraph Deserialize(System.IO.Stream stream, GeometricGraphProfile profile)
        {
            var version = stream.ReadByte();
            if(version != 1)
            {
                throw new Exception(string.Format("Cannot deserialize geometric graph: Invalid version #: {0}.", version));
            }
            var graph = Graph.Deserialize(stream, profile == null ? null : profile.GraphProfile);
            var initialPosition = stream.Position;
            var size = 0L;

            ArrayBase<float> coordinates;
            ShapesArray shapes;
            if (profile == null)
            { // don't use the stream, the read from it.
                coordinates = new MemoryArray<float>(graph.VertexCount * 2);
                coordinates.CopyFrom(stream);
                size += graph.VertexCount * 2 * 4;
                long shapeSize;
                shapes = ShapesArray.CreateFrom(stream, true, out shapeSize);
                size += shapeSize;
            }
            else
            { // use the stream as a map.
                var position = stream.Position;
                var map = new MemoryMapStream(new CappedStream(stream, position, graph.VertexCount * 4 * 2));
                coordinates = new Array<float>(map.CreateSingle(graph.VertexCount * 2), profile.CoordinatesProfile);
                size += graph.VertexCount * 2 * 4;
                stream.Seek(position + graph.VertexCount * 4 * 2, System.IO.SeekOrigin.Begin);
                long shapeSize;
                shapes = ShapesArray.CreateFrom(stream, false, out shapeSize);
                size += shapeSize;
            }

            // make stream is positioned correctly.
            stream.Seek(initialPosition + size, System.IO.SeekOrigin.Begin);

            return new GeometricGraph(graph, coordinates, shapes);
        }
    }
}