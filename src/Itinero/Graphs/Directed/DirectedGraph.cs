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

using Itinero.Algorithms.Sorting;
using Reminiscence.Arrays;
using Reminiscence.IO;
using Reminiscence.IO.Streams;
using System;
using System.Collections.Generic;

namespace Itinero.Graphs.Directed
{
    /// <summary>
    /// An directed graph.
    /// </summary>
    public sealed class DirectedGraph : IDisposable
    {
        private const int VERTEX_SIZE = 2; // holds the first edge index and the edge count.
        private const int FIRST_EDGE = 0;
        private const int EDGE_COUNT = 1;
        private const int MINIMUM_EDGE_SIZE = 1; // holds only the target vertex.
        private const uint NO_EDGE = uint.MaxValue; // a dummy value indication that there is no edge.

        private readonly int _edgeSize = -1;
        private readonly int _edgeDataSize = -1;
        private readonly Action<uint, uint> _switchEdge;
        private readonly ArrayBase<uint> _vertices; // Holds all vertices pointing to it's first edge.
        private readonly ArrayBase<uint> _edges;

        private long _nextEdgePointer;
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
        public DirectedGraph(int edgeDataSize, Action<uint, uint> switchEdge)
            : this(edgeDataSize, 1000, switchEdge)
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
            Context.ArrayFactory.CreateMemoryBackedArray<uint>(sizeEstimate),
            Context.ArrayFactory.CreateMemoryBackedArray<uint>(sizeEstimate * 3 * edgeDataSize + MINIMUM_EDGE_SIZE), switchEdge)
        {

        }

        /// <summary>
        /// Creates a new graph using the given memorymapped file.
        /// </summary>
        public DirectedGraph(MemoryMap file, int edgeDataSize, long sizeEstimate, Action<uint, uint> switchEdge)
        {
            _vertices = new Array<uint>(file, sizeEstimate);
            for (var idx = 0; idx < sizeEstimate; idx++)
            {
                _vertices[idx] = 0;
            }
            _edges = new Array<uint>(file, sizeEstimate);
            _edgeCount = 0;
            _edgeDataSize = edgeDataSize;
            _switchEdge = switchEdge;
        }

        /// <summary>
        /// Creates a new graph using the existing data in the given arrays.
        /// </summary>
        private DirectedGraph(int edgeDataSize, long edgeCount, ArrayBase<uint> vertices,
            ArrayBase<uint> edges)
        {
            _edgeSize = edgeDataSize + MINIMUM_EDGE_SIZE;
            _edgeDataSize = edgeDataSize;

            _edgeCount = edgeCount;
            _vertices = vertices;
            _edges = edges;
            _nextEdgePointer = (edges.Length / _edgeSize);
        }

        /// <summary>
        /// Creates a graph.
        /// </summary>
        private DirectedGraph(int edgeDataSize, long sizeEstimate,
            ArrayBase<uint> vertices, ArrayBase<uint> edges, Action<uint, uint> switchEdge)
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
        /// Gets the edge data size.
        /// </summary>
        public int EdgeDataSize
        {
            get
            {
                return _edgeDataSize;
            }
        }

        /// <summary>
        /// Adds an edge with the associated data.
        /// </summary>
        public uint AddEdge(uint vertex1, uint vertex2, uint data)
        {
            if (_readonly) { throw new Exception("Graph is readonly."); }
            if (vertex1 == vertex2) { throw new ArgumentException("Given vertices must be different."); }
            _vertices.EnsureMinimumSize(Math.Max(vertex1, vertex2) * VERTEX_SIZE + EDGE_COUNT + 1);
            if (_edgeDataSize != 1) { throw new ArgumentOutOfRangeException("Dimension of data doesn't match."); }
            
            if ((_nextEdgePointer / _edgeSize) > uint.MaxValue) { throw new Exception($"Cannot add another edge, this graph can only handle a max of {uint.MaxValue} edges.");}
            
            var vertexPointer = vertex1 * VERTEX_SIZE;
            var edgeCount = _vertices[vertexPointer + EDGE_COUNT];
            var edgePointer = _vertices[vertexPointer + FIRST_EDGE] * _edgeSize;
            var edgeId = uint.MaxValue;

            if (edgeCount == 0)
            { // no edge yet, just add the end.
                _vertices[vertexPointer + EDGE_COUNT] = 1;
                _vertices[vertexPointer + FIRST_EDGE] = (uint)(_nextEdgePointer / _edgeSize);
                edgeId = (uint)(_nextEdgePointer / _edgeSize);

                // make sure we can add another edge.
                _edges.EnsureMinimumSize(_nextEdgePointer + (1 * _edgeSize) + 1);

                _edges[_nextEdgePointer] = vertex2;
                _edges[_nextEdgePointer + MINIMUM_EDGE_SIZE + 0] = data;
                _nextEdgePointer += _edgeSize;
            }
            else if ((edgeCount & (edgeCount - 1)) == 0)
            { // edgeCount is a power of two, increase space.
                // make sure we can add another edge.
                _edges.EnsureMinimumSize(_nextEdgePointer + (edgeCount * _edgeSize) + 1);

                if (edgePointer == (_nextEdgePointer - (edgeCount * _edgeSize)))
                { // these edge are at the end of the edge-array, don't copy just increase size.
                    _edges[_nextEdgePointer] = vertex2;
                    edgeId = (uint)(_nextEdgePointer / _edgeSize);
                    _edges[_nextEdgePointer + MINIMUM_EDGE_SIZE + 0] = data;
                    _nextEdgePointer += (edgeCount * _edgeSize); // duplicate space for this vertex.
                    _vertices[vertexPointer + EDGE_COUNT] = edgeCount + 1;
                }
                else
                { // not at the end, copy edges to the end.
                    _vertices[vertexPointer + FIRST_EDGE] = (uint)(_nextEdgePointer / _edgeSize);
                    _vertices[vertexPointer + EDGE_COUNT] = edgeCount + 1;

                    // keep new pointer & duplicate space for this vertex.
                    var newNextEdgePointer = _nextEdgePointer + (edgeCount * 2 * _edgeSize);

                    // make sure we can add another edge.
                    _edges.EnsureMinimumSize(_nextEdgePointer + (edgeCount * _edgeSize) + 1);

                    for (var edge = 0; edge < edgeCount; edge++)
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
                                (uint)(_nextEdgePointer / _edgeSize));
                        }
                        _nextEdgePointer += _edgeSize;

                        // make sure we can add another edge.
                        _edges.EnsureMinimumSize(_nextEdgePointer + 1);
                    }

                    // make sure we can add another edge.
                    _edges.EnsureMinimumSize(_nextEdgePointer + (edgeCount * _edgeSize) + 1);

                    // add at the end.
                    _edges[_nextEdgePointer] = vertex2;
                    edgeId = (uint)(_nextEdgePointer / _edgeSize);
                    _edges[_nextEdgePointer + MINIMUM_EDGE_SIZE + 0] = data;
                    _nextEdgePointer = newNextEdgePointer;
                }
            }
            else
            { // just add the edge.
                _edges[edgePointer + (edgeCount * _edgeSize)] = vertex2;
                edgeId = (uint)((edgePointer + (edgeCount * _edgeSize)) / _edgeSize);
                _edges[edgePointer + (edgeCount * _edgeSize) + MINIMUM_EDGE_SIZE + 0] = data;

                _vertices[vertexPointer + EDGE_COUNT] = edgeCount + 1;
            }
            _edgeCount++;

            return edgeId;
        }

        /// <summary>
        /// Adds an edge with the associated data.
        /// </summary>
        public uint AddEdge(uint vertex1, uint vertex2, params uint[] data)
        {
            if (_readonly) { throw new Exception("Graph is readonly."); }
            if (vertex1 == vertex2) { throw new ArgumentException("Given vertices must be different."); }
            _vertices.EnsureMinimumSize(Math.Max(vertex1, vertex2) * VERTEX_SIZE + EDGE_COUNT + 1);
            
            if ((_nextEdgePointer / _edgeSize) > uint.MaxValue) { throw new Exception($"Cannot add another edge, this graph can only handle a max of {uint.MaxValue} edges.");}

            var vertexPointer = vertex1 * VERTEX_SIZE;
            var edgeCount = _vertices[vertexPointer + EDGE_COUNT];
            var edgePointer = _vertices[vertexPointer + FIRST_EDGE] * (uint)_edgeSize;
            var edgeId = uint.MaxValue;

            if (edgeCount == 0)
            { // no edge yet, just add the end.
                _vertices[vertexPointer + EDGE_COUNT] = 1;
                _vertices[vertexPointer + FIRST_EDGE] = (uint)(_nextEdgePointer / _edgeSize);
                edgeId = (uint)(_nextEdgePointer / _edgeSize);

                // make sure we can add another edge.
                _edges.EnsureMinimumSize(_nextEdgePointer + (1 * _edgeSize) + 1);

                _edges[_nextEdgePointer] = vertex2;
                for (uint i = 0; i < _edgeDataSize; i++)
                {
                    _edges[_nextEdgePointer + MINIMUM_EDGE_SIZE + i] =
                        data[i];
                }
                _nextEdgePointer += _edgeSize;
            }
            else if ((edgeCount & (edgeCount - 1)) == 0)
            { // edgeCount is a power of two, increase space.
                // make sure we can add another edge.
                _edges.EnsureMinimumSize(_nextEdgePointer + (edgeCount * _edgeSize) + 1);

                if (edgePointer == (_nextEdgePointer - (edgeCount * _edgeSize)))
                { // these edge are at the end of the edge-array, don't copy just increase size.
                    _edges[_nextEdgePointer] = vertex2;
                    edgeId = (uint)(_nextEdgePointer / _edgeSize);
                    for (uint i = 0; i < _edgeDataSize; i++)
                    {
                        _edges[_nextEdgePointer + MINIMUM_EDGE_SIZE + i] =
                            data[i];
                    }
                    _nextEdgePointer += (edgeCount * _edgeSize); // duplicate space for this vertex.
                    _vertices[vertexPointer + EDGE_COUNT] = edgeCount + 1;
                }
                else
                { // not at the end, copy edges to the end.
                    _vertices[vertexPointer + FIRST_EDGE] = (uint)(_nextEdgePointer / _edgeSize);
                    _vertices[vertexPointer + EDGE_COUNT] = edgeCount + 1;

                    // keep new pointer & duplicate space for this vertex.
                    var newNextEdgePointer = _nextEdgePointer + (edgeCount * 2 * _edgeSize);

                    // make sure we can add another edge.
                    _edges.EnsureMinimumSize(_nextEdgePointer + (edgeCount * _edgeSize) + 1);

                    for (var edge = 0; edge < edgeCount; edge++)
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
                                (uint)(_nextEdgePointer / _edgeSize));
                        }
                        _nextEdgePointer += _edgeSize;

                        // make sure we can add another edge.
                        _edges.EnsureMinimumSize(_nextEdgePointer + 1);
                    }

                    // make sure we can add another edge.
                    _edges.EnsureMinimumSize(_nextEdgePointer + (edgeCount * _edgeSize) + 1);

                    // add at the end.
                    _edges[_nextEdgePointer] = vertex2;
                    edgeId = (uint)(_nextEdgePointer / _edgeSize);
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
        public uint UpdateEdge(uint vertex1, uint vertex2, Func<uint[], bool> update, params uint[] data)
        {
            if (_readonly) { throw new Exception("Graph is readonly."); }
            if (vertex1 == vertex2) { throw new ArgumentException("Given vertices must be different."); }
            _vertices.EnsureMinimumSize(Math.Max(vertex1, vertex2) * VERTEX_SIZE + EDGE_COUNT + 1);

            var vertexPointer = vertex1 * VERTEX_SIZE;
            var edgeCount = _vertices[vertexPointer + EDGE_COUNT];
            var edgePointer = _vertices[vertexPointer + FIRST_EDGE] * _edgeSize;
            var lastEdgePointer = edgePointer + (edgeCount * _edgeSize);

            var currentData = new uint[_edgeDataSize];
            while (edgePointer < lastEdgePointer)
            {
                for (uint i = 0; i < _edgeDataSize; i++)
                {
                    currentData[i] = _edges[edgePointer + MINIMUM_EDGE_SIZE + i];
                }
                if (_edges[edgePointer] == vertex2)
                {
                    if (update(currentData))
                    { // yes, update here.
                        for (uint i = 0; i < _edgeDataSize; i++)
                        {
                            _edges[edgePointer + MINIMUM_EDGE_SIZE + i] =
                                data[i];
                        }
                        return (uint)(edgePointer / _edgeSize);
                    }
                }
                edgePointer += _edgeSize;
            }
            return Constants.NO_EDGE;
        }

        /// <summary>
        /// Updates and edge's associated data.
        /// </summary>
        public uint UpdateEdgeIfBetter(uint vertex1, uint vertex2, Func<uint[], bool> update, params uint[] data)
        {
            if (_readonly) { throw new Exception("Graph is readonly."); }
            if (vertex1 == vertex2) { throw new ArgumentException("Given vertices must be different."); }
            _vertices.EnsureMinimumSize(Math.Max(vertex1, vertex2) * VERTEX_SIZE + EDGE_COUNT + 1);

            var vertexPointer = vertex1 * VERTEX_SIZE;
            var edgeCount = _vertices[vertexPointer + EDGE_COUNT];
            if (edgeCount == 0)
            {
                return Constants.NO_EDGE;
            }
            var edgePointer = _vertices[vertexPointer + FIRST_EDGE] * _edgeSize;
            var lastEdgePointer = edgePointer + (edgeCount * _edgeSize);

            var currentData = new uint[_edgeDataSize];
            while (edgePointer < lastEdgePointer)
            {
                for (uint i = 0; i < _edgeDataSize; i++)
                {
                    currentData[i] = _edges[edgePointer + MINIMUM_EDGE_SIZE + i];
                }
                if (_edges[edgePointer] == vertex2)
                {
                    if (update(currentData))
                    { // yes, update here.
                        for (uint i = 0; i < _edgeDataSize; i++)
                        {
                            _edges[edgePointer + MINIMUM_EDGE_SIZE + i] =
                                data[i];
                        }
                    }
                    return (uint)(edgePointer / _edgeSize);
                }
                edgePointer += _edgeSize;
            }
            return Constants.NO_EDGE;
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
            var edgePointer = _vertices[vertexPointer + FIRST_EDGE] * _edgeSize;

            for (var removeEdgePointer = edgePointer; removeEdgePointer < edgePointer + (edgeCount * (uint)_edgeSize);
                removeEdgePointer += _edgeSize)
            {
                if (_edges[removeEdgePointer] == vertex2)
                { // edge found, remove it.
                    removed++;
                    _edgeCount--;

                    // reduce edge count.
                    edgeCount--;
                    if (removeEdgePointer == edgePointer + (edgeCount * _edgeSize))
                    { // no need to move data anymore, this is the last edge being removed.
                        break;
                    }

                    // move the last edge and overwrite the current edge.
                    _edges[removeEdgePointer] = _edges[edgePointer + (edgeCount * _edgeSize)];
                    for (var i = 0; i < _edgeDataSize; i++)
                    {
                        _edges[removeEdgePointer + MINIMUM_EDGE_SIZE + i] =
                            _edges[edgePointer + (edgeCount * _edgeSize) + MINIMUM_EDGE_SIZE + i];
                    }

                    // report on the move.
                    if (_switchEdge != null)
                    {
                        _switchEdge((uint)((edgePointer + (edgeCount * _edgeSize)) / _edgeSize),
                            (uint)(removeEdgePointer / _edgeSize));
                    }

                    removeEdgePointer -= _edgeSize;
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
        /// Returns the space used in edges.
        /// </summary>
        public uint EdgeSpaceCount
        {
            get { return (uint)(_edges.Length / (this.EdgeDataSize + 1)); }
        }

        /// <summary>
        /// Trims the internal data structures of this graph.
        /// </summary>
        public void Trim()
        {
            this.Trim(out _);
        }

        /// <summary>
        /// Trims the internal data structures of this graph.
        /// </summary>
        public void Trim(out long maxEdgeId)
        {
            // remove all vertices without edges at the end.
            var maxVertexId = uint.MinValue;
            for (uint i = 0; i < _vertices.Length / VERTEX_SIZE; i++)
            {
                var pointer = _vertices[i * VERTEX_SIZE + FIRST_EDGE] * _edgeSize;
                var count = _vertices[i * VERTEX_SIZE + EDGE_COUNT];
                for (var e = pointer; e < pointer + (count * _edgeSize); e += _edgeSize)
                {
                    var vertex = _edges[e];
                    if (maxVertexId < vertex)
                    {
                        maxVertexId = vertex;
                    }
                }
                if (count > 0 && maxVertexId < i)
                { // also take into account the largest vertex pointing down.
                    maxVertexId = i;
                }
            }
            _vertices.Resize((maxVertexId + 1) * VERTEX_SIZE);

            // resize edges.
            var edgesLength = _nextEdgePointer;
            if (edgesLength == 0)
            { // keep minimum room for one edge.
                edgesLength = _edgeSize;
            }
            _edges.Resize(edgesLength);

            // store the max edge id.
            maxEdgeId = _edges.Length / _edgeSize;
        }

        /// <summary>
        /// Compresses the data in this graph to it's smallest size.
        /// </summary>
        public void Compress(bool toReadonly)
        {
            this.Compress(toReadonly, out _);
        }

        /// <summary>
        /// Compresses the data in this graph to it's smallest size.
        /// </summary>
        public void Compress(bool toReadonly, out long maxEdgeId)
        {
            // trim first.
            this.Trim();

            // build a list of all vertices sorted by their first position.
            var sortedVertices = Context.ArrayFactory.CreateMemoryBackedArray<uint>(_vertices.Length / VERTEX_SIZE);
            for (uint i = 0; i < sortedVertices.Length; i++)
            {
                sortedVertices[i] = i;
            }

            // sort vertices and coordinates.
            QuickSort.Sort((i) => _vertices[sortedVertices[i] * VERTEX_SIZE] * sortedVertices.Length +
                    sortedVertices[i],
                (i, j) =>
                    {
                        var tempRef = sortedVertices[i];
                        sortedVertices[i] = sortedVertices[j];
                        sortedVertices[j] = tempRef;
                    }, 0, this.VertexCount - 1);

            // move data down.
            long pointer = 0;
            for (uint i = 0; i < sortedVertices.Length; i++)
            {
                // move data.
                var vertexPointer = sortedVertices[i] * VERTEX_SIZE;
                var count = _vertices[vertexPointer + EDGE_COUNT];
                var edgePointer = _vertices[vertexPointer + FIRST_EDGE] * _edgeSize;
                _vertices[vertexPointer + FIRST_EDGE] = (uint)(pointer / _edgeSize);
                for (uint e = 0; e < count * _edgeSize; e += (uint)_edgeSize)
                {
                    if (pointer != edgePointer)
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
                            _switchEdge((uint)((edgePointer + e) / _edgeSize),
                                (uint)((pointer + e) / _edgeSize));
                        }
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

                pointer += count * _edgeSize;
            }
            _nextEdgePointer = pointer;
            _readonly = toReadonly;

            // store the max edge id.
            _edges.Resize(_nextEdgePointer);
            maxEdgeId = (_edges.Length / _edgeSize);
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
        public sealed class EdgeEnumerator : IEnumerable<Edge>, IEnumerator<Edge>
        {
            private readonly DirectedGraph _graph;
            private long _currentEdgePointer;
            private int _currentCount;
            private long _startEdgeId;
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
                _currentEdgePointer = long.MaxValue;
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
                    _currentEdgePointer += _graph._edgeSize;
                    _currentCount++;
                }
                if (_currentCount < _count)
                {
                    while (_neighbour != 0 &&
                        _neighbour != this.Neighbour)
                    {
                        _currentEdgePointer += _graph._edgeSize;
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
                    return (uint)(_currentEdgePointer / _graph._edgeSize);
                }
            }

            /// <summary>
            /// Resets this enumerator.
            /// </summary>
            public void Reset()
            {
                _currentEdgePointer = long.MaxValue;
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
                var edgeCountPointer = vertexId + EDGE_COUNT;
                if (edgeCountPointer >= _graph._vertices.Length)
                { // vertex doesn't exist.
                    return false;
                }
                _startEdgeId = _graph._vertices[vertexId + FIRST_EDGE] * _graph._edgeSize;
                _count = _graph._vertices[vertexId + EDGE_COUNT];
                _neighbour = 0;

                // reset.
                _currentEdgePointer = long.MaxValue;
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
            return this.Serialize(stream, true);
        }

        /// <summary>
        /// Serializes this graph to disk.
        /// </summary>
        public long Serialize(System.IO.Stream stream, bool compress)
        {
            if (compress)
            {
                this.Compress();
            }

            long vertexCount = this.VertexCount;
            long edgeCount = (_nextEdgePointer / _edgeSize);

            // write vertex and edge count.
            long size = 1;
            stream.WriteByte(1);
            stream.Write(BitConverter.GetBytes(vertexCount), 0, 8); // write exact number of vertices.
            size = size + 8;
            stream.Write(BitConverter.GetBytes(edgeCount), 0, 8); // write exact number of edges.
            size = size + 8;
            stream.Write(BitConverter.GetBytes(2), 0, 4); // write the vertex size.
            size = size + 4;
            stream.Write(BitConverter.GetBytes(_edgeSize), 0, 4); // write the edge size.
            size = size + 4;

            // write actual data.
            size += _vertices.CopyTo(stream);
            size += _edges.CopyTo(stream);

            return size;
        }

        /// <summary>
        /// Deserializes a graph from the given stream.
        /// </summary>
        /// <returns></returns>
        public static DirectedGraph Deserialize(System.IO.Stream stream, DirectedGraphProfile profile)
        {
            var initialPosition = stream.Position;

            // read sizes.
            long size = 1;
            var version = stream.ReadByte();
            if (version != 1)
            {
                throw new Exception(string.Format("Cannot deserialize directed graph: Invalid version #: {0}.", version));
            }
            var bytes = new byte[8];
            stream.Read(bytes, 0, 8);
            size = size + 8;
            var vertexLength = BitConverter.ToInt64(bytes, 0);
            stream.Read(bytes, 0, 8);
            size = size + 8;
            var edgeLength = BitConverter.ToInt64(bytes, 0);
            stream.Read(bytes, 0, 4);
            size = size + 4;
            var vertexSize = BitConverter.ToInt32(bytes, 0);
            stream.Read(bytes, 0, 4);
            size = size + 4;
            var edgeSize = BitConverter.ToInt32(bytes, 0);

            ArrayBase<uint> vertices;
            ArrayBase<uint> edges;
            if (profile == null)
            { // just create arrays and read the data.
                vertices = Context.ArrayFactory.CreateMemoryBackedArray<uint>(vertexLength * vertexSize);
                vertices.CopyFrom(stream);
                size += vertexLength * vertexSize * 4;
                edges = Context.ArrayFactory.CreateMemoryBackedArray<uint>(edgeLength * edgeSize);
                edges.CopyFrom(stream);
                size += edgeLength * edgeSize * 4;
            }
            else
            { // create accessors over the exact part of the stream that represents vertices/edges.
                var position = stream.Position;
                var map1 = new MemoryMapStream(new CappedStream(stream, position, vertexLength * vertexSize * 4));
                vertices = new Array<uint>(map1.CreateUInt32(vertexLength * vertexSize), profile.VertexProfile);
                size += vertexLength * vertexSize * 4;
                var map2 = new MemoryMapStream(new CappedStream(stream, position + vertexLength * vertexSize * 4,
                    edgeLength * edgeSize * 4));
                edges = new Array<uint>(map2.CreateUInt32(edgeLength * edgeSize), profile.EdgeProfile);
                size += edgeLength * edgeSize * 4;
            }

            // make sure stream is positioned at the correct location.
            stream.Seek(initialPosition + size, System.IO.SeekOrigin.Begin);

            return new DirectedGraph(edgeSize - MINIMUM_EDGE_SIZE, edgeLength, vertices, edges);
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