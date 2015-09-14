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
using OsmSharp.IO;
using OsmSharp.IO.MemoryMappedFiles;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Graphs.Directed
{
    /// <summary>
    /// A direct graph with extra meta-data per edge.
    /// </summary>
    public class DirectedMetaGraph
    {
        private readonly DirectedGraph _graph;
        private readonly HugeArrayBase<uint> _edgeData;
        private readonly int _edgeDataSize = int.MaxValue;
        private const int BLOCK_SIZE = 1000;

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
            _edgeData = new HugeArray<uint>(_edgeDataSize * _graph.EdgeCount);
        }

        /// <summary>
        /// Creates a new graph based on existing data.
        /// </summary>
        private DirectedMetaGraph(DirectedGraph graph, int edgeDataSize, HugeArrayBase<uint> edgeData)
        {
            _graph = graph;
            _edgeData = edgeData;
            _edgeDataSize = edgeDataSize;
        }

        /// <summary>
        /// Switched two edges.
        /// </summary>
        private void SwitchEdge(uint oldId, uint newId)
        {
            var oldEdgePointer = oldId * _edgeDataSize;
            var newEdgePointer = newId * _edgeDataSize;
            for (var i = 0; i < _edgeDataSize; i++)
            {
                _edgeData[newEdgePointer + i] = _edgeData[oldEdgePointer + i];
            }
        }

        /// <summary>
        /// Increase edge data size to fit at least the given edge.
        /// </summary>
        private void IncreaseSizeEdgeData(uint edgePointer)
        {
            var size = _edgeData.Length;
            while (edgePointer >= size)
            {
                size += BLOCK_SIZE;
            }
            _edgeData.Resize(size);
        }

        /// <summary>
        /// Adds a new edge.
        /// </summary>
        /// <returns></returns>
        public uint AddEdge(uint vertex1, uint vertex2, uint[] data, params uint[] metaData)
        {
            var edgeId = _graph.AddEdge(vertex1, vertex2, data);
            if (edgeId >= _edgeData.Length)
            {
                this.IncreaseSizeEdgeData(edgeId);
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
            _graph.Compress();
            _edgeData.Resize(_graph.EdgeCount);
        }

        /// <summary>
        /// Resizes the internal data structures to their smallest size possible.
        /// </summary>
        public void Trim()
        {
            _graph.Trim();
            _edgeData.Resize(_graph.EdgeCount);
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
        public class EdgeEnumerator : IEnumerable<MetaEdge>, IEnumerator<MetaEdge>
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
        /// Serializes to a stream.
        /// </summary>
        public long Serialize(System.IO.Stream stream)
        {
            this.Compress();

            var size = _graph.Serialize(stream);

            stream.Write(BitConverter.GetBytes(0), 0, 4); // write the vertex size.
            size = size + 4;
            stream.Write(BitConverter.GetBytes(_edgeDataSize), 0, 4); // write the edge size.
            size = size + 4;

            var edgeCount = _graph.EdgeCount;
            var edgeSize = 1;
            using (var file = new OsmSharp.IO.MemoryMappedFiles.MemoryMappedStream(
                new OsmSharp.IO.LimitedStream(stream)))
            {
                // write edges (each edge = 4 uints (16 bytes)).
                var edgeArray = new MemoryMappedHugeArrayUInt32(file, edgeCount * edgeSize, edgeCount * edgeSize, 1024);
                edgeArray.CopyFrom(_edgeData, edgeCount * edgeSize);
                edgeArray.Dispose(); // written, get rid of it!
                size = size + (edgeCount * 4 * edgeSize);
            }
            return size;
        }

        /// <summary>
        /// Deserializes from a stream.
        /// </summary>
        /// <returns></returns>
        public static DirectedMetaGraph Deserialize(System.IO.Stream stream, bool copy)
        {
            var graph = DirectedGraph.Deserialize(stream, copy);

            long position = 0;
            var bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            position = position + 4;
            var vertexSize = BitConverter.ToInt32(bytes, 0);
            stream.Read(bytes, 0, 4);
            position = position + 4;
            var edgeSize = BitConverter.ToInt32(bytes, 0);

            var edgeCount = graph.EdgeCount;

            var bufferSize = 128;
            var cacheSize = 64 * 8;
            var file = new MemoryMappedStream(new LimitedStream(stream));
            var edgeData = new MemoryMappedHugeArrayUInt32(file, edgeCount * edgeSize, edgeCount * edgeSize, bufferSize,
                cacheSize * 16);

            return new DirectedMetaGraph(graph, edgeSize, edgeData);
        }
    }
}