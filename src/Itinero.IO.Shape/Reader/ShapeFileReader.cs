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

using Itinero.Algorithms;
using System.Collections.Generic;
using System;
using NetTopologySuite.Geometries;
using Itinero.Logging;
using Itinero.LocalGeo;
using Itinero.Attributes;
using Itinero.Algorithms.Search.Hilbert;
using Itinero.Data.Network;
using Itinero.Profiles;
using System.Linq;
using System.Threading;
using NetTopologySuite.IO;

namespace Itinero.IO.Shape.Reader
{
    /// <summary>
    /// A reader that reads shapefile(s) and builds a routing network.
    /// </summary>
    public class ShapefileReader : AlgorithmBase
    {
        private readonly IList<ShapefileDataReader> _shapefileReaders;
        private readonly RouterDb _routerDb;
        private readonly Vehicle[] _vehicles;
        private readonly VehicleCache _vehicleCache;
        private readonly string _sourceVertexColumn;
        private readonly string _targetVertexColumn;

        /// <summary>
        /// Creates a new reader.
        /// </summary>
        public ShapefileReader(RouterDb routerDb, IList<ShapefileDataReader> shapefileReaders, Vehicle[] vehicles, string sourceVertexColumn, string targetVertexColumn)
        {
            _routerDb = routerDb;
            _shapefileReaders = shapefileReaders;
            _vehicles = vehicles;
            _sourceVertexColumn = sourceVertexColumn;
            _targetVertexColumn = targetVertexColumn;

            _vehicleCache = new VehicleCache(vehicles);

            if (string.IsNullOrEmpty(_sourceVertexColumn) && 
                string.IsNullOrEmpty(_targetVertexColumn))
            { // check for these in the vehicle(s).
                foreach (var vehicle in vehicles)
                {
                    var profileSourceVertex = string.Empty;
                    var profileTargetVertex = string.Empty;

                    if (vehicle.Parameters != null &&
                        vehicle.Parameters.TryGetValue("source_vertex", out profileSourceVertex) &&
                        vehicle.Parameters.TryGetValue("target_vertex", out profileTargetVertex))
                    {
                        if (string.IsNullOrWhiteSpace(_sourceVertexColumn))
                        {
                            _sourceVertexColumn = profileSourceVertex;
                        }
                        else if (_sourceVertexColumn != profileSourceVertex)
                        {
                            throw new Exception(string.Format(
                                "Cannot configure shapefile reader: Multiple vehicle definitions but different source vertex column defined: {0} and {1} found.",
                                    _sourceVertexColumn, profileSourceVertex));
                        }

                        if (string.IsNullOrWhiteSpace(_targetVertexColumn))
                        {
                            _targetVertexColumn = profileTargetVertex;
                        }
                        else if (_targetVertexColumn != profileTargetVertex)
                        {
                            throw new Exception(string.Format(
                                "Cannot configure shapefile reader: Multiple vehicle definitions but different target vertex column defined: {0} and {1} found.",
                                    _targetVertexColumn, profileTargetVertex));
                        }
                    }
                }
            }
        }

        private const long PointInterval = 10000;
        private const long LineStringInterval = 10000;
        
        private int _points;
        private int _lineStrings;

        /// <summary>
        /// Executes the actual algorithm.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            foreach (var vehicle in _vehicles)
            {
                _routerDb.AddSupportedVehicle(vehicle);
            }

            var nodeToVertex = new Dictionary<long, uint>();

            // read all vertices.
            var startTicks = DateTime.Now.Ticks;
            for (int readerIdx = 0; readerIdx < _shapefileReaders.Count; readerIdx++)
            {
                var reader = _shapefileReaders[readerIdx];
                var header = new Dictionary<string, int>();

                // make sure the header is loaded.
                if (header.Count == 0)
                { // build header.
                    for (int idx = 0; idx < reader.DbaseHeader.Fields.Length; idx++)
                    {
                        header.Add(reader.DbaseHeader.Fields[idx].Name, idx + 1);
                    }

                    // check if all columns are in the header.
                    if (!header.ContainsKey(_sourceVertexColumn))
                    { // no node from column.
                        throw new InvalidOperationException(string.Format("No column with name {0} found.", _sourceVertexColumn));
                    }
                    if (!header.ContainsKey(_targetVertexColumn))
                    { // no node to column.
                        throw new InvalidOperationException(string.Format("No column with name {0} found.", _targetVertexColumn));
                    }
                }

                // read all vertices.
                double latestProgress = 0;
                int current = 0;
                while (reader.Read())
                {
                    _points += 2;

                    // get the geometry.
                    var lineString = reader.Geometry as LineString;

                    // read nodes
                    long fromId = reader.GetInt64(header[_sourceVertexColumn]);
                    if (!nodeToVertex.ContainsKey(fromId))
                    { // the node has not been processed yet.
                        var vertexId = _routerDb.Network.VertexCount;
                        _routerDb.Network.AddVertex(vertexId,
                                (float)lineString.Coordinates[0].Y,
                                (float)lineString.Coordinates[0].X);
                        nodeToVertex.Add(fromId, vertexId);
                    }

                    long toId = reader.GetInt64(header[_targetVertexColumn]);
                    if (!nodeToVertex.ContainsKey(toId))
                    { // the node has not been processed yet.
                        var vertexId = _routerDb.Network.VertexCount;
                        _routerDb.Network.AddVertex(vertexId,
                            (float)lineString.Coordinates[lineString.Coordinates.Length - 1].Y,
                            (float)lineString.Coordinates[lineString.Coordinates.Length - 1].X);
                        nodeToVertex.Add(toId, vertexId);
                    }

                    // report progress.
                    float progress = (float)System.Math.Round((((double)current / (double)reader.RecordCount) * 100));
                    current++;
                    if (progress != latestProgress)
                    {
                        var pointSpan = new TimeSpan(DateTime.Now.Ticks - startTicks);
                        var pointPerSecond = System.Math.Round((double)_points / pointSpan.TotalSeconds, 0);
                        Itinero.Logging.Logger.Log("ShapeFileReader", TraceEventType.Information,
                            "Reading vertices from file {1}/{2}... {0}% @ {3}/s", progress, readerIdx + 1, _shapefileReaders.Count, pointPerSecond);
                        latestProgress = progress;
                    }
                }
            }

            // read all edges.
            startTicks = DateTime.Now.Ticks;
            var attributes = new AttributeCollection();
            for (int readerIdx = 0; readerIdx < _shapefileReaders.Count; readerIdx++)
            {
                var reader = _shapefileReaders[readerIdx];
                var header = new Dictionary<string, int>();

                // make sure the header is loaded.
                if (header.Count == 0)
                { // build header.
                    for (int idx = 0; idx < reader.DbaseHeader.Fields.Length; idx++)
                    {
                        header.Add(reader.DbaseHeader.Fields[idx].Name, idx + 1);
                    }
                }

                // reset reader and read all edges/arcs.
                double latestProgress = 0;
                int current = 0;
                reader.Reset();
                while (reader.Read())
                {
                    _lineStrings++;

                    // get the geometry.
                    var lineString = reader.Geometry as LineString;

                    // read nodes
                    long vertex1Shape = reader.GetInt64(header[_sourceVertexColumn]);
                    long vertex2Shape = reader.GetInt64(header[_targetVertexColumn]);
                    uint vertex1, vertex2;
                    if (nodeToVertex.TryGetValue(vertex1Shape, out vertex1) &&
                        nodeToVertex.TryGetValue(vertex2Shape, out vertex2))
                    { // the node has not been processed yet.
                        // add intermediates.
                        var intermediates = new List<Coordinate>(lineString.Coordinates.Length);
                        for (int i = 1; i < lineString.Coordinates.Length - 1; i++)
                        {
                            intermediates.Add(new Coordinate()
                            {
                                Latitude = (float)lineString.Coordinates[i].Y,
                                Longitude = (float)lineString.Coordinates[i].X
                            });
                        }

                        // calculate the distance.
                        float distance = 0;
                        float latitudeFrom, latitudeTo, longitudeFrom, longitudeTo;
                        if (_routerDb.Network.GetVertex(vertex1, out latitudeFrom, out longitudeFrom) &&
                            _routerDb.Network.GetVertex(vertex2, out latitudeTo, out longitudeTo))
                        { // calculate distance.
                            var fromLocation = new Coordinate(latitudeFrom, longitudeFrom);
                            for (int i = 0; i < intermediates.Count; i++)
                            {
                                var currentLocation = new Coordinate(intermediates[i].Latitude, intermediates[i].Longitude);
                                distance = distance + Coordinate.DistanceEstimateInMeter(fromLocation, currentLocation);
                                fromLocation = currentLocation;
                            }
                            var toLocation = new Coordinate(latitudeTo, longitudeTo);
                            distance = distance + Coordinate.DistanceEstimateInMeter(fromLocation, toLocation);
                        }

                        // get profile and meta attributes.
                        var profile = new AttributeCollection();
                        var meta = new AttributeCollection();
                        var profileWhiteList = new Whitelist();
                        attributes.Clear();
                        reader.AddToAttributeCollection(attributes);
                        _vehicleCache.AddToWhiteList(attributes, profileWhiteList);
                        for (var i = 1; i < reader.FieldCount; i++)
                        {
                            var name = reader.GetName(i);
                            var value = reader.GetValue(i);
                            var valueString = string.Empty;
                            if (value != null)
                            { 
                                valueString = value.ToInvariantString();
                            }

                            if (profileWhiteList.Contains(name) ||
                                _vehicles.IsOnProfileWhiteList(name))
                            {
                                profile.AddOrReplace(name, valueString);
                            }
                            else if(_vehicles.IsOnMetaWhiteList(name))
                            {
                                meta.AddOrReplace(name, valueString);
                            }
                        }

                        // add edge.
                        var profileId = _routerDb.EdgeProfiles.Add(profile);
                        if (profileId >= Data.Edges.EdgeDataSerializer.MAX_PROFILE_COUNT)
                        {
                            throw new Exception("Maximum supported profiles exeeded, make sure only routing attributes are included in the profiles.");
                        }
                        var metaId = _routerDb.EdgeMeta.Add(meta);
                        if (vertex1 != vertex2)
                        {
                            if (distance > _routerDb.Network.MaxEdgeDistance)
                            { // edge is too long to fit into the network, adding an itermediate vertex.
                                var shape = intermediates;
                                if (shape == null)
                                { // make sure there is a shape.
                                    shape = new List<Coordinate>();
                                }

                                shape = new List<Coordinate>(shape);
                                shape.Insert(0, _routerDb.Network.GetVertex(vertex1));
                                shape.Add(_routerDb.Network.GetVertex(vertex2));

                                var tooBig = true;
                                while (tooBig)
                                {
                                    tooBig = false;
                                    for (var s = 1; s < shape.Count; s++)
                                    {
                                        var localDistance = Coordinate.DistanceEstimateInMeter(shape[s - 1], shape[s]);
                                        if (localDistance >= _routerDb.Network.MaxEdgeDistance)
                                        { // insert a new intermediate.
                                            shape.Insert(s,
                                                new Coordinate()
                                                {
                                                    Latitude = (float)(((double)shape[s - 1].Latitude +
                                                        (double)shape[s].Latitude) / 2.0),
                                                    Longitude = (float)(((double)shape[s - 1].Longitude +
                                                        (double)shape[s].Longitude) / 2.0),
                                                });
                                            tooBig = true;
                                            s--;
                                        }
                                    }
                                }

                                var i = 0;
                                var shortShape = new List<Coordinate>();
                                var shortDistance = 0.0f;
                                uint shortVertex = Constants.NO_VERTEX;
                                Coordinate? shortPoint;
                                i++;
                                while (i < shape.Count)
                                {
                                    var localDistance = Coordinate.DistanceEstimateInMeter(shape[i - 1], shape[i]);
                                    if (localDistance + shortDistance > _routerDb.Network.MaxEdgeDistance)
                                    { // ok, previous shapepoint was the maximum one.
                                        shortPoint = shortShape[shortShape.Count - 1];
                                        shortShape.RemoveAt(shortShape.Count - 1);

                                        // add vertex.            
                                        shortVertex = _routerDb.Network.VertexCount;
                                        _routerDb.Network.AddVertex(shortVertex, shortPoint.Value.Latitude,
                                            shortPoint.Value.Longitude);

                                        // add edge.
                                        _routerDb.Network.AddEdge(vertex1, shortVertex, new Data.Network.Edges.EdgeData()
                                        {
                                            Distance = (float)shortDistance,
                                            MetaId = metaId,
                                            Profile = (ushort)profileId
                                        }, shortShape);
                                        vertex1 = shortVertex;

                                        // set new short distance, empty shape.
                                        shortShape.Clear();
                                        shortShape.Add(shape[i]);
                                        shortDistance = localDistance;
                                        i++;
                                    }
                                    else
                                    { // just add short distance and move to the next shape point.
                                        shortShape.Add(shape[i]);
                                        shortDistance += localDistance;
                                        i++;
                                    }
                                }

                                // add final segment.
                                if (shortShape.Count > 0)
                                {
                                    shortShape.RemoveAt(shortShape.Count - 1);
                                }

                                // add edge.
                                _routerDb.Network.AddEdge(vertex1, vertex2, new Data.Network.Edges.EdgeData()
                                {
                                    Distance = (float)shortDistance,
                                    MetaId = metaId,
                                    Profile = (ushort)profileId
                                }, shortShape);
                            }
                            else
                            {
                                this.AddEdge(vertex1, vertex2, new Data.Network.Edges.EdgeData()
                                {
                                    Distance = distance,
                                    MetaId = metaId,
                                    Profile = (ushort)profileId
                                }, intermediates);
                            }
                        }
                    }

                    // report progress.
                    float progress = (float)System.Math.Round((((double)current / (double)reader.RecordCount) * 100));
                    current++;
                    if (progress != latestProgress)
                    {
                        var span = new TimeSpan(DateTime.Now.Ticks - startTicks);
                        var perSecond = System.Math.Round((double)_lineStrings / span.TotalSeconds, 0);
                        Itinero.Logging.Logger.Log("ShapeFileReader", TraceEventType.Information,
                            "Reading edges {1}/{2}... {0}% @ {3}/s", progress, readerIdx + 1, _shapefileReaders.Count, perSecond);
                        latestProgress = progress;
                    }
                }
            }

            // sort the network.
            Itinero.Logging.Logger.Log("ShapeFileReader", TraceEventType.Information, "Sorting vertices...");
            _routerDb.Sort();

            this.HasSucceeded = true;
        }
        
        /// <summary>
        /// Adds a new edge.
        /// </summary>
        private void AddEdge(uint vertex1, uint vertex2, Data.Network.Edges.EdgeData edgeData, List<Coordinate> intermediates)
        {
            var edge = _routerDb.Network.GetEdgeEnumerator(vertex1).FirstOrDefault(x => x.To == vertex2);
            if (edge == null)
            { // everything is fine, no edge exists yet.
                _routerDb.Network.AddEdge(vertex1, vertex2, edgeData, new Graphs.Geometric.Shapes.ShapeEnumerable(intermediates));
            }
            else
            { // an edge exists already, split the current one or the one that exists already.
                var meta = edgeData.MetaId;
                var profile = edgeData.Profile;
                var distance = edgeData.Distance;

                if (edge.Data.Distance == edgeData.Distance &&
                                edge.Data.Profile == edgeData.Profile &&
                                edge.Data.MetaId == edgeData.MetaId)
                {
                    // do nothing, identical duplicate data.
                }
                else
                { // try and use intermediate points if any.
                  // try and use intermediate points.
                    var splitMeta = edgeData.MetaId;
                    var splitProfile = edgeData.Profile;
                    var splitDistance = edgeData.Distance;
                    if (intermediates.Count == 0 &&
                        edge != null &&
                        edge.Shape != null)
                    { // no intermediates in current edge.
                      // save old edge data.
                        intermediates = new List<Coordinate>(edge.Shape);
                        vertex1 = edge.From;
                        vertex2 = edge.To;
                        splitMeta = edge.Data.MetaId;
                        splitProfile = edge.Data.Profile;
                        splitDistance = edge.Data.Distance;

                        // just add edge, and split the other one.
                        _routerDb.Network.RemoveEdges(vertex1, vertex2); // make sure to overwrite and not add an extra edge.
                        _routerDb.Network.AddEdge(vertex1, vertex2, new Data.Network.Edges.EdgeData()
                        {
                            MetaId = meta,
                            Distance = System.Math.Max(distance, 0.0f),
                            Profile = (ushort)profile
                        }, null);
                    }

                    if (intermediates.Count > 0)
                    { // intermediates found, use the first intermediate as the core-node.
                        var newCoreVertex = _routerDb.Network.VertexCount;
                        _routerDb.Network.AddVertex(newCoreVertex, intermediates[0].Latitude, intermediates[0].Longitude);

                        // calculate new distance and update old distance.
                        var newDistance = Coordinate.DistanceEstimateInMeter(
                            _routerDb.Network.GetVertex(vertex1), intermediates[0]);
                        splitDistance -= newDistance;

                        // add first part.
                        _routerDb.Network.AddEdge(vertex1, newCoreVertex, new Data.Network.Edges.EdgeData()
                        {
                            MetaId = splitMeta,
                            Distance = System.Math.Max(newDistance, 0.0f),
                            Profile = (ushort)splitProfile
                        }, null);

                        // add second part.
                        intermediates.RemoveAt(0);
                        _routerDb.Network.AddEdge(newCoreVertex, vertex2, new Data.Network.Edges.EdgeData()
                        {
                            MetaId = splitMeta,
                            Distance = System.Math.Max(splitDistance, 0.0f),
                            Profile = (ushort)splitProfile
                        }, intermediates);
                    }
                    else
                    { // no intermediate or shapepoint found in either one. two identical edge overlayed with different profiles.
                      // add two other vertices with identical positions as the ones given.
                      // connect them with an edge of length '0'.
                        var fromLocation = _routerDb.Network.GetVertex(vertex1);
                        var newFromVertex = _routerDb.Network.VertexCount;
                        _routerDb.Network.AddVertex(newFromVertex, fromLocation.Latitude, fromLocation.Longitude);
                        _routerDb.Network.AddEdge(vertex1, newFromVertex, new Data.Network.Edges.EdgeData()
                        {
                            Distance = 0,
                            MetaId = splitMeta,
                            Profile = (ushort)splitProfile
                        }, null);
                        var toLocation = _routerDb.Network.GetVertex(vertex2);
                        var newToVertex = _routerDb.Network.VertexCount;
                        _routerDb.Network.AddVertex(newToVertex, toLocation.Latitude, toLocation.Longitude);
                        _routerDb.Network.AddEdge(newToVertex, vertex1, new Data.Network.Edges.EdgeData()
                        {
                            Distance = 0,
                            MetaId = splitMeta,
                            Profile = (ushort)splitProfile
                        }, null);

                        _routerDb.Network.AddEdge(newFromVertex, newToVertex, new Data.Network.Edges.EdgeData()
                        {
                            Distance = splitDistance,
                            MetaId = splitMeta,
                            Profile = (ushort)splitProfile
                        }, null);
                    }
                }
            }
        }
    }
}