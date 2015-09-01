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
using OsmSharp.Collections.Sorting;
using OsmSharp.IO.MemoryMappedFiles;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Graph.Directed
{
    /// <summary>
    /// An directed graph.
    /// </summary>
    public class DirectedGraph<TEdgeData>
        where TEdgeData : struct, IEdgeData
    {
        private const int VERTEX_SIZE = 2; // holds the first edge index and the edge count.
        private const int FIRST_EDGE = 0;
        private const int EDGE_COUNT = 1;
        private const int EDGE_SIZE = 1; // holds only the target vertex.
        private const uint NO_EDGE = uint.MaxValue; // a dummy value indication that there is no edge.

        private uint _nextEdgeId;

        private readonly HugeArrayBase<uint> _vertices; // Holds all vertices pointing to it's first edge.
        private readonly HugeArrayBase<uint> _edges;
        private readonly HugeArrayBase<TEdgeData> _edgeData;
        private bool _readonly = false;

        /// <summary>
        /// Creates a new graph.
        /// </summary>
        public DirectedGraph()
            : this(1000)
        {

        }

        /// <summary>
        /// Creates a new graph using the existing data in the given arrays.
        /// </summary>
        private DirectedGraph(HugeArrayBase<uint> vertexArray,
            HugeArrayBase<uint> edgesArray,
            HugeArrayBase<TEdgeData> edgeDataArray)
        {
            _edgeCount = 0;
            _vertices = vertexArray;
            _edges = edgesArray;
            _edgeData = edgeDataArray;
            _nextEdgeId = (uint)(edgesArray.Length / EDGE_SIZE);
        }

        /// <summary>
        /// Creates a new graph.
        /// </summary>
        public DirectedGraph(long sizeEstimate)
            : this(sizeEstimate,
            new HugeArray<uint>(sizeEstimate),
            new HugeArray<uint>(sizeEstimate * 3 * EDGE_SIZE),
            new HugeArray<TEdgeData>(sizeEstimate * 3))
        {

        }

        /// <summary>
        /// Creates a graph.
        /// </summary>
        private DirectedGraph(long sizeEstimate, 
            HugeArrayBase<uint> vertexArray, HugeArrayBase<uint> edgesArray, HugeArrayBase<TEdgeData> edgeDataArray)
        {
            _nextEdgeId = 0;
            _edgeCount = 0;
            _vertices = vertexArray;
            _vertices.Resize(sizeEstimate);
            for (var idx = 0; idx < sizeEstimate; idx++)
            {
                _vertices[idx] = 0;
            }
            _edges = edgesArray;
            _edges.Resize(sizeEstimate * 3 * EDGE_SIZE);
            _edgeData = edgeDataArray;
            _edgeData.Resize(sizeEstimate * 3);
        }

        /// <summary>
        /// Creates a new graph using the given memorymapped file.
        /// </summary>
        public DirectedGraph(MemoryMappedFile file, long estimatedSize)
        {
            var mapper = default(TEdgeData) as IMappedEdgeData<TEdgeData>;
            if (mapper == null)
            { // oeps, this is impossible.
                throw new InvalidOperationException(
                    string.Format("Cannot create a memory-mapped graph with edge data type: {0}. " +
                        "There is no IMappedEdgeData implementation.", typeof(TEdgeData).ToInvariantString()));
            }

            _vertices = new MemoryMappedHugeArrayUInt32(file, estimatedSize);
            for (var idx = 0; idx < estimatedSize; idx++)
            {
                _vertices[idx] = 0;
            }
            _edges = new MemoryMappedHugeArrayUInt32(file, estimatedSize);
            _edgeData = new MappedHugeArray<TEdgeData, uint>(
                new MemoryMappedHugeArrayUInt32(file, estimatedSize * mapper.MappedSize), mapper.MappedSize,
                    mapper.MapToDelegate, mapper.MapFromDelegate);
            _edgeCount = 0;
        }

        private long _edgeCount;

        /// <summary>
        /// Increases the memory allocation.
        /// </summary>
        private void IncreaseVertexSize()
        {
            this.IncreaseVertexSize(_vertices.Length + 10000);
        }

        /// <summary>
        /// Increases the memory allocation.
        /// </summary>
        /// <param name="size"></param>
        private void IncreaseVertexSize(long size)
        {
            if (_readonly) { throw new Exception("Graph is readonly."); }

            var oldLength = _vertices.Length;
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
        }

        /// <summary>
        /// Adds an edge with the associated data.
        /// </summary>
        /// 
        public void AddEdge(uint vertex1, uint vertex2, TEdgeData data)
        {
            if (_readonly) { throw new Exception("Graph is readonly."); }
            if (vertex1 == vertex2) { throw new ArgumentException("Given vertices must be different."); }
            if (vertex1 * VERTEX_SIZE > _vertices.Length - 1 ) { this.IncreaseVertexSize(); }
            if (vertex2 * VERTEX_SIZE > _vertices.Length - 1) { this.IncreaseVertexSize(); }

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

            _edgeCount++;
            return;
        }

        /// <summary>
        /// Deletes all edges leading from/to the given vertex. 
        /// </summary>
        /// <remarks>Only deletes all edges vertex->* NOT *->vertex</remarks>
        public int RemoveEdges(uint vertex)
        {
            var removed = 0;
            var edges = this.GetEdgeEnumerator(vertex);
            while (edges.MoveNext())
            {
                removed += this.RemoveEdge(vertex, edges.Neighbour);
            }
            return removed;
        }

        /// <summary>
        /// Deletes the edge between the two given vertices.
        /// </summary>
        /// <remarks>Only deletes edge vertex1->vertex2 NOT vertex2 -> vertex1.</remarks>
        public int RemoveEdge(uint vertex1, uint vertex2)
        {
            if (_readonly) { throw new Exception("Graph is readonly."); }
            if (vertex1 >= _vertices.Length) { return 0; }
            if (vertex2 >= _vertices.Length) { return 0; }

            var removed = 0;
            var vertex1Idx = vertex1 * VERTEX_SIZE;
            var edgeCount = _vertices[vertex1Idx + EDGE_COUNT];
            var edgeId = _vertices[vertex1Idx + FIRST_EDGE];

            for (var removeIdx = edgeId; removeIdx < edgeId + edgeCount; removeIdx++)
            {
                if (_edges[removeIdx] == vertex2)
                {
                    edgeCount--;
                    _edges[removeIdx] = _edges[edgeId + edgeCount];
                    _edgeData[removeIdx] = _edgeData[edgeId + edgeCount];
                    removed++;
                    removeIdx--;

                    _edgeCount--;
                }
            }
            _vertices[vertex1Idx + EDGE_COUNT] = edgeCount;
            return removed;
        }

        /// <summary>
        /// Deletes the edge between the two given vertices.
        /// </summary>
        /// <remarks>Only deletes edge vertex1->vertex2 NOT vertex2 -> vertex1.</remarks>
        public bool RemoveEdge(uint vertex1, uint vertex2, TEdgeData data)
        {
            if (_readonly) { throw new Exception("Graph is readonly."); }
            if (vertex1 >= _vertices.Length) { return false; }
            if (vertex2 >= _vertices.Length) { return false; }

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
                    removed = true;

                    _edgeCount--;
                }
            }
            _vertices[vertex1Idx + EDGE_COUNT] = edgeCount;
            return removed;
        }

        /// <summary>
        /// Returns an empty edge enumerator.
        /// </summary>
        /// <returns></returns>
        public EdgeEnumerator GetEdgeEnumerator()
        {
            return new EdgeEnumerator(this);
        }

        /// <summary>
        /// Returns all edges starting at the given vertex.
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public EdgeEnumerator GetEdgeEnumerator(uint vertex)
        {
            var enumerator = new EdgeEnumerator(this);
            enumerator.MoveTo(vertex);
            return enumerator;
        }

        /// <summary>
        /// Returns the number of vertices in this graph.
        /// </summary>
        public uint VertexCount
        {
            get { return (uint)(_vertices.Length / VERTEX_SIZE); }
        }

        /// <summary>
        /// Returns the number of edges in this graph.
        /// </summary>
        public uint EdgeCount
        {
            get { return (uint)_edgeCount; }
        }

        /// <summary>
        /// Trims the internal data structures of this graph.
        /// </summary>
        public void Trim()
        {
            // remove all vertices without edges at the end.
            var maxVertexId = uint.MinValue;
            for(var i = 0; i < _vertices.Length / 2; i++)
            {
                var position = _vertices[i * 2 + FIRST_EDGE];
                var count = _vertices[i * 2 + EDGE_COUNT];
                for(var e = position; e < position + count; e++)
                {
                    var vertex = _edges[e];
                    if(maxVertexId < vertex)
                    {
                        maxVertexId = vertex;
                    }
                }
                if(count > 0 &&
                    maxVertexId < i * 2)
                { // also take into account the largest vertex pointing down.
                    maxVertexId = (uint)i * 2;
                }
            }
            _vertices.Resize((maxVertexId + 1) * VERTEX_SIZE);

            // resize edges.
            var edgeSize = _nextEdgeId;
            if (edgeSize == 0)
            { // keep minimum room for one edge.
                edgeSize = EDGE_SIZE;
            }
            _edgeData.Resize(edgeSize / EDGE_SIZE);
            _edges.Resize(edgeSize * EDGE_SIZE);
        }

        /// <summary>
        /// Compresses the data in this graph to it's smallest size.
        /// </summary>
        /// <param name="toReadonly">Flag to make the graph even smaller by converting it to a readonly version.</param>
        public void Compress(bool toReadonly)
        {
            // trim first.
            this.Trim();

            // build a list of all vertices sorted by their first position.
            var sortedVertices = new HugeArray<uint>(_vertices.Length / VERTEX_SIZE);
            for (uint i = 0; i < sortedVertices.Length; i++)
            {
                sortedVertices[i] = i;
            }

            // sort vertices and coordinates.
            QuickSort.Sort((i) => _vertices[i * 2], (i, j) =>
            {
                var tempRef = sortedVertices[i];
                sortedVertices[i] = sortedVertices[j];
                sortedVertices[j] = tempRef;
            }, 0, this.VertexCount - 1);

            // move data down.
            uint pointer = 0;
            for (uint i = 0; i < sortedVertices.Length; i++)
            {
                // move data.
                var vertex = sortedVertices[i] * VERTEX_SIZE;
                var count = _vertices[vertex + EDGE_COUNT];
                var position = _vertices[vertex + FIRST_EDGE];
                _vertices[vertex + FIRST_EDGE] = pointer;
                for (var e = 0; e < count; e++)
                {
                    _edgeData[pointer + e] = _edgeData[position + e];
                    _edges[pointer + e] = _edges[position + e];
                }

                if (!toReadonly && count > 2)
                { // next power of 2, don't do this on readonly to save space.
                    count |= count >> 1;
                    count |= count >> 2;
                    count |= count >> 4;
                    count |= count >> 8;
                    count |= count >> 16;
                    count++;
                }

                pointer += count;
            }
            _nextEdgeId = pointer;
            _readonly = toReadonly;
        }

        /// <summary>
        /// Compresses the data in this graph to it's smallest size.
        /// </summary>
        public void Compress()
        {
            this.Compress(false);
        }

        /// <summary>
        /// Represents the internal edge enumerator.
        /// </summary>
        public class EdgeEnumerator : IEnumerable<Edge<TEdgeData>>, IEnumerator<Edge<TEdgeData>>
        {
            private readonly DirectedGraph<TEdgeData> _graph;
            private uint _currentEdgeId;
            private int _currentCount;
            private uint _startEdgeId;
            private uint _count;
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
            public bool MoveNext()
            {
                if (_currentCount < 0)
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

                        if (_currentCount >= _count)
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
            public uint Neighbour
            {
                get { return _graph._edges[_currentEdgeId]; }
            }

            /// <summary>
            /// Returns the current edge data.
            /// </summary>
            public TEdgeData EdgeData
            {
                get
                {
                    return _graph._edgeData[_currentEdgeId];
                }
            }

            /// <summary>
            /// Resets this enumerator.
            /// </summary>
            public void Reset()
            {
                _currentEdgeId = uint.MaxValue;
                _currentCount = -1;
            }

            /// <summary>
            /// Gets the enumerator.
            /// </summary>
            /// <returns></returns>
            public IEnumerator<Edge<TEdgeData>> GetEnumerator()
            {
                this.Reset();
                return this;
            }

            /// <summary>
            /// Gets the current edge.
            /// </summary>
            public Edge<TEdgeData> Current
            {
                get { return new Edge<TEdgeData>(this); }
            }

            /// <summary>
            /// Disposes.
            /// </summary>
            public void Dispose()
            {

            }

            /// <summary>
            /// Returns the number of edges in this enumerator.
            /// </summary>
            public int Count
            {
                get { return (int)_count; }
            }

            /// <summary>
            /// Moves this enumerator to the given vertex.
            /// </summary>
            public bool MoveTo(uint vertex)
            {
                var vertexId = vertex * VERTEX_SIZE;
                _startEdgeId = _graph._vertices[vertexId + FIRST_EDGE];
                _count = _graph._vertices[vertexId + EDGE_COUNT];
                _neighbour = 0;

                // reset.
                _currentEdgeId = uint.MaxValue;
                _currentCount = -1;

                return _count > 0;
            }

            /// <summary>
            /// Returns the enumerator.
            /// </summary>
            /// <returns></returns>
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Gets the current edge.
            /// </summary>
            object System.Collections.IEnumerator.Current
            {
                get { throw new NotImplementedException(); }
            }
        }

        /// <summary>
        /// Returns the readonly flag.
        /// </summary>
        public bool IsReadonly
        {
            get
            {
                return _readonly;
            }
        }

        /// <summary>
        /// Sorts the graph based on the given transformations.
        /// </summary>
        public void Sort(HugeArrayBase<uint> transformations)
        {
            // update edges.
            for (var i = 0; i < _nextEdgeId; i++)
            {
                _edges[i] = transformations[_edges[i]];
            }

            // sort vertices and coordinates.
            QuickSort.Sort((i) => transformations[i], (i, j) =>
            {
                var tempRef = _vertices[i * 2];
                _vertices[i * 2] = _vertices[j * 2];
                _vertices[j * 2] = tempRef;
                tempRef = _vertices[i * 2 + 1];
                _vertices[i * 2 + 1] = _vertices[j * 2 + 1];
                _vertices[j * 2 + 1] = tempRef;

                var trans = transformations[i];
                transformations[i] = transformations[j];
                transformations[j] = trans;
            }, 1, this.VertexCount);
        }

        #region Serialization

        /// <summary>
        /// Serializes this graph to disk.
        /// </summary>
        public long Serialize(System.IO.Stream stream)
        {
            var mapper = default(TEdgeData) as IMappedEdgeData<TEdgeData>;
            if (mapper == null)
            { // oeps, this is impossible.
                throw new InvalidOperationException(
                    string.Format("Cannot create a memory-mapped graph with edge data type: {0}. " +
                        "There is no IMappedEdgeData implementation.", typeof(TEdgeData).ToInvariantString()));
            }

            var mapTo = mapper.MapToDelegate;
            var mapFrom = mapper.MapFromDelegate;
            var edgeDataSize = mapper.MappedSize;

            this.Compress();

            long vertexCount = this.VertexCount;
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
            return position;
        }

        /// <summary>
        /// Deserializes a graph from the given stream.
        /// </summary>
        /// <returns></returns>
        public static DirectedGraph<TEdgeData> Deserialize(System.IO.Stream stream, bool copy)
        {
            var mapper = default(TEdgeData) as IMappedEdgeData<TEdgeData>;
            if (mapper == null)
            { // oeps, this is impossible.
                throw new InvalidOperationException(
                    string.Format("Cannot create a memory-mapped graph with edge data type: {0}. " +
                        "There is no IMappedEdgeData implementation.", typeof(TEdgeData).ToInvariantString()));
            }

            var mapTo = mapper.MapToDelegate;
            var mapFrom = mapper.MapFromDelegate;
            var edgeDataSize = mapper.MappedSize;

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
            var edgeArray = new MemoryMappedHugeArrayUInt32(file, edgeLength * EDGE_SIZE, edgeLength * EDGE_SIZE, bufferSize / 2, cacheSize * 4);
            position = position + (edgeLength * EDGE_SIZE * 4);
            var edgeDataArray = new MappedHugeArray<TEdgeData, uint>(
                new MemoryMappedHugeArrayUInt32(file, edgeLength * edgeDataSize, edgeLength * edgeDataSize, bufferSize * 2, cacheSize * 2), edgeDataSize, mapTo, mapFrom);
            position = position + (edgeLength * edgeDataSize * 4);

            // deserialize shapes.
            stream.Seek(position, System.IO.SeekOrigin.Begin);
            var cappedStream = new OsmSharp.IO.LimitedStream(stream);

            if (copy)
            { // copy the data.
                var vertexArrayCopy = new HugeArray<uint>(vertexArray.Length);
                vertexArrayCopy.CopyFrom(vertexArray);
                var edgeArrayCopy = new HugeArray<uint>(edgeArray.Length);
                edgeArrayCopy.CopyFrom(edgeArray);
                var edgeDataArrayCopy = new HugeArray<TEdgeData>(edgeDataArray.Length);
                edgeDataArrayCopy.CopyFrom(edgeDataArray);

                file.Dispose();

                return new DirectedGraph<TEdgeData>(vertexArrayCopy, edgeArrayCopy, edgeDataArrayCopy);
            }

            return new DirectedGraph<TEdgeData>(vertexArray, edgeArray, edgeDataArray);
        }

        #endregion
    }
}