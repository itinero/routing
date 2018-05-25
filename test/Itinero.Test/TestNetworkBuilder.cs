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

using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Itinero.Data.Network;
using Itinero.LocalGeo;
using System.Collections.Generic;
using System.IO;
using Itinero.Geo;
using Itinero.Data.Network.Restrictions;
using Itinero.Attributes;

namespace Itinero.Test
{
    /// <summary>
    /// Builds test networks based on geojson files.
    /// </summary>
    public static class TestNetworkBuilder
    {
        private const float Tolerance = 20; // 10 meter.

        /// <summary>
        /// Loads a test network.
        /// </summary>
        public static void LoadTestNetwork(this RouterDb db, Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            {
                db.LoadTestNetwork(streamReader.ReadToEnd());
            }
        }

        /// <summary>
        /// Loads a test network from geojson.
        /// </summary>
        public static void LoadTestNetwork(this RouterDb db, string geoJson, float tolerance = 20)
        {
            var nodeIds = db.VertexData.AddInt64("node_id");
            var edgeIds = db.EdgeData.AddInt64("edge_id");

            var geoJsonReader = new NetTopologySuite.IO.GeoJsonReader();
            var features = geoJsonReader.Read<FeatureCollection>(geoJson);

            foreach (var feature in features.Features)
            {
                if (feature.Geometry is Point)
                {
                    var point = feature.Geometry as Point;
                    uint id;
                    if (feature.Attributes.Exists("id") &&
                       uint.TryParse(feature.Attributes["id"].ToInvariantString(), out id))
                    { // has an id, add as vertex.
                        db.Network.AddVertex(id,
                            (float)point.Coordinate.Y,
                            (float)point.Coordinate.X);

                        nodeIds[id] = id;

                        var vertexMeta = new AttributeCollection();
                        foreach (var name in feature.Attributes.GetNames())
                        {
                            if (name == "id" ||
                                name.StartsWith("marker"))
                            {
                                continue;
                            }

                            var value = feature.Attributes[name];
                            if (value == null)
                            {
                                vertexMeta.AddOrReplace(name, string.Empty);
                            }
                            else
                            {
                                vertexMeta.AddOrReplace(name, value.ToInvariantString());
                            }
                        }

                        if (vertexMeta.Count > 0)
                        {
                            db.VertexMeta[id] = vertexMeta;
                        }
                    }
                }
            }

            foreach (var feature in features.Features)
            {
                if (feature.Geometry is LineString)
                {
                    if (feature.Attributes.Contains("restriction", "yes"))
                    {
                        continue;
                    }

                    var line = feature.Geometry as LineString;
                    var profile = new Itinero.Attributes.AttributeCollection();
                    var names = feature.Attributes.GetNames();
                    foreach (var name in names)
                    {
                        if (!name.StartsWith("meta:") &&
                            !name.StartsWith("stroke"))
                        {
                            profile.AddOrReplace(name, feature.Attributes[name].ToInvariantString());
                        }
                    }
                    var meta = new Itinero.Attributes.AttributeCollection();
                    foreach (var name in names)
                    {
                        if (name.StartsWith("meta:"))
                        {
                            meta.AddOrReplace(name.Remove(0, "meta:".Length),
                                feature.Attributes[name].ToInvariantString());
                        }
                    }

                    var profileId = db.EdgeProfiles.Add(profile);
                    var metaId = db.EdgeMeta.Add(meta);

                    var vertex1 = db.SearchVertexFor(
                        (float)line.Coordinates[0].Y,
                        (float)line.Coordinates[0].X, tolerance);
                    var distance = 0.0;
                    var shape = new List<Coordinate>();
                    for (var i = 1; i < line.Coordinates.Length; i++)
                    {
                        var vertex2 = db.SearchVertexFor(
                            (float)line.Coordinates[i].Y,
                            (float)line.Coordinates[i].X, tolerance);
                        distance += Coordinate.DistanceEstimateInMeter(
                            (float)line.Coordinates[i - 1].Y, (float)line.Coordinates[i - 1].X,
                            (float)line.Coordinates[i].Y, (float)line.Coordinates[i].X);
                        if (vertex2 == Itinero.Constants.NO_VERTEX)
                        { // add this point as shapepoint.
                            shape.Add(line.Coordinates[i].FromCoordinate());
                            continue;
                        }
                        var edgeId = db.Network.AddEdge(vertex1, vertex2, new Itinero.Data.Network.Edges.EdgeData()
                        {
                            Distance = (float)distance,
                            MetaId = metaId,
                            Profile = (ushort)profileId
                        }, shape);
                        shape.Clear();

                        edgeIds[edgeId] = vertex1.GetHashCode() ^ vertex2.GetHashCode();

                        vertex1 = vertex2;
                        distance = 0;
                    }
                }
            }

            foreach (var feature in features.Features)
            {
                if (feature.Geometry is LineString &&
                    feature.Attributes.Contains("restriction", "yes"))
                {
                    var line = feature.Geometry as LineString;
                    var sequence = new List<uint>();
                    sequence.Add(db.SearchVertexFor(
                        (float)line.Coordinates[0].Y,
                        (float)line.Coordinates[0].X, tolerance));
                    for (var i = 1; i < line.Coordinates.Length; i++)
                    {
                        sequence.Add(db.SearchVertexFor(
                            (float)line.Coordinates[i].Y,
                            (float)line.Coordinates[i].X, tolerance));
                    }

                    var vehicleType = string.Empty;
                    if (!feature.Attributes.TryGetValueAsString("vehicle_type", out vehicleType))
                    {
                        vehicleType = string.Empty;
                    }
                    RestrictionsDb restrictions;
                    if (!db.TryGetRestrictions(vehicleType, out restrictions))
                    {
                        restrictions = new RestrictionsDb();
                        db.AddRestrictions(vehicleType, restrictions);
                    }

                    restrictions.Add(sequence.ToArray());
                }

                if (feature.Geometry is Point &&
                    feature.Attributes.Contains("restriction", "yes"))
                {
                    var point = feature.Geometry as Point;
                    var sequence = new List<uint>();
                    sequence.Add(db.SearchVertexFor(
                        (float)point.Coordinate.Y,
                        (float)point.Coordinate.X, tolerance));

                    var vehicleType = string.Empty;
                    if (!feature.Attributes.TryGetValueAsString("vehicle_type", out vehicleType))
                    {
                        vehicleType = string.Empty;
                    }
                    RestrictionsDb restrictions;
                    if (!db.TryGetRestrictions(vehicleType, out restrictions))
                    {
                        restrictions = new RestrictionsDb();
                        db.AddRestrictions(vehicleType, restrictions);
                    }

                    restrictions.Add(sequence.ToArray());
                }
            }
        }

        /// <summary>
        /// Searches a vertex for the given location.
        /// </summary>
        public static uint SearchVertexFor(this RouterDb db, float latitude, float longitude,
            float tolerance = TestNetworkBuilder.Tolerance)
        {
            for (uint vertex = 0; vertex < db.Network.VertexCount; vertex++)
            {
                float lat, lon;
                if (db.Network.GetVertex(vertex, out lat, out lon))
                {
                    var dist = Coordinate.DistanceEstimateInMeter(latitude, longitude,
                        lat, lon);
                    if (dist < tolerance)
                    {
                        return vertex;
                    }
                }
            }
            return Itinero.Constants.NO_VERTEX;
        }
    }
}