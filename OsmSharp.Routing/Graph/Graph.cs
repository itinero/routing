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
using OsmSharp.IO;
using OsmSharp.IO.MemoryMappedFiles;
using OsmSharp.Math.Geo.Simple;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Graph
{
    /// <summary>
    /// An implementation of an in-memory dynamic graph.
    /// </summary>
    public class Graph<TEdgeData> : GraphBase<TEdgeData>, IDisposable
        where TEdgeData : struct, IGraphEdgeData
    {
        private const int EDGE_SIZE = 4;
        private const uint NO_EDGE = uint.MaxValue;
        private const int NODEA = 0;
        private const int NODEB = 1;
        private const int NEXTNODEA = 2;
        private const int NEXTNODEB = 3;

        /// <summary>
        /// Holds the next id.
        /// </summary>
        private uint _nextVertexId;

        /// <summary>
        /// Holds the next edge id.
        /// </summary>
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
        /// Creates a new in-memory graph.
        /// </summary>
        public Graph()
            : this(1000)
        {

        }

        /// <summary>
        /// Creates a new in-memory graph.
        /// </summary>
        public Graph(long sizeEstimate)
            : this(sizeEstimate, 
            new HugeArray<GeoCoordinateSimple>(sizeEstimate), 
            new HugeArray<uint>(sizeEstimate), 
            new HugeArray<uint>(sizeEstimate * 3 * EDGE_SIZE), 
            new HugeArray<TEdgeData>(sizeEstimate * 3), 
            new HugeCoordinateCollectionIndex(sizeEstimate * 3))
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
        public Graph(
            HugeArrayBase<GeoCoordinateSimple> coordinateArray,
            HugeArrayBase<uint> vertexArray,
            HugeArrayBase<uint> edgesArray,
            HugeArrayBase<TEdgeData> edgeDataArray,
            HugeCoordinateCollectionIndex edgeShapeArray)
        {
            _vertices = vertexArray;
            //_vertices.Resize(sizeEstimate);
            //for (int idx = 0; idx < sizeEstimate; idx++)
            //{
            //    _vertices[idx] = NO_EDGE;
            //}
            _coordinates = coordinateArray;
            //_coordinates.Resize(sizeEstimate);
            _edges = edgesArray;
            //_edges.Resize(sizeEstimate * 3 * EDGE_SIZE);
            //for (int idx = 0; idx < sizeEstimate * 3 * EDGE_SIZE; idx++)
            //{
            //    _edges[idx] = NO_EDGE;
            //}
            _edgeData = edgeDataArray;
            //_edgeData.Resize(sizeEstimate * 3);
            _edgeShapes = edgeShapeArray;
            //_edgeShapes.Resize(sizeEstimate * 3);
            _nextVertexId = (uint)(_vertices.Length);
            _nextEdgeId = (uint)(edgesArray.Length + 1);
        }

        /// <summary>
        /// Creates a new graph using the given arrays.
        /// </summary>
        /// <param name="sizeEstimate"></param>
        /// <param name="coordinateArray"></param>
        /// <param name="vertexArray"></param>
        /// <param name="edgesArray"></param>
        /// <param name="edgeDataArray"></param>
        /// <param name="edgeShapeArray"></param>
        public Graph(long sizeEstimate, 
            HugeArrayBase<GeoCoordinateSimple> coordinateArray, 
            HugeArrayBase<uint> vertexArray, 
            HugeArrayBase<uint> edgesArray, 
            HugeArrayBase<TEdgeData> edgeDataArray, 
            HugeCoordinateCollectionIndex edgeShapeArray)
        {
            _nextVertexId = 1;
            _nextEdgeId = 0;
            _vertices = vertexArray;
            _vertices.Resize(sizeEstimate);
            for (int idx = 0; idx < sizeEstimate; idx++)
            {
                _vertices[idx] = NO_EDGE;
            }
            _coordinates = coordinateArray;
            _coordinates.Resize(sizeEstimate);
            _edges = edgesArray;
            _edges.Resize(sizeEstimate * 3 * EDGE_SIZE);
            for (int idx = 0; idx < sizeEstimate * 3 * EDGE_SIZE; idx++)
            {
                _edges[idx] = NO_EDGE;
            }
            _edgeData = edgeDataArray;
            _edgeData.Resize(sizeEstimate * 3);
            _edgeShapes = edgeShapeArray;
            _edgeShapes.Resize(sizeEstimate * 3);
        }

        /// <summary>
        /// Creates a new memory mapped graph using the given file.
        /// </summary>
        /// <param name="file">The file to store data at.</param>
        /// <param name="estimatedSize">The estimated size.</param>
        /// <param name="mapFrom">Delegate to map edge data from uints.</param>
        /// <param name="mapTo">Delegate to map edge data to uints.</param>
        /// <param name="edgeDataSize">The edge data size in uints.</param>
        public Graph(MemoryMappedFile file, long estimatedSize,
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
        public Graph(long vertexCount, long edgesCount, 
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
            var oldLength = _coordinates.Length;
            _coordinates.Resize(size);
            _vertices.Resize(size);
            for (long idx = oldLength; idx < size; idx++)
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
            // make sure vertices array is large enough.
            if (_nextVertexId >= _vertices.Length)
            {
                this.IncreaseVertexSize();
            }

            // create vertex.
            uint newId = _nextVertexId;
            _coordinates[newId] = new GeoCoordinateSimple()
            {
                Latitude = latitude,
                Longitude = longitude
            };
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
            if (_nextVertexId > id && id > 0)
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
        public override void AddEdge(uint vertex1, uint vertex2, TEdgeData data, ICoordinateCollection coordinates)
        {
            if (vertex1 == vertex2) { throw new ArgumentException("Given vertices must be different."); }
            if (_nextVertexId <= vertex1) { throw new ArgumentOutOfRangeException("vertex1", "vertex1 is not part of this graph."); }
            if (_nextVertexId <= vertex2) { throw new ArgumentOutOfRangeException("vertex2", "vertex2 is not part of this graph."); }

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
                            if (coordinates != null)
                            { // also reverse coordinates!
                                coordinates = coordinates.Reverse();
                            }
                        }
                        _edgeData[previousEdgeId / 4] = data;
                        _edgeShapes[previousEdgeId / 4] = coordinates;
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
                _edgeShapes[edgeId / 4] = coordinates;
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
                _edgeShapes[edgeId / 4] = coordinates;
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
        public override void RemoveEdges(uint vertex)
        {
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
        public override bool RemoveEdge(uint vertex1, uint vertex2)
        {
            if (_nextVertexId <= vertex1) { throw new ArgumentOutOfRangeException("vertex1", "vertex1 is not part of this graph."); }
            if (_nextVertexId <= vertex2) { throw new ArgumentOutOfRangeException("vertex2", "vertex2 is not part of this graph."); }

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
                        _edgeShapes[currentEdgeId / EDGE_SIZE] = null;
                        return true;
                    }
                }
                throw new Exception("Edge could not be reached from vertex2. Data in graph is invalid.");
            }
            return removed;
        }

        /// <summary>
        /// Deletes the edge between the two given vertices.
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="data"></param>
        public override bool RemoveEdge(uint vertex1, uint vertex2, TEdgeData data)
        {
            if (_nextVertexId <= vertex1) { throw new ArgumentOutOfRangeException("vertex1", "vertex1 is not part of this graph."); }
            if (_nextVertexId <= vertex2) { throw new ArgumentOutOfRangeException("vertex2", "vertex2 is not part of this graph."); }

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
                        _edgeShapes[currentEdgeId / EDGE_SIZE] = null;
                        return false;
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
        /// Returns true if the given vertex has neighbour as a neighbour.
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <returns></returns>
        public override bool ContainsEdges(uint vertex1, uint vertex2)
        {
            if (_nextVertexId <= vertex1) { throw new ArgumentOutOfRangeException("vertex1", "vertex1 is not part of this graph."); }
            if (_nextVertexId <= vertex2) { throw new ArgumentOutOfRangeException("vertex2", "vertex2 is not part of this graph."); }

            if (_vertices[vertex1] == NO_EDGE)
            { // no edges here!
                return false;
            }
            var edgeId = _vertices[vertex1];
            uint nextEdgeSlot = 0;
            while (edgeId != NO_EDGE)
            { // keep looping.
                uint otherVertexId = 0;
                if (_edges[edgeId + NODEA] == vertex1)
                {
                    otherVertexId = _edges[edgeId + NODEB];
                    edgeId = _edges[edgeId + NEXTNODEA];
                    nextEdgeSlot = edgeId + NEXTNODEA;
                }
                else
                {
                    otherVertexId = _edges[edgeId + NODEA];
                    edgeId = _edges[edgeId + NEXTNODEB];
                    nextEdgeSlot = edgeId + NEXTNODEB;
                }
                if (otherVertexId == vertex2)
                { // this is the edge we need.
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
        public override bool ContainsEdge(uint vertex1, uint vertex2, TEdgeData data)
        {
            if (_nextVertexId <= vertex1) { throw new ArgumentOutOfRangeException("vertex1", "vertex1 is not part of this graph."); }
            if (_nextVertexId <= vertex2) { throw new ArgumentOutOfRangeException("vertex2", "vertex2 is not part of this graph."); }

            if (_vertices[vertex1] == NO_EDGE)
            { // no edges here!
                return false;
            }
            var edgeId = _vertices[vertex1];
            uint nextEdgeSlot = 0;
            while (edgeId != NO_EDGE)
            { // keep looping.
                uint otherVertexId = 0;
                if (_edges[edgeId + NODEA] == vertex1)
                {
                    otherVertexId = _edges[edgeId + NODEB];
                    edgeId = _edges[edgeId + NEXTNODEA];
                    nextEdgeSlot = edgeId + NEXTNODEA;
                }
                else
                {
                    otherVertexId = _edges[edgeId + NODEA];
                    edgeId = _edges[edgeId + NEXTNODEB];
                    nextEdgeSlot = edgeId + NEXTNODEB;
                }
                if (otherVertexId == vertex2)
                { // this is the edge we need.
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the index associated with the given edge and return true if it exists.
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="edgeDataIdx"></param>
        /// <param name="edgeDataForward"></param>
        /// <returns></returns>
        private bool GetEdgeIdx(uint vertex1, uint vertex2, out long edgeDataIdx, out bool edgeDataForward)
        {
            if (_nextVertexId <= vertex1) { throw new ArgumentOutOfRangeException("vertex1", "vertex1 is not part of this graph."); }
            if (_nextVertexId <= vertex2) { throw new ArgumentOutOfRangeException("vertex2", "vertex2 is not part of this graph."); }

            if (_vertices[vertex1] == NO_EDGE)
            { // no edges here!
                edgeDataIdx = -1;
                edgeDataForward = false;
                return false;
            }
            var edgeId = _vertices[vertex1];
            uint nextEdgeSlot = 0;
            while (edgeId != NO_EDGE)
            { // keep looping.
                uint otherVertexId = 0;
                var currentEdgeId = edgeId;
                edgeDataForward = true;
                if (_edges[edgeId + NODEA] == vertex1)
                {
                    otherVertexId = _edges[edgeId + NODEB];
                    edgeId = _edges[edgeId + NEXTNODEA];
                    nextEdgeSlot = edgeId + NEXTNODEA;
                }
                else
                {
                    otherVertexId = _edges[edgeId + NODEA];
                    edgeId = _edges[edgeId + NEXTNODEB];
                    nextEdgeSlot = edgeId + NEXTNODEB;
                    edgeDataForward = false;
                }
                if (otherVertexId == vertex2)
                { // this is the edge we need.
                    edgeDataIdx = currentEdgeId / EDGE_SIZE;
                    return true;
                }
            }
            edgeDataForward = false;
            edgeDataIdx = -1;
            return false;
        }

        /// <summary>
        /// Returns all edges between the given vertices.
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
        /// Gets the data associated with the given edge and return true if it exists. Throw exception if this graph allows duplicates.
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public override bool GetEdge(uint vertex1, uint vertex2, out TEdgeData data)
        {
            if (this.CanHaveDuplicates) { throw new InvalidOperationException("Cannot use GetEdge on a graph that can have duplicate edges."); }

            long edgeDataIdx;
            bool edgeDataForward;
            if (this.GetEdgeIdx(vertex1, vertex2, out edgeDataIdx, out edgeDataForward))
            { // the edge exists.
                data = _edgeData[edgeDataIdx];
                if (!edgeDataForward)
                { // edge is backward.
                    data = (TEdgeData)data.Reverse();
                }
                return true;
            }
            data = default(TEdgeData);
            return false;
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
            long edgeDataIdx;
            bool edgeDataForward;
            if (this.GetEdgeIdx(vertex1, vertex2, out edgeDataIdx, out edgeDataForward))
            { // the edge exists.
                shape = _edgeShapes[edgeDataIdx];
                if (shape != null && !edgeDataForward)
                { // invert shape.
                    shape = shape.Reverse();
                }
                return true;
            }
            shape = null;
            return false;
        }

        /// <summary>
        /// Trims the internal data structures of this graph.
        /// </summary>
        public override void Trim()
        {
            // resize coordinates/vertices.
            _coordinates.Resize(_nextVertexId);
            // Array.Resize<GeoCoordinateSimple>(ref _coordinates, (int)_nextVertexId);
            _vertices.Resize(_nextVertexId);
            // Array.Resize<uint>(ref _vertices, (int)_nextVertexId);

            // resize edges.
            _edgeData.Resize(_nextEdgeId / EDGE_SIZE);
            _edgeShapes.Resize(_nextEdgeId / EDGE_SIZE);
            _edges.Resize(_nextEdgeId);
        }

        /// <summary>
        /// Resizes the internal data structures of the graph to handle the number of vertices/edges estimated.
        /// </summary>
        /// <param name="vertexEstimate"></param>
        /// <param name="edgeEstimate"></param>
        public override void Resize(long vertexEstimate, long edgeEstimate)
        {
            // resize coordinates/vertices.
            this.IncreaseVertexSize((int)vertexEstimate);

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
        /// Trims the size of this graph to it's smallest possible size.
        /// </summary>
        public override void Compress()
        {
            // trim edges.
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

            // trim vertices.
            uint minUnAllocatedVertexId = 0;
            for (uint vertexId = 0; vertexId < _nextVertexId; vertexId++)
            {
                if (_vertices[vertexId] != NO_EDGE)
                {
                    minUnAllocatedVertexId = vertexId;
                }
            }
            _nextVertexId = minUnAllocatedVertexId + 1;

            this.Trim();

            _edgeShapes.Compress();
        }

        /// <summary>
        /// Moves an edge from one location to another.
        /// </summary>
        /// <param name="oldEdgeId"></param>
        /// <param name="newEdgeId"></param>
        private void MoveEdge(uint oldEdgeId, uint newEdgeId)
        {
            // first copy the data.
            _edges[newEdgeId + NODEA] = _edges[oldEdgeId + NODEA];
            _edges[newEdgeId + NODEB] = _edges[oldEdgeId + NODEB];
            _edges[newEdgeId + NEXTNODEA] = _edges[oldEdgeId + NEXTNODEA];
            _edges[newEdgeId + NEXTNODEB] = _edges[oldEdgeId + NEXTNODEB];
            _edgeData[newEdgeId / EDGE_SIZE] = _edgeData[oldEdgeId / EDGE_SIZE];
            _edgeShapes[newEdgeId / EDGE_SIZE] = _edgeShapes[oldEdgeId / EDGE_SIZE];

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
            _edgeShapes[oldEdgeId / EDGE_SIZE] = null;
        }

        /// <summary>
        /// Represents the internal edge enumerator.
        /// </summary>
        class EdgeEnumerator : EdgeEnumerator<TEdgeData>
        {
            /// <summary>
            /// Holds the graph.
            /// </summary>
            private Graph<TEdgeData> _graph;

            /// <summary>
            /// Holds the next edgeId.
            /// </summary>
            private uint _nextEdgeId;

            /// <summary>
            /// Holds the current edgeId.
            /// </summary>
            private uint _currentEdgeId;

            /// <summary>
            /// Holds the direction flag for the current edge.
            /// </summary>
            private bool _currentEdgeInverted = false;

            /// <summary>
            /// Holds the current vertex.
            /// </summary>
            private uint _vertex;

            /// <summary>
            /// Holds the first start vertex.
            /// </summary>
            private uint _startVertex1;

            /// <summary>
            /// Holds the second start vertex if any (0 otherwise).
            /// </summary>
            private uint _startVertex2;

            /// <summary>
            /// Holds the start edge.
            /// </summary>
            private uint _startEdge;

            /// <summary>
            /// Creates a new edge enumerator.
            /// </summary>
            /// <param name="graph"></param>
            /// <param name="edgeId"></param>
            /// <param name="vertex1"></param>
            public EdgeEnumerator(Graph<TEdgeData> graph)
            {
                _graph = graph;
                _currentEdgeId = 0;
                _vertex = 0;

                _startVertex1 = 0;
                _startVertex2 = 0;
                _startEdge = 0;
                _currentEdgeInverted = false;
            }

            /// <summary>
            /// Creates a new edge enumerator.
            /// </summary>
            /// <param name="graph"></param>
            /// <param name="edgeId"></param>
            /// <param name="vertex1"></param>
            /// <param name="vertex2"></param>
            public EdgeEnumerator(Graph<TEdgeData> graph, uint edgeId, uint vertex1, uint vertex2)
            {
                _graph = graph;
                _nextEdgeId = edgeId;
                _currentEdgeId = 0;
                _vertex = vertex1;

                _startVertex1 = vertex1;
                _startVertex2 = vertex2;
                _startEdge = edgeId;
                _currentEdgeInverted = false;
            }

            /// <summary>
            /// Move to the next edge.
            /// </summary>
            /// <returns></returns>
            public override bool MoveNext()
            {
                if(_nextEdgeId != NO_EDGE)
                { // there is a next edge.
                    do
                    { // if search for specific edges, keep looping until found.
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
                    } while (_startVertex2 != 0 && _neighbour != _startVertex2);
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Holds the current neigbour.
            /// </summary>
            private uint _neighbour;

            /// <summary>
            /// Returns the current neighbour.
            /// </summary>
            public override uint Neighbour
            {
                get { return _neighbour; }
            }

            /// <summary>
            /// Returns the current edge data.
            /// </summary>
            public override TEdgeData EdgeData
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
            public override bool isInverted
            {
                get { return _currentEdgeInverted; }
            }

            /// <summary>
            /// Returns the inverted edge data.
            /// </summary>
            public override TEdgeData InvertedEdgeData
            {
                get
                {
                    if (!_currentEdgeInverted)
                    {
                        return (TEdgeData)_graph._edgeData[_currentEdgeId / 4].Reverse();
                    }
                    return _graph._edgeData[_currentEdgeId / 4];
                }
            }

            /// <summary>
            /// Returns the current intermediates.
            /// </summary>
            public override ICoordinateCollection Intermediates
            {

                get
                {
                    if(_currentEdgeInverted)
                    {
                        var intermediates = _graph._edgeShapes[_currentEdgeId / 4];
                        if (intermediates != null)
                        {
                            return intermediates.Reverse();
                        }
                        return null;
                    }
                    return _graph._edgeShapes[_currentEdgeId / 4];
                }
            }

            /// <summary>
            /// Returns the count.
            /// </summary>
            /// <returns></returns>
            public override int Count
            {
                get
                {
                    int count = 0;
                    while (this.MoveNext())
                    {
                        count++;
                    }
                    return count;
                }
            }

            /// <summary>
            /// Resets this enumerator.
            /// </summary>
            public override void Reset()
            {
                _nextEdgeId = _startEdge;
                _currentEdgeId = 0;
                _vertex = _startVertex1;
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
                get { return false; }
            }

            /// <summary>
            /// Moves this enumerator to the given vertex.
            /// </summary>
            /// <param name="vertex"></param>
            public override void MoveTo(uint vertex)
            {
                var edgeId = _graph._vertices[vertex];
                _nextEdgeId = edgeId;
                _currentEdgeId = 0;
                _vertex = vertex;

                _startVertex1 = vertex;
                _startVertex2 = 0;
                _startEdge = edgeId;
                _currentEdgeInverted = false;
            }

            /// <summary>
            /// Moves this enumerator to the given vertices.
            /// </summary>
            /// <param name="vertex1"></param>
            /// <param name="vertex2"></param>
            public override void MoveTo(uint vertex1, uint vertex2)
            {
                var edgeId = _graph._vertices[vertex1];
                _nextEdgeId = edgeId;
                _currentEdgeId = 0;
                _vertex = vertex1;

                _startVertex1 = vertex1;
                _startVertex2 = vertex2;
                _startEdge = edgeId;
                _currentEdgeInverted = false;
            }
        }

        /// <summary>
        /// Returns true if the graph is directed.
        /// </summary>
        public override bool IsDirected
        {
            get { return false; }
        }

        /// <summary>
        /// Returns true if this graph can have duplicate edges.
        /// </summary>
        public override bool CanHaveDuplicates
        {
            get { return false; }
        }

        /// <summary>
        /// Sorts the graph based on the given transformations.
        /// </summary>
        public override void Sort(HugeArrayBase<uint> transformations)
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
                    var temp = _coordinates[i];
                    _coordinates[i] = _coordinates[j];
                    _coordinates[j] = temp;

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
            _coordinates.Dispose();
            _coordinates = null;
            _edges.Dispose();
            _edges = null;
            _edgeData.Dispose();
            _edgeData = null;
            _vertices.Dispose();
            _vertices = null;
            _edgeShapes.Dispose();
            _edgeShapes = null;
        }

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
            stream.Write(BitConverter.GetBytes((int)2), 0, 4);
            position = position + 4;
            stream.Write(BitConverter.GetBytes(vertexCount), 0, 8); // write exact number of vertices.
            position = position + 8;
            stream.Write(BitConverter.GetBytes(edgeCount), 0, 8); // write exact number of edges.
            position = position + 8;

            // write in this order: vertices, vertexCoordinates, edges, edgeData, edgeShapes.
            using (var file = new MemoryMappedStream(new LimitedStream(stream)))
            {
                // write vertices (each vertex = 1 uint (4 bytes)).
                var vertexArray = new MemoryMappedHugeArrayUInt32(file, vertexCount + 1, vertexCount + 1, 1024);
                vertexArray.CopyFrom(_vertices, 0, 0, vertexCount + 1);
                vertexArray.Dispose(); // written, get rid of it!
                position = position + ((vertexCount + 1) * 4);

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
                position = position + ((vertexCount + 1) * 8);

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

            // write shapes.
            stream.Seek(position, System.IO.SeekOrigin.Begin);
            position = position + _edgeShapes.Serialize(new LimitedStream(stream));
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
        public new static Graph<TEdgeData> Deserialize(System.IO.Stream stream, int edgeDataSize, MappedHugeArray<TEdgeData, uint>.MapFrom mapFrom,
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

            var bufferSize = 128;
            var cacheSize = 64 * 8;
            var file = new MemoryMappedStream(new LimitedStream(stream));
            var vertexArray = new MemoryMappedHugeArrayUInt32(file, (vertexLength + 1), vertexLength + 1, bufferSize, cacheSize);
            position = position + ((vertexLength + 1) * 4);
            var vertexCoordinateArray = new MappedHugeArray<GeoCoordinateSimple, float>(
                new MemoryMappedHugeArraySingle(file, (vertexLength + 1) * 2, (vertexLength + 1) * 2, bufferSize, cacheSize),
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
            position = position + ((vertexLength + 1) * 8);
            var edgeArray = new MemoryMappedHugeArrayUInt32(file, edgeLength * 4, edgeLength * 4, bufferSize, cacheSize * 16);
            position = position + (edgeLength * 4 * 4);
            var edgeDataArray = new MappedHugeArray<TEdgeData, uint>(
                new MemoryMappedHugeArrayUInt32(file, edgeLength * edgeDataSize, edgeLength * edgeDataSize, bufferSize, cacheSize * 32), edgeDataSize, mapTo, mapFrom);
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

                return new Graph<TEdgeData>(vertexCoordinateArrayCopy, vertexArrayCopy, edgeArrayCopy, edgeDataArrayCopy, shapes);
            }

            return new Graph<TEdgeData>(vertexCoordinateArray, vertexArray, edgeArray, edgeDataArray, shapes);
        }
    }
}