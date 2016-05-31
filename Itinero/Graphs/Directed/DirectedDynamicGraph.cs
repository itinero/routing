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

using Itinero.Algorithms.Sorting;
using Reminiscence.Arrays;
using Reminiscence.IO;
using Reminiscence.IO.Streams;
using System;
using System.Collections.Generic;
using System.Collections;

namespace Itinero.Graphs.Directed
{
    /// <summary>
    /// An directed graph.
    /// </summary>
    public class DirectedDynamicGraph : IDisposable
    {
        private const uint NO_EDGE = uint.MaxValue; // a dummy value indication that there is no edge.
        private const uint MAX_DYNAMIC_PAYLOAD = uint.MaxValue / 4; // the maximum payload for a dynamic data field and the last of the fixed fields. two bits are needed for bookkeeping.

        private readonly int _fixedEdgeDataSize = -1; // the fixed part of the edge-data.
        private readonly ArrayBase<uint> _vertices; // Holds pointers to where the edge-data starts per verex.
        private readonly ArrayBase<uint> _edges; // Holds all edge-data.

        private uint _nextEdgePointer; // the next pointer.
        private bool _readonly = false; 
        private long _edgeCount; // the number of edges.

        /// <summary>
        /// Creates a new dynamic directed graph.
        /// </summary>
        public DirectedDynamicGraph(int fixedEdgeDataSize = 1)
            : this(1000, fixedEdgeDataSize)
        {

        }

        /// <summary>
        /// Creates a new dynamic directed graph.
        /// </summary>
        public DirectedDynamicGraph(int sizeEstimate, int fixedEdgeDataSize = 1)
        {
            if (fixedEdgeDataSize < 1) { throw new ArgumentOutOfRangeException("fixedEdgeDataSize", "Fixed edge data size needs too greater than or equal to 1."); }
            if (sizeEstimate <= 0) { throw new ArgumentOutOfRangeException("sizeEstimate"); }

            _vertices = new MemoryArray<uint>(sizeEstimate);
            for (var i = 0; i < _vertices.Length; i++)
            {
                _vertices[i] = NO_EDGE;
            }
            _edges = new MemoryArray<uint>(sizeEstimate * 4);

            _fixedEdgeDataSize = fixedEdgeDataSize;
            _edgeCount = 0;
            _nextEdgePointer = 0;
        }

        /// <summary>
        /// Creates a new dynamic directed graph.
        /// </summary>
        public DirectedDynamicGraph(MemoryMap file, int sizeEstimate, int fixedEdgeDataSize = 1)
        {
            if (fixedEdgeDataSize < 1) { throw new ArgumentOutOfRangeException("fixedEdgeDataSize", "Fixed edge data size needs too greater than or equal to 1."); }
            if (sizeEstimate <= 0) { throw new ArgumentOutOfRangeException("sizeEstimate"); }

            _vertices = new Array<uint>(file, sizeEstimate);
            for(var i = 0; i < _vertices.Length; i++)
            {
                _vertices[i] = NO_EDGE;
            }
            _edges = new Array<uint>(file, sizeEstimate * 4);

            _fixedEdgeDataSize = fixedEdgeDataSize;
            _edgeCount = 0;
            _nextEdgePointer = 0;
        }

        /// <summary>
        /// Creates a new dynamic directed graph.
        /// </summary>
        private DirectedDynamicGraph(ArrayBase<uint> vertices, ArrayBase<uint> edges, long edgeCount, int fixedEdgeDataSize)
        {
            _vertices = vertices;
            _edges = edges;

            _fixedEdgeDataSize = fixedEdgeDataSize;
            _edgeCount = edgeCount;
            _nextEdgePointer = 0;
            _readonly = true;
        }

        /// <summary>
        /// Increases the size of the vector-array.
        /// </summary>
        private void IncreaseVertexSize()
        {
            this.IncreaseVertexSize(_vertices.Length + 10000);
        }

        /// <summary>
        /// Increases the size of the vector-array.
        /// </summary>
        private void IncreaseVertexSize(long size)
        {
            if (_readonly) { throw new Exception("Graph is readonly."); }

            var oldLength = _vertices.Length;
            _vertices.Resize(size);
            for(var i = oldLength; i < _vertices.Length; i++)
            {
                _vertices[i] = NO_EDGE;
            }
        }

        /// <summary>
        /// Increases the size of the vector-data array.
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
        public uint AddEdge(uint vertex1, uint vertex2, uint data)
        {
            if (_readonly) { throw new Exception("Graph is readonly."); }
            if (vertex1 == vertex2) { throw new ArgumentException("Given vertices must be different."); }
            if (data > MAX_DYNAMIC_PAYLOAD) { throw new ArgumentOutOfRangeException("data", "Data payload too big."); }
            while (vertex1 > _vertices.Length - 1) { this.IncreaseVertexSize(); }
            while (vertex2 > _vertices.Length - 1) { this.IncreaseVertexSize(); }

            var vertexPointer = vertex1;
            var edgePointer = _vertices[vertexPointer];
            var edgeId = uint.MaxValue;

            if (edgePointer == NO_EDGE)
            { // no edge yet, just add the end.
                _vertices[vertexPointer] = _nextEdgePointer;
                edgeId = _nextEdgePointer;

                if (_nextEdgePointer + 1 >= _edges.Length)
                { // make sure we can add another edge.
                    this.IncreaseEdgeSize();
                }

                _edges[_nextEdgePointer] = vertex2;
                _edges[_nextEdgePointer + 1] = DirectedDynamicGraph.AddLastEdgeAndLastFieldFlag(data);
                _nextEdgePointer += 2;
            }
            else
            { // there are already edges present for this vertex.
                var startPointer = edgePointer;
                edgePointer += (uint)(_fixedEdgeDataSize - 1);
                while(true)
                {
                    var e = _edges[edgePointer];
                    if (e != NO_EDGE && IsLastFieldInLastEdge(e))
                    {
                        break;
                    }
                    edgePointer++;
                }

                // check size and allocate new space or move if needed.
                var size = edgePointer - startPointer + 1;
                var totalSpace = NextPowerOfTwoOrPowerOfTwo(size);
                var requiredSpace = size + 2;
                if (requiredSpace > totalSpace)
                { // allocate enough space.
                    var newTotalSpace = NextPowerOfTwoOrPowerOfTwo(requiredSpace);
                    if (startPointer + totalSpace == _nextEdgePointer)
                    { // at the end, just make sure the edges array is big enough.
                        while (newTotalSpace + startPointer >= _edges.Length)
                        {
                            this.IncreaseEdgeSize();
                        }
                        _nextEdgePointer += (newTotalSpace - totalSpace);
                    }
                    else
                    { // move everything to the end, there isn't enough free space here.
                        // make sure the edges array is big enough.
                        while (newTotalSpace + _nextEdgePointer >= _edges.Length)
                        {
                            this.IncreaseEdgeSize();
                        }

                        // move existing data to the end and update pointer.
                        _vertices[vertexPointer] = _nextEdgePointer;
                        for(uint i = 0; i < size; i++)
                        {
                            _edges[_nextEdgePointer + i] = _edges[startPointer + i];
                        }
                        edgePointer = _nextEdgePointer + size - 1;
                        _nextEdgePointer += newTotalSpace;
                    }
                }

                // mark the current field as the non-last field.
                _edges[edgePointer] = RemoveLastEdgeFlag(_edges[edgePointer]);

                // add new data.
                _edges[edgePointer + 1] = vertex2;
                _edges[edgePointer + 2] = DirectedDynamicGraph.AddLastEdgeAndLastFieldFlag(data);
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
            if (data == null || data.Length == 0) { throw new ArgumentException("Data payload must contain at least one entry."); }
            for (var i = 0; i < data.Length; i++)
            {
                if (data[i] > MAX_DYNAMIC_PAYLOAD) { throw new ArgumentOutOfRangeException("data", "One of the entries in the data payload is too big."); }
            }
            while (vertex1 > _vertices.Length - 1) { this.IncreaseVertexSize(); }
            while (vertex2 > _vertices.Length - 1) { this.IncreaseVertexSize(); }

            var vertexPointer = vertex1;
            var edgePointer = _vertices[vertexPointer];
            var edgeId = uint.MaxValue;

            if (edgePointer == NO_EDGE)
            { // no edge yet, just add the end.
                _vertices[vertexPointer] = _nextEdgePointer;
                edgeId = _nextEdgePointer;

                if (_nextEdgePointer + data.Length >= _edges.Length)
                { // make sure we can add another edge.
                    this.IncreaseEdgeSize();
                }

                _edges[_nextEdgePointer] = vertex2;
                for (var i = 0; i < data.Length - 1; i++)
                {
                    _edges[_nextEdgePointer + i + 1] = data[i];
                }
                _edges[_nextEdgePointer + data.Length - 1 + 1] = DirectedDynamicGraph.AddLastEdgeAndLastFieldFlag(data[data.Length - 1]);
                _nextEdgePointer += DirectedDynamicGraph.NextPowerOfTwoOrPowerOfTwo((uint)(1 + data.Length));
            }
            else
            { // there are already edges present for this vertex.
                var startPointer = edgePointer;
                edgePointer += (uint)(_fixedEdgeDataSize - 1);
                while (true)
                {
                    var e = _edges[edgePointer];
                    if (e != NO_EDGE && IsLastFieldInLastEdge(e))
                    {
                        break;
                    }
                    edgePointer++;
                }

                // check size and allocate new space or move if needed.
                var size = edgePointer - startPointer + 1;
                var totalSpace = NextPowerOfTwoOrPowerOfTwo(size);
                var requiredSpace = size + 1 + (uint)data.Length;
                if (requiredSpace > totalSpace)
                { // allocate enough space.
                    var newTotalSpace = NextPowerOfTwoOrPowerOfTwo(requiredSpace);
                    if (startPointer + totalSpace == _nextEdgePointer)
                    { // at the end, just make sure the edges array is big enough.
                        while (newTotalSpace + startPointer >= _edges.Length)
                        {
                            this.IncreaseEdgeSize();
                        }
                        _nextEdgePointer += (newTotalSpace - totalSpace);
                    }
                    else
                    { // move everything to the end, there isn't enough free space here.
                        // make sure the edges array is big enough.
                        while (newTotalSpace + _nextEdgePointer >= _edges.Length)
                        {
                            this.IncreaseEdgeSize();
                        }

                        // move existing data to the end and update pointer.
                        _vertices[vertexPointer] = _nextEdgePointer;
                        for (uint i = 0; i < size; i++)
                        {
                            _edges[_nextEdgePointer + i] = _edges[startPointer + i];
                        }
                        edgePointer = _nextEdgePointer + size - 1;
                        _nextEdgePointer += newTotalSpace;
                    }
                }

                // mark the current field as the non-last field.
                _edges[edgePointer] = RemoveLastEdgeFlag(_edges[edgePointer]);

                // add new data.
                _edges[edgePointer + 1] = vertex2;
                for (var i = 0; i < data.Length - 1; i++)
                {
                    _edges[edgePointer + i + 2] = data[i];
                }
                _edges[edgePointer + data.Length - 1 + 2] = DirectedDynamicGraph.AddLastEdgeAndLastFieldFlag(data[data.Length - 1]);
            }
            _edgeCount++;

            return edgeId;
        }

        /// <summary>
        /// Updates and edge's associated data.
        /// </summary>
        public uint UpdateEdge(uint vertex1, uint vertex2, Func<uint[], bool> update, params uint[] data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes all edges leading from/to the given vertex. 
        /// </summary>
        /// <remarks>Only deletes all edges vertex->* NOT *->vertex</remarks>
        public int RemoveEdges(uint vertex)
        {
            var removed = 0;
            var edges = this.GetEdgeEnumerator();
            if (edges.MoveTo(vertex))
            {
                while (edges.MoveNext())
                {
                    removed += this.RemoveEdge(vertex, edges.Neighbour);
                }
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
            var vertexPointer = vertex1;
            var firstPointer = _vertices[vertexPointer];
            var edgePointer = firstPointer;
            
            var previousPointer = NO_EDGE;
            var currentPointer = edgePointer;
            var success = true;
            while (success)
            {
                success = false;

                if (currentPointer == NO_EDGE)
                {
                    break;
                }
                var nextPointer = MoveNextEdge(currentPointer);
                while (_edges[currentPointer] != vertex2)
                {
                    previousPointer = currentPointer;
                    currentPointer = nextPointer;
                    if (currentPointer == NO_EDGE)
                    {
                        break;
                    }
                    nextPointer = MoveNextEdge(currentPointer);
                }
                if (currentPointer != NO_EDGE)
                {
                    if (_edges[currentPointer] == vertex2)
                    {
                        if(RemoveEdge(vertex1, previousPointer, currentPointer, nextPointer) == NO_EDGE)
                        {
                            removed++;
                            _edgeCount--;
                            return removed;
                        }
                        success = true;
                        _edgeCount--;
                        removed++;
                        
                        edgePointer = firstPointer;

                        previousPointer = NO_EDGE;
                        currentPointer = edgePointer;
                        continue;
                    }
                    previousPointer = currentPointer;
                    currentPointer = nextPointer;
                }
            }
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
        /// Returns the number of vertices in this graph.
        /// </summary>
        public uint VertexCount
        {
            get { return (uint)(_vertices.Length); }
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
            long maxEdgeId;
            this.Trim(out maxEdgeId);
        }

        /// <summary>
        /// Trims the internal data structures of this graph.
        /// </summary>
        public void Trim(out long maxEdgeId)
        {
            // remove all vertices without edges at the end.
            var maxVertexId = uint.MinValue;
            for (uint i = 0; i < _vertices.Length; i++)
            {
                var pointer = _vertices[i];
                if (pointer == NO_EDGE)
                {
                    continue;
                }
                while(_edges[pointer] == NO_EDGE)
                {
                    pointer++;
                }
                do
                {
                    var vertex = _edges[pointer];
                    if (maxVertexId < vertex)
                    { // also take into account the largest vertex pointing down.
                        maxVertexId = vertex;
                    }
                    pointer = MoveNextEdge(pointer);
                } while (pointer != NO_EDGE);
                if (maxVertexId < i)
                { // also take into account the largest vertex pointing down.
                    maxVertexId = i;
                }
            }
            _vertices.Resize(maxVertexId + 1);

            // resize edges.
            var edgesLength = _nextEdgePointer;
            if (edgesLength == 0)
            { // keep minimum room for one edge.
                edgesLength = (uint)_fixedEdgeDataSize;
            }
            _edges.Resize(edgesLength);

            // store the max edge id.
            maxEdgeId = _edges.Length;
        }

        /// <summary>
        /// Compresses the data in this graph to it's smallest size.
        /// </summary>
        public void Compress(bool toReadonly)
        {
            long maxEdgeId;
            this.Compress(toReadonly, out maxEdgeId);
        }

        /// <summary>
        /// Compresses the data in this graph to it's smallest size.
        /// </summary>
        public void Compress(bool toReadonly, out long maxEdgeId)
        {
            // trim first.
            this.Trim();

            // build a list of all vertices sorted by their first position.
            var sortedVertices = new MemoryArray<uint>(_vertices.Length);
            for (uint i = 0; i < sortedVertices.Length; i++)
            {
                sortedVertices[i] = i;
            }

            // sort vertices and coordinates.
            QuickSort.Sort((i) => _vertices[sortedVertices[i]] * sortedVertices.Length +
                    sortedVertices[i],
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
                var vertex = sortedVertices[i];
                var vertexPointer = _vertices[vertex];
                if (vertexPointer == NO_EDGE)
                {
                    continue;
                }
                _vertices[vertex] = pointer;

                // skip removed data.
                var pointerBefore = pointer;
                while (_edges[vertexPointer] == NO_EDGE)
                {
                    vertexPointer++;
                }
                do
                {
                    // copy fixed-data.
                    for (var e = 0; e < _fixedEdgeDataSize; e++)
                    {
                        _edges[pointer + e] = _edges[vertexPointer + e];
                    }
                    pointer += (uint)_fixedEdgeDataSize;
                    vertexPointer += (uint)_fixedEdgeDataSize;
                    while (!DirectedDynamicGraph.IsLastField(_edges[pointer - 1]))
                    {
                        _edges[pointer] = _edges[vertexPointer];
                        pointer++;
                        vertexPointer++;
                    }
                } while (!DirectedDynamicGraph.IsLastFieldInLastEdge(_edges[pointer - 1]));

                var count = pointer - pointerBefore;
                if (!toReadonly && count > 2 && !((count & (count - 1)) == 0))
                { // next power of 2, don't do this on readonly to save space.
                    count |= count >> 1;
                    count |= count >> 2;
                    count |= count >> 4;
                    count |= count >> 8;
                    count |= count >> 16;
                    count++;

                    pointer += (count - (pointer - pointerBefore));
                }
            }
            _nextEdgePointer = pointer;
            _readonly = toReadonly;

            // store the max edge id.
            _edges.Resize(_nextEdgePointer);
            maxEdgeId = _edges.Length;
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
        public class EdgeEnumerator : IEnumerable<DynamicEdge>, IEnumerator<DynamicEdge>
        {
            private readonly DirectedDynamicGraph _graph;
            private uint _currentEdgePointer;
            private uint _startPointer;

            /// <summary>
            /// Creates a new edge enumerator.
            /// </summary>
            public EdgeEnumerator(DirectedDynamicGraph graph)
            {
                _graph = graph;
                _currentEdgePointer = NO_EDGE;
                _startPointer = NO_EDGE;
            }

            /// <summary>
            /// Move to the next edge.
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                if (_startPointer == NO_EDGE) { return false; }

                if (_currentEdgePointer == NO_EDGE)
                {
                    _currentEdgePointer = _startPointer;
                    while(_graph._edges[_currentEdgePointer] == NO_EDGE)
                    {
                        _currentEdgePointer++;
                    }
                    return true;
                }

                _currentEdgePointer = _graph.MoveNextEdge(_currentEdgePointer);
                return _currentEdgePointer != NO_EDGE;
            }

            /// <summary>
            /// Returns the current neighbour.
            /// </summary>
            public uint Neighbour
            {
                get { return _graph._edges[_currentEdgePointer]; }
            }

            /// <summary>
            /// Returns the dynamic part of the edge data.
            /// </summary>
            public uint[] DynamicData
            {
                get
                {
                    var p = _currentEdgePointer + _graph._fixedEdgeDataSize;
                    if (DirectedDynamicGraph.IsLastField(_graph._edges[p]))
                    { // no dynamic data!
                        return new uint[0];
                    }

                    var dataList = new List<uint>(4);
                    p++;
                    do
                    {
                        var data = _graph._edges[p];
                        if (DirectedDynamicGraph.IsLastField(data))
                        {
                            data = DirectedDynamicGraph.RemoveFlags(data);
                            dataList.Add(data);
                            break;
                        }
                        dataList.Add(data);
                        p++;
                    } while (true);
                    return dataList.ToArray();
                }
            }

            /// <summary>
            /// Fills the given array with the dynamic part of the edge-data.
            /// </summary>
            public int FillWithDynamicData(ref uint[] data)
            {
                var p = _currentEdgePointer + _graph._fixedEdgeDataSize;
                if (DirectedDynamicGraph.IsLastField(_graph._edges[p]))
                { // no dynamic data!
                    return 0;
                }

                var i = 0;
                p++;
                do
                {
                    data[i] = _graph._edges[p];
                    if (DirectedDynamicGraph.IsLastField(data[i]))
                    {
                        data[i] = DirectedDynamicGraph.RemoveFlags(data[i]);
                        break;
                    }
                    i++;
                    p++;
                } while (true);
                return i + 1;
            }

            /// <summary>
            /// Returns the first entry in the edge data if fixed data count > 0.
            /// </summary>
            public uint Data0
            {
                get
                {
                    if (_graph._fixedEdgeDataSize == 0)
                    {
                        throw new InvalidOperationException("There is not fixed data at position 0.");
                    }
                    return DirectedDynamicGraph.RemoveFlags(_graph._edges[_currentEdgePointer + 0 + 1]);
                }
            }

            /// <summary>
            /// Returns the first entry in the edge data if fixed data count > 1.
            /// </summary>
            public uint Data1
            {
                get
                {
                    if (_graph._fixedEdgeDataSize <= 1)
                    {
                        throw new InvalidOperationException("There is not fixed data at position 1.");
                    }
                    return DirectedDynamicGraph.RemoveFlags(_graph._edges[_currentEdgePointer + 1 + 1]);
                }
            }

            /// <summary>
            /// Returns the dynamic part of the edge data.
            /// </summary>
            public uint[] Data
            {
                get
                {
                    var data = new uint[_graph._fixedEdgeDataSize];
                    for (var i = 0; i < _graph._fixedEdgeDataSize; i++)
                    {
                        data[i] = DirectedDynamicGraph.RemoveFlags(_graph._edges[_currentEdgePointer + 1 + i]);
                    }
                    return data;
                }
            }

            /// <summary>
            /// Fills the given array with the fixed data.
            /// </summary>
            public int FillWithData(ref uint[] data)
            {
                for (var i = 0; i < _graph._fixedEdgeDataSize && i < data.Length; i++)
                {
                    data[i] = DirectedDynamicGraph.RemoveFlags(_graph._edges[_currentEdgePointer + 1 + i]);
                }
                return System.Math.Min(_graph._fixedEdgeDataSize, data.Length);
            }

            /// <summary>
            /// Gets the current edge.
            /// </summary>
            public DynamicEdge Current
            {
                get { return new DynamicEdge(this); }
            }

            /// <summary>
            /// Gets the current edge.
            /// </summary>
            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }

            /// <summary>
            /// Returns the edge id.
            /// </summary>
            public uint Id
            {
                get
                {
                    return _currentEdgePointer;
                }
            }

            /// <summary>
            /// Disposes.
            /// </summary>
            public void Dispose()
            {

            }

            /// <summary>
            /// Moves this enumerator to the given vertex.
            /// </summary>
            public bool MoveTo(uint vertex)
            {
                if (vertex > _graph.VertexCount)
                {
                    return false;
                }
                _currentEdgePointer = NO_EDGE;
                var vertexId = vertex;
                var edgePointer = _graph._vertices[vertexId];
                _startPointer = edgePointer;
                return true;
            }

            /// <summary>
            /// Moves this enumerator to the given edge.
            /// </summary>
            public bool MoveToEdge(uint edge)
            {
                _currentEdgePointer = NO_EDGE;
                _startPointer = edge;
                _currentEdgePointer = _startPointer;
                return true;
            }

            /// <summary>
            /// Gets the enumerator.
            /// </summary>
            /// <returns></returns>
            public IEnumerator<DynamicEdge> GetEnumerator()
            {
                this.Reset();
                return this;
            }

            /// <summary>
            /// Returns the enumerator.
            /// </summary>
            /// <returns></returns>
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            /// <summary>
            /// Resets this enumerator.
            /// </summary>
            public void Reset()
            {
                _currentEdgePointer = NO_EDGE;
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
            long edgeArraySize = _nextEdgePointer;
            long edgeCount = this.EdgeCount;

            // write vertex and edge count.
            long size = 1;
            stream.WriteByte(1); // write the version byte.
            stream.Write(BitConverter.GetBytes(vertexCount), 0, 8); // write exact number of vertices.
            size = size + 8;
            stream.Write(BitConverter.GetBytes(edgeCount), 0, 8); // write exact number of edges.
            size = size + 8;
            stream.Write(BitConverter.GetBytes(edgeArraySize), 0, 8); // write exact number of edges.
            size = size + 8;
            stream.Write(BitConverter.GetBytes(_fixedEdgeDataSize), 0, 4); // write the edge fixed size.
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
        public static DirectedDynamicGraph Deserialize(System.IO.Stream stream, DirectedGraphProfile profile)
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
            stream.Read(bytes, 0, 8);
            size = size + 8;
            var edgeArraySize = BitConverter.ToInt64(bytes, 0);
            stream.Read(bytes, 0, 4);
            size = size + 4;
            var fixedEdgeDataSize = BitConverter.ToInt32(bytes, 0);

            ArrayBase<uint> vertices;
            ArrayBase<uint> edges;
            if (profile == null)
            { // just create arrays and read the data.
                vertices = new MemoryArray<uint>(vertexLength);
                vertices.CopyFrom(stream);
                size += vertexLength * 4;
                edges = new MemoryArray<uint>(edgeArraySize);
                edges.CopyFrom(stream);
                size += edgeArraySize * 4;
            }
            else
            { // create accessors over the exact part of the stream that represents vertices/edges.
                var position = stream.Position;
                var map1 = new MemoryMapStream(new CappedStream(stream, position, vertexLength * 4));
                vertices = new Array<uint>(map1.CreateUInt32(vertexLength), profile.VertexProfile);
                size += vertexLength * 4;
                var map2 = new MemoryMapStream(new CappedStream(stream, position + vertexLength * 4,
                    edgeArraySize * 4));
                edges = new Array<uint>(map2.CreateUInt32(edgeArraySize), profile.EdgeProfile);
                size += edgeArraySize * 4;
            }

            // make sure stream is positioned at the correct location.
            stream.Seek(initialPosition + size, System.IO.SeekOrigin.Begin);

            return new DirectedDynamicGraph(vertices, edges, edgeLength, fixedEdgeDataSize);
        }

        #endregion

        #region Bookkeeping
        
        /// <summary>
        /// Mark this data payload at the last field in the current edge.
        /// </summary>
        private static uint AddLastFieldFlag(uint data)
        {
            return data + 1 << 30;
        }

        /// <summary>
        /// Mark this data payload as the last field in the last edge.
        /// </summary>
        private static uint AddLastEdgeAndLastFieldFlag(uint data)
        {
            return data + ((uint)1 << 30) + ((uint)1 << 31);
        }

        /// <summary>
        /// Removes the mark that markes this data payload as the last field in the last edge.
        /// </summary>
        private static uint RemoveLastEdgeFlag(uint data)
        {
            if (!IsLastFieldInLastEdge(data)) { throw new ArgumentOutOfRangeException("data"); }

            return data - ((uint)1 << 31);
        }

        /// <summary>
        /// Returns true if this data payload is the last field in the current edge.
        /// </summary>
        private static bool IsLastField(uint data)
        {
            if (data == NO_EDGE) { return false; }

            return (data & ((uint)1 << 30)) != 0;
        }

        /// <summary>
        /// Return true if this data payload is the last field in the last edge.
        /// </summary>
        private static bool IsLastFieldInLastEdge(uint data)
        {
            if (data == NO_EDGE) { return false; }

            return (data & ((uint)1 << 31)) != 0;
        }

        /// <summary>
        /// Removes all flags.
        /// </summary>
        public static uint RemoveFlags(uint data)
        {
            if (IsLastFieldInLastEdge(data))
            {
                data = RemoveLastEdgeFlag(data);
            }
            if (IsLastField(data))
            {
                data = data - ((uint)1 << 30);
            }
            return data;
        }

        /// <summary>
        /// Returns the next power of 2.
        /// </summary>
        private static uint NextPowerOfTwoOrPowerOfTwo(uint count)
        {
            if (count == 1 || ((count & (count - 1)) == 0))
            { // already a power of 2.
                return count;
            }
            count |= count >> 1;
            count |= count >> 2;
            count |= count >> 4;
            count |= count >> 8;
            count |= count >> 16;
            count++;
            return count;
        }

        /// <summary>
        /// Moves to the next edge and returns the pointer of that next edge. Returns NO_EDGE if no next edge was found.
        /// 
        /// The given pointer is expected to be the first position of an edge.
        /// </summary>
        private uint MoveNextEdge(uint edgePointer)
        {
            // move past first fixed-data fields to the last one.
            var data = _edges[edgePointer];
            if (data == NO_EDGE)
            { // just move to the first non-NO_EDGE field.
                while (data == NO_EDGE)
                {
                    edgePointer++;
                    data = _edges[edgePointer];
                }
                return edgePointer;
            }
            edgePointer += (uint)_fixedEdgeDataSize;
            data = _edges[edgePointer];
            while (!DirectedDynamicGraph.IsLastField(data))
            { // move until the last data field.
                edgePointer++;
                data = _edges[edgePointer];
            }
            if (DirectedDynamicGraph.IsLastFieldInLastEdge(data))
            { // if the last data field is the last edge then just stop.
                return NO_EDGE;
            }
            edgePointer++;
            data = _edges[edgePointer];
            while (data == NO_EDGE)
            {
                edgePointer++;
                data = _edges[edgePointer];
            }
            return edgePointer;
        }

        /// <summary>
        /// Removes an edge and moves to the next edge after the removed edge. Returns NO_EDGE if no next edge was found.
        /// 
        /// The given pointer is expected to be the first position of an edge starting a the given vertex. The previous edge pointer is NO_EDGE if there is not previous edge.
        /// </summary>
        public uint RemoveEdge(uint vertex, uint previousEdgePointer, uint edgePointer, uint nextPointer)
        {
            var originalEdgePointer = edgePointer;
            if (previousEdgePointer == NO_EDGE && nextPointer == NO_EDGE)
            { // only one edge left.
                _vertices[vertex] = NO_EDGE;
                return NO_EDGE;
            }

            var startPointer = edgePointer;
            // remove the fixed-data.
            for(var i = 0; i < _fixedEdgeDataSize - 1; i++)
            {
                _edges[edgePointer] = NO_EDGE;
                edgePointer++;
            }
            var data = _edges[edgePointer];
            while (!DirectedDynamicGraph.IsLastField(data))
            { // move until the last data field.
                _edges[edgePointer] = NO_EDGE;
                edgePointer++;
                data = _edges[edgePointer];
            }
            if (DirectedDynamicGraph.IsLastFieldInLastEdge(data))
            { // if the last data field is the last edge then just stop.
                if(!this.MarkAsLast(previousEdgePointer))
                { // there is no data at the previous edge, this can only mean one thing: no more edges left.
                    _vertices[vertex] = NO_EDGE;
                }
                return NO_EDGE;
            }
            _edges[edgePointer] = NO_EDGE;
            return edgePointer + 1;
        }

        /// <summary>
        /// Marks the edge at the given location as the last edge.
        /// </summary>
        /// <returns>False if there is no data at the given location, true otherwise.</returns>
        private bool MarkAsLast(uint edgePointer)
        {
            var data = _edges[edgePointer];
            if (data == NO_EDGE)
            {
                return false;
            }
            while(!DirectedDynamicGraph.IsLastField(data))
            {
                edgePointer++;
                data = _edges[edgePointer];
            }
            data = DirectedDynamicGraph.RemoveFlags(data);
            _edges[edgePointer] = DirectedDynamicGraph.AddLastEdgeAndLastFieldFlag(
                 data);
            return true;
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