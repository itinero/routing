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

namespace OsmSharp.Routing.Graphs.Directed
{
    /// <summary>
    /// An directed graph.
    /// </summary>
    public class DirectedGraph : IDisposable
    {
        private const int VERTEX_SIZE = 2; // holds the first edge index and the edge count.
        private const int FIRST_EDGE = 0;
        private const int EDGE_COUNT = 1;
        private const int MINIMUM_EDGE_SIZE = 1; // holds only the target vertex.
        private const uint NO_EDGE = uint.MaxValue; // a dummy value indication that there is no edge.

        private readonly int _edgeSize = -1;
        private readonly int _edgeDataSize = -1;
        private readonly Action<uint, uint> _switchEdge;
        private readonly HugeArrayBase<uint> _vertices; // Holds all vertices pointing to it's first edge.
        private readonly HugeArrayBase<uint> _edges;

        private uint _nextEdgePointer;
        private bool _readonly = false;

        /// <summary>
        /// Creates a new graph.
        /// </summary>
        public DirectedGraph(int edgeDataSize)
            : this(edgeDataSize, 1000)
        {

        }

        /// <summary>
        /// Creates a new graph.
        /// </summary>
        public DirectedGraph(int edgeDataSize, long sizeEstimate)
            : this(edgeDataSize, sizeEstimate, null)
        {

        }

        /// <summary>
        /// Creates a new graph.
        /// </summary>
        public DirectedGraph(int edgeDataSize, long sizeEstimate, Action<uint, uint> switchEdge)
            : this(edgeDataSize, sizeEstimate,
            new HugeArray<uint>(sizeEstimate),
            new HugeArray<uint>(sizeEstimate * 3 * edgeDataSize + MINIMUM_EDGE_SIZE), switchEdge)
        {

        }

        /// <summary>
        /// Creates a new graph using the given memorymapped file.
        /// </summary>
        public DirectedGraph(MemoryMappedFile file, int edgeDataSize, long sizeEstimate, Action<uint, uint> switchEdge)
        {
            _vertices = new MemoryMappedHugeArrayUInt32(file, sizeEstimate);
            for (var idx = 0; idx < sizeEstimate; idx++)
            {
                _vertices[idx] = 0;
            }
            _edges = new MemoryMappedHugeArrayUInt32(file, sizeEstimate);
            _edgeCount = 0;
            _edgeDataSize = edgeDataSize;
            _switchEdge = switchEdge;
        }

        /// <summary>
        /// Creates a new graph using the existing data in the given arrays.
        /// </summary>
        private DirectedGraph(int edgeDataSize, long edgeCount, HugeArrayBase<uint> vertices,
            HugeArrayBase<uint> edges)
        {
            _edgeSize = edgeDataSize + MINIMUM_EDGE_SIZE;
            _edgeDataSize = edgeDataSize;

            _edgeCount = edgeCount;
            _vertices = vertices;
            _edges = edges;
            _nextEdgePointer = (uint)(edges.Length / _edgeSize);
        }

        /// <summary>
        /// Creates a graph.
        /// </summary>
        private DirectedGraph(int edgeDataSize, long sizeEstimate,
            HugeArrayBase<uint> vertices, HugeArrayBase<uint> edges, Action<uint, uint> switchEdge)
        {
            _edgeSize = edgeDataSize + MINIMUM_EDGE_SIZE;
            _edgeDataSize = edgeDataSize;

            _nextEdgePointer = 0;
            _edgeCount = 0;
            _vertices = vertices;
            _vertices.Resize(sizeEstimate);
            for (var idx = 0; idx < sizeEstimate; idx++)
            {
                _vertices[idx] = 0;
            }
            _edges = edges;
            _edges.Resize(sizeEstimate * 3 * _edgeSize);
            _switchEdge = switchEdge;
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
        }

        /// <summary>
        /// Adds an edge with the associated data.
        /// </summary>
        public uint AddEdge(uint vertex1, uint vertex2, params uint[] data)
        {
            if (_readonly) { throw new Exception("Graph is readonly."); }
            if (vertex1 == vertex2) { throw new ArgumentException("Given vertices must be different."); }
            if (vertex1 * VERTEX_SIZE > _vertices.Length - 1 ) { this.IncreaseVertexSize(); }
            if (vertex2 * VERTEX_SIZE > _vertices.Length - 1) { this.IncreaseVertexSize(); }

            var vertexPointer = vertex1 * VERTEX_SIZE;
            var edgeCount = _vertices[vertexPointer + EDGE_COUNT];
            var edgePointer = _vertices[vertexPointer + FIRST_EDGE] * (uint)_edgeSize;
            var edgeId = uint.MaxValue;

            if (edgeCount == 0)
            { // no edge yet, just add the end.
                _vertices[vertexPointer + EDGE_COUNT] = 1;
                _vertices[vertexPointer + FIRST_EDGE] = _nextEdgePointer / (uint)_edgeSize;
                edgeId = _nextEdgePointer / (uint)_edgeSize;

                if (_nextEdgePointer + (1 * _edgeSize) >= _edges.Length)
                { // make sure we can add another edge.
                    this.IncreaseEdgeSize();
                }

                _edges[_nextEdgePointer] = vertex2;
                for (uint i = 0; i < _edgeDataSize; i++)
                {
                    _edges[_nextEdgePointer + MINIMUM_EDGE_SIZE + i] =
                        data[i];
                }
                _nextEdgePointer += (uint)(_edgeSize);
            }
            else if ((edgeCount & (edgeCount - 1)) == 0)
            { // edgeCount is a power of two, increase space.
                if (_nextEdgePointer + (edgeCount * _edgeSize) >= _edges.Length)
                { // make sure we can add another edge.
                    this.IncreaseEdgeSize();
                }

                if(edgePointer == (_nextEdgePointer - (edgeCount * _edgeSize)))
                { // these edge are at the end of the edge-array, don't copy just increase size.
                    _edges[_nextEdgePointer] = vertex2;
                    edgeId = _nextEdgePointer / (uint)_edgeSize;
                    for(uint i = 0; i < _edgeDataSize; i++)
                    {
                        _edges[_nextEdgePointer + MINIMUM_EDGE_SIZE + i] =
                            data[i];
                    }
                    _nextEdgePointer += (uint)(edgeCount * _edgeSize); // duplicate space for this vertex.
                    _vertices[vertexPointer + EDGE_COUNT] = edgeCount + 1;
                }
                else
                { // not at the end, copy edges to the end.
                    _vertices[vertexPointer + FIRST_EDGE] = _nextEdgePointer / (uint)_edgeSize;
                    _vertices[vertexPointer + EDGE_COUNT] = edgeCount + 1;

                    // keep new pointer & duplicate space for this vertex.
                    var newNextEdgePointer = _nextEdgePointer + (uint)(edgeCount * 2 * _edgeSize);

                    if (_nextEdgePointer + (edgeCount * _edgeSize) >= _edges.Length)
                    { // make sure we can add another edge.
                        this.IncreaseEdgeSize();
                    }

                    for(var edge = 0; edge <  edgeCount; edge++)
                    {
                        _edges[_nextEdgePointer] = _edges[edgePointer + (edge * _edgeSize)];
                        for (uint i = 0; i < _edgeDataSize; i++)
                        {
                            _edges[_nextEdgePointer + MINIMUM_EDGE_SIZE + i] =
                                _edges[edgePointer + MINIMUM_EDGE_SIZE + i + (edge * _edgeSize)];
                        }
                        if (_switchEdge != null)
                        { // report on the edge switch.
                            _switchEdge((uint)((edgePointer + (edge * _edgeSize)) / (long)_edgeSize),
                                _nextEdgePointer / (uint)_edgeSize);
                        }
                        _nextEdgePointer += (uint)_edgeSize;

                        if (_nextEdgePointer >= _edges.Length)
                        { // make sure we can add another edge.
                            this.IncreaseEdgeSize();
                        }
                    }

                    if (_nextEdgePointer + (edgeCount * _edgeSize) >= _edges.Length)
                    { // make sure we can add another edge.
                        this.IncreaseEdgeSize();
                    }

                    // add at the end.
                    _edges[_nextEdgePointer] = vertex2;
                    edgeId = _nextEdgePointer / (uint)_edgeSize;
                    for (uint i = 0; i < _edgeDataSize; i++)
                    {
                        _edges[_nextEdgePointer + MINIMUM_EDGE_SIZE + i] =
                            data[i];
                    }
                    _nextEdgePointer = newNextEdgePointer;
                }
            }
            else
            { // just add the edge.
                _edges[edgePointer + (edgeCount * (uint)_edgeSize)] = vertex2;
                edgeId = (edgePointer + (edgeCount * (uint)_edgeSize)) / (uint)_edgeSize;
                for (uint i = 0; i < _edgeDataSize; i++)
                {
                    _edges[edgePointer + (edgeCount * (uint)_edgeSize) + MINIMUM_EDGE_SIZE + i] =
                        data[i];
                }

                _vertices[vertexPointer + EDGE_COUNT] = edgeCount + 1;
            }
            _edgeCount++;

            return edgeId;
        }

        /// <summary>
        /// Updates and edge's associated data.
        /// </summary>
        public bool UpdateEdge(uint vertex1, uint vertex2, Func<uint[], bool> update, params uint[] data)
        {
            if (_readonly) { throw new Exception("Graph is readonly."); }
            if (vertex1 == vertex2) { throw new ArgumentException("Given vertices must be different."); }
            if (vertex1 * VERTEX_SIZE > _vertices.Length - 1) { this.IncreaseVertexSize(); }
            if (vertex2 * VERTEX_SIZE > _vertices.Length - 1) { this.IncreaseVertexSize(); }

            var vertexPointer = vertex1 * VERTEX_SIZE;
            var edgeCount = _vertices[vertexPointer + EDGE_COUNT];
            var edgePointer = _vertices[vertexPointer + FIRST_EDGE] * (uint)_edgeSize;
            var lastEdgePointer = edgePointer + (edgeCount * (uint)_edgeSize);

            var currentData = new uint[_edgeDataSize];
            while (edgePointer <= lastEdgePointer)
            {
                for (uint i = 0; i < _edgeDataSize; i++)
                {
                    currentData[i] = _edges[edgePointer + MINIMUM_EDGE_SIZE + i];
                }
                if(_edges[edgePointer] == vertex2)
                {
                    if (update(currentData))
                    { // yes, update here.
                        for (uint i = 0; i < _edgeDataSize; i++)
                        {
                            _edges[edgePointer + MINIMUM_EDGE_SIZE + i] =
                                data[i];
                        }
                        return true;
                    }
                }
                edgePointer += (uint)_edgeSize;
            }
            return false;
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
            var vertexPointer = vertex1 * VERTEX_SIZE;
            var edgeCount = _vertices[vertexPointer + EDGE_COUNT];
            var edgePointer = _vertices[vertexPointer + FIRST_EDGE] * (uint)_edgeSize;

            for (var removeEdgePointer = edgePointer; removeEdgePointer < edgePointer + (edgeCount * (uint)_edgeSize); 
                removeEdgePointer += (uint)_edgeSize)
            {
                if (_edges[removeEdgePointer] == vertex2)
                { // edge found, remove it.
                    removed++;
                    _edgeCount--;

                    // reduce edge count.
                    edgeCount--;
                    if (removeEdgePointer == edgePointer + (edgeCount * (uint)_edgeSize))
                    { // no need to move data anymore, this is the last edge being removed.
                        break;
                    }

                    // move the last edge and overwrite the current edge.
                    _edges[removeEdgePointer] = _edges[edgePointer + (edgeCount * (uint)_edgeSize)];
                    for (var i = 0; i < _edgeDataSize; i++)
                    {
                        _edges[removeEdgePointer + MINIMUM_EDGE_SIZE + i] =
                            _edges[edgePointer + (edgeCount * (uint)_edgeSize) + MINIMUM_EDGE_SIZE + i];
                    }

                    // report on the move.
                    if(_switchEdge != null)
                    {
                        _switchEdge((uint)((edgePointer + (edgeCount * (uint)_edgeSize)) / _edgeSize),
                            (uint)(removeEdgePointer / _edgeSize));
                    }

                    removeEdgePointer -= (uint)_edgeSize;
                }
            }
            _vertices[vertexPointer + EDGE_COUNT] = edgeCount;
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
            for(uint i = 0; i < _vertices.Length / VERTEX_SIZE; i++)
            {
                var pointer = _vertices[i * VERTEX_SIZE + FIRST_EDGE] * _edgeSize;
                var count = _vertices[i * VERTEX_SIZE + EDGE_COUNT];
                for(var e = pointer; e < pointer + (count * _edgeSize); e += _edgeSize)
                {
                    var vertex = _edges[e];
                    if(maxVertexId < vertex)
                    {
                        maxVertexId = vertex;
                    }
                }
                if(count > 0 &&
                   maxVertexId < i)
                { // also take into account the largest vertex pointing down.
                    maxVertexId = i;
                }
            }
            _vertices.Resize((maxVertexId + 1) * VERTEX_SIZE);

            // resize edges.
            var edgesLength = _nextEdgePointer;
            if (edgesLength == 0)
            { // keep minimum room for one edge.
                edgesLength = (uint)_edgeSize;
            }
            _edges.Resize(edgesLength);
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
            QuickSort.Sort((i) => _vertices[sortedVertices[i] * VERTEX_SIZE], 
                (i, j) =>
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
                var vertexPointer = sortedVertices[i] * VERTEX_SIZE;
                var count = _vertices[vertexPointer + EDGE_COUNT];
                var edgePointer = _vertices[vertexPointer + FIRST_EDGE] * (uint)_edgeSize;
                _vertices[vertexPointer + FIRST_EDGE] = pointer / (uint)_edgeSize;
                for (uint e = 0; e < count * (uint)_edgeSize; e += (uint)_edgeSize)
                {
                    _edges[pointer + e] = _edges[edgePointer + e];
                    for (var j = 0; j < _edgeDataSize; j++)
                    {
                        _edges[pointer + e + MINIMUM_EDGE_SIZE + j] =
                            _edges[edgePointer + e + MINIMUM_EDGE_SIZE + j];
                    }

                    // report on the move.
                    if (_switchEdge != null)
                    {
                        _switchEdge((uint)(edgePointer / _edgeSize),
                            (uint)(pointer / _edgeSize));
                    }
                }

                if (!toReadonly && count > 2 && !((count & (count - 1)) == 0))
                { // next power of 2, don't do this on readonly to save space.
                    count |= count >> 1;
                    count |= count >> 2;
                    count |= count >> 4;
                    count |= count >> 8;
                    count |= count >> 16;
                    count++;
                }

                pointer += count * (uint)_edgeSize;
            }
            _nextEdgePointer = pointer;
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
        public class EdgeEnumerator : IEnumerable<Edge>, IEnumerator<Edge>
        {
            private readonly DirectedGraph _graph;
            private uint _currentEdgePointer;
            private int _currentCount;
            private uint _startEdgeId;
            private uint _count;
            private uint _neighbour;

            /// <summary>
            /// Creates a new edge enumerator.
            /// </summary>
            public EdgeEnumerator(DirectedGraph graph)
            {
                _graph = graph;
                _startEdgeId = 0;
                _count = 0;
                _neighbour = 0;

                // reset.
                _currentEdgePointer = uint.MaxValue;
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
                    _currentEdgePointer = _startEdgeId;
                    _currentCount = 0;
                }
                else
                {
                    _currentEdgePointer += (uint)_graph._edgeSize;
                    _currentCount++;
                }
                if (_currentCount < _count)
                {
                    while (_neighbour != 0 &&
                        _neighbour != this.Neighbour)
                    {
                        _currentEdgePointer += (uint)_graph._edgeSize;
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
                get { return _graph._edges[_currentEdgePointer]; }
            }

            /// <summary>
            /// Returns the current edge data.
            /// </summary>
            public uint[] Data
            {
                get
                {
                    var data = new uint[_graph._edgeDataSize];
                    for (var i = 0; i < _graph._edgeDataSize; i++)
                    {
                        data[i] = _graph._edges[_currentEdgePointer + MINIMUM_EDGE_SIZE + i];
                    }
                    return data;
                }
            }

            /// <summary>
            /// Returns the first entry in the edge data.
            /// </summary>
            public uint Data0
            {
                get
                {
                    return _graph._edges[_currentEdgePointer + MINIMUM_EDGE_SIZE + 0];
                }
            }

            /// <summary>
            /// Returns the second entry in the edge data.
            /// </summary>
            public uint Data1
            {
                get
                {
                    return _graph._edges[_currentEdgePointer + MINIMUM_EDGE_SIZE + 1];
                }
            }

            /// <summary>
            /// Returns the edge id.
            /// </summary>
            public uint Id
            {
                get
                {
                    return _currentEdgePointer / (uint)_graph._edgeSize;
                }
            }

            /// <summary>
            /// Resets this enumerator.
            /// </summary>
            public void Reset()
            {
                _currentEdgePointer = uint.MaxValue;
                _currentCount = -1;
            }

            /// <summary>
            /// Gets the enumerator.
            /// </summary>
            /// <returns></returns>
            public IEnumerator<Edge> GetEnumerator()
            {
                this.Reset();
                return this;
            }

            /// <summary>
            /// Gets the current edge.
            /// </summary>
            public Edge Current
            {
                get { return new Edge(this); }
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
                _startEdgeId = _graph._vertices[vertexId + FIRST_EDGE] * (uint)_graph._edgeSize;
                _count = _graph._vertices[vertexId + EDGE_COUNT];
                _neighbour = 0;

                // reset.
                _currentEdgePointer = uint.MaxValue;
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

        #region Serialization

        /// <summary>
        /// Serializes this graph to disk.
        /// </summary>
        public long Serialize(System.IO.Stream stream)
        {
            this.Compress();

            long vertexCount = this.VertexCount;
            long edgeCount = (_nextEdgePointer / _edgeSize);

            // write vertex and edge count.
            long position = 0;
            stream.Write(BitConverter.GetBytes(vertexCount), 0, 8); // write exact number of vertices.
            position = position + 8;
            stream.Write(BitConverter.GetBytes(edgeCount), 0, 8); // write exact number of edges.
            position = position + 8;
            stream.Write(BitConverter.GetBytes(2), 0, 4); // write the vertex size.
            position = position + 4;
            stream.Write(BitConverter.GetBytes(_edgeSize), 0, 4); // write the edge size.
            position = position + 4;

            // write in this order: vertices, vertexCoordinates, edges, edgeData, edgeShapes.
            using (var file = new MemoryMappedStream(new OsmSharp.IO.LimitedStream(stream)))
            {
                // write vertices (each vertex = 1 uint (4 bytes)).
                var vertexArray = new MemoryMappedHugeArrayUInt32(file, vertexCount * VERTEX_SIZE, vertexCount * VERTEX_SIZE, 1024);
                vertexArray.CopyFrom(_vertices, 0, 0, vertexCount * VERTEX_SIZE);
                vertexArray.Dispose(); // written, get rid of it!
                position = position + vertexCount * VERTEX_SIZE * 4;

                // write edges (each edge = 4 uints (16 bytes)).
                var edgeArray = new MemoryMappedHugeArrayUInt32(file, edgeCount * _edgeSize, edgeCount * _edgeSize, 1024);
                edgeArray.CopyFrom(_edges, edgeCount * _edgeSize);
                edgeArray.Dispose(); // written, get rid of it!
                position = position + edgeCount * _edgeSize * 4;
            }
            return position;
        }

        /// <summary>
        /// Deserializes a graph from the given stream.
        /// </summary>
        /// <returns></returns>
        public static DirectedGraph Deserialize(System.IO.Stream stream, bool copy)
        {
            // read sizes.
            long position = 0;
            var bytes = new byte[8];
            stream.Read(bytes, 0, 8);
            position = position + 8;
            var vertexLength = BitConverter.ToInt64(bytes, 0);
            stream.Read(bytes, 0, 8);
            position = position + 8;
            var edgeLength = BitConverter.ToInt64(bytes, 0);
            stream.Read(bytes, 0, 4);
            position = position + 4;
            var vertexSize = BitConverter.ToInt32(bytes, 0);
            stream.Read(bytes, 0, 4);
            position = position + 4;
            var edgeSize = BitConverter.ToInt32(bytes, 0);

            var bufferSize = 32;
            var cacheSize = MemoryMappedHugeArrayUInt32.DefaultCacheSize;
            var file = new MemoryMappedStream(new OsmSharp.IO.LimitedStream(stream));
            var vertexArray = new MemoryMappedHugeArrayUInt32(file, vertexLength * VERTEX_SIZE, vertexLength * VERTEX_SIZE, bufferSize / 4, cacheSize * 4);
            position = position + (vertexLength * VERTEX_SIZE * 4);
            var edgeArray = new MemoryMappedHugeArrayUInt32(file, edgeLength * edgeSize, edgeLength * edgeSize, bufferSize / 2, cacheSize * 4);
            position = position + (edgeLength * edgeSize * 4);

            // deserialize shapes.
            stream.Seek(position, System.IO.SeekOrigin.Begin);
            var cappedStream = new OsmSharp.IO.LimitedStream(stream);

            if (copy)
            { // copy the data.
                var vertexArrayCopy = new HugeArray<uint>(vertexArray.Length);
                vertexArrayCopy.CopyFrom(vertexArray);
                var edgeArrayCopy = new HugeArray<uint>(edgeArray.Length);
                edgeArrayCopy.CopyFrom(edgeArray);

                file.Dispose();

                return new DirectedGraph(edgeSize - MINIMUM_EDGE_SIZE, edgeLength, vertexArrayCopy, edgeArrayCopy);
            }
            return new DirectedGraph(edgeSize - MINIMUM_EDGE_SIZE, edgeLength, vertexArray, edgeArray);
        }

        #endregion

        /// <summary>
        /// Releases unmanaged resources associated with this graph.
        /// </summary>
        public void Dispose()
        {
            _edges.Dispose();
            _vertices.Dispose();
        }
    }
}