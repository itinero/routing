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
using OsmSharp.IO;
using OsmSharp.IO.MemoryMappedFiles;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Graph
{
    /// <summary>
    /// Represents a undirected graph.
    /// </summary>
    public class Graph<TEdgeData> : IDisposable
        where TEdgeData : struct, IEdgeData
    {
        private const int EDGE_SIZE = 4;
        private const uint NO_EDGE = uint.MaxValue;
        private const int NODEA = 0;
        private const int NODEB = 1;
        private const int NEXTNODEA = 2;
        private const int NEXTNODEB = 3;
        private const int BLOCKSIZE = 1000;

        private readonly HugeArrayBase<uint> _vertices; // Holds all vertices pointing to it's first edge.
        private readonly HugeArrayBase<uint> _edges;
        private readonly HugeArrayBase<TEdgeData> _edgeData;

        /// <summary>
        /// Creates a new in-memory graph.
        /// </summary>
        public Graph()
            : this(BLOCKSIZE)
        {

        }

        /// <summary>
        /// Creates a new in-memory graph.
        /// </summary>
        public Graph(long sizeEstimate)
            : this(sizeEstimate, 
            new HugeArray<uint>(sizeEstimate), 
            new HugeArray<uint>(sizeEstimate * 3 * EDGE_SIZE), 
            new HugeArray<TEdgeData>(sizeEstimate * 3))
        {

        }

        /// <summary>
        /// Creates a graph using the existing data in the given arrays.
        /// </summary>
        private Graph(HugeArrayBase<uint> vertexArray,
            HugeArrayBase<uint> edgesArray,
            HugeArrayBase<TEdgeData> edgeDataArray)
        {
            _vertices = vertexArray;
            _edges = edgesArray;
            _edgeData = edgeDataArray;
            _nextEdgeId = (uint)(edgesArray.Length + 1);
            _edgeCount = _nextEdgeId / 4;
        }

        /// <summary>
        /// Creates a new graph using the given arrays.
        /// </summary>
        private Graph(long sizeEstimate,
            HugeArrayBase<uint> vertexArray, 
            HugeArrayBase<uint> edgesArray, 
            HugeArrayBase<TEdgeData> edgeDataArray)
        {
            _nextEdgeId = 0;
            _vertices = vertexArray;
            _vertices.Resize(sizeEstimate);
            for (int idx = 0; idx < sizeEstimate; idx++)
            {
                _vertices[idx] = NO_EDGE;
            }
            _edges = edgesArray;
            _edges.Resize(sizeEstimate * 3 * EDGE_SIZE);
            for (int idx = 0; idx < sizeEstimate * 3 * EDGE_SIZE; idx++)
            {
                _edges[idx] = NO_EDGE;
            }
            _edgeData = edgeDataArray;
            _edgeData.Resize(sizeEstimate * 3);
        }

        /// <summary>
        /// Creates a new using the given file.
        /// </summary>
        public Graph(MemoryMappedFile file, long estimatedSize)
        {
            var mapper = default(TEdgeData) as IMappedEdgeData<TEdgeData>;
            if(mapper == null)
            { // oeps, this is impossible.
                throw new InvalidOperationException(
                    string.Format("Cannot create a memory-mapped graph with edge data type: {0}. " +
                        "There is no IMappedEdgeData implementation.", typeof(TEdgeData).ToInvariantString()));
            }

            _vertices = new MemoryMappedHugeArrayUInt32(file, estimatedSize);
            _edges = new MemoryMappedHugeArrayUInt32(file, estimatedSize);
            _edgeData = new MappedHugeArray<TEdgeData, uint>(
                new MemoryMappedHugeArrayUInt32(file, estimatedSize * mapper.MappedSize), mapper.MappedSize,
                    mapper.MapToDelegate, mapper.MapFromDelegate);
        }

        private uint _nextEdgeId;
        private long _edgeCount = 0;

        /// <summary>
        /// Increases the size of the vertex-array.
        /// </summary>
        private void IncreaseVertexSize()
        {
            this.IncreaseVertexSize(_vertices.Length + BLOCKSIZE);
        }

        /// <summary>
        /// Increases the size of the vertex-array.
        /// </summary>
        private void IncreaseVertexSize(long min)
        {
            var newSize = (long)(System.Math.Floor(min / BLOCKSIZE) + 1) * (long)BLOCKSIZE;
            if(newSize < _vertices.Length)
            { // no need to increase, already big enough.
                return;
            }
            var oldLength = _vertices.Length;
            _vertices.Resize(newSize);
            for (long idx = oldLength; idx < newSize; idx++)
            {
                _vertices[idx] = NO_EDGE;
            }
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
            var oldLength = _edges.Length;
            _edges.Resize(size);
            for (long idx = oldLength; idx < size; idx++)
            {
                _edges[idx] = NO_EDGE;
            }
            _edgeData.Resize(size / EDGE_SIZE);
        }

        /// <summary>
        /// Adds an edge with the associated data.
        /// </summary>
        public void AddEdge(uint vertex1, uint vertex2, TEdgeData data)
        {
            if (vertex1 == vertex2) { throw new ArgumentException("Given vertices must be different."); }
            if (vertex1 > _vertices.Length - 1) { this.IncreaseVertexSize(vertex1); }
            if (vertex2 > _vertices.Length - 1) { this.IncreaseVertexSize(vertex2); }

            var edgeId = _vertices[vertex1];
            if (_vertices[vertex1] != NO_EDGE)
            { // check for an existing edge first.
                // check if the arc exists already.
                edgeId = _vertices[vertex1];
                uint nextEdgeSlot = 0;
                while (edgeId != NO_EDGE)
                { // keep looping.
                    uint otherVertexId = 0;
                    uint previousEdgeId = edgeId;
                    bool forward = true;
                    if (_edges[edgeId + NODEA] == vertex1)
                    {
                        otherVertexId = _edges[edgeId + NODEB];
                        nextEdgeSlot = edgeId + NEXTNODEA;
                        edgeId = _edges[edgeId + NEXTNODEA];
                    }
                    else
                    {
                        otherVertexId = _edges[edgeId + NODEA];
                        nextEdgeSlot = edgeId + NEXTNODEB;
                        edgeId = _edges[edgeId + NEXTNODEB];
                        forward = false;
                    }
                    if (otherVertexId == vertex2)
                    { // this is the edge we need.
                        if (!forward)
                        {
                            data = (TEdgeData)data.Reverse();
                        }
                        _edgeData[previousEdgeId / 4] = data;
                        return;
                    }
                }

                // create a new edge.
                edgeId = _nextEdgeId;
                if (_nextEdgeId + NEXTNODEB >= _edges.Length)
                { // there is a need to increase edges array.
                    this.IncreaseEdgeSize();
                }
                _edges[_nextEdgeId + NODEA] = vertex1;
                _edges[_nextEdgeId + NODEB] = vertex2;
                _edges[_nextEdgeId + NEXTNODEA] = NO_EDGE;
                _edges[_nextEdgeId + NEXTNODEB] = NO_EDGE;
                _nextEdgeId = _nextEdgeId + EDGE_SIZE;

                // append the new edge to the from list.
                _edges[nextEdgeSlot] = edgeId;

                // set data.
                _edgeData[edgeId / 4] = data;
                _edgeCount++;
            }
            else
            { // create a new edge and set.
                edgeId = _nextEdgeId;
                _vertices[vertex1] = _nextEdgeId;

                if (_nextEdgeId + NEXTNODEB >= _edges.Length)
                { // there is a need to increase edges array.
                    this.IncreaseEdgeSize();
                }
                _edges[_nextEdgeId + NODEA] = vertex1;
                _edges[_nextEdgeId + NODEB] = vertex2;
                _edges[_nextEdgeId + NEXTNODEA] = NO_EDGE;
                _edges[_nextEdgeId + NEXTNODEB] = NO_EDGE;
                _nextEdgeId = _nextEdgeId + EDGE_SIZE;

                // set data.
                _edgeData[edgeId / 4] = data;
                _edgeCount++;
            }

            var toEdgeId = _vertices[vertex2];
            if (toEdgeId != NO_EDGE)
            { // there are existing edges.
                uint nextEdgeSlot = 0;
                while (toEdgeId != NO_EDGE)
                { // keep looping.
                    uint otherVertexId = 0;
                    if (_edges[toEdgeId + NODEA] == vertex2)
                    {
                        otherVertexId = _edges[toEdgeId + NODEB];
                        nextEdgeSlot = toEdgeId + NEXTNODEA;
                        toEdgeId = _edges[toEdgeId + NEXTNODEA];
                    }
                    else
                    {
                        otherVertexId = _edges[toEdgeId + NODEA];
                        nextEdgeSlot = toEdgeId + NEXTNODEB;
                        toEdgeId = _edges[toEdgeId + NEXTNODEB];
                    }
                }
                _edges[nextEdgeSlot] = edgeId;
            }
            else
            { // there are no existing edges point the vertex straight to it's first edge.
                _vertices[vertex2] = edgeId;
            }

            return;
        }

        /// <summary>
        /// Deletes all edges leading from/to the given vertex. 
        /// </summary>
        /// <param name="vertex"></param>
        public int RemoveEdges(uint vertex)
        {
            var removed = 0;
            var edges =  this.GetEdgeEnumerator(vertex);
            while (edges.MoveNext())
            {
                if(this.RemoveEdge(vertex, edges.Neighbour))
                {
                    removed++;
                }
            }
            return removed;
        }

        /// <summary>
        /// Deletes the edge between the two given vertices.
        /// </summary>
        public bool RemoveEdge(uint vertex1, uint vertex2)
        {
            if (vertex1 >= _vertices.Length) { throw new ArgumentOutOfRangeException("vertex1", "vertex1 is not part of this graph."); }
            if (vertex2 >= _vertices.Length) { throw new ArgumentOutOfRangeException("vertex2", "vertex2 is not part of this graph."); }

            if (_vertices[vertex1] == NO_EDGE ||
                _vertices[vertex2] == NO_EDGE)
            { // no edge to remove here!
                return false;
            }

            // remove for vertex1.
            var removed = false;
            var nextEdgeId = _vertices[vertex1];
            uint nextEdgeSlot = 0;
            uint previousEdgeSlot = 0;
            uint currentEdgeId = 0;
            while (nextEdgeId != NO_EDGE)
            { // keep looping.
                uint otherVertexId = 0;
                currentEdgeId = nextEdgeId;
                previousEdgeSlot = nextEdgeSlot;
                if (_edges[nextEdgeId + NODEA] == vertex1)
                {
                    otherVertexId = _edges[nextEdgeId + NODEB];
                    nextEdgeSlot = nextEdgeId + NEXTNODEA;
                    nextEdgeId = _edges[nextEdgeId + NEXTNODEA];
                }
                else
                {
                    otherVertexId = _edges[nextEdgeId + NODEA];
                    nextEdgeSlot = nextEdgeId + NEXTNODEB;
                    nextEdgeId = _edges[nextEdgeId + NEXTNODEB];
                }
                if (otherVertexId == vertex2)
                { // this is the edge we need.
                    if (_vertices[vertex1] == currentEdgeId)
                    { // the edge being remove if the 'first' edge.
                        // point to the next edge.
                        _vertices[vertex1] = nextEdgeId;
                    }
                    else
                    { // the edge being removed is not the 'first' edge.
                        // set the previous edge slot to the current edge id being the next one.
                        _edges[previousEdgeSlot] = nextEdgeId;
                    }
                    removed = true;
                    _edgeCount--;
                    break;
                }
            }

            // remove for vertex2.
            if (removed)
            {
                nextEdgeId = _vertices[vertex2];
                nextEdgeSlot = 0;
                previousEdgeSlot = 0;
                currentEdgeId = 0;
                while (nextEdgeId != NO_EDGE)
                { // keep looping.
                    uint otherVertexId = 0;
                    currentEdgeId = nextEdgeId;
                    previousEdgeSlot = nextEdgeSlot;
                    if (_edges[nextEdgeId + NODEA] == vertex2)
                    {
                        otherVertexId = _edges[nextEdgeId + NODEB];
                        nextEdgeSlot = nextEdgeId + NEXTNODEA;
                        nextEdgeId = _edges[nextEdgeId + NEXTNODEA];
                    }
                    else
                    {
                        otherVertexId = _edges[nextEdgeId + NODEA];
                        nextEdgeSlot = nextEdgeId + NEXTNODEB;
                        nextEdgeId = _edges[nextEdgeId + NEXTNODEB];
                    }
                    if (otherVertexId == vertex1)
                    { // this is the edge we need.
                        if (_vertices[vertex2] == currentEdgeId)
                        { // the edge being remove if the 'first' edge.
                            // point to the next edge.
                            _vertices[vertex2] = nextEdgeId;
                        }
                        else
                        { // the edge being removed is not the 'first' edge.
                            // set the previous edge slot to the current edge id being the next one.
                            _edges[previousEdgeSlot] = nextEdgeId;
                        }

                        // reset everything about this edge.
                        _edges[currentEdgeId + NODEA] = NO_EDGE;
                        _edges[currentEdgeId + NODEB] = NO_EDGE;
                        _edges[currentEdgeId + NEXTNODEA] = NO_EDGE;
                        _edges[currentEdgeId + NEXTNODEB] = NO_EDGE;
                        _edgeData[currentEdgeId / EDGE_SIZE] = default(TEdgeData);
                        return true;
                    }
                }
                throw new Exception("Edge could not be reached from vertex2. Data in graph is invalid.");
            }
            return removed;
        }

        /// <summary>
        /// Returns an empty edge enumerator.
        /// </summary>
        /// <returns></returns>
        public GraphEdgeEnumerator GetEdgeEnumerator()
        {
            return new GraphEdgeEnumerator(this);
        }

        /// <summary>
        /// Returns all edges starting at the given vertex.
        /// </summary>
        /// <returns></returns>
        public GraphEdgeEnumerator GetEdgeEnumerator(uint vertex)
        {
            if (vertex >= _vertices.Length) { throw new ArgumentOutOfRangeException("vertex", "vertex is not part of this graph."); }

            var enumerator = new GraphEdgeEnumerator(this);
            enumerator.MoveTo(vertex);
            return enumerator;
        }

        /// <summary>
        /// Relocates data internally in the most compact way possible.
        /// </summary>
        public void Compress()
        {
            // move edges down.
            uint maxAllocatedEdgeId = 0;
            for (uint edgeId = 0; edgeId < _nextEdgeId; edgeId = edgeId + EDGE_SIZE)
            {
                if (_edges[edgeId] != NO_EDGE)
                { // this edge is allocated.
                    if (edgeId != maxAllocatedEdgeId)
                    { // there is data here.
                        this.MoveEdge(edgeId, maxAllocatedEdgeId);
                    }
                    maxAllocatedEdgeId = maxAllocatedEdgeId + EDGE_SIZE;
                }
            }
            _nextEdgeId = maxAllocatedEdgeId;

            this.Trim();
        }

        /// <summary>
        /// Resizes the internal data structures to their smallest size possible.
        /// </summary>
        public void Trim()
        {
            var edgeSize = _nextEdgeId;
            if (edgeSize == 0)
            { // keep minimum room for one edge.
                edgeSize = EDGE_SIZE;
            }
            // resize edges.
            _edgeData.Resize(edgeSize / EDGE_SIZE);
            _edges.Resize(edgeSize);

            // remove all vertices without edges.
            var size = _vertices.Length;
            while (_vertices[size - 1] == NO_EDGE)
            {
                size--;
                if (size == 0)
                { // keep minimum of 1 vertex.
                    size = 1;
                    break;
                }
            }
            _vertices.Resize(size);
        }

        /// <summary>
        /// Returns the number of vertices in this graph.
        /// </summary>
        public long VertexCount
        {
            get { return _vertices.Length; }
        }

        /// <summary>
        /// Returns the number of edges in this graph.
        /// </summary>
        public long EdgeCount
        {
            get { return _edgeCount; }
        }

        /// <summary>
        /// Represents a graph edge enumerator.
        /// </summary>
        public class GraphEdgeEnumerator : IEnumerable<Edge<TEdgeData>>, IEnumerator<Edge<TEdgeData>>
        {
            private readonly Graph<TEdgeData> _graph;
            private uint _nextEdgeId;
            private uint _vertex;
            private uint _currentEdgeId;
            private bool _currentEdgeInverted = false;
            private uint _startVertex;
            private uint _startEdge;
            private uint _neighbour;

            /// <summary>
            /// Creates a new edge enumerator.
            /// </summary>
            internal GraphEdgeEnumerator(Graph<TEdgeData> graph)
            {
                _graph = graph;
                _currentEdgeId = NO_EDGE;
                _vertex = NO_EDGE;

                _startVertex = NO_EDGE;
                _startEdge = NO_EDGE;
                _currentEdgeInverted = false;
            }

            /// <summary>
            /// Returns true if there is at least one edge.
            /// </summary>
            public bool HasData
            {
                get { return _startVertex != NO_EDGE && 
                    _graph._vertices[_startVertex] != NO_EDGE; }
            }

            /// <summary>
            /// Move to the next edge.
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                if (_nextEdgeId != NO_EDGE)
                { // there is a next edge.
                    _currentEdgeId = _nextEdgeId;
                    _neighbour = 0; // reset neighbour.
                    if (_graph._edges[_nextEdgeId + NODEA] == _vertex)
                    {
                        _neighbour = _graph._edges[_nextEdgeId + NODEB];
                        _nextEdgeId = _graph._edges[_nextEdgeId + NEXTNODEA];
                        _currentEdgeInverted = false;
                    }
                    else
                    {
                        _neighbour = _graph._edges[_nextEdgeId + NODEA];
                        _nextEdgeId = _graph._edges[_nextEdgeId + NEXTNODEB];
                        _currentEdgeInverted = true;
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
                get { return _neighbour; }
            }

            /// <summary>
            /// Returns the current edge data.
            /// </summary>
            public TEdgeData EdgeData
            {
                get
                {
                    if (_currentEdgeInverted)
                    {
                        return (TEdgeData)_graph._edgeData[_currentEdgeId / 4].Reverse();
                    }
                    return _graph._edgeData[_currentEdgeId / 4];
                }
            }

            /// <summary>
            /// Returns true if the edge data is inverted by default.
            /// </summary>
            public bool IsInverted
            {
                get { return _currentEdgeInverted; }
            }

            /// <summary>
            /// Resets this enumerator.
            /// </summary>
            public void Reset()
            {
                _nextEdgeId = _startEdge;
                _currentEdgeId = 0;
                _vertex = _startVertex;
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
            /// Disposes this enumerator.
            /// </summary>
            public void Dispose()
            {

            }

            /// <summary>
            /// Moves this enumerator to the given vertex.
            /// </summary>
            public bool MoveTo(uint vertex)
            {
                var edgeId = _graph._vertices[vertex];
                _nextEdgeId = edgeId;
                _currentEdgeId = 0;
                _vertex = vertex;

                _startVertex = vertex;
                _startEdge = edgeId;
                _currentEdgeInverted = false;

                return edgeId != NO_EDGE;
            }

            /// <summary>
            /// Gets the current edge.
            /// </summary>
            object System.Collections.IEnumerator.Current
            {
                get { return this.Current; }
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
        /// Sorts the graph based on the given transformations.
        /// </summary>
        public void Sort(HugeArrayBase<uint> transformations)
        {
            // update edges.
            for (var i = 0; i < _nextEdgeId; i = i + 4)
            {
                if (_edges[i + NODEA] != NO_EDGE)
                {
                    _edges[i + NODEA] = transformations[_edges[i + NODEA]];
                }
                if (_edges[i + NODEB] != NO_EDGE)
                {
                    _edges[i + NODEB] = transformations[_edges[i + NODEB]];
                }
            }

            // sort vertices and coordinates.
            QuickSort.Sort((i) => 
                {
                    if(i < 0)
                    { // return 'false: this value doesn't exist.
                        return long.MaxValue;
                    }
                    else if(i >= transformations.Length)
                    {
                        return long.MaxValue;
                    }
                    return transformations[i];
                }, (i, j) =>
                {
                    var tempRef = _vertices[i];
                    _vertices[i] = _vertices[j];
                    _vertices[j] = tempRef;

                    var trans = transformations[i];
                    transformations[i] = transformations[j];
                    transformations[j] = trans;
                }, 0, this.VertexCount);
        }

        /// <summary>
        /// Disposes of all native resources associated with this memory dynamic graph.
        /// </summary>
        public void Dispose()
        {
            _edges.Dispose();
            _edgeData.Dispose();
            _vertices.Dispose();
        }

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

            var edgeDataSize = mapper.MappedSize;
            var mapTo = mapper.MapToDelegate;
            var mapFrom = mapper.MapFromDelegate;

            if(_edgeData.Length > this.EdgeCount)
            { // can be compressed, do this first.
                this.Compress();
            }
            if(_vertices.Length > this.VertexCount)
            { // can be trimmed, this this first.
                this.Trim();
            }

            var vertexCount = _vertices.Length;
            var edgeCount = (long)(_nextEdgeId / EDGE_SIZE);

            // write vertex and edge count.
            long position = 0;
            stream.Write(BitConverter.GetBytes(vertexCount), 0, 8); // write exact number of vertices.
            position = position + 8;
            stream.Write(BitConverter.GetBytes(edgeCount), 0, 8); // write exact number of edges.
            position = position + 8;

            // write in this order: vertices, vertexCoordinates, edges, edgeData, edgeShapes.
            using (var file = new MemoryMappedStream(new LimitedStream(stream)))
            {
                // write vertices (each vertex = 1 uint (4 bytes)).
                var vertexArray = new MemoryMappedHugeArrayUInt32(file, vertexCount, vertexCount, 1024);
                vertexArray.CopyFrom(_vertices, 0, 0, vertexCount);
                vertexArray.Dispose(); // written, get rid of it!
                position = position + ((vertexCount) * 4);

                // write edges (each edge = 4 uints (16 bytes)).
                var edgeArray = new MemoryMappedHugeArrayUInt32(file, edgeCount * 4, edgeCount * 4, 1024);
                edgeArray.CopyFrom(_edges, edgeCount * 4);
                edgeArray.Dispose(); // written, get rid of it!
                position = position + (edgeCount * 4 * 4);

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
        public static Graph<TEdgeData> Deserialize(System.IO.Stream stream, bool copy)
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
            var longBytes = new byte[8];
            stream.Read(longBytes, 0, 8);
            position = position + 8;
            var vertexLength = BitConverter.ToInt64(longBytes, 0);
            stream.Read(longBytes, 0, 8);
            position = position + 8;
            var edgeLength = BitConverter.ToInt64(longBytes, 0);

            var bufferSize = 128;
            var cacheSize = 64 * 8;
            var file = new MemoryMappedStream(new LimitedStream(stream));
            var vertexArray = new MemoryMappedHugeArrayUInt32(file, vertexLength, vertexLength, bufferSize, cacheSize);
            position = position + (vertexLength * 4);
            var edgeArray = new MemoryMappedHugeArrayUInt32(file, edgeLength * 4, edgeLength * 4, bufferSize, cacheSize * 16);
            position = position + (edgeLength * 4 * 4);
            var edgeDataArray = new MappedHugeArray<TEdgeData, uint>(
                new MemoryMappedHugeArrayUInt32(file, edgeLength * edgeDataSize, edgeLength * edgeDataSize, bufferSize, cacheSize * 32), edgeDataSize, mapTo, mapFrom);
            position = position + (edgeLength * edgeDataSize * 4);
            
            if (copy)
            { // copy the data.
                var vertexArrayCopy = new HugeArray<uint>(vertexArray.Length);
                vertexArrayCopy.CopyFrom(vertexArray);
                var edgeArrayCopy = new HugeArray<uint>(edgeArray.Length);
                edgeArrayCopy.CopyFrom(edgeArray);
                var edgeDataArrayCopy = new HugeArray<TEdgeData>(edgeDataArray.Length);
                edgeDataArrayCopy.CopyFrom(edgeDataArray);

                file.Dispose();

                return new Graph<TEdgeData>(vertexArrayCopy, edgeArrayCopy, edgeDataArrayCopy);
            }

            return new Graph<TEdgeData>(vertexArray, edgeArray, edgeDataArray);
        }

        #region Data Management

        /// <summary>
        /// Moves an edge from one location to another.
        /// </summary>
        private void MoveEdge(uint oldEdgeId, uint newEdgeId)
        {
            // first copy the data.
            _edges[newEdgeId + NODEA] = _edges[oldEdgeId + NODEA];
            _edges[newEdgeId + NODEB] = _edges[oldEdgeId + NODEB];
            _edges[newEdgeId + NEXTNODEA] = _edges[oldEdgeId + NEXTNODEA];
            _edges[newEdgeId + NEXTNODEB] = _edges[oldEdgeId + NEXTNODEB];
            _edgeData[newEdgeId / EDGE_SIZE] = _edgeData[oldEdgeId / EDGE_SIZE];

            // loop over all edges of vertex1 and replace the oldEdgeId with the new one.
            uint vertex1 = _edges[oldEdgeId + NODEA];
            var edgeId = _vertices[vertex1];
            if (edgeId == oldEdgeId)
            { // edge is the first one, easy!
                _vertices[vertex1] = newEdgeId;
            }
            else
            { // edge is somewhere in the edges list.
                while (edgeId != NO_EDGE)
                { // keep looping.
                    var edgeIdLocation = edgeId + NEXTNODEB;
                    if (_edges[edgeId + NODEA] == vertex1)
                    { // edge loction is different.
                        edgeIdLocation = edgeId + NEXTNODEA;
                    }
                    edgeId = _edges[edgeIdLocation];
                    if (edgeId == oldEdgeId)
                    {
                        _edges[edgeIdLocation] = newEdgeId;
                        break;
                    }
                }
            }

            // loop over all edges of vertex2 and replace the oldEdgeId with the new one.
            uint vertex2 = _edges[oldEdgeId + NODEB];
            edgeId = _vertices[vertex2];
            if (edgeId == oldEdgeId)
            { // edge is the first one, easy!
                _vertices[vertex2] = newEdgeId;
            }
            else
            { // edge is somewhere in the edges list.
                while (edgeId != NO_EDGE)
                { // keep looping.
                    var edgeIdLocation = edgeId + NEXTNODEB;
                    if (_edges[edgeId + NODEA] == vertex2)
                    { // edge loction is different.
                        edgeIdLocation = edgeId + NEXTNODEA;
                    }
                    edgeId = _edges[edgeIdLocation];
                    if (edgeId == oldEdgeId)
                    {
                        _edges[edgeIdLocation] = newEdgeId;
                        break;
                    }
                }
            }

            // remove the old data.
            _edges[oldEdgeId + NODEA] = NO_EDGE;
            _edges[oldEdgeId + NODEB] = NO_EDGE;
            _edges[oldEdgeId + NEXTNODEA] = NO_EDGE;
            _edges[oldEdgeId + NEXTNODEB] = NO_EDGE;
            _edgeData[oldEdgeId / EDGE_SIZE] = default(TEdgeData);
        }

        #endregion
    }
}