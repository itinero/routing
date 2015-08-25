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
using OsmSharp.Collections.Sorting;
using OsmSharp.IO.MemoryMappedFiles;
using OsmSharp.Math.Geo.Simple;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Graph
{
    /// <summary>
    /// An implementation of an in-memory dynamic graph but with explicitly directed edges.
    /// </summary>
    public class DirectedGraph<TEdgeData> : GraphBase<TEdgeData>
        where TEdgeData : struct, IGraphEdgeData
    {
        protected const int VERTEX_SIZE = 2; // holds the first edge index and the edge count.
        protected const int FIRST_EDGE = 0;
        protected const int EDGE_COUNT = 1;
        protected const int EDGE_SIZE = 1; // holds only the target vertext.
        protected const uint NO_EDGE = uint.MaxValue; // a dummy value indication that there is no edge.

        private uint _nextVertexId;
        private uint _nextEdgeId;

        /// <summary>
        /// Holds the coordinates of the vertices.
        /// </summary>
        private HugeArrayBase<GeoCoordinateSimple> _coordinates;

        /// <summary>
        /// Holds all vertices pointing to it's first edge.
        /// </summary>
        private HugeArrayBase<uint> _vertices;

        /// <summary>
        /// Holds all edges (meaning vertex1-vertex2)
        /// </summary>
        private HugeArrayBase<uint> _edges;

        /// <summary>
        /// Holds all data associated with edges.
        /// </summary>
        private HugeArrayBase<TEdgeData> _edgeData;

        /// <summary>
        /// Holds all shapes associated with edges.
        /// </summary>
        private HugeCoordinateCollectionIndex _edgeShapes;

        /// <summary>
        /// Holds the readonly flag.
        /// </summary>
        private bool _readonly = false;

        /// <summary>
        /// Creates a new in-memory graph.
        /// </summary>
        public DirectedGraph()
            : this(1000)
        {

        }

        /// <summary>
        /// Creates a graph using the existing data in the given arrays.
        /// </summary>
        /// <param name="coordinateArray"></param>
        /// <param name="vertexArray"></param>
        /// <param name="edgesArray"></param>
        /// <param name="edgeDataArray"></param>
        /// <param name="edgeShapeArray"></param>
        public DirectedGraph(
            HugeArrayBase<GeoCoordinateSimple> coordinateArray,
            HugeArrayBase<uint> vertexArray,
            HugeArrayBase<uint> edgesArray,
            HugeArrayBase<TEdgeData> edgeDataArray,
            HugeCoordinateCollectionIndex edgeShapeArray)
        {
            _vertices = vertexArray;
            _coordinates = coordinateArray;
            _edges = edgesArray;
            _edgeData = edgeDataArray;
            _edgeShapes = edgeShapeArray;
            _nextVertexId = (uint)(_vertices.Length / VERTEX_SIZE);
            _nextEdgeId = (uint)(edgesArray.Length / EDGE_SIZE);
        }

        /// <summary>
        /// Creates a new in-memory graph.
        /// </summary>
        public DirectedGraph(long sizeEstimate)
            : this(sizeEstimate, 
            new HugeArray<GeoCoordinateSimple>(sizeEstimate), 
            new HugeArray<uint>(sizeEstimate), 
            new HugeArray<uint>(sizeEstimate * 3 * EDGE_SIZE), 
            new HugeArray<TEdgeData>(sizeEstimate * 3), 
            new HugeCoordinateCollectionIndex(sizeEstimate * 3))
        {

        }

        /// <summary>
        /// Creates a new in-memory graph.
        /// </summary>
        /// <param name="sizeEstimate"></param>
        /// <param name="coordinateArray"></param>
        /// <param name="vertexArray"></param>
        /// <param name="edgesArray"></param>
        /// <param name="edgeDataArray"></param>
        /// <param name="edgeShapeArray"></param>
        public DirectedGraph(long sizeEstimate, HugeArrayBase<GeoCoordinateSimple> coordinateArray, 
            HugeArrayBase<uint> vertexArray, HugeArrayBase<uint> edgesArray, HugeArrayBase<TEdgeData> edgeDataArray, HugeCoordinateCollectionIndex edgeShapeArray)
        {
            _nextVertexId = 1;
            _nextEdgeId = 0;
            _vertices = vertexArray;
            _vertices.Resize(sizeEstimate);
            _coordinates = coordinateArray;
            _coordinates.Resize(sizeEstimate);
            _edges = edgesArray;
            _edges.Resize(sizeEstimate * 3 * EDGE_SIZE);
            _edgeData = edgeDataArray;
            _edgeData.Resize(sizeEstimate * 3);
            _edgeShapes = edgeShapeArray;
            _edgeShapes.Resize(sizeEstimate * 3);
        }

        /// <summary>
        /// Creates a new memory mapped graph using the given file.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="estimatedSize"></param>
        /// <param name="mapFrom"></param>
        /// <param name="mapTo"></param>
        public DirectedGraph(MemoryMappedFile file, long estimatedSize,
            MappedHugeArray<TEdgeData, uint>.MapFrom mapFrom,
            MappedHugeArray<TEdgeData, uint>.MapTo mapTo,
            int edgeDataSize)
            : this(estimatedSize,
            new MappedHugeArray<GeoCoordinateSimple, float>(new MemoryMappedHugeArraySingle(file, estimatedSize * 2), 2,
            (array, idx, value) =>
            {
                array[idx] = value.Latitude;
                array[idx + 1] = value.Longitude;
            },
            (array, idx) =>
            {
                return new GeoCoordinateSimple()
                {
                    Latitude = array[idx],
                    Longitude = array[idx + 1]
                };
            }),
            new MemoryMappedHugeArrayUInt32(file, estimatedSize),
            new MemoryMappedHugeArrayUInt32(file, estimatedSize),
            new MappedHugeArray<TEdgeData, uint>(new MemoryMappedHugeArrayUInt32(file, estimatedSize * edgeDataSize), edgeDataSize, 
                mapTo, mapFrom),
            new HugeCoordinateCollectionIndex(file, estimatedSize))
        {

        }

        /// <summary>
        /// Creates a new memory mapped graph from existing data.
        /// </summary>
        /// <param name="vertexCount"></param>
        /// <param name="edgesCount"></param>
        /// <param name="verticesFile"></param>
        /// <param name="verticesCoordinatesFile"></param>
        /// <param name="edgesFile"></param>
        /// <param name="edgeDataFile"></param>
        /// <param name="shapesIndexFile"></param>
        /// <param name="shapesIndexLength"></param>
        /// <param name="shapesCoordinateFile"></param>
        /// <param name="shapesCoordinateLength"></param>
        /// <param name="mapFrom"></param>
        /// <param name="mapTo"></param>
        /// <param name="edgeDataSize"></param>
        public DirectedGraph(long vertexCount, long edgesCount, 
            MemoryMappedFile verticesFile,
            MemoryMappedFile verticesCoordinatesFile,
            MemoryMappedFile edgesFile,
            MemoryMappedFile edgeDataFile,
            MemoryMappedFile shapesIndexFile, long shapesIndexLength,
            MemoryMappedFile shapesCoordinateFile, long shapesCoordinateLength,
            MappedHugeArray<TEdgeData, uint>.MapFrom mapFrom,
            MappedHugeArray<TEdgeData, uint>.MapTo mapTo,
            int edgeDataSize)
            : this(
                new MappedHugeArray<GeoCoordinateSimple, float>(new MemoryMappedHugeArraySingle(verticesCoordinatesFile, vertexCount * 2), 2,
                (array, idx, value) =>
                {
                    array[idx] = value.Latitude;
                    array[idx + 1] = value.Longitude;
                },
                (array, idx) =>
                {
                    return new GeoCoordinateSimple()
                    {
                        Latitude = array[idx],
                        Longitude = array[idx + 1]
                    };
                }),
                new MemoryMappedHugeArrayUInt32(verticesFile, vertexCount),
                new MemoryMappedHugeArrayUInt32(edgesFile, edgesCount),
                new MappedHugeArray<TEdgeData, uint>(new MemoryMappedHugeArrayUInt32(edgeDataFile, edgesCount * edgeDataSize), edgeDataSize,
                    mapTo,
                    mapFrom),
                new HugeCoordinateCollectionIndex(shapesIndexLength, new MemoryMappedHugeArrayUInt64(shapesIndexFile, shapesIndexLength),
                    new MemoryMappedHugeArraySingle(shapesCoordinateFile, shapesCoordinateLength)))
        {

        }

        /// <summary>
        /// Increases the memory allocation.
        /// </summary>
        private void IncreaseVertexSize()
        {
            this.IncreaseVertexSize(_coordinates.Length + 10000);
        }

        /// <summary>
        /// Increases the memory allocation.
        /// </summary>
        /// <param name="size"></param>
        private void IncreaseVertexSize(long size)
        {
            if (_readonly) { throw new Exception("Graph is readonly."); }

            var oldLength = _coordinates.Length;
            _coordinates.Resize(size);
            _vertices.Resize(size);
        }

        /// <summary>
        /// Increases the memory allocation.
        /// </summary>
        private void IncreaseEdgeSize()
        {
            this.IncreaseEdgeSize(_edges.Length + 10000);
        }

        /// <summary>
        /// Increases the memory allocation.
        /// </summary>
        private void IncreaseEdgeSize(long size)
        {
            if (_readonly) { throw new Exception("Graph is readonly."); }

            var oldLength = _edges.Length;
            _edges.Resize(size);
            _edgeData.Resize(size / EDGE_SIZE);
            _edgeShapes.Resize(size / EDGE_SIZE);
        }

        /// <summary>
        /// Adds a new vertex.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        public override uint AddVertex(float latitude, float longitude)
        {
            if (_readonly) { throw new Exception("Graph is readonly."); }

            // create vertex.
            var newId = _nextVertexId;
            var vertexIdx = newId * VERTEX_SIZE;
            if (vertexIdx + 1 >= _vertices.Length)
            {
                this.IncreaseVertexSize();
            }

            _coordinates[newId] = new GeoCoordinateSimple()
            {
                Latitude = latitude,
                Longitude = longitude
            };
            _vertices[vertexIdx + EDGE_COUNT] = 0;
            _nextVertexId++; // increase for next vertex.
            return newId;
        }

        /// <summary>
        /// Sets a vertex.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public override void SetVertex(uint vertex, float latitude, float longitude)
        {
            if (_readonly) { throw new Exception("Graph is readonly."); }
            if (_nextVertexId <= vertex) { throw new ArgumentOutOfRangeException("vertex", "vertex is not part of this graph."); }

            var coordinate = _coordinates[vertex];
            coordinate.Latitude = latitude;
            coordinate.Longitude = longitude;
            _coordinates[vertex] = coordinate;
        }

        /// <summary>
        /// Returns the information in the current vertex.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        public override bool GetVertex(uint id, out float latitude, out float longitude)
        {
            if (_nextVertexId > id & id > 0)
            {
                latitude = _coordinates[id].Latitude;
                longitude = _coordinates[id].Longitude;
                return true;
            }
            latitude = float.MaxValue;
            longitude = float.MaxValue;
            return false;
        }

        /// <summary>
        /// Adds an edge with the associated data.
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="data"></param>
        public override void AddEdge(uint vertex1, uint vertex2, TEdgeData data)
        {
            this.AddEdge(vertex1, vertex2, data, null);
        }

        /// <summary>
        /// Adds an edge with the associated data.
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="data"></param>
        /// <param name="coordinates"></param>
        /// <remarks>This only adds edge vertex1->vertex2 NOT vertex2->vertex1</remarks>
        public override void AddEdge(uint vertex1, uint vertex2, TEdgeData data, ICoordinateCollection coordinates)
        {
            if (_readonly) { throw new Exception("Graph is readonly."); }
            if (vertex1 == vertex2) { throw new ArgumentException("Given vertices must be different."); }
            if (_nextVertexId <= vertex1) { throw new ArgumentOutOfRangeException("vertex1", "vertex1 is not part of this graph."); }
            if (_nextVertexId <= vertex2) { throw new ArgumentOutOfRangeException("vertex2", "vertex2 is not part of this graph."); }

            var vertex1Idx = vertex1 * VERTEX_SIZE;
            var edgeCount = _vertices[vertex1Idx + EDGE_COUNT];
            var edgeId = _vertices[vertex1Idx + FIRST_EDGE];

            if ((edgeCount & (edgeCount - 1)) == 0)
            { // edgeCount is a power of two, increase space.
                // update vertex.
                uint newEdgeId = _nextEdgeId;
                _vertices[vertex1Idx + FIRST_EDGE] = newEdgeId;

                // move edges.
                if (edgeCount > 0)
                {
                    if (newEdgeId + (2 * edgeCount) >= _edges.Length)
                    { // edges need to be increased.
                        this.IncreaseEdgeSize();
                    }

                    for (uint toMoveIdx = edgeId; toMoveIdx < edgeId + edgeCount; toMoveIdx = toMoveIdx + EDGE_SIZE)
                    {
                        _edges[newEdgeId] = _edges[toMoveIdx];
                        _edgeData[newEdgeId] = _edgeData[toMoveIdx];
                        _edgeShapes[newEdgeId] = _edgeShapes[toMoveIdx];

                        newEdgeId++;
                    }

                    // the edge id is the last new edge id.
                    edgeId = newEdgeId;

                    // increase the nextEdgeId, these edges have been added at the end of the edge-array.
                    _nextEdgeId = _nextEdgeId + (2 * edgeCount);
                }
                else
                { // just add next edge id.
                    if (_nextEdgeId + 1 >= _edges.Length)
                    { // edges need to be increased.
                        this.IncreaseEdgeSize();
                    }

                    edgeId = _nextEdgeId;
                    _nextEdgeId++;
                }
            }
            else
            { // calculate edgeId of new edge.
                if (_nextEdgeId + 1 >= _edges.Length)
                { // edges need to be increased.
                    this.IncreaseEdgeSize();
                }

                edgeId = edgeId + edgeCount;
                _nextEdgeId++;
            }

            // update edge count in vertex.
            edgeCount++;
            _vertices[vertex1Idx + EDGE_COUNT] = edgeCount;

            // update edge.
            _edges[edgeId] = vertex2;
            _edgeData[edgeId] = data;
            _edgeShapes[edgeId] = coordinates;
            return;
        }

        /// <summary>
        /// Deletes all edges leading from/to the given vertex. 
        /// </summary>
        /// <param name="vertex"></param>
        /// <remarks>Only deletes all edges vertex->* NOT *->vertex</remarks>
        public override void RemoveEdges(uint vertex)
        {
            if (_readonly) { throw new Exception("Graph is readonly."); }

            var edges =  this.GetEdges(vertex);
            while (edges.MoveNext())
            {
                this.RemoveEdge(vertex, edges.Neighbour);
            }
        }

        /// <summary>
        /// Deletes the edge between the two given vertices.
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <remarks>Only deletes edge vertex1->vertex2 NOT vertex2 -> vertex1.</remarks>
        public override bool RemoveEdge(uint vertex1, uint vertex2)
        {
            if (_readonly) { throw new Exception("Graph is readonly."); }
            if (_nextVertexId <= vertex1) { throw new ArgumentOutOfRangeException("vertex1", "vertex1 is not part of this graph."); }
            if (_nextVertexId <= vertex2) { throw new ArgumentOutOfRangeException("vertex2", "vertex2 is not part of this graph."); }

            var removed = false;
            var vertex1Idx = vertex1 * VERTEX_SIZE;
            var edgeCount = _vertices[vertex1Idx + EDGE_COUNT];
            var edgeId = _vertices[vertex1Idx + FIRST_EDGE];

            for(var removeIdx = edgeId; removeIdx < edgeId + edgeCount; removeIdx++)
            {
                if(_edges[removeIdx] == vertex2)
                {
                    edgeCount--;
                    _edges[removeIdx] = _edges[edgeId + edgeCount];
                    _edgeData[removeIdx] = _edgeData[edgeId + edgeCount];
                    _edgeShapes[removeIdx] = _edgeShapes[edgeId + edgeCount];
                    removed = true;
                }
            }
            _vertices[vertex1Idx + EDGE_COUNT] = edgeCount;
            return removed;
        }

        /// <summary>
        /// Deletes the edge between the two given vertices.
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="data"></param>
        /// <remarks>Only deletes edge vertex1->vertex2 NOT vertex2 -> vertex1.</remarks>
        public override bool RemoveEdge(uint vertex1, uint vertex2, TEdgeData data)
        {
            if (_readonly) { throw new Exception("Graph is readonly."); }
            if (_nextVertexId <= vertex1) { throw new ArgumentOutOfRangeException("vertex1", "vertex1 is not part of this graph."); }
            if (_nextVertexId <= vertex2) { throw new ArgumentOutOfRangeException("vertex2", "vertex2 is not part of this graph."); }

            var vertex1Idx = vertex1 * VERTEX_SIZE;
            var edgeCount = _vertices[vertex1Idx + EDGE_COUNT];
            var edgeId = _vertices[vertex1Idx + FIRST_EDGE];

            bool removed = false;
            for (var removeIdx = edgeId; removeIdx < edgeId + edgeCount; removeIdx++)
            {
                if (_edges[removeIdx] == vertex2 &&
                    _edgeData[removeIdx].Equals(data))
                {
                    edgeCount--;
                    _edges[removeIdx] = _edges[edgeId + edgeCount];
                    _edgeData[removeIdx] = _edgeData[edgeId + edgeCount];
                    _edgeShapes[removeIdx] = _edgeShapes[edgeId + edgeCount];
                    removed = true;
                }
            }
            _vertices[vertex1Idx + EDGE_COUNT] = edgeCount;
            return removed;
        }

        /// <summary>
        /// Returns an empty edge enumerator.
        /// </summary>
        /// <returns></returns>
        public override EdgeEnumerator<TEdgeData> GetEdgeEnumerator()
        {
            return new EdgeEnumerator(this);
        }

        /// <summary>
        /// Returns all arcs starting at the given vertex.
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public override EdgeEnumerator<TEdgeData> GetEdges(uint vertex)
        {
            if (_nextVertexId <= vertex) { throw new ArgumentOutOfRangeException("vertex", "vertex is not part of this graph."); }

            var enumerator = new EdgeEnumerator(this);
            enumerator.MoveTo(vertex);
            return enumerator;
        }

        /// <summary>
        /// Gets the data associated with the given edge and return true if it exists.
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <returns></returns>
        public override EdgeEnumerator<TEdgeData> GetEdges(uint vertex1, uint vertex2)
        {
            if (_nextVertexId <= vertex1) { throw new ArgumentOutOfRangeException("vertex1", "vertex is not part of this graph."); }
            if (_nextVertexId <= vertex2) { throw new ArgumentOutOfRangeException("vertex2", "vertex is not part of this graph."); }

            var enumerator = new EdgeEnumerator(this);
            enumerator.MoveTo(vertex1, vertex2);
            return enumerator;
        }

        /// <summary>
        /// Returns true if the given vertex has neighbour as a neighbour.
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <returns></returns>
        /// <remarks>Returns true ONLY when edge vertex1->vertex2 is there NOT when only vertex2->vertex1.</remarks>
        public override bool ContainsEdges(uint vertex1, uint vertex2)
        {
            if (_nextVertexId <= vertex1) { throw new ArgumentOutOfRangeException("vertex1", "vertex1 is not part of this graph."); }
            if (_nextVertexId <= vertex2) { throw new ArgumentOutOfRangeException("vertex2", "vertex2 is not part of this graph."); }

            var vertex1Idx = vertex1 * VERTEX_SIZE;
            var edgeCount = _vertices[vertex1Idx + EDGE_COUNT];
            var edgeId = _vertices[vertex1Idx + FIRST_EDGE];

            for (var searchIdx = edgeId; searchIdx < edgeId + edgeCount; searchIdx++)
            {
                if (_edges[searchIdx] == vertex2)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the given vertex has neighbour as a neighbour.
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <remarks>Returns true ONLY when edge vertex1->vertex2 is there NOT when only vertex2->vertex1.</remarks>
        public override bool ContainsEdge(uint vertex1, uint vertex2, TEdgeData data)
        {
            if (_nextVertexId <= vertex1) { throw new ArgumentOutOfRangeException("vertex1", "vertex1 is not part of this graph."); }
            if (_nextVertexId <= vertex2) { throw new ArgumentOutOfRangeException("vertex2", "vertex2 is not part of this graph."); }

            var vertex1Idx = vertex1 * VERTEX_SIZE;
            var edgeCount = _vertices[vertex1Idx + EDGE_COUNT];
            var edgeId = _vertices[vertex1Idx + FIRST_EDGE];

            for (var searchIdx = edgeId; searchIdx < edgeId + edgeCount; searchIdx++)
            {
                if (_edges[searchIdx] == vertex2 &&
                    _edgeData[searchIdx].Equals(data))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the data associated with the given edge and return true if it exists. Throw exception if this graph allows duplicates.
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public override bool GetEdge(uint vertex1, uint vertex2, out TEdgeData data)
        {
            throw new InvalidOperationException("Cannot use GetEdge on a graph that can have duplicate edges.");
        }

        /// <summary>
        /// Gets the shape associated with the given edge and returns true if it exists.
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="shape"></param>
        /// <returns></returns>
        public override bool GetEdgeShape(uint vertex1, uint vertex2, out ICoordinateCollection shape)
        {
            throw new InvalidOperationException("Cannot use GetEdgeShape on a graph that can have duplicate edges.");
        }

        /// <summary>
        /// Trims the internal data structures of this graph.
        /// </summary>
        public override void Trim()
        {
            if (_readonly) { throw new Exception("Graph is readonly."); }

            // resize coordinates/vertices.
            _coordinates.Resize(_nextVertexId);
            _vertices.Resize(_nextVertexId * VERTEX_SIZE);

            // resize edges.
            _edgeData.Resize(_nextEdgeId / EDGE_SIZE);
            _edgeShapes.Resize(_nextEdgeId / EDGE_SIZE);
            _edges.Resize(_nextEdgeId * EDGE_SIZE);
        }

        /// <summary>
        /// Resizes the internal data structures of the graph to handle the number of vertices/edges estimated.
        /// </summary>
        /// <param name="vertexEstimate"></param>
        /// <param name="edgeEstimate"></param>
        public override void Resize(long vertexEstimate, long edgeEstimate)
        {
            if (_readonly) { throw new Exception("Graph is readonly."); }

            // resize coordinates/vertices.
            this.IncreaseVertexSize((int)vertexEstimate * VERTEX_SIZE);

            // resize edges.
            this.IncreaseEdgeSize((int)(edgeEstimate * EDGE_SIZE));
        }

        /// <summary>
        /// Returns the number of vertices in this graph.
        /// </summary>
        public override uint VertexCount
        {
            get { return _nextVertexId - 1; }
        }

        /// <summary>
        /// Compresses the data in this graph to it's smallest size.
        /// </summary>
        /// <param name="toReadonly">Flag to make the graph even smaller by converting it to a readonly version.</param>
        public void Compress(bool toReadonly)
        {
            if (toReadonly)
            {
                if (_readonly) { throw new Exception("Graph is readonly."); }

                // copy everything in a better structure after the last edge.
                var offset = _nextEdgeId;
                var nextEdgeId = _nextEdgeId;
                for (int vertex = 2; vertex < _nextVertexId * VERTEX_SIZE; vertex = vertex + 2)
                { // assume vertices are sorted correctly.
                    var newEdge = nextEdgeId;

                    // move edges.
                    for (uint oldEdgeId = _vertices[vertex + FIRST_EDGE]; oldEdgeId < _vertices[vertex + EDGE_COUNT] + _vertices[vertex + FIRST_EDGE]; oldEdgeId++)
                    {
                        if (nextEdgeId + 1 >= _edges.Length)
                        { // edges need to be increased.
                            this.IncreaseEdgeSize();
                        }

                        _edges[nextEdgeId] = _edges[oldEdgeId];
                        _edgeData[nextEdgeId] = _edgeData[oldEdgeId];
                        _edgeShapes[nextEdgeId] = _edgeShapes[oldEdgeId];
                        nextEdgeId++;
                    }

                    // update vertex.
                    _vertices[vertex + FIRST_EDGE] = newEdge - offset;
                }

                // copy everything back to the beginning.
                for (uint edgeId = 0; edgeId < nextEdgeId - _nextEdgeId; edgeId++)
                {
                    _edges[edgeId] = _edges[edgeId + offset];
                    _edgeData[edgeId] = _edgeData[edgeId + offset];
                    _edgeShapes[edgeId] = _edgeShapes[edgeId + offset];
                }
                _nextEdgeId = nextEdgeId - _nextEdgeId;

                // ... and trim.
                this.Trim();

                // ... compress the coordinates index.
                _edgeShapes.Compress();

                // ... last, but not least, set readonly flag.
                _readonly = true;
            }
        }

        /// <summary>
        /// Compresses the data in this graph to it's smallest size.
        /// </summary>
        public override void Compress()
        {
            this.Compress(false);
        }

        /// <summary>
        /// Represents the internal edge enumerator.
        /// </summary>
        class EdgeEnumerator : EdgeEnumerator<TEdgeData>
        {
            /// <summary>
            /// Holds the graph.
            /// </summary>
            private DirectedGraph<TEdgeData> _graph;

            /// <summary>
            /// Holds the current edge id.
            /// </summary>
            private uint _currentEdgeId;

            /// <summary>
            /// Holds the current count (for performance reasons, this is duplicate information).
            /// </summary>
            private int _currentCount;

            /// <summary>
            /// Holds the start edge id.
            /// </summary>
            private uint _startEdgeId;

            /// <summary>
            /// Holds the edge count.
            /// </summary>
            private uint _count;

            /// <summary>
            /// Holds the neighbour.
            /// </summary>
            private uint _neighbour;

            /// <summary>
            /// Creates a new edge enumerator.
            /// </summary>
            /// <param name="graph"></param>
            public EdgeEnumerator(DirectedGraph<TEdgeData> graph)
            {
                _graph = graph;
                _startEdgeId = 0;
                _count = 0;
                _neighbour = 0;

                // reset.
                _currentEdgeId = uint.MaxValue;
                _currentCount = -1;
            }

            /// <summary>
            /// Move to the next edge.
            /// </summary>
            /// <returns></returns>
            public override bool MoveNext()
            {
                if(_currentCount < 0)
                {
                    _currentEdgeId = _startEdgeId;
                    _currentCount = 0;
                }
                else
                {
                    _currentEdgeId++;
                    _currentCount++;
                }
                if (_currentCount < _count)
                {
                    while (_neighbour != 0 &&
                        _neighbour != this.Neighbour)
                    {
                        _currentEdgeId++;
                        _currentCount++;

                        if(_currentCount >= _count)
                        {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Returns the current neighbour.
            /// </summary>
            public override uint Neighbour
            {
                get { return _graph._edges[_currentEdgeId]; }
            }

            /// <summary>
            /// Returns the current edge data.
            /// </summary>
            public override TEdgeData EdgeData
            {
                get
                {
                    return _graph._edgeData[_currentEdgeId];
                }
            }

            /// <summary>
            /// Returns true if the edge data is inverted by default.
            /// </summary>
            public override bool isInverted
            {
                get { return false; }
            }

            /// <summary>
            /// Returns the inverted edge data.
            /// </summary>
            public override TEdgeData InvertedEdgeData
            {
                get
                {
                    return (TEdgeData)_graph._edgeData[_currentEdgeId].Reverse();
                }
            }

            /// <summary>
            /// Returns the current intermediates.
            /// </summary>
            public override ICoordinateCollection Intermediates
            {

                get
                {
                    return _graph._edgeShapes[_currentEdgeId];
                }
            }

            /// <summary>
            /// Resets this enumerator.
            /// </summary>
            public override void Reset()
            {
                _currentEdgeId = uint.MaxValue;
                _currentCount = -1;
            }

            public override IEnumerator<Edge<TEdgeData>> GetEnumerator()
            {
                this.Reset();
                return this;
            }

            public override Edge<TEdgeData> Current
            {
                get { return new Edge<TEdgeData>(this); }
            }

            public override void Dispose()
            {

            }


            public override bool HasCount
            {
                get { return _neighbour == 0; }
            }

            public override int Count
            {
                get { return (int)_count; }
            }

            /// <summary>
            /// Moves this enumerator to the given vertex.
            /// </summary>
            /// <param name="vertex"></param>
            public override void MoveTo(uint vertex)
            {
                 var vertexId = vertex * VERTEX_SIZE;
                 _startEdgeId = _graph._vertices[vertexId + FIRST_EDGE];
                 _count = _graph._vertices[vertexId + EDGE_COUNT];
                _neighbour = 0;

                // reset.
                _currentEdgeId = uint.MaxValue;
                _currentCount = -1;
            }

            /// <summary>
            /// Moves this enumerator to the given vertex with the given neighbour.
            /// </summary>
            /// <param name="vertex1"></param>
            /// <param name="vertex2"></param>
            public override void MoveTo(uint vertex1, uint vertex2)
            {
                var vertexId = vertex1 * VERTEX_SIZE;
                _startEdgeId = _graph._vertices[vertexId + FIRST_EDGE];
                _count = _graph._vertices[vertexId + EDGE_COUNT];
                _neighbour = vertex2;

                // reset.
                _currentEdgeId = uint.MaxValue;
                _currentCount = -1;
            }
        }

        /// <summary>
        /// Returns true if this graph is directed.
        /// </summary>
        public override bool IsDirected
        {
            get { return true; }
        }

        /// <summary>
        /// Returns true if this graph can have duplicate edges.
        /// </summary>
        public override bool CanHaveDuplicates
        {
            get { return true; }
        }

        /// <summary>
        /// Returns the readonly flag.
        /// </summary>
        public override bool IsReadonly
        {
            get
            {
                return _readonly;
            }
        }

        /// <summary>
        /// Sorts the graph based on the given transformations.
        /// </summary>
        public override void Sort(HugeArrayBase<uint> transformations)
        {
            // update edges.
            for (var i = 0; i <_nextEdgeId; i++)
            {
                _edges[i] = transformations[_edges[i]];
            }

            // sort vertices and coordinates.
            QuickSort.Sort((i) => transformations[i], (i, j) =>
            {
                var temp = _coordinates[i];
                _coordinates[i] = _coordinates[j];
                _coordinates[j] = temp;

                var tempRef = _vertices[i * 2];
                _vertices[i * 2] = _vertices[j * 2];
                _vertices[j * 2] = tempRef;
                tempRef = _vertices[i * 2 + 1];
                _vertices[i * 2 + 1] = _vertices[j * 2 + 1];
                _vertices[j * 2 + 1] = tempRef;

                var trans = transformations[i];
                transformations[i] = transformations[j];
                transformations[j] = trans;
            }, 1, _nextVertexId - 1);
        }

        #region Serialization

        /// <summary>
        /// Serializes this graph to disk.
        /// </summary>
        /// <param name="stream">The stream to write to. Writing will start at position 0.</param>
        /// <param name="edgeDataSize">The edge data size.</param>
        /// <param name="mapFrom">The map from for the edge data.</param>
        /// <param name="mapTo">The map to for the edge data.</param>
        public override long Serialize(System.IO.Stream stream, int edgeDataSize, MappedHugeArray<TEdgeData, uint>.MapFrom mapFrom,
            MappedHugeArray<TEdgeData, uint>.MapTo mapTo)
        {
            long vertexCount = (_nextVertexId - 1);
            long edgeCount = (_nextEdgeId / EDGE_SIZE);

            // write vertex and edge count.
            long position = 0;
            stream.Write(BitConverter.GetBytes((int)1), 0, 4);
            position = position + 4;
            stream.Write(BitConverter.GetBytes(vertexCount), 0, 8); // write exact number of vertices.
            position = position + 8;
            stream.Write(BitConverter.GetBytes(edgeCount), 0, 8); // write exact number of edges.
            position = position + 8;

            // write in this order: vertices, vertexCoordinates, edges, edgeData, edgeShapes.
            using (var file = new MemoryMappedStream(new OsmSharp.IO.LimitedStream(stream)))
            {
                // write vertices (each vertex = 1 uint (4 bytes)).
                var vertexArray = new MemoryMappedHugeArrayUInt32(file, (vertexCount + 1) * VERTEX_SIZE, (vertexCount + 1) * VERTEX_SIZE, 1024);
                vertexArray.CopyFrom(_vertices, 0, 0, (vertexCount + 1) * VERTEX_SIZE);
                vertexArray.Dispose(); // written, get rid of it!
                position = position + ((vertexCount + 1) * VERTEX_SIZE * 4);

                // write vertex coordinates (each vertex coordinate = 2 floats (8 bytes)).
                var vertexCoordinateArray = new MappedHugeArray<GeoCoordinateSimple, float>(
                    new MemoryMappedHugeArraySingle(file, (vertexCount + 1) * 2, (vertexCount + 1) * 2, 1024),
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
                vertexCoordinateArray.CopyFrom(_coordinates, 0, 0, (vertexCount + 1));
                vertexCoordinateArray.Dispose(); // written, get rid of it!
                position = position + ((vertexCount + 1) * 2 * 4);

                // write edges (each edge = 4 uints (16 bytes)).
                var edgeArray = new MemoryMappedHugeArrayUInt32(file, edgeCount * EDGE_SIZE, edgeCount * EDGE_SIZE, 1024);
                edgeArray.CopyFrom(_edges, edgeCount * EDGE_SIZE);
                edgeArray.Dispose(); // written, get rid of it!
                position = position + (edgeCount * EDGE_SIZE * 4);

                // write edge data (each edgeData = edgeDataSize units (edgeDataSize * 4 bytes)).
                var edgeDataArray = new MappedHugeArray<TEdgeData, uint>(
                    new MemoryMappedHugeArrayUInt32(file, edgeCount * edgeDataSize, edgeCount * edgeDataSize, 1024), edgeDataSize, mapTo, mapFrom);
                edgeDataArray.CopyFrom(_edgeData, edgeCount);
                edgeDataArray.Dispose(); // written, get rid of it!
                position = position + (edgeCount * edgeDataSize * 4);
            }

            // write shapes.
            stream.Seek(position, System.IO.SeekOrigin.Begin);
            position = position + _edgeShapes.Serialize(new OsmSharp.IO.LimitedStream(stream));
            return position;
        }

        /// <summary>
        /// Deserializes a graph from the given stream.
        /// </summary>
        /// <param name="stream">The stream to read from. Reading will start at position 0.</param>
        /// <param name="edgeDataSize">The edge data size.</param>
        /// <param name="mapFrom">The map from for the edge data.</param>
        /// <param name="mapTo">The map to for the edge data.</param>
        /// <param name="copy">Flag to make an in-memory copy.</param>
        /// <returns></returns>
        public new static DirectedGraph<TEdgeData> Deserialize(System.IO.Stream stream, int edgeDataSize, MappedHugeArray<TEdgeData, uint>.MapFrom mapFrom,
            MappedHugeArray<TEdgeData, uint>.MapTo mapTo, bool copy)
        {
            // read sizes.
            long position = 0;
            stream.Seek(4, System.IO.SeekOrigin.Begin);
            position = position + 4;
            var longBytes = new byte[8];
            stream.Read(longBytes, 0, 8);
            position = position + 8;
            var vertexLength = BitConverter.ToInt64(longBytes, 0);
            stream.Read(longBytes, 0, 8);
            position = position + 8;
            var edgeLength = BitConverter.ToInt64(longBytes, 0);

            var bufferSize = 32;
            var cacheSize = MemoryMappedHugeArrayUInt32.DefaultCacheSize;
            var file = new MemoryMappedStream(new OsmSharp.IO.LimitedStream(stream));
            var vertexArray = new MemoryMappedHugeArrayUInt32(file, (vertexLength + 1) * VERTEX_SIZE, (vertexLength + 1) * VERTEX_SIZE, bufferSize / 4, cacheSize * 4);
            position = position + ((vertexLength + 1) * VERTEX_SIZE * 4);
            var vertexCoordinateArray = new MappedHugeArray<GeoCoordinateSimple, float>(
                new MemoryMappedHugeArraySingle(file, (vertexLength + 1) * 2, (vertexLength + 1) * 2, bufferSize / 4, cacheSize * 4),
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
            position = position + ((vertexLength + 1) * 2 * 4);
            var edgeArray = new MemoryMappedHugeArrayUInt32(file, edgeLength * EDGE_SIZE, edgeLength * EDGE_SIZE, bufferSize / 2, cacheSize * 4);
            position = position + (edgeLength * EDGE_SIZE * 4);
            var edgeDataArray = new MappedHugeArray<TEdgeData, uint>(
                new MemoryMappedHugeArrayUInt32(file, edgeLength * edgeDataSize, edgeLength * edgeDataSize, bufferSize * 2, cacheSize * 2), edgeDataSize, mapTo, mapFrom);
            position = position + (edgeLength * edgeDataSize * 4);

            // deserialize shapes.
            stream.Seek(position, System.IO.SeekOrigin.Begin);
            var cappedStream = new OsmSharp.IO.LimitedStream(stream);

            var shapes = HugeCoordinateCollectionIndex.Deserialize(cappedStream, copy); 
            if (copy)
            { // copy the data.
                var vertexArrayCopy = new HugeArray<uint>(vertexArray.Length);
                vertexArrayCopy.CopyFrom(vertexArray);
                var vertexCoordinateArrayCopy = new HugeArray<GeoCoordinateSimple>(vertexCoordinateArray.Length);
                vertexCoordinateArrayCopy.CopyFrom(vertexCoordinateArray);
                var edgeArrayCopy = new HugeArray<uint>(edgeArray.Length);
                edgeArrayCopy.CopyFrom(edgeArray);
                var edgeDataArrayCopy = new HugeArray<TEdgeData>(edgeDataArray.Length);
                edgeDataArrayCopy.CopyFrom(edgeDataArray);

                file.Dispose();

                return new DirectedGraph<TEdgeData>(vertexCoordinateArrayCopy, vertexArrayCopy, edgeArrayCopy, edgeDataArrayCopy, shapes);
            }

            return new DirectedGraph<TEdgeData>(vertexCoordinateArray, vertexArray, edgeArray, edgeDataArray, shapes);
        }

        #endregion
    }
}