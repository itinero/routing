// Itinero - Routing for .NET
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

using Itinero.Algorithms;
using NetTopologySuite.IO;
using System.Collections.Generic;
using System;
using NetTopologySuite.Geometries;
using Itinero.Logging;
using Itinero.LocalGeo;
using Itinero.IO.Shape.Vehicles;
using Itinero.Attributes;

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
        }

        /// <summary>
        /// Executes the actual algorithm.
        /// </summary>
        protected override void DoRun()
        {
            var nodeToVertex = new Dictionary<long, uint>();

            // read all vertices.
            for (int readerIdx = 0; readerIdx < _shapefileReaders.Count; readerIdx++)
            {
                var reader = _shapefileReaders[readerIdx];
                var header = new Dictionary<string, int>();

                // make sure the header is loaded.
                if (header.Count == 0)
                { // build header.
                    for (int idx = 0; idx < reader.DbaseHeader.Fields.Length; idx++)
                    {
                        header.Add(reader.DbaseHeader.Fields[idx].Name, idx);
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
                        Itinero.Logging.Logger.Log("ShapefileLiveGraphReader", TraceEventType.Information,
                            "Reading vertices from file {1}/{2}... {0}%", progress, readerIdx + 1, _shapefileReaders.Count);
                        latestProgress = progress;
                    }
                }
            }

            // read all edges.
            for (int readerIdx = 0; readerIdx < _shapefileReaders.Count; readerIdx++)
            {
                var reader = _shapefileReaders[readerIdx];
                var header = new Dictionary<string, int>();

                // make sure the header is loaded.
                if (header.Count == 0)
                { // build header.
                    for (int idx = 0; idx < reader.DbaseHeader.Fields.Length; idx++)
                    {
                        header.Add(reader.DbaseHeader.Fields[idx].Name, idx);
                    }
                }

                // reset reader and read all edges/arcs.
                double latestProgress = 0;
                int current = 0;
                reader.Reset();
                while (reader.Read())
                {
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
                        foreach (var field in reader.DbaseHeader.Fields)
                        {
                            var valueString = string.Empty;
                            var value = reader[field.Name];
                            if (value != null)
                            { // TODO: make sure this is culture-invariant!
                                valueString = value.ToInvariantString();
                            }

                            if (_vehicles.IsRelevantForProfileAny(field.Name))
                            {
                                profile.AddOrReplace(field.Name, valueString);
                            }
                            else
                            {
                                meta.AddOrReplace(field.Name, valueString);
                            }
                        }

                        // add edge.
                        var profileId = _routerDb.EdgeProfiles.Add(profile);
                        var metaId = _routerDb.EdgeMeta.Add(meta);
                        if (vertex1 != vertex2)
                        {
                            _routerDb.Network.AddEdge(vertex1, vertex2, new Data.Network.Edges.EdgeData()
                            {
                                Distance = distance,
                                MetaId = metaId,
                                Profile = (ushort)profileId
                            }, new Graphs.Geometric.Shapes.ShapeEnumerable(intermediates));
                        }
                    }

                    // report progress.
                    float progress = (float)System.Math.Round((((double)current / (double)reader.RecordCount) * 100));
                    current++;
                    if (progress != latestProgress)
                    {
                        Itinero.Logging.Logger.Log("ShapefileLiveGraphReader", TraceEventType.Information,
                            "Reading edges {1}/{2}... {0}%", progress, readerIdx + 1, _shapefileReaders.Count);
                        latestProgress = progress;
                    }
                }
            }

            this.HasSucceeded = true;
        }
    }
}