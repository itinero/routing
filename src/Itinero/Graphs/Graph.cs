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
using Itinero.Logging;
using Reminiscence.Arrays;
using Reminiscence.IO;
using Reminiscence.IO.Streams;
using System;
using System.Collections.Generic;

namespace Itinero.Graphs
{
    /// <summary>
    /// Represents a undirected graph.
    /// </summary>
    public class Graph : IDisposable
    {
        private bool _isSimple = true;
        private const int MINIMUM_EDGE_SIZE = 4;
        private const int NODEA = 0;
        private const int NODEB = 1;
        private const int NEXTNODEA = 2;
        private const int NEXTNODEB = 3;
        private const int DEFAULT_SIZE_ESTIMATE = 1024;

        private readonly int _edgeSize = -1;
        private readonly int _edgeDataSize = -1;
        private readonly ArrayBase<uint> _vertices; // Holds all vertices pointing to it's first edge.
        private readonly ArrayBase<uint> _edges; // Holds all edges and their data converted to uint's.

        /// <summary>
        /// Creates a new graph.
        /// </summary>
        public Graph(int edgeDataSize)
            : this(edgeDataSize, DEFAULT_SIZE_ESTIMATE)
        {

        }

        /// <summary>
        /// Creates a new graph.
        /// </summary>
        public Graph(int edgeDataSize, long sizeEstimate)
            : this(edgeDataSize, sizeEstimate, 
            Context.ArrayFactory.CreateMemoryBackedArray<uint>(sizeEstimate),
            Context.ArrayFactory.CreateMemoryBackedArray<uint>(sizeEstimate * 3 * (MINIMUM_EDGE_SIZE + edgeDataSize)))
        {

        }

        /// <summary>
        /// Creates a graph using the existing data in the given arrays.
        /// </summary>
        private Graph(int edgeDataSize,
            ArrayBase<uint> vertices,
            ArrayBase<uint> edges)
        {
            _edgeDataSize = edgeDataSize;
            _edgeSize = MINIMUM_EDGE_SIZE + edgeDataSize;

            _vertices = vertices;
            _maxVertex = null;
            if (_vertices.Length > 0)
            {
                _maxVertex = (uint)(vertices.Length - 1);
            }
            _edges = edges;
            _nextEdgeId = (uint)(edges.Length);
            _edgeCount = _nextEdgeId / _edgeSize;
        }

        /// <summary>
        /// Creates a new empty graph using the given arrays.
        /// </summary>
        private Graph(int edgeDataSize, long sizeEstimate,
            ArrayBase<uint> vertices,
            ArrayBase<uint> edges)
        {
            _edgeDataSize = edgeDataSize;
            _edgeSize = MINIMUM_EDGE_SIZE + edgeDataSize;

            _nextEdgeId = 0;
            _vertices = vertices;
            _vertices.Resize(sizeEstimate);
            for (int i = 0; i < sizeEstimate; i++)
            {
                _vertices[i] = Constants.NO_VERTEX;
            }
            _edges = edges;
            _edges.Resize(sizeEstimate * 3 * _edgeSize);
            for (int i = 0; i < sizeEstimate * 3 * _edgeSize; i++)
            {
                _edges[i] = Constants.NO_EDGE;
            }
        }

        /// <summary>
        /// Creates a new using the given file.
        /// </summary>
        public Graph(MemoryMap map, int edgeDataSize, long estimatedSize)
        {
            _edgeDataSize = edgeDataSize;
            _edgeSize = MINIMUM_EDGE_SIZE + edgeDataSize;

            _vertices = new Array<uint>(map, estimatedSize);
            for (int i = 0; i < _vertices.Length; i++)
            {
                _vertices[i] = Constants.NO_VERTEX;
            }
            _edges = new Array<uint>(map, estimatedSize * 3 * _edgeSize);
            for (int i = 0; i < _edges.Length; i++)
            { 
                _edges[i] = Constants.NO_EDGE;
            }
        }

        /// <summary>
        /// Creates a new using the given file.
        /// </summary>
        public Graph(MemoryMap map, GraphProfile profile, int edgeDataSize, long estimatedSize)
        {
            _edgeDataSize = edgeDataSize;
            _edgeSize = MINIMUM_EDGE_SIZE + edgeDataSize;
            _vertices = new Array<uint>(map, estimatedSize, profile.VertexProfile);
            for (int i = 0; i < _vertices.Length; i++)
            {
                _vertices[i] = Constants.NO_VERTEX;
            }
            _edges = new Array<uint>(map, estimatedSize * 3 * _edgeSize, profile.EdgeProfile);
            for (int i = 0; i < _edges.Length; i++)
            {
                _edges[i] = Constants.NO_EDGE;
            }
        }

        private uint _nextEdgeId;
        private long _edgeCount = 0;
        private uint? _maxVertex = null;
        
        /// <summary>
        /// Returns true if this graph is simple (max one edge between any two vertices).
        /// </summary>
        /// <returns></returns>
        public bool IsSimple
        {
            get
            {
                return _isSimple;
            }
        }

        /// <summary>
        /// Marks this graph as a multigraph, from now on duplicate edges are allowed.
        /// </summary>    
        public void MarkAsMulti()
        {
            _isSimple = false;
        }

        /// <summary>
        /// Verifies if this graph is simple and marks it as such.
        /// </summary>
        /// <remarks>
        /// - Checks for duplicate edges, returns false if it still finds one.
        /// - Marks this graph as simple if the checks succeed.
        /// </remarks>
        public bool MarkAsSimple()
        {
            var neighbours = new HashSet<uint>();
            var enumerator = this.GetEdgeEnumerator();
            for (uint v = 0; v < this.VertexCount; v++)
            {
                if (!enumerator.MoveTo(v))
                { // no edge here!
                    continue;
                }

                neighbours.Clear();
                while (enumerator.MoveNext())
                {
                    if (enumerator.To == v)
                    { // a loop was found.
                        return false;
                    }

                    if (neighbours.Contains(enumerator.To))
                    { // a duplicate was found!
                        return false;
                    }
                    neighbours.Add(enumerator.To);
                }
            }

            _isSimple = true;
            return true;
        }

        /// <summary>
        /// Adds a new vertex.
        /// </summary>
        public void AddVertex(uint vertex)
        {
            if (_maxVertex == null || _maxVertex.Value < vertex)
            { // update max vertex.
                _maxVertex = vertex;
            }

            // increase space if needed.
            _vertices.EnsureMinimumSize(vertex + 1, Constants.NO_VERTEX);
            if (_vertices[vertex] == Constants.NO_VERTEX)
            { // only overwrite if this vertex is not there yet.
                _vertices[vertex] = Constants.NO_EDGE;
            }
        }

        /// <summary>
        /// Returns true if this graph has the given vertex.
        /// </summary>
        public bool HasVertex(uint vertex)
        {
            if (vertex > _vertices.Length - 1)
            {
                return false;
            }

            if (_vertices[vertex] != Constants.NO_VERTEX)
            { // also remove edges.
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the given vertex.
        /// </summary>
        public bool RemoveVertex(uint vertex)
        {
            if (vertex > _vertices.Length - 1)
            { // vertex does not exist.
                return false;
            }

            if (_vertices[vertex] != Constants.NO_VERTEX)
            { // also remove edges.
                this.RemoveEdges(vertex);

                _vertices[vertex] = Constants.NO_VERTEX;

                // update max vertex.
                if (vertex == _maxVertex)
                {
                    while (vertex > 0)
                    {
                        vertex--;
                        if (_vertices[vertex] != Constants.NO_VERTEX)
                        { // this vertex is the highest vertex that is not unset.
                            _maxVertex = vertex;
                            break;
                        }
                        else if (vertex == 0)
                        { // no vertices left.
                            _maxVertex = null;
                        }
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds an edge with the associated data.
        /// </summary>
        public uint AddEdge(uint vertex1, uint vertex2, params uint[] data)
        {
            if (_isSimple)
            { // no single-edge loops in a simple graph.
                if (vertex1 == vertex2) { throw new ArgumentException("Given vertices must be different."); }
            }
            if (vertex1 > _vertices.Length - 1) { throw new ArgumentException(string.Format("Vertex {0} does not exist.", vertex1)); }
            if (vertex2 > _vertices.Length - 1) { throw new ArgumentException(string.Format("Vertex {0} does not exist.", vertex2)); }
            if (_vertices[vertex1] == Constants.NO_VERTEX) { throw new ArgumentException(string.Format("Vertex {0} does not exist.", vertex1)); }
            if (_vertices[vertex2] == Constants.NO_VERTEX) { throw new ArgumentException(string.Format("Vertex {0} does not exist.", vertex2)); }
            if (data.Length != _edgeDataSize) { throw new ArgumentException("Data block has incorrect size, needs to match exactly edge data size."); }

            var edgeId = _vertices[vertex1];
            if (_vertices[vertex1] != Constants.NO_EDGE)
            { // check for an existing edge first.
                // check if the arc exists already.
                edgeId = _vertices[vertex1];
                uint nextEdgeSlot = 0;
                while (edgeId != Constants.NO_EDGE)
                { // keep looping.
                    uint otherVertexId = 0;
                    uint previousEdgeId = edgeId;
                    //bool forward = true;
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
                    }

                    if (otherVertexId == vertex2)
                    {
                        if (_isSimple)
                        {
                            throw new ArgumentException("Cannot add a duplicate edge to a simple graph, there can be only one edge between any vertex-pair.");
                        }

                        // REMARK: we used to replace the original data here, but now use UpdateEdgeData for this.
                        //if (otherVertexId == vertex2)
                        //{ // this is the edge we need.
                        //    if(!forward)
                        //    { // switch things around.
                        //        var temp = _edges[previousEdgeId + NODEA];
                        //        _edges[previousEdgeId + NODEA] = _edges[previousEdgeId + NODEB];
                        //        _edges[previousEdgeId + NODEB] = temp;
                        //        temp = _edges[previousEdgeId + NEXTNODEA];
                        //        _edges[previousEdgeId + NEXTNODEA] = _edges[previousEdgeId + NEXTNODEB];
                        //        _edges[previousEdgeId + NEXTNODEB] = temp;
                        //    }

                        //    // overwrite data.
                        //    for (var i = 0; i < _edgeDataSize; i++)
                        //    {
                        //        _edges[previousEdgeId + MINIMUM_EDGE_SIZE + i] =
                        //            data[i];
                        //    }
                        //    return (uint)(previousEdgeId / _edgeSize);
                        //}
                    }
                }

                // create a new edge.
                edgeId = _nextEdgeId;

                // there may be a need to increase edges array.
                _edges.EnsureMinimumSize(_nextEdgeId + NEXTNODEB + 1, Constants.NO_EDGE);
                _edges[_nextEdgeId + NODEA] = vertex1;
                _edges[_nextEdgeId + NODEB] = vertex2;
                _edges[_nextEdgeId + NEXTNODEA] = Constants.NO_EDGE;
                _edges[_nextEdgeId + NEXTNODEB] = Constants.NO_EDGE;
                _nextEdgeId = (uint)(_nextEdgeId + _edgeSize);

                // append the new edge to the from list.
                _edges[nextEdgeSlot] = edgeId;

                // set data.
                for (var i = 0; i < _edgeDataSize; i++)
                {
                    _edges[edgeId + MINIMUM_EDGE_SIZE + i] =
                        data[i];
                }
                _edgeCount++;
            }
            else
            { // create a new edge and set.
                edgeId = _nextEdgeId;
                _vertices[vertex1] = _nextEdgeId;

                // there may be a need to increase edges array.
                _edges.EnsureMinimumSize(_nextEdgeId + NEXTNODEB + 1, Constants.NO_EDGE);
                _edges[_nextEdgeId + NODEA] = vertex1;
                _edges[_nextEdgeId + NODEB] = vertex2;
                _edges[_nextEdgeId + NEXTNODEA] = Constants.NO_EDGE;
                _edges[_nextEdgeId + NEXTNODEB] = Constants.NO_EDGE;
                _nextEdgeId = (uint)(_nextEdgeId + _edgeSize);

                // set data.
                for (var i = 0; i < _edgeDataSize; i++)
                {
                    _edges[edgeId + MINIMUM_EDGE_SIZE + i] =
                        data[i];
                }
                _edgeCount++;
            }

            if (vertex1 != vertex2)
            {
                var toEdgeId = _vertices[vertex2];
                if (toEdgeId != Constants.NO_EDGE)
                { // there are existing edges.
                    uint nextEdgeSlot = 0;
                    while (toEdgeId != Constants.NO_EDGE)
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
            }

            return (uint)(edgeId / _edgeSize);
        }
        
        /// <summary>
        /// Updates the data associated with this edge.
        /// </summary>
        public void UpdateEdgeData(uint edgeId, params uint[] data)
        {
            var internalEdgeId = edgeId * _edgeSize;
            for (var i = 0; i < _edgeDataSize; i++)
            {
                _edges[internalEdgeId + MINIMUM_EDGE_SIZE + i] =
                    data[i];
            }
        }

        /// <summary>
        /// Returns the edge with the given id.
        /// </summary>
        /// <returns></returns>
        public Edge GetEdge(uint edgeId)
        {
            var edgePointer = edgeId * (uint)_edgeSize;
            if (_edges.Length < edgePointer + (uint)_edgeSize)
            { // edge not part of graph.
                throw new ArgumentOutOfRangeException();
            }

            var data = new uint[_edgeDataSize];
            for(var i = 0; i < _edgeDataSize; i++)
            {
                data[i] =
                    _edges[edgePointer + MINIMUM_EDGE_SIZE + i];
            }
            return new Edge(
                edgeId,
                _edges[edgePointer + NODEA],
                _edges[edgePointer + NODEB],
                data,
                false);
        }

        /// <summary>
        /// Deletes all edges leading from/to the given vertex. 
        /// </summary>
        public int RemoveEdges(uint vertex)
        {
            var removed = 0;
            var edges = this.GetEdgeEnumerator(vertex);
            while (edges.MoveNext())
            {
                removed += this.RemoveEdges(vertex, edges.To);
            }
            return removed;
        }

        /// <summary>
        /// Deletes the edge with the given id.
        /// </summary>
        /// <returns></returns>
        public bool RemoveEdge(uint edgeId)
        {
            edgeId = edgeId * (uint)_edgeSize;
            if (_edges.Length < edgeId + (uint)_edgeSize)
            { // edge not part of graph.
                return false;
            }

            return this.RemoveEdges(_edges[edgeId + NODEA], _edges[edgeId + NODEB], edgeId) > 0;
        }

        /// <summary>
        /// Deletes the edge between the two given vertices.
        /// </summary>
        public int RemoveEdges(uint vertex1, uint vertex2)
        {
            return this.RemoveEdges(vertex1, vertex2, uint.MaxValue);
        }

        /// <summary>
        /// Deletes the edge between the two given vertices.
        /// </summary>
        private int RemoveEdges(uint vertex1, uint vertex2, uint edgeId = uint.MaxValue)
        {
            if (_isSimple)
            { // this can happen on non-simple graphs.
                if (vertex1 == vertex2) { throw new ArgumentException("Given vertices must be different."); }
            }
            if (vertex1 > _vertices.Length - 1) { throw new ArgumentException(string.Format("Vertex {0} does not exist.", vertex1)); }
            if (vertex2 > _vertices.Length - 1) { throw new ArgumentException(string.Format("Vertex {0} does not exist.", vertex2)); }
            if (_vertices[vertex1] == Constants.NO_VERTEX) { throw new ArgumentException(string.Format("Vertex {0} does not exist.", vertex1)); }
            if (_vertices[vertex2] == Constants.NO_VERTEX) { throw new ArgumentException(string.Format("Vertex {0} does not exist.", vertex2)); }

            if (_vertices[vertex1] == Constants.NO_EDGE ||
                _vertices[vertex2] == Constants.NO_EDGE)
            { // no edge to remove here!
                return 0;
            }

            // remove for vertex1.
            var removed = 0;
            var nextEdgeId = _vertices[vertex1];
            uint nextEdgeSlot = 0;
            uint previousEdgeSlot = 0;
            uint currentEdgeId = 0;
            while (nextEdgeId != Constants.NO_EDGE)
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
                if (otherVertexId == vertex2 && 
                    (edgeId == uint.MaxValue || edgeId == currentEdgeId))
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

                    if (vertex1 == vertex2)
                    { // the edge won't be reset in the second part.
                        // reset everything about this edge.
                        _edges[currentEdgeId + NODEA] = Constants.NO_EDGE;
                        _edges[currentEdgeId + NODEB] = Constants.NO_EDGE;
                        _edges[currentEdgeId + NEXTNODEA] = Constants.NO_EDGE;
                        _edges[currentEdgeId + NEXTNODEB] = Constants.NO_EDGE;
                        for (var i = 0; i < _edgeDataSize; i++)
                        {
                            _edges[currentEdgeId + MINIMUM_EDGE_SIZE + i] = Constants.NO_EDGE;
                        }
                    }

                    removed++;
                    _edgeCount--;
                    if (_isSimple || edgeId != uint.MaxValue)
                    { // is the graph is simple, there's only one edge.
                        // if we seek only one edge also break.
                        break;
                    }
                }
            }

            // remove for vertex2.
            if (removed != 0)
            {
                nextEdgeId = _vertices[vertex2];
                nextEdgeSlot = 0;
                previousEdgeSlot = 0;
                currentEdgeId = 0;
                while (nextEdgeId != Constants.NO_EDGE)
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
                    if (otherVertexId == vertex1 &&
                        (edgeId == uint.MaxValue || edgeId == currentEdgeId))
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
                        _edges[currentEdgeId + NODEA] = Constants.NO_EDGE;
                        _edges[currentEdgeId + NODEB] = Constants.NO_EDGE;
                        _edges[currentEdgeId + NEXTNODEA] = Constants.NO_EDGE;
                        _edges[currentEdgeId + NEXTNODEB] = Constants.NO_EDGE;
                        for (var i = 0; i < _edgeDataSize; i++)
                        {
                            _edges[currentEdgeId + MINIMUM_EDGE_SIZE + i] = Constants.NO_EDGE;
                        }
                        if (_isSimple || edgeId != uint.MaxValue)
                        { // is the graph is simple, there's only one edge.
                            // if we seek only one edge also break.
                            return removed;
                        }
                    }
                }
            }
            return removed;
        }

        /// <summary>
        /// Switches the two vertices.
        /// </summary>
        public void Switch(uint vertex1, uint vertex2)
        {
            if (vertex1 == vertex2) { throw new ArgumentException("Given vertices must be different."); }
            if (vertex1 > _vertices.Length - 1) { throw new ArgumentException(string.Format("Vertex {0} does not exist.", vertex1)); }
            if (vertex2 > _vertices.Length - 1) { throw new ArgumentException(string.Format("Vertex {0} does not exist.", vertex2)); }
            if (_vertices[vertex1] == Constants.NO_VERTEX) { throw new ArgumentException(string.Format("Vertex {0} does not exist.", vertex1)); }
            if (_vertices[vertex2] == Constants.NO_VERTEX) { throw new ArgumentException(string.Format("Vertex {0} does not exist.", vertex2)); }

            if (!_isSimple)
            { // it's not supported to sort vertices in a multi-graph.
                throw new NotSupportedException("It's not supported to switch vertices in a multi-graph.");
            }
            
            // change things around in the edges of vertex1.
            var pointer = _vertices[vertex1];
            while (pointer != Constants.NO_EDGE)
            {
                if (_edges[pointer + NODEA] == vertex1)
                { // was NODEA.
                    if (_edges[pointer + NODEB] == vertex2)
                    { // vertex2 was NODEB.
                        _edges[pointer + NODEB] = vertex1;
                    }
                    _edges[pointer + NODEA] = vertex2;
                    pointer = _edges[pointer + NEXTNODEA];
                }
                else
                { // was NODEB.
                    if (_edges[pointer + NODEA] == vertex2)
                    { // vertex2 was NODEA.
                        _edges[pointer + NODEA] = vertex1;
                    }
                    _edges[pointer + NODEB] = vertex2;
                    pointer = _edges[pointer + NEXTNODEB];
                }
            }

            // change things around in the edges of vertex2.
            pointer = _vertices[vertex2];
            while (pointer != Constants.NO_EDGE)
            {
                if (_edges[pointer + NODEA] == vertex2)
                { // was NODEA.
                    if (_edges[pointer + NODEB] == vertex1)
                    { // ok, NODEB is now vertex1, meaning it used to be vertex2, this edge has already been changed.
                        pointer = _edges[pointer + NEXTNODEB];
                    }
                    else
                    { // ok, NODEB is not vertex1, meaning this edge has not been changed yet.
                        _edges[pointer + NODEA] = vertex1;
                        pointer = _edges[pointer + NEXTNODEA];
                    }
                }
                else
                { // was NODEB.
                    if (_edges[pointer + NODEA] == vertex1)
                    { // ok, NODEA is now vertex1, meaning it used to be vertex2, this edge has already been changed.
                        pointer = _edges[pointer + NEXTNODEA];
                    }
                    else
                    { // ok, NODEA is not vertex1, meaning this edge has not been changed yet.
                        _edges[pointer + NODEB] = vertex1;
                        pointer = _edges[pointer + NEXTNODEB];
                    }
                }
            }

            // switch points.
            var vertex1Pointer = _vertices[vertex1];
            _vertices[vertex1] = _vertices[vertex2];
            _vertices[vertex2] = vertex1Pointer;
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
        /// <returns></returns>
        public EdgeEnumerator GetEdgeEnumerator(uint vertex)
        {
            if (vertex >= _vertices.Length) { throw new ArgumentOutOfRangeException("vertex", "vertex is not part of this graph."); }

            var enumerator = new EdgeEnumerator(this);
            enumerator.MoveTo(vertex);
            return enumerator;
        }

        /// <summary>
        /// Relocates data internally in the most compact way possible.
        /// </summary>
        /// <param name="updateEdgeId">The edge id's may change. This action can be used to hook into every change.</param>
        public void Compress(Action<uint, uint> updateEdgeId)
        {
            // check if compression is needed.
            if (_edgeCount == _edges.Length / _edgeSize)
            { // no need, data structures already at their smallest possible size.
                return;
            }

            // move edges down.
            uint maxAllocatedEdgeId = 0;
            for (uint edgePointer = 0; edgePointer < _nextEdgeId; edgePointer = (uint)(edgePointer + _edgeSize))
            {
                if (_edges[edgePointer] != Constants.NO_EDGE)
                { // this edge is allocated.
                    if (edgePointer != maxAllocatedEdgeId)
                    { // there is data here.
                        this.MoveEdge(edgePointer, maxAllocatedEdgeId);
                        if (updateEdgeId != null)
                        { // report that this edge id has changed.
                            updateEdgeId.Invoke((uint)(edgePointer / _edgeSize), (uint)(maxAllocatedEdgeId / _edgeSize));
                        }
                    }
                    maxAllocatedEdgeId = (uint)(maxAllocatedEdgeId + _edgeSize);
                }
            }
            _nextEdgeId = maxAllocatedEdgeId;

            if (_edgeCount != _nextEdgeId / _edgeSize)
            {
                Itinero.Logging.Logger.Log("Graph", TraceEventType.Warning, 
                    "Error in graph bookkeeping: {0} edges found, {1} counted.", 
                        _nextEdgeId / _edgeSize, _edgeCount);
            }

            this.Trim();
        }

        /// <summary>
        /// Relocates data internally in the most compact way possible.
        /// </summary>
        public void Compress()
        {
            this.Compress(null);
        }

        /// <summary>
        /// Resizes the internal data structures to their smallest size possible.
        /// </summary>
        public void Trim()
        {
            // resize edges.
            var edgeSize = _nextEdgeId;
            _edges.Resize(edgeSize);

            // remove all vertices that are unset.
            var size = _vertices.Length;
            while (size > 0 && _vertices[size - 1] == Constants.NO_VERTEX)
            {
                size--;
            }
            _vertices.Resize(size);
        }

        /// <summary>
        /// Returns the number of vertices in this graph.
        /// </summary>
        public uint VertexCount
        {
            get
            {
                if (_maxVertex == null)
                { // no vertices set yet!
                    return 0;
                }
                return _maxVertex.Value + 1;
            }
        }

        /// <summary>
        /// Returns the capacity for vertices.
        /// </summary>
        public long VertexCapacity
        {
            get
            {
                return _vertices.Length;
            }
        }

        /// <summary>
        /// Returns the number of edges in this graph.
        /// </summary>
        public long EdgeCount
        {
            get { return _edgeCount; }
        }

        /// <summary>
        /// Returns the capacity for edges.
        /// </summary>
        public long EdgeCapacity
        {
            get
            {
                return _edges.Length / _edgeSize;
            }
        }

        /// <summary>
        /// An edge enumerator.
        /// </summary>
        public class EdgeEnumerator : IEnumerable<Edge>, IEnumerator<Edge>
        {
            private readonly Graph _graph;
            private uint _nextEdgePointer;
            private uint _vertex;
            private uint _currentEdgePointer;
            private bool _currentEdgeInverted = false;
            private uint _startVertex;
            private uint _startEdgePointer;
            private uint _neighbour;

            /// <summary>
            /// Creates a new edge enumerator.
            /// </summary>
            internal EdgeEnumerator(Graph graph)
            {
                _graph = graph;
                _currentEdgePointer = Constants.NO_EDGE;
                _vertex = Constants.NO_EDGE;

                _startVertex = Constants.NO_EDGE;
                _startEdgePointer = Constants.NO_EDGE;
                _currentEdgeInverted = false;
            }

            /// <summary>
            /// Returns true if there is at least one edge.
            /// </summary>
            public bool HasData
            {
                get
                {
                    return _startVertex != Constants.NO_EDGE &&
                    _graph._vertices[_startVertex] != Constants.NO_EDGE;
                }
            }

            /// <summary>
            /// Move to the next edge.
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                if (_nextEdgePointer != Constants.NO_EDGE)
                { // there is a next edge.
                    _currentEdgePointer = _nextEdgePointer;
                    _neighbour = 0; // reset neighbour.
                    if (_graph._edges[_nextEdgePointer + NODEA] == _vertex)
                    {
                        _neighbour = _graph._edges[_nextEdgePointer + NODEB];
                        _nextEdgePointer = _graph._edges[_nextEdgePointer + NEXTNODEA];
                        _currentEdgeInverted = false;
                    }
                    else
                    {
                        _neighbour = _graph._edges[_nextEdgePointer + NODEA];
                        _nextEdgePointer = _graph._edges[_nextEdgePointer + NEXTNODEB];
                        _currentEdgeInverted = true;
                    }
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Returns the vertex at the beginning.
            /// </summary>
            public uint From
            {
                get
                {
                    return _vertex;
                }
            }

            /// <summary>
            /// Returns the vertex at the end.
            /// </summary>
            public uint To
            {
                get { return _neighbour; }
            }

            /// <summary>
            /// Returns the edge data.
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
            /// Returns the first data element.
            /// </summary>
            public uint Data0
            {
                get
                {
                    return _graph._edges[_currentEdgePointer + MINIMUM_EDGE_SIZE + 0];
                }
            }

            /// <summary>
            /// Returns the second data element.
            /// </summary>
            public uint Data1
            {
                get
                {
                    return _graph._edges[_currentEdgePointer + MINIMUM_EDGE_SIZE + 1];
                }
            }

            /// <summary>
            /// Returns true if the edge data is inverted by default.
            /// </summary>
            public bool DataInverted
            {
                get { return _currentEdgeInverted; }
            }

            /// <summary>
            /// Gets the current edge id.
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
                _nextEdgePointer = _startEdgePointer;
                _currentEdgePointer = 0;
                _vertex = _startVertex;
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
                var edgePointer = _graph._vertices[vertex];
                _nextEdgePointer = edgePointer;
                _currentEdgePointer = 0;
                _vertex = vertex;

                _startVertex = vertex;
                _startEdgePointer = edgePointer;
                _currentEdgeInverted = false;

                return edgePointer != Constants.NO_EDGE;
            }
            
            /// <summary>
            /// Moves this enumerator to the given edge.
            /// </summary>
            public void MoveToEdge(uint edge)
            {
                var edgePointer = edge * (uint)_graph._edgeSize;

                _nextEdgePointer = edgePointer;
                _currentEdgePointer = _nextEdgePointer;

                _vertex = _graph._edges[_nextEdgePointer + NODEA];
                _neighbour = _graph._edges[_nextEdgePointer + NODEB];

                _startVertex = _vertex;
                _startEdgePointer = edgePointer;
                _currentEdgeInverted = false;
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
        public void Sort(ArrayBase<uint> transformations)
        {
            // update edges.
            for (var i = 0; i < _nextEdgeId; i = i + _edgeSize)
            {
                if (_edges[i + NODEA] != Constants.NO_EDGE)
                {
                    _edges[i + NODEA] = transformations[_edges[i + NODEA]];
                }
                if (_edges[i + NODEB] != Constants.NO_EDGE)
                {
                    _edges[i + NODEB] = transformations[_edges[i + NODEB]];
                }
            }

            // sort vertices and coordinates.
            QuickSort.Sort((i) =>
                {
                    if (i < 0)
                    { // return 'false: this value doesn't exist.
                        return long.MaxValue;
                    }
                    else if (i >= transformations.Length)
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
            _vertices.Dispose();
        }

        /// <summary>
        /// Returns the size in bytes as if serialized.
        /// </summary>
        /// <returns></returns>
        public long SizeInBytes
        {
            get
            {
                return 1 + 8 + 8 + 4 + 4 + // the header: two longs representing vertex and edge count and one int for edge size and one for vertex size.
                    this.VertexCount * 4 + // the bytes for the vertex-index: 2 vertices, pointing to 0.
                    this.EdgeCount * 4 * (4 + 1); // the bytes for the one edge: one edge = 4 uints + edge data size.
            }
        }

        /// <summary>
        /// Serializes this graph to disk.
        /// </summary>
        public long Serialize(System.IO.Stream stream)
        {
            this.Compress();

            if (_vertices.Length > this.VertexCount)
            { // can be trimmed, this this first.
                this.Trim();
            }

            var vertexCount = _vertices.Length;
            var edgeCount = (long)(_nextEdgeId / _edgeSize);

            // write vertex and edge count.
            long size = 1;
            stream.WriteByte(1); // write default version.
            stream.Write(BitConverter.GetBytes(vertexCount), 0, 8); // write exact number of vertices.
            size += 8;
            stream.Write(BitConverter.GetBytes(edgeCount), 0, 8); // write exact number of edges.
            size += 8;
            stream.Write(BitConverter.GetBytes(1), 0, 4); // write the vertex size.
            size += 4;
            stream.Write(BitConverter.GetBytes(_edgeSize), 0, 4); // write edge size.
            size += 4;

            // write actual data.
            size += _vertices.CopyTo(stream);
            size += _edges.CopyTo(stream);

            return size;
        }

        /// <summary>
        /// Deserializes a graph from the given stream.
        /// </summary>
        /// <returns></returns>
        public static Graph Deserialize(System.IO.Stream stream, GraphProfile profile)
        {
            var initialPosition = stream.Position;

            // read sizes.
            long size = 1;
            var version = stream.ReadByte();
            if(version != 1)
            {
                throw new Exception(string.Format("Cannot deserialize graph: Invalid version #: {0}.", version));
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

            return new Graph(edgeSize - MINIMUM_EDGE_SIZE, vertices, edges);
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
            for (var i = 0; i < _edgeDataSize; i++)
            {
                _edges[newEdgeId + MINIMUM_EDGE_SIZE + i] =
                     _edges[oldEdgeId + MINIMUM_EDGE_SIZE + i];
            }

            // loop over all edges of vertex1 and replace the oldEdgeId with the new one.
            uint vertex1 = _edges[oldEdgeId + NODEA];
            var edgeId = _vertices[vertex1];
            if (edgeId == oldEdgeId)
            { // edge is the first one, easy!
                _vertices[vertex1] = newEdgeId;
            }
            else
            { // edge is somewhere in the edges list.
                while (edgeId != Constants.NO_EDGE)
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
                while (edgeId != Constants.NO_EDGE)
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
            _edges[oldEdgeId + NODEA] = Constants.NO_EDGE;
            _edges[oldEdgeId + NODEB] = Constants.NO_EDGE;
            _edges[oldEdgeId + NEXTNODEA] = Constants.NO_EDGE;
            _edges[oldEdgeId + NEXTNODEB] = Constants.NO_EDGE;
            for (var i = 0; i < _edgeDataSize; i++)
            {
                _edges[oldEdgeId + MINIMUM_EDGE_SIZE + i] =
                     Constants.NO_EDGE;
            }
        }

        #endregion
    }
}