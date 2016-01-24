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

using NUnit.Framework;
using OsmSharp.Geo.Features;
using OsmSharp.Geo.Geometries;
using OsmSharp.Math.Geo;
using OsmSharp.Routing.Osm.Vehicles;
using OsmSharp.Routing.Profiles;
using System;
using System.IO;
using System.Reflection;

namespace OsmSharp.Routing.Test.Functional.Tests
{
    /// <summary>
    /// The test runner.
    /// </summary>
    public static class Runner
    {
        /// <summary>
        /// Default resolver test function.
        /// </summary>
        public static Func<Router, GeoCoordinate, Result<RouterPoint>> Default = (router, coordinate) => 
            {
                return router.TryResolve(Vehicle.Car.Fastest(), coordinate);
            };

        /// <summary>
        /// Tests resolving all points in the given feature collection.
        /// </summary>
        public static void TestResolve(Router router, FeatureCollection features, 
            Func<Router, GeoCoordinate, Result<RouterPoint>> resolve)
        {
            foreach(var feature in features)
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
            Func<Router, GeoCoordinate, Result<RouterPoint>> resolve)
        {
            FeatureCollection featureCollection;
            using (var stream = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedResourceId)))
            {
                featureCollection = OsmSharp.Geo.Streams.GeoJson.GeoJsonConverter.ToFeatureCollection(stream.ReadToEnd());
            }
            TestResolve(router, featureCollection, resolve);
        }
        
        /// <summary>
        /// Tests building a router db.
        /// </summary>
        public static RouterDb TestBuildRouterDb(string file, float maxEdgeDistance, params Vehicle[] vehicles)
        {
            using (var stream = File.OpenRead(file))
            {
                var routerdb = new RouterDb(maxEdgeDistance);
                var source = new OsmSharp.Osm.PBF.Streams.PBFOsmStreamSource(stream);
                var progress = new OsmSharp.Osm.Streams.Filters.OsmStreamFilterProgress();
                progress.RegisterSource(source);
                var target = new OsmSharp.Routing.Osm.Streams.RouterDbStreamTarget(routerdb, vehicles);
                target.RegisterSource(progress);
                target.Pull();
                return routerdb;
            }
        }
    }
}