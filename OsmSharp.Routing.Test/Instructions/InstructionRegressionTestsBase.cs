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

using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using OsmSharp.Collections.Tags;
using OsmSharp.Math.Geo;
using OsmSharp.Osm.Streams.Filters;
using OsmSharp.Osm.Xml.Streams;
using OsmSharp.Routing;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Routing;
using OsmSharp.Routing.Instructions;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Units.Distance;
using OsmSharp.Collections.Tags.Index;
using OsmSharp.Collections;
using OsmSharp.Units.Time;
using OsmSharp.Routing.Osm.Streams;
using OsmSharp.Routing.Vehicles;

namespace OsmSharp.Test.Unittests.Routing.Instructions
{
    /// <summary>
    /// Holds some instruction generation regression tests.
    /// </summary>
    public abstract class InstructionRegressionTestsBase
    {
        /// <summary>
        /// Creates a router based on the resource.
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="manifestResourceName"></param>
        /// <returns></returns>
        protected Router CreateReferenceRouter(IOsmRoutingInterpreter interpreter, string manifestResourceName)
        {
            TagsIndex tagsIndex = new TagsIndex();

            // do the data processing.
            var memoryData =
                new RouterDataSource<Edge>(new Graph<Edge>(), tagsIndex);
            var targetData = new GraphOsmStreamTarget(
                memoryData, interpreter, tagsIndex, null, false);
            var dataProcessorSource = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(manifestResourceName));
            var sorter = new OsmStreamFilterSort();
            sorter.RegisterSource(dataProcessorSource);
            targetData.RegisterSource(sorter);
            targetData.Pull();

            IRoutingAlgorithm<Edge> basicRouter = new Dykstra();
            return Router.CreateFrom(memoryData, basicRouter, interpreter);
        }

        /// <summary>
        /// Creates a router based on the resource.
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="manifestResourceName"></param>
        /// <returns></returns>
        protected abstract Router CreateRouter(IOsmRoutingInterpreter interpreter, string manifestResourceName);

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
                    reference.ProjectOn(referenceCoordinate, out referenceProjected, out referenceDistance, out referenceTime);
                    route.ProjectOn(referenceCoordinate, out projected, out distance, out time);

                    Assert.AreEqual(0, referenceProjected.DistanceReal(projected).Value, delta); // projected points have to match.
                    Assert.AreEqual(referenceDistance.Value, distance.Value, 0.1); // compare calculated distance to 10cm accuracy.
                    Assert.AreEqual(referenceProjected.Latitude, projected.Latitude, delta);
                    Assert.AreEqual(referenceProjected.Longitude, projected.Longitude, delta);
                }
            }
        }

        /// <summary>
        /// Issue with generation instructions but where streetnames seem to be stripped.
        /// Some streetnames are missing from the instructions.
        /// </summary>
        protected void DoInstructionRegressionTest1()
        {
            OsmRoutingInterpreter interpreter = new OsmRoutingInterpreter();

            Router router = this.CreateRouter(interpreter, "OsmSharp.Test.Unittests.test_routing_regression1.osm");

            // resolve the three points in question.
            GeoCoordinate point35 = new GeoCoordinate(51.01257, 4.000753);
            RouterPoint point35resolved = router.Resolve(Vehicle.Car, point35, true);
            GeoCoordinate point45 = new GeoCoordinate(51.01315, 3.999588);
            RouterPoint point45resolved = router.Resolve(Vehicle.Car, point45, true);
            GeoCoordinate point40 = new GeoCoordinate(51.01250, 4.000013);
            RouterPoint point40resolved = router.Resolve(Vehicle.Car, point40, true);

            // calculate two smaller routes.
            Route route3545 = router.Calculate(Vehicle.Car, point35resolved, point45resolved);
            Route route4540 = router.Calculate(Vehicle.Car, point45resolved, point40resolved);
            Route route3540concatenated = Route.Concatenate(route3545, route4540, true);

            Route route3540 = router.Calculate(Vehicle.Car, point35resolved, point40resolved);

            // check if both routes are equal.
            Assert.AreEqual(route3540.Segments.Length, route3540concatenated.Segments.Length);
            for (int idx = 0; idx < route3540.Segments.Length; idx++)
            {
                Assert.AreEqual(route3540.Segments[idx].Distance, route3540concatenated.Segments[idx].Distance);
                Assert.AreEqual(route3540.Segments[idx].Latitude, route3540concatenated.Segments[idx].Latitude);
                Assert.AreEqual(route3540.Segments[idx].Longitude, route3540concatenated.Segments[idx].Longitude);
                Assert.AreEqual(route3540.Segments[idx].Time, route3540concatenated.Segments[idx].Time);
                Assert.AreEqual(route3540.Segments[idx].Type, route3540concatenated.Segments[idx].Type);
                Assert.AreEqual(route3540.Segments[idx].Name, route3540concatenated.Segments[idx].Name);

                // something that is allowed to be different in this case!
                // route3540.Entries[idx].Points != null

            //    // check sidestreets.
            //    if (route3540.Entries[idx].SideStreets != null &&
            //        route3540.Entries[idx].SideStreets.Length > 0)
            //    { // check if the sidestreets represent the same information.
            //        for (int metricIdx = 0; metricIdx < route3540concatenated.Entries[idx].SideStreets.Length; metricIdx++)
            //        {
            //            Assert.AreEqual(route3540.Entries[idx].SideStreets[metricIdx].WayName,
            //                route3540concatenated.Entries[idx].SideStreets[metricIdx].WayName);
            //            Assert.AreEqual(route3540.Entries[idx].SideStreets[metricIdx].Latitude,
            //                route3540concatenated.Entries[idx].SideStreets[metricIdx].Latitude);
            //            Assert.AreEqual(route3540.Entries[idx].SideStreets[metricIdx].Longitude,
            //                route3540concatenated.Entries[idx].SideStreets[metricIdx].Longitude);
            //        }
            //    }
            //    else
            //    {
            //        Assert.IsTrue(route3540concatenated.Entries[idx].SideStreets == null ||
            //            route3540concatenated.Entries[idx].SideStreets.Length == 0);
            //    }


            //    if (route3540.Entries[idx].Tags != null &&
            //        route3540.Entries[idx].Tags.Length > 0)
            //    { // check if the Tags represent the same information.
            //        for (int metricIdx = 0; metricIdx < route3540concatenated.Entries[idx].Tags.Length; metricIdx++)
            //        {
            //            Assert.AreEqual(route3540.Entries[idx].Tags[metricIdx].Key,
            //                route3540concatenated.Entries[idx].Tags[metricIdx].Key);
            //            Assert.AreEqual(route3540.Entries[idx].Tags[metricIdx].Value,
            //                route3540concatenated.Entries[idx].Tags[metricIdx].Value);
            //        }
            //    }
            //    else
            //    {
            //        Assert.IsTrue(route3540concatenated.Entries[idx].Tags == null ||
            //            route3540concatenated.Entries[idx].Tags.Length == 0);
            //    }

            //    Assert.AreEqual(route3540.Entries[idx].Distance, route3540concatenated.Entries[idx].Distance);
            }
            if (route3540.Tags != null &&
                route3540.Tags.Length > 0)
            {
                for (int tagIdx = 0; tagIdx < route3540.Tags.Length; tagIdx++)
                {
                    if (route3540.Tags[tagIdx].Key != "debug_route")
                    {
                        Assert.AreEqual(route3540.Tags[tagIdx].Key, route3540concatenated.Tags[tagIdx].Key);
                        Assert.AreEqual(route3540.Tags[tagIdx].Value, route3540concatenated.Tags[tagIdx].Value);
                    }
                }
            }
            else
            {
                Assert.IsTrue(route3540concatenated.Tags == null ||
                    route3540concatenated.Tags.Length == 0);
            }
            if (route3540.Metrics != null)
            {
                for (int metricIdx = 0; metricIdx < route3540concatenated.Segments.Length; metricIdx++)
                {
                    Assert.AreEqual(route3540.Metrics[metricIdx].Key, route3540concatenated.Metrics[metricIdx].Key);
                    Assert.AreEqual(route3540.Metrics[metricIdx].Value, route3540concatenated.Metrics[metricIdx].Value);
                }
            }
            else
            {
                Assert.IsNull(route3540concatenated.Metrics);
            }

            // remove the point in between, the only difference between the regular and the concatenated route.
            route3540concatenated.Segments[7].Points = null;

            // create the language generator.
            var languageGenerator = new LanguageTestGenerator();

            // generate the instructions.
            List<Instruction> instructions =
                InstructionGenerator.Generate(route3540, interpreter, languageGenerator);
            List<Instruction> instructionsConcatenated =
                InstructionGenerator.Generate(route3540concatenated, interpreter, languageGenerator);

            Assert.AreEqual(instructions.Count, instructionsConcatenated.Count);
            for (int idx = 0; idx < instructions.Count; idx++)
            {
                Assert.AreEqual(instructions[idx].Location.Center,
                    instructionsConcatenated[idx].Location.Center);
                Assert.AreEqual(instructions[idx].Text,
                    instructionsConcatenated[idx].Text);
            }
        }

        /// <summary>
        /// Calculates routes, generates instructions and compares instructions.
        /// </summary>
        /// <param name="embeddedXml"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        protected void DoInstructionComparisonTest(string embeddedXml, GeoCoordinate point1, GeoCoordinate point2)
        {
            Meter delta = 1;

            var interpreter = new OsmRoutingInterpreter();

            var router = this.CreateRouter(interpreter,
                embeddedXml);
            var referenceRouter = this.CreateReferenceRouter(interpreter,
                embeddedXml);

            // resolve the three points in question.
            var point1resolved = router.Resolve(Vehicle.Car, point1, true);
            var point2resolved = router.Resolve(Vehicle.Car, point2, true);

            // calculate two smaller routes.
            var route12 = router.Calculate(Vehicle.Car,
                point1resolved, point2resolved);

            // resolve the three points in question.
            RouterPoint pointReference1resolved = referenceRouter.Resolve(Vehicle.Car, point1, true);
            RouterPoint pointReference2resolved = referenceRouter.Resolve(Vehicle.Car, point2, true);

            // test the resolved points.
            Assert.AreEqual(0, point1resolved.Location.DistanceReal(pointReference1resolved.Location).Value, delta.Value);
            Assert.AreEqual(0, point2resolved.Location.DistanceReal(pointReference2resolved.Location).Value, delta.Value);

            // calculate two smaller routes.
            Route routeReference12 = referenceRouter.Calculate(Vehicle.Car,
                pointReference1resolved, pointReference2resolved);

            // compares the two routes.
            this.CompareRoutes(routeReference12, route12);

            // create the language generator.
            var languageGenerator = new LanguageTestGenerator();

            // generate the instructions.
            var instructions =  InstructionGenerator.Generate(route12, interpreter, languageGenerator);
            var instructionsReference = InstructionGenerator.Generate(routeReference12, interpreter, languageGenerator);

            Assert.AreEqual(instructions.Count, instructionsReference.Count);
            for (int idx = 0; idx < instructions.Count; idx++)
            {
                Assert.AreEqual(instructions[idx].Location.Center,
                    instructionsReference[idx].Location.Center);
                Assert.AreEqual(instructions[idx].Text,
                    instructionsReference[idx].Text);
            }
        }
    }
}