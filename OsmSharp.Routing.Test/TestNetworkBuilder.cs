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

using OsmSharp.Collections.Coordinates.Collections;
using OsmSharp.Collections.Tags;
using OsmSharp.Routing.Network;
using OsmSharp.Geo.Geometries;
using OsmSharp.Geo.Streams.GeoJson;
using OsmSharp.Math.Geo;
using System.Collections.Generic;
using System.IO;

namespace OsmSharp.Routing.Test
{
    /// <summary>
    /// Builds test networks based on geojson files.
    /// </summary>
    public static class TestNetworkBuilder
    {
        private static float Tolerance = 10; // 10 meter.

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
        public static void LoadTestNetwork(this RouterDb db, string geoJson)
        {
            var features = GeoJsonConverter.ToFeatureCollection(geoJson);

            foreach (var feature in features)
            {
                if (feature.Geometry is Point)
                {
                    var point = feature.Geometry as Point;
                    uint id;
                    if (feature.Attributes.ContainsKey("id") &&
                       uint.TryParse(feature.Attributes["id"].ToInvariantString(), out id))
                    { // has and id, add as vertex.
                        db.Network.AddVertex(id,
                            (float)point.Coordinate.Latitude,
                            (float)point.Coordinate.Longitude);
                    }
                }
            }

            foreach (var feature in features)
            {
                if (feature.Geometry is LineString)
                {
                    var line = feature.Geometry as LineString;
                    var profile = new TagsCollection();
                    foreach (var attribute in feature.Attributes)
                    {
                        if (!attribute.Key.StartsWith("meta:") &&
                            !attribute.Key.StartsWith("stroke"))
                        {
                            profile.Add(attribute.Key, attribute.Value.ToInvariantString());
                        }
                    }
                    var meta = new TagsCollection();
                    foreach (var attribute in feature.Attributes)
                    {
                        if (attribute.Key.StartsWith("meta:"))
                        {
                            meta.Add(attribute.Key.Remove(0, "meta:".Length),
                                attribute.Value.ToInvariantString());
                        }
                    }

                    var profileId = db.EdgeProfiles.Add(profile);
                    var metaId = db.EdgeMeta.Add(meta);

                    var vertex1 = db.SearchVertexFor(
                        (float)line.Coordinates[0].Latitude,
                        (float)line.Coordinates[0].Longitude);
                    var distance = 0.0;
                    var shape = new List<ICoordinate>();
                    for (var i = 1; i < line.Coordinates.Count; i++)
                    {
                        var vertex2 = db.SearchVertexFor(
                            (float)line.Coordinates[i].Latitude,
                            (float)line.Coordinates[i].Longitude);
                        distance += GeoCoordinate.DistanceEstimateInMeter(line.Coordinates[i - 1],
                            line.Coordinates[i]);
                        if (vertex2 == OsmSharp.Routing.Constants.NO_VERTEX)
                        { // add this point as shapepoint.
                            shape.Add(line.Coordinates[i]);
                            continue;
                        }
                        db.Network.AddEdge(vertex1, vertex2, new Routing.Network.Data.EdgeData()
                        {
                            Distance = (float)distance,
                            MetaId = metaId,
                            Profile = (ushort)profileId
                        }, shape);
                        shape.Clear();
                        vertex1 = vertex2;
                        distance = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Searches a vertex for the given location.
        /// </summary>
        public static uint SearchVertexFor(this RouterDb db, float latitude, float longitude)
        {
            for (uint vertex = 0; vertex < db.Network.VertexCount; vertex++)
            {
                float lat, lon;
                if (db.Network.GetVertex(vertex, out lat, out lon))
                {
                    var dist = GeoCoordinate.DistanceEstimateInMeter(latitude, longitude,
                        lat, lon);
                    if (dist < Tolerance)
                    {
                        return vertex;
                    }
                }
            }
            return OsmSharp.Routing.Constants.NO_VERTEX;
        }
    }
}