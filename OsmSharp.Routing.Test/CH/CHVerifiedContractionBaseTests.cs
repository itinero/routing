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
using OsmSharp.Routing.CH;
using OsmSharp.Routing.CH.Preprocessing;
using OsmSharp.Routing.CH.Preprocessing.Ordering;
using OsmSharp.Routing.CH.Preprocessing.Witnesses;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Routing;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Osm.Streams;
using OsmSharp.Routing.Vehicles;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace OsmSharp.Test.Unittests.Routing.CH
{
    /// <summary>
    /// Executes the CH contractions while verifying each step.
    /// </summary>
    [TestFixture]
    public class CHVerifiedContractionBaseTests
    {
        /// <summary>
        /// Executes the tests.
        /// </summary>
        /// <param name="xml"></param>
        private static void ExecuteEdgeDifference(string xml)
        {
            CHVerifiedContractionBaseTests tester = new CHVerifiedContractionBaseTests();
            tester.DoTestCHEdgeDifferenceVerifiedContraction(xml, false);
        }

        /// <summary>
        /// Executes the tests.
        /// </summary>
        /// <param name="xml"></param>
        private static void ExecuteSparse(string xml)
        {
            CHVerifiedContractionBaseTests tester = new CHVerifiedContractionBaseTests();
            tester.DoTestCHSparseVerifiedContraction(xml, false);
        }

        #region Testing Code

        /// <summary>
        /// Holds the data.
        /// </summary>
        private RouterDataSource<CHEdgeData> _data;

        /// <summary>
        /// Holds the interpreter.
        /// </summary>
        private IOsmRoutingInterpreter _interpreter;

        /// <summary>
        /// Holds the reference router.
        /// </summary>
        private Router _referenceRouter;

        /// <summary>
        /// Builds a raw router to compare against.
        /// </summary>
        /// <returns></returns>
        public void BuildDykstraRouter(string embeddedName,
            IOsmRoutingInterpreter interpreter)
        {
            var tagsIndex = new TagsIndex();

            // do the data processing.
            var data = new RouterDataSource<Edge>(new Graph<Edge>(), tagsIndex);
            var targetData = new GraphOsmStreamTarget(
                data, interpreter, tagsIndex, new Vehicle[] { Vehicle.Car });
            var dataProcessorSource = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedName));
            var sorter = new OsmStreamFilterSort();
            sorter.RegisterSource(dataProcessorSource);
            targetData.RegisterSource(sorter);
            targetData.Pull();

            // initialize the router.
            _referenceRouter = Router.CreateFrom(data, new Dykstra(), interpreter);
        }

        /// <summary>
        /// Executes the CH contractions while verifying each step.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="crazyVerification"></param>
        private void DoTestCHSparseVerifiedContraction(string xml, bool crazyVerification)
        {
            _pathsBeforeContraction = new Dictionary<uint, Dictionary<uint, Dictionary<uint, PathSegment<long>>>>();
            _referenceRouter = null;
            if (crazyVerification)
            {
                this.BuildDykstraRouter(xml, new OsmRoutingInterpreter());
            }
            this.DoTestCHSparseVerifiedContraction(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(xml));
        }

        /// <summary>
        /// Executes the CH contractions while verifying each step.
        /// </summary>
        /// <param name="stream"></param>
        public void DoTestCHSparseVerifiedContraction(Stream stream)
        {
            _pathsBeforeContraction = new Dictionary<uint, Dictionary<uint, Dictionary<uint, PathSegment<long>>>>();
            _interpreter = new OsmRoutingInterpreter();

            var tagsIndex = new TagsIndex();

            // do the data processing.
            _data = new RouterDataSource<CHEdgeData>(new DirectedGraph<CHEdgeData>(), tagsIndex);
            var targetData = new CHEdgeGraphOsmStreamTarget(
                _data, _interpreter, tagsIndex, Vehicle.Car);
            var dataProcessorSource = new XmlOsmStreamSource(stream);
            var sorter = new OsmStreamFilterSort();
            sorter.RegisterSource(dataProcessorSource);
            targetData.RegisterSource(sorter);
            targetData.Pull();

            //// do the pre-processing part.
            //var witnessCalculator = new DykstraWitnessCalculator();
            //var preProcessor = new CHPreProcessor(_data,
            //    new EdgeDifferenceContractedSearchSpace(_data, witnessCalculator), witnessCalculator);
            //preProcessor.OnBeforeContractionEvent += new CHPreProcessor.VertexDelegate(pre_processor_OnBeforeContractionEvent);
            //preProcessor.OnAfterContractionEvent += new CHPreProcessor.VertexDelegate(pre_processor_OnAfterContractionEvent);
            //preProcessor.Start();
        }

        /// <summary>
        /// Executes the CH contractions while verifying each step.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="crazyVerification"></param>
        internal void DoTestCHEdgeDifferenceVerifiedContraction(string xml, bool crazyVerification)
        {
            _referenceRouter = null;
            if (crazyVerification)
            {
                this.BuildDykstraRouter(xml, new OsmRoutingInterpreter());
            }
            this.DoTestCHSparseVerifiedContraction(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(xml));
        }

        /// <summary>
        /// Executes the CH contractions while verifying each step.
        /// </summary>
        /// <param name="stream"></param>
        public void DoTestCHEdgeDifferenceVerifiedContraction(Stream stream)
        {
            _interpreter = new OsmRoutingInterpreter();

            var tagsIndex = new TagsIndex();

            // do the data processing.
            _data = new RouterDataSource<CHEdgeData>(new DirectedGraph<CHEdgeData>(), tagsIndex);
            var targetData = new CHEdgeGraphOsmStreamTarget(
                _data, _interpreter, tagsIndex, Vehicle.Car);
            var dataProcessorSource = new XmlOsmStreamSource(stream);
            var sorter = new OsmStreamFilterSort();
            sorter.RegisterSource(dataProcessorSource);
            targetData.RegisterSource(sorter);
            targetData.Pull();

            // do the pre-processing part.
            var witnessCalculator = new DykstraWitnessCalculator();
            var preProcessor = new CHPreprocessor(_data,
                new EdgeDifferenceContractedSearchSpace(_data, witnessCalculator), witnessCalculator);
            preProcessor.OnBeforeContractionEvent += 
                new CHPreprocessor.VertexDelegate(pre_processor_OnBeforeContractionEvent);
            preProcessor.OnAfterContractionEvent += 
                new CHPreprocessor.VertexDelegate(pre_processor_OnAfterContractionEvent);
            preProcessor.Start();
        }

        /// <summary>
        /// Holds the paths calculate before contraction.
        /// </summary>
        private Dictionary<uint, Dictionary<uint, Dictionary<uint, PathSegment<long>>>> _pathsBeforeContraction;

        /// <summary>
        /// Called right after the contraction.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="edges"></param>
        void pre_processor_OnAfterContractionEvent(uint vertex, List<Edge<CHEdgeData>> edges)
        {
            // get dictionary for vertex.
            var pathsBeforeContraction = _pathsBeforeContraction[vertex];            

            // create a new CHRouter
            var router = new CHRouter();

            // calculate all the routes between the neighbours of the contracted vertex.
            foreach (var from in edges)
            {
                // initialize the from-list.
                var fromList = new PathSegmentVisitList();
                fromList.UpdateVertex(new PathSegment<long>(from.Neighbour));

                // initalize the from dictionary.
                var fromDic = pathsBeforeContraction[from.Neighbour];
                foreach (var to in edges)
                {
                    // initialize the to-list.
                    var toList = new PathSegmentVisitList();
                    toList.UpdateVertex(new PathSegment<long>(to.Neighbour));

                    // calculate the route.
                    var route = router.Calculate(_data, _interpreter, Vehicle.Car, fromList, toList, double.MaxValue, null);
                    if ((fromDic[to.Neighbour] == null && route != null) ||
                        (fromDic[to.Neighbour] != null && route == null))
                    { // the route match!
                        Assert.Fail("Routes are different before/after contraction!");
                    }
                    else if (fromDic[to.Neighbour] != null && route != null)
                    {
                        this.ComparePaths(fromDic[to.Neighbour], route);
                    }
                }
            }

            if (_referenceRouter != null)
            { // do crazy verification!
                var chRouter = Router.CreateCHFrom(_data, router, new OsmRoutingInterpreter());

                // loop over all nodes and resolve their locations.
                var resolvedReference = new RouterPoint[_data.VertexCount - 1];
                var resolved = new RouterPoint[_data.VertexCount - 1];
                for (uint idx = 1; idx < _data.VertexCount; idx++)
                { // resolve each vertex.
                    float latitude, longitude;
                    if (_data.GetVertex(idx, out latitude, out longitude))
                    {
                        resolvedReference[idx - 1] = _referenceRouter.Resolve(Vehicle.Car, new GeoCoordinate(latitude, longitude));
                        resolved[idx - 1] = chRouter.Resolve(Vehicle.Car, new GeoCoordinate(latitude, longitude));
                    }

                    Assert.IsNotNull(resolvedReference[idx - 1]);
                    Assert.IsNotNull(resolved[idx - 1]);

                    Assert.AreEqual(resolvedReference[idx - 1].Location.Latitude,
                        resolved[idx - 1].Location.Latitude, 0.0001);
                    Assert.AreEqual(resolvedReference[idx - 1].Location.Longitude,
                        resolved[idx - 1].Location.Longitude, 0.0001);
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
                            Route referenceRoute = _referenceRouter.Calculate(Vehicle.Car,
                                resolvedReference[fromIdx], resolvedReference[toIdx]);
                            Route route = chRouter.Calculate(Vehicle.Car,
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

            _pathsBeforeContraction.Remove(vertex);
        }
        /// <summary>
        /// Compares the two given routes.
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="route"></param>
        protected void CompareRoutes(Route reference, Route route)
        {
            if (reference.Segments == null)
            {
                Assert.IsNull(route.Segments);
            }
            else
            {
                Assert.AreEqual(reference.Segments.Length, route.Segments.Length);
                for (int idx = 0; idx < reference.Segments.Length; idx++)
                {
                    Assert.AreEqual(reference.Segments[idx].Distance,
                        route.Segments[idx].Distance);
                    Assert.AreEqual(reference.Segments[idx].Latitude,
                        route.Segments[idx].Latitude);
                    Assert.AreEqual(reference.Segments[idx].Longitude,
                        route.Segments[idx].Longitude);
                    Assert.AreEqual(reference.Segments[idx].Time,
                        route.Segments[idx].Time);
                    Assert.AreEqual(reference.Segments[idx].Type,
                        route.Segments[idx].Type);
                    Assert.AreEqual(reference.Segments[idx].Name,
                        route.Segments[idx].Name);
                }
            }
        }

        /// <summary>
        /// Compares the two paths.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        protected void ComparePaths(PathSegment<long> expected, PathSegment<long> actual)
        {
            Assert.AreEqual(expected.VertexId, actual.VertexId);
            Assert.AreEqual(expected.Weight, actual.Weight, 0.001);

            if(expected.From != null)
            {
                Assert.IsNotNull(actual.From);
                this.ComparePaths(expected.From, actual.From);
                return;
            }
            Assert.IsNull(actual.From);
        }

        /// <summary>
        /// Called left before the contraction.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="edges"></param>
        void pre_processor_OnBeforeContractionEvent(uint vertex, List<Edge<CHEdgeData>> edges)
        {
            // create a new CHRouter
            var router = new CHRouter();

            // calculate all the routes between the neighbours of the contracted vertex.
            var pathsBeforeContraction = new Dictionary<uint, Dictionary<uint, PathSegment<long>>>();
            _pathsBeforeContraction.Add(vertex, pathsBeforeContraction);
            foreach (var from in edges)
            {
                // initialize the from-list.
                var fromList = new PathSegmentVisitList();
                fromList.UpdateVertex(new PathSegment<long>(from.Neighbour));

                // initalize the from dictionary.
                var fromDic = new Dictionary<uint, PathSegment<long>>();
                pathsBeforeContraction[from.Neighbour] = fromDic;
                foreach (var to in edges)
                {
                    // initialize the to-list.
                    var toList = new PathSegmentVisitList();
                    toList.UpdateVertex(new PathSegment<long>(to.Neighbour));

                    // calculate the route.
                    fromDic[to.Neighbour] = router.Calculate(_data, _interpreter,
                        Vehicle.Car, fromList, toList, double.MaxValue, null); ;
                }
            }
        }

        #endregion
    }
}