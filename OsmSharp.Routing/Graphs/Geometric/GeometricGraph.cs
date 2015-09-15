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

using OsmSharp.Collections.Arrays;
using OsmSharp.Collections.Arrays.MemoryMapped;
using OsmSharp.Collections.Coordinates.Collections;
using OsmSharp.Math.Geo.Simple;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Graphs.Geometric
{
    /// <summary>
    /// A geometric graph.
    /// </summary>
    public class GeometricGraph
    {
        private static GeoCoordinateSimple NO_COORDINATE = new GeoCoordinateSimple()
        {
            Latitude = float.MaxValue,
            Longitude = float.MaxValue
        };

        private readonly Graph _graph;
        private readonly HugeArrayBase<GeoCoordinateSimple> _coordinates;
        private readonly HugeCoordinateCollectionIndex _shapes;

        /// <summary>
        /// Creates a new geometric graph.
        /// </summary>
        public GeometricGraph(int edgeDataSize)
        {
            _graph = new Graph(edgeDataSize);
            _coordinates = new HugeArray<GeoCoordinateSimple>(1000);
            for (var i = 0; i < _coordinates.Length; i++)
            {
                _coordinates[i] = NO_COORDINATE;
            }
            _shapes = new HugeCoordinateCollectionIndex(1000);
        }

        /// <summary>
        /// Creates a new geometric graph.
        /// </summary>
        public GeometricGraph(int edgeDataSize, int size)
        {
            _graph = new Graph(edgeDataSize, size);
            _coordinates = new HugeArray<GeoCoordinateSimple>(size);
            for (var i = 0; i < _coordinates.Length; i++)
            {
                _coordinates[i] = NO_COORDINATE;
            }
            _shapes = new HugeCoordinateCollectionIndex(1000);
        }

        /// <summary>
        /// Creates a new geometric graph.
        /// </summary>
        private GeometricGraph(Graph graph, HugeArrayBase<GeoCoordinateSimple> coordinates,
            HugeCoordinateCollectionIndex shapes)
        {
            _graph = graph;
            _coordinates = coordinates;
            _shapes = shapes;
        }

        /// <summary>
        /// Gets the given vertex.
        /// </summary>
        /// <returns></returns>
        public GeoCoordinateSimple GetVertex(uint vertex)
        {
            if (vertex >= _coordinates.Length) { throw new ArgumentException(string.Format("Vertex {0} does not exist.", vertex)); }
            if (_coordinates[vertex].Equals(NO_COORDINATE)) { throw new ArgumentException(string.Format("Vertex {0} does not exist.", vertex)); }

            return _coordinates[vertex];
        }

        /// <summary>
        /// Gets the given vertex.
        /// </summary>
        /// <returns></returns>
        public bool GetVertex(uint vertex, out float latitude, out float longitude)
        {
            if (vertex < _coordinates.Length)
            { // vertex exists.
                var coordinate = _coordinates[vertex];
                if (!coordinate.Equals(NO_COORDINATE))
                { // there is a coordinate set.
                    latitude = coordinate.Latitude;
                    longitude = coordinate.Longitude;
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
            { // removes the vertex.
                _coordinates[vertex] = NO_COORDINATE;
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

            if (vertex >= _coordinates.Length)
            { // increase coordinates length.
                var newBlocks = 1;
                if (vertex - _coordinates.Length > 0)
                { // increase more.
                    newBlocks = (int)System.Math.Floor(vertex - _coordinates.Length) + 1;
                }
                var oldLength = _coordinates.Length;
                _coordinates.Resize(_coordinates.Length + (newBlocks * 1000));
                for (var i = oldLength; i < _coordinates.Length; i++)
                {
                    _coordinates[i] = NO_COORDINATE;
                }
            }
            _coordinates[vertex] = new GeoCoordinateSimple()
            {
                Latitude = latitude,
                Longitude = longitude
            };
        }

        /// <summary>
        /// Adds a new edge.
        /// </summary>
        /// <returns></returns>
        public uint AddEdge(uint vertex1, uint vertex2, uint[] data, ICoordinateCollection shape)
        {
            var edgeId = _graph.AddEdge(vertex1, vertex2, data);
            _shapes[edgeId] = shape;
            return edgeId;
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
            var temp = _coordinates[vertex1];
            _coordinates[vertex1] = _coordinates[vertex2];
            _coordinates[vertex2] = temp;
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
                    _shapes.Switch(originalId, newId);
                });
            _shapes.Resize(_graph.EdgeCount);
            _shapes.Compress();
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
            public ICoordinateCollection Shape
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
            this.Compress();

            var size = _graph.Serialize(stream);
            using (var file = new OsmSharp.IO.MemoryMappedFiles.MemoryMappedStream(
                new OsmSharp.IO.LimitedStream(stream)))
            {
                var vertexCoordinateArray = new MappedHugeArray<GeoCoordinateSimple, float>(
                    new MemoryMappedHugeArraySingle(file, _graph.VertexCount * 2, _graph.VertexCount * 2, 1024),
                        2, (array, idx, coordinate) =>
                        {
                            array[idx] = coordinate.Latitude;
                            array[idx + 1] = coordinate.Longitude;
                        },
                        (Array, idx) =>
                        {
                            return new GeoCoordinateSimple()
                            {
                                Latitude = Array[idx],
                                Longitude = Array[idx + 1]
                            };
                        });
                vertexCoordinateArray.CopyFrom(_coordinates, 0, 0, _graph.VertexCount);
                vertexCoordinateArray.Dispose(); // written, get rid of it!
                size = size + (_graph.VertexCount * 2 * 4);
            }

            // serialize shapes right after.
            _shapes.Resize(_graph.EdgeCount);
            var shapeSize = _shapes.Serialize(stream);
            size += shapeSize;
            return size;
        }

        /// <summary>
        /// Deserializes a graph from the stream.
        /// </summary>
        /// <returns></returns>
        public static GeometricGraph Deserialize(System.IO.Stream stream, bool copy)
        {
            var graph = Graph.Deserialize(stream, copy);

            // get vertices.
            var position = stream.Position;
            HugeArrayBase<GeoCoordinateSimple> coordinates = null;
            if (graph.VertexCount == 0)
            { // no coordinates.
                coordinates = new HugeArray<GeoCoordinateSimple>(0);
            }
            else if (graph.VertexCount > 0)
            { // at least one vertex, otherwise write '0'.
                var bufferSize = 32;
                var cacheSize = MemoryMappedHugeArrayUInt32.DefaultCacheSize;
                var file = new OsmSharp.IO.MemoryMappedFiles.MemoryMappedStream(new OsmSharp.IO.LimitedStream(stream));
                coordinates = new MappedHugeArray<GeoCoordinateSimple, float>(
                    new MemoryMappedHugeArraySingle(file, graph.VertexCount * 2, graph.VertexCount * 2, bufferSize, cacheSize),
                        2, (array, idx, coordinate) =>
                        {
                            array[idx] = coordinate.Latitude;
                            array[idx + 1] = coordinate.Longitude;
                        },
                        (Array, idx) =>
                        {
                            return new GeoCoordinateSimple()
                            {
                                Latitude = Array[idx],
                                Longitude = Array[idx + 1]
                            };
                        });
                position = position + (graph.VertexCount * 2 * 4);
            }

            // deserialize shapes.
            stream.Seek(position, System.IO.SeekOrigin.Begin);
            var edgeShapes = HugeCoordinateCollectionIndex.Deserialize(stream, copy);

            return new GeometricGraph(graph, coordinates, edgeShapes);
        }
    }
}