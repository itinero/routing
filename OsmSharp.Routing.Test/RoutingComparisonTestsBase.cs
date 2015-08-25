// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
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
using OsmSharp.Collections.Tags.Index;
using OsmSharp.Math.Geo;
using OsmSharp.Osm.Streams.Filters;
using OsmSharp.Osm.Xml.Streams;
using OsmSharp.Routing;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Routing;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Vehicles;
using OsmSharp.Units.Distance;
using OsmSharp.Units.Time;
using System.Reflection;

namespace OsmSharp.Test.Unittests.Routing
{
    /// <summary>
    /// Base class with tests around IRouter objects.
    /// </summary>
    public abstract class RoutingComparisonTestsBase
    {
        /// <summary>
        /// Returns a router test object.
        /// </summary>
        /// <returns></returns>
        public abstract Router BuildRouter(IOsmRoutingInterpreter interpreter, string embeddedName);

        /// <summary>
        /// Builds a raw data source.
        /// </summary>
        /// <returns></returns>
        public RouterDataSource<Edge> BuildDykstraDataSource(
            IOsmRoutingInterpreter interpreter, string embeddedName)
        {
            var tagsIndex = new TagsIndex();

            // do the data processing.
            var data = new RouterDataSource<Edge>(new Graph<Edge>(), tagsIndex);
            var targetData = new ReferenceGraphTarget(
                data, interpreter, tagsIndex, new Vehicle[] { Vehicle.Car });
            var dataProcessorSource = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format(
                    "OsmSharp.Routing.Test.data.{0}", embeddedName)));
            var sorter = new OsmStreamFilterSort();
            sorter.RegisterSource(dataProcessorSource);
            targetData.RegisterSource(sorter);
            targetData.Pull();

            return data;
        }

        /// <summary>
        /// Builds a raw router to compare against.
        /// </summary>
        /// <returns></returns>
        public Router BuildDykstraRouter(IRoutingAlgorithmData<Edge> data,
            IRoutingInterpreter interpreter, IRoutingAlgorithm<Edge> basicRouter)
        {
            // initialize the router.
            return Router.CreateFrom(data, basicRouter, interpreter);
        }

        /// <summary>
        /// Tests one route.
        /// </summary>
        protected void TestCompareOne(string embeddedName, GeoCoordinate from, GeoCoordinate to)
        {
            // build the routing settings.
            var interpreter = new OsmSharp.Routing.Osm.Interpreter.OsmRoutingInterpreter();

            // get the osm data source.
            var data = this.BuildDykstraDataSource(interpreter, embeddedName);

            // build the reference router.;
            var referenceRouter = this.BuildDykstraRouter(
                this.BuildDykstraDataSource(interpreter, embeddedName), interpreter,
                    new Dykstra());

            // build the router to be tested.
            var router = this.BuildRouter(interpreter, embeddedName);

            var referenceResolvedFrom = referenceRouter.Resolve(Vehicle.Car, from);
            var referenceResolvedTo = referenceRouter.Resolve(Vehicle.Car, to);
            var resolvedFrom = router.Resolve(Vehicle.Car, from);
            var resolvedTo = router.Resolve(Vehicle.Car, to);

            var referenceRoute = referenceRouter.Calculate(Vehicle.Car, referenceResolvedFrom, referenceResolvedTo);
            var route = router.Calculate(Vehicle.Car, resolvedFrom, resolvedTo);

            this.CompareRoutes(referenceRoute, route);
        }

        /// <summary>
        /// Compares all routes against the reference router.
        /// </summary>
        protected void TestCompareAll(string embeddedName)
        {
            // build the routing settings.
            var interpreter = new OsmSharp.Routing.Osm.Interpreter.OsmRoutingInterpreter();

            // get the osm data source.
            var data = this.BuildDykstraDataSource(interpreter, embeddedName);

            // build the reference router.;
            var referenceRouter = this.BuildDykstraRouter(
                data, interpreter, 
                    new Dykstra());

            // build the router to be tested.
            var router = this.BuildRouter(interpreter, embeddedName);

            this.TestCompareAll(data, referenceRouter, router);
        }

        /// <summary>
        /// Tests the the given router class by comparing calculated routes agains a given reference router.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="referenceRouter"></param>
        /// <param name="router"></param>
        protected void TestCompareAll<TEdgeData>(IRoutingAlgorithmData<TEdgeData> data, Router referenceRouter, Router router)
            where TEdgeData : IGraphEdgeData
        {       
            // loop over all nodes and resolve their locations.
            var resolvedReference = new RouterPoint[data.VertexCount - 1];
            var resolved = new RouterPoint[data.VertexCount - 1];
            for (uint idx = 1; idx < data.VertexCount; idx++)
            { // resolve each vertex.
                float latitude, longitude;
                if (data.GetVertex(idx, out latitude, out longitude))
                {
                    resolvedReference[idx - 1] = referenceRouter.Resolve(Vehicle.Car, new GeoCoordinate(latitude, longitude), true);
                    resolved[idx - 1] = router.Resolve(Vehicle.Car, new GeoCoordinate(latitude, longitude), true);
                }

                // reference and resolved have to exist.
                Assert.IsNotNull(resolvedReference[idx - 1]);
                Assert.IsNotNull(resolved[idx - 1]);

                // reference and resolved cannot be more than 10cm apart.
                Assert.AreEqual(0, resolvedReference[idx - 1].Location.DistanceReal(
                    resolved[idx - 1].Location).Value, 0.1, "Reference and resolved are more than 10cm apart."); 
            }

            // limit tests to a fixed number.
            int maxTestCount = 100;
            int testEveryOther = (resolved.Length * resolved.Length) / maxTestCount;
            testEveryOther = System.Math.Max(testEveryOther, 1);

            // check all the routes having the same weight(s).
            for (int fromIdx = 0; fromIdx < resolved.Length; fromIdx++)
            {
                for (int toIdx = 0; toIdx < resolved.Length; toIdx++)
                {
                    int testNumber = fromIdx * resolved.Length + toIdx;
                    if (testNumber % testEveryOther == 0)
                    {
                        OsmSharp.Logging.Log.TraceEvent("RoutingComparisonTestBase.TestCompareAll", Logging.TraceEventType.Information,
                            "Testing point {0} -> {1}", fromIdx, toIdx);
                        var referenceRoute = referenceRouter.Calculate(Vehicle.Car,
                            resolvedReference[fromIdx], resolvedReference[toIdx]);
                        var route = router.Calculate(Vehicle.Car,
                            resolved[fromIdx], resolved[toIdx]);

                        if (referenceRoute != null)
                        {
                            Assert.IsNotNull(referenceRoute);
                            Assert.IsNotNull(route);
                            this.CompareRoutes(referenceRoute, route);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Compares the two given routes.
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="route"></param>
        protected void CompareRoutes(Route reference, Route route)
        {
            double delta = 0.0001;

            if (reference.Segments == null)
            { // both routes are empty.
                Assert.IsNull(route.Segments);
            }
            else
            { // compare the geometry of the routes.
                for (int idx = 0; idx < reference.Segments.Length; idx++)
                {
                    var referenceCoordinate = new GeoCoordinate(reference.Segments[idx].Latitude,
                        reference.Segments[idx].Longitude);
                    Meter referenceDistance, distance;
                    GeoCoordinate referenceProjected, projected;
                    Second referenceTime, time;
                    int entryIdx;
                    reference.ProjectOn(referenceCoordinate, out referenceProjected, out entryIdx, out referenceDistance, out referenceTime);
                    route.ProjectOn(referenceCoordinate, out projected, out entryIdx, out distance, out time);

                    Assert.AreEqual(0, referenceProjected.DistanceReal(projected).Value, delta); // projected points have to match.
                    Assert.AreEqual(referenceDistance.Value, distance.Value, 0.1); // compare calculated distance to 10cm accuracy.
                    Assert.AreEqual(referenceProjected.Latitude, projected.Latitude, delta);
                    Assert.AreEqual(referenceProjected.Longitude, projected.Longitude, delta);

                    var referenceEntry = reference.Segments[entryIdx];
                    var routeEntry = route.Segments[entryIdx];

                    //if (referenceEntry.SideStreets != null && referenceEntry.SideStreets.Length > 0)
                    //{ // there are way names.
                    //    Assert.IsNotNull(routeEntry.SideStreets);
                    //    Assert.AreEqual(referenceEntry.SideStreets.Length, routeEntry.SideStreets.Length);
                    //}
                    //else
                    //{ // there are no way names.
                    //    Assert.IsTrue(routeEntry.SideStreets == null || routeEntry.SideStreets.Length == 0);
                    //}
                }
            }
        }
    }
}