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

using Reminiscence.Arrays;
using Reminiscence.IO;
using Reminiscence.IO.Streams;
using System;
using System.Collections.Generic;

namespace Itinero.Graphs.Directed
{
    /// <summary>
    /// A direct graph with extra meta-data per edge.
    /// </summary>
    public sealed class DirectedMetaGraph
    {
        private readonly DirectedGraph _graph;
        private readonly ArrayBase<uint> _edgeData;
        private readonly int _edgeDataSize = int.MaxValue;

        /// <summary>
        /// Creates a new graph.
        /// </summary>
        public DirectedMetaGraph(int edgeDataSize, int edgeMetaDataSize)
            : this(edgeDataSize, edgeMetaDataSize, 1000)
        {

        }

        /// <summary>
        /// Creates a new graph.
        /// </summary>
        public DirectedMetaGraph(int edgeDataSize, int edgeMetaDataSize, long sizeEstimate)
        {
            _edgeDataSize = edgeMetaDataSize;
            _graph = new DirectedGraph(edgeDataSize, sizeEstimate, (x, y) =>
            {
                this.SwitchEdge(x, y);
            });
            _edgeData = Context.ArrayFactory.CreateMemoryBackedArray<uint>(_edgeDataSize * _graph.EdgeCount);
        }

        /// <summary>
        /// Creates a new graph based on existing data.
        /// </summary>
        private DirectedMetaGraph(DirectedGraph graph, int edgeDataSize, ArrayBase<uint> edgeData)
        {
            _graph = graph;
            _edgeData = edgeData;
            _edgeDataSize = edgeDataSize;
        }

        /// <summary>
        /// Gets the basic graph.
        /// </summary>
        public DirectedGraph Graph
        {
            get
            {
                return _graph;
            }
        }

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
        /// Switched two edges.
        /// </summary>
        private void SwitchEdge(uint oldId, uint newId)
        {
            var oldEdgePointer = oldId * _edgeDataSize;
            var newEdgePointer = newId * _edgeDataSize;
            _edgeData.EnsureMinimumSize(newEdgePointer + _edgeDataSize + 1);
            for (var i = 0; i < _edgeDataSize; i++)
            {
                _edgeData[newEdgePointer + i] = _edgeData[oldEdgePointer + i];
            }
        }

        /// <summary>
        /// Adds a new edge.
        /// </summary>
        /// <returns></returns>
        public uint AddEdge(uint vertex1, uint vertex2, uint data, uint metaData)
        {
            if (_edgeDataSize != 1) { throw new ArgumentOutOfRangeException("Dimension of meta-data doesn't match."); }

            var edgeId = _graph.AddEdge(vertex1, vertex2, data);
            var edgePointer = edgeId * _edgeDataSize;
            _edgeData.EnsureMinimumSize(edgePointer + 1);
            _edgeData[edgePointer + 0] = metaData;
            return edgeId;
        }

        /// <summary>
        /// Adds a new edge.
        /// </summary>
        /// <returns></returns>
        public uint AddEdge(uint vertex1, uint vertex2, uint[] data, params uint[] metaData)
        {
            var edgeId = _graph.AddEdge(vertex1, vertex2, data);
            var edgePointer = edgeId * _edgeDataSize;
            _edgeData.EnsureMinimumSize(edgePointer + _edgeDataSize + 1);
            for (var i = 0; i < _edgeDataSize; i++)
            {
                _edgeData[edgePointer + i] = metaData[i];
            }
            return edgeId;
        }

        /// <summary>
        /// Updates and edge's associated data.
        /// </summary>
        public uint UpdateEdge(uint vertex1, uint vertex2, Func<uint[], bool> update, uint[] data, params uint[] metaData)
        {
            var edgeId = _graph.UpdateEdge(vertex1, vertex2, update, data);
            if(edgeId == Constants.NO_EDGE)
            {
                return Constants.NO_EDGE;
            }
            var edgePointer = edgeId * _edgeDataSize;
            for (var i = 0; i < _edgeDataSize; i++)
            {
                _edgeData[edgePointer + i] = metaData[i];
            }
            return edgeId;
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
        public int RemoveEdge(uint vertex1, uint vertex2)
        {
            return _graph.RemoveEdge(vertex1, vertex2);
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
            this.Compress(false);
        }

        /// <summary>
        /// Relocates data internally in the most compact way possible.
        /// </summary>
        public void Compress(bool toReadonly)
        {
            long maxEdgeId;
            _graph.Compress(toReadonly, out maxEdgeId);
            _edgeData.Resize(maxEdgeId * _edgeDataSize);
        }

        /// <summary>
        /// Resizes the internal data structures to their smallest size possible.
        /// </summary>
        public void Trim()
        {
            long maxEdgeId;
            _graph.Trim(out maxEdgeId);
            _edgeData.Resize(maxEdgeId * _edgeDataSize);
        }

        /// <summary>
        /// Returns the number of vertices in this graph.
        /// </summary>
        public long VertexCount
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
        /// Represents the internal edge enumerator.
        /// </summary>
        public sealed class EdgeEnumerator : IEnumerable<MetaEdge>, IEnumerator<MetaEdge>
        {
            private readonly DirectedMetaGraph _graph;
            private readonly DirectedGraph.EdgeEnumerator _enumerator;

            /// <summary>
            /// Creates a new edge enumerator.
            /// </summary>
            public EdgeEnumerator(DirectedMetaGraph graph, DirectedGraph.EdgeEnumerator enumerator)
            {
                _graph = graph;
                _enumerator = enumerator;
            }

            /// <summary>
            /// Move to the next edge.
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            /// <summary>
            /// Returns the current neighbour.
            /// </summary>
            public uint Neighbour
            {
                get { return _enumerator.Neighbour; }
            }

            /// <summary>
            /// Returns the current edge data.
            /// </summary>
            public uint[] Data
            {
                get
                {
                    return _enumerator.Data;
                }
            }

            /// <summary>
            /// Returns the first entry in the edge data.
            /// </summary>
            public uint Data0
            {
                get
                {
                    return _enumerator.Data0;
                }
            }

            /// <summary>
            /// Returns the current edge meta-data.
            /// </summary>
            public uint[] MetaData
            {
                get
                {
                    var metaData = new uint[_graph._edgeDataSize];
                    var edgePointer = _enumerator.Id * _graph._edgeDataSize;
                    for(var i = 0; i < _graph._edgeDataSize; i++)
                    {
                        metaData[i] = _graph._edgeData[edgePointer + i];
                    }
                    return metaData;
                }
            }

            /// <summary>
            /// Returns the current edge meta-data.
            /// </summary>
            public uint MetaData0
            {
                get
                {
                    return _graph._edgeData[(_enumerator.Id * _graph._edgeDataSize) + 0];
                }
            }

            /// <summary>
            /// Returns the edge id.
            /// </summary>
            public uint Id
            {
                get
                {
                    return _enumerator.Id;
                }
            }

            /// <summary>
            /// Resets this enumerator.
            /// </summary>
            public void Reset()
            {
                _enumerator.Reset();
            }

            /// <summary>
            /// Gets the enumerator.
            /// </summary>
            /// <returns></returns>
            public IEnumerator<MetaEdge> GetEnumerator()
            {
                this.Reset();
                return this;
            }

            /// <summary>
            /// Gets the current edge.
            /// </summary>
            public MetaEdge Current
            {
                get { return new MetaEdge(this); }
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
                get { return _enumerator.Count; }
            }

            /// <summary>
            /// Moves this enumerator to the given vertex.
            /// </summary>
            public bool MoveTo(uint vertex)
            {
                return _enumerator.MoveTo(vertex);
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
            /// Gets the current edge.
            /// </summary>
            object System.Collections.IEnumerator.Current
            {
                get { throw new NotImplementedException(); }
            }
        }

        /// <summary>
        /// Serializes to a stream.
        /// </summary>
        public long Serialize(System.IO.Stream stream)
        {
            return this.Serialize(stream, true);
        }

        /// <summary>
        /// Serializes to a stream.
        /// </summary>
        public long Serialize(System.IO.Stream stream, bool toReadonly)
        {
            this.Compress(toReadonly);

            long size = 1;
            stream.WriteByte(1);
            size += _graph.Serialize(stream, false);
            stream.Write(BitConverter.GetBytes(0), 0, 4); // write the vertex size.
            size += 4;
            stream.Write(BitConverter.GetBytes(_edgeDataSize), 0, 4); // write the edge size.
            size += 4;
            size += _edgeData.CopyTo(stream);

            return size;
        }

        /// <summary>
        /// Deserializes from a stream.
        /// </summary>
        /// <returns></returns>
        public static DirectedMetaGraph Deserialize(System.IO.Stream stream, DirectedMetaGraphProfile profile)
        {
            var version = stream.ReadByte();
            if (version != 1)
            {
                throw new Exception(string.Format("Cannot deserialize directed meta graph: Invalid version #: {0}.", version));
            }

            var graph = DirectedGraph.Deserialize(stream, profile == null ? null : profile.DirectedGraphProfile);
            var initialPosition = stream.Position;

            long size = 0;
            var bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            size = size + 4;
            var vertexSize = BitConverter.ToInt32(bytes, 0);
            stream.Read(bytes, 0, 4);
            size = size + 4;
            var edgeSize = BitConverter.ToInt32(bytes, 0);

            var edgeLength = graph.EdgeCount;

            ArrayBase<uint> edges;
            if (profile == null)
            { // just create arrays and read the data.
                edges = Context.ArrayFactory.CreateMemoryBackedArray<uint>(edgeLength * edgeSize);
                edges.CopyFrom(stream);
                size += edgeLength * edgeSize * 4;
            }
            else
            { // create accessors over the exact part of the stream that represents vertices/edges.
                var position = stream.Position;
                var map1 = new MemoryMapStream(new CappedStream(stream, position,
                    edgeLength * edgeSize * 4));
                edges = new Array<uint>(map1.CreateUInt32(edgeLength * edgeSize), profile.EdgeMetaProfile);
                size += edgeLength * edgeSize * 4;
            }

            // make sure stream is positioned at the correct location.
            stream.Seek(initialPosition + size, System.IO.SeekOrigin.Begin);

            return new DirectedMetaGraph(graph, edgeSize, edges);
        }
    }
}