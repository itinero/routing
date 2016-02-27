// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
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

using Itinero.Algorithms.Search.Hilbert;
using Itinero.Geo;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using NUnit.Framework;
using Itinero.Osm.Vehicles;
using System;
using System.IO;
using System.Reflection;

namespace Itinero.Test.Functional.Tests
{
    /// <summary>
    /// The test runner.
    /// </summary>
    public static class Runner
    {
        /// <summary>
        /// Default resolver test function.
        /// </summary>
        public static Func<Router, GeoAPI.Geometries.Coordinate, Result<RouterPoint>> Default = (router, coordinate) => 
            {
                return router.TryResolve(Vehicle.Car.Fastest(), coordinate);
            };

        /// <summary>
        /// Tests resolving all points in the given feature collection.
        /// </summary>
        public static void TestResolve(Router router, FeatureCollection features, 
            Func<Router, GeoAPI.Geometries.Coordinate, Result<RouterPoint>> resolve)
        {
            foreach(var feature in features.Features)
            {
                if(feature.Geometry is Point)
                {
                    Assert.IsNotNull(resolve(router, (feature.Geometry as Point).Coordinate));
                }
            }
        }

        /// <summary>
        /// Tests resolving all points in the given feature collection.
        /// </summary>
        public static void TestResolve(Router router, string embeddedResourceId, 
            Func<Router, GeoAPI.Geometries.Coordinate, Result<RouterPoint>> resolve)
        {
            FeatureCollection featureCollection;
            using (var stream = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedResourceId)))
            {
                var jsonReader = new JsonTextReader(stream);
                var geoJsonSerializer = new NetTopologySuite.IO.GeoJsonSerializer();
                featureCollection = geoJsonSerializer.Deserialize(jsonReader) as FeatureCollection;
            }
            TestResolve(router, featureCollection, resolve);
        }
        
        /// <summary>
        /// Tests building a router db.
        /// </summary>
        public static RouterDb TestBuildRouterDb(string file, params Vehicle[] vehicles)
        {
            using (var stream = File.OpenRead(file))
            {
                var routerdb = new RouterDb();
                var source = new OsmSharp.Streams.PBFOsmStreamSource(stream);
                var progress = new OsmSharp.Streams.Filters.OsmStreamFilterProgress();
                progress.RegisterSource(source);
                var target = new Itinero.IO.Osm.Streams.RouterDbStreamTarget(routerdb, vehicles);
                target.RegisterSource(progress);
                target.Pull();
                
                routerdb.Network.Sort();

                return routerdb;
            }
        }
    }
}