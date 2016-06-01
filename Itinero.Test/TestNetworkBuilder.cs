// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
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
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Itinero.Data.Network;
using Itinero.LocalGeo;
using System.Collections.Generic;
using System.IO;
using Itinero.Geo;

namespace Itinero.Test
{
    /// <summary>
    /// Builds test networks based on geojson files.
    /// </summary>
    public static class TestNetworkBuilder
    {
        private static float Tolerance = 20; // 10 meter.

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
                    { // has and id, add as vertex.
                        db.Network.AddVertex(id,
                            (float)point.Coordinate.Y,
                            (float)point.Coordinate.X);
                    }
                }
            }

            foreach (var feature in features.Features)
            {
                if (feature.Geometry is LineString)
                {
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
                        (float)line.Coordinates[0].X);
                    var distance = 0.0;
                    var shape = new List<Coordinate>();
                    for (var i = 1; i < line.Coordinates.Length; i++)
                    {
                        var vertex2 = db.SearchVertexFor(
                            (float)line.Coordinates[i].Y,
                            (float)line.Coordinates[i].X);
                        distance += Coordinate.DistanceEstimateInMeter(
                            (float)line.Coordinates[i - 1].Y, (float)line.Coordinates[i - 1].X,
                            (float)line.Coordinates[i].Y, (float)line.Coordinates[i].X);
                        if (vertex2 == Itinero.Constants.NO_VERTEX)
                        { // add this point as shapepoint.
                            shape.Add(line.Coordinates[i].FromCoordinate());
                            continue;
                        }
                        db.Network.AddEdge(vertex1, vertex2, new Itinero.Data.Network.Edges.EdgeData()
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
                    var dist = Coordinate.DistanceEstimateInMeter(latitude, longitude,
                        lat, lon);
                    if (dist < Tolerance)
                    {
                        return vertex;
                    }
                }
            }
            return Itinero.Constants.NO_VERTEX;
        }
    }
}