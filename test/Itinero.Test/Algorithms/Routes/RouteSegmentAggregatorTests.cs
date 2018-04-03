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

using NUnit.Framework;
using Itinero.Algorithms.Routes;
using Itinero.Attributes;
using Itinero.LocalGeo;
using System.Collections.Generic;

namespace Itinero.Test.Algorithms.Routes
{
    /// <summary>
    /// Contains test for the route segment aggregator.
    /// </summary>
    [TestFixture]
    public class RouteSegmentAggregatorTests
    {
        /// <summary>
        /// Tests a route with one segment.
        /// </summary>
        [Test]
        public void Test1()
        {
            var route = new Route()
            {
                Shape = new Coordinate[]
                {
                    new Coordinate()
                    {
                        Latitude = 1,
                        Longitude = 2
                    },
                    new Coordinate()
                    {
                        Latitude = 3,
                        Longitude = 4
                    }
                },
                ShapeMeta = new Route.Meta[]
                {
                    new Route.Meta()
                    {
                        Shape = 0
                    },
                    new Route.Meta()
                    {
                        Shape = 1,
                        Time = 60,
                        Distance = 1000
                    }
                },
                TotalDistance = 1000,
                TotalTime = 60
            };

            var aggregator = new RouteSegmentAggregator(route, (x, y) => null);
            aggregator.Run();

            Assert.IsTrue(aggregator.HasRun);
            Assert.IsTrue(aggregator.HasSucceeded);

            var result = aggregator.AggregatedRoute;

            Assert.IsNotNull(result);

            Assert.IsNotNull(result.Shape);
            Assert.AreEqual(2, result.Shape.Length);
            Assert.AreEqual(1, result.Shape[0].Latitude);
            Assert.AreEqual(2, result.Shape[0].Longitude);
            Assert.AreEqual(3, result.Shape[1].Latitude);
            Assert.AreEqual(4, result.Shape[1].Longitude);

            Assert.IsNotNull(result.ShapeMeta);
            Assert.AreEqual(2, result.ShapeMeta.Length);
            var meta = result.ShapeMeta[0];
            Assert.IsNull(meta.Attributes);
            Assert.AreEqual(0, meta.Shape);
            meta = result.ShapeMeta[1];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(1, meta.Shape);
            Assert.AreEqual(2, meta.Attributes.Count);
            Assert.AreEqual(1000, meta.Distance, 0.1);
            Assert.AreEqual(60, meta.Time, 0.1);

            Assert.AreEqual(result.ShapeMeta[result.ShapeMeta.Length - 1].Distance, result.TotalDistance);
            Assert.AreEqual(result.ShapeMeta[result.ShapeMeta.Length - 1].Time, result.TotalTime);
        }

        /// <summary>
        /// Tests a route with two segments.
        /// </summary>
        [Test]
        public void Test2()
        {
            var route = new Route()
            {
                Shape = new Coordinate[]
                {
                    new Coordinate(1, 2),
                    new Coordinate(3 ,4),
                    new Coordinate(5, 6)
                },
                ShapeMeta = new Route.Meta[]
                {
                    new Route.Meta()
                    {
                        Shape = 0
                    },
                    new Route.Meta()
                    {
                        Shape = 1,
                        Time = 60,
                        Distance = 1000
                    },
                    new Route.Meta()
                    {
                        Shape = 2,
                        Time = 120,
                        Distance = 2000
                    }
                },
                TotalDistance = 2000,
                TotalTime = 120
            };

            var aggregator = new RouteSegmentAggregator(route, (x, y) => null);
            aggregator.Run();

            Assert.IsTrue(aggregator.HasRun);
            Assert.IsTrue(aggregator.HasSucceeded);

            var result = aggregator.AggregatedRoute;

            Assert.IsNotNull(result);

            Assert.IsNotNull(result.Shape);
            Assert.AreEqual(3, result.Shape.Length);
            Assert.AreEqual(1, result.Shape[0].Latitude);
            Assert.AreEqual(2, result.Shape[0].Longitude);
            Assert.AreEqual(3, result.Shape[1].Latitude);
            Assert.AreEqual(4, result.Shape[1].Longitude);
            Assert.AreEqual(5, result.Shape[2].Latitude);
            Assert.AreEqual(6, result.Shape[2].Longitude);

            Assert.IsNotNull(result.ShapeMeta);
            Assert.AreEqual(3, result.ShapeMeta.Length);
            var meta = result.ShapeMeta[0];
            Assert.IsNull(meta.Attributes);
            Assert.AreEqual(0, meta.Shape);
            meta = result.ShapeMeta[1];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(1, meta.Shape);
            Assert.AreEqual(2, meta.Attributes.Count);
            Assert.AreEqual(1000, meta.Distance, 0.1);
            Assert.AreEqual(60, meta.Time, 0.1);
            meta = result.ShapeMeta[2];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(2, meta.Shape);
            Assert.AreEqual(2, meta.Attributes.Count);
            Assert.AreEqual(2000, meta.Distance, 0.1);
            Assert.AreEqual(120, meta.Time, 0.1);

            Assert.AreEqual(result.ShapeMeta[result.ShapeMeta.Length - 1].Distance, result.TotalDistance);
            Assert.AreEqual(result.ShapeMeta[result.ShapeMeta.Length - 1].Time, result.TotalTime);

            aggregator = new RouteSegmentAggregator(route, (x, y) => y);
            aggregator.Run();

            Assert.IsTrue(aggregator.HasRun);
            Assert.IsTrue(aggregator.HasSucceeded);

            result = aggregator.AggregatedRoute;

            Assert.IsNotNull(result);

            Assert.IsNotNull(result.Shape);
            Assert.AreEqual(3, result.Shape.Length);
            Assert.AreEqual(1, result.Shape[0].Latitude);
            Assert.AreEqual(2, result.Shape[0].Longitude);
            Assert.AreEqual(3, result.Shape[1].Latitude);
            Assert.AreEqual(4, result.Shape[1].Longitude);
            Assert.AreEqual(5, result.Shape[2].Latitude);
            Assert.AreEqual(6, result.Shape[2].Longitude);

            Assert.IsNotNull(result.ShapeMeta);
            Assert.AreEqual(2, result.ShapeMeta.Length);
            meta = result.ShapeMeta[0];
            Assert.IsNull(meta.Attributes);
            Assert.AreEqual(0, meta.Shape);
            meta = result.ShapeMeta[1];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(2, meta.Shape);
            Assert.AreEqual(2, meta.Attributes.Count);
            Assert.AreEqual(2000, meta.Distance, 0.1);
            Assert.AreEqual(120, meta.Time, 0.1);

            Assert.AreEqual(result.ShapeMeta[result.ShapeMeta.Length - 1].Distance, result.TotalDistance);
            Assert.AreEqual(result.ShapeMeta[result.ShapeMeta.Length - 1].Time, result.TotalTime);
        }

        /// <summary>
        /// Tests a route with segments with different vehicle profiles and aggregated according to those profiles.
        /// </summary>
        [Test]
        public void Test2Modal()
        {
            var route = new Route()
            {
                Shape = new Coordinate[]
                {
                    new Coordinate(1, 2),
                    new Coordinate(3, 4),
                    new Coordinate(5, 6),
                    new Coordinate(7, 8),
                    new Coordinate(9, 10),
                    new Coordinate(11, 12),
                    new Coordinate(13, 14)
                },
                ShapeMeta = new Route.Meta[]
                {
                    new Route.Meta()
                    {
                        Shape = 0
                    },
                    new Route.Meta()
                    {
                        Shape = 1,
                        Time = 60,
                        Distance = 1000,
                        Profile = "Car"
                    },
                    new Route.Meta()
                    {
                        Shape = 2,
                        Time = 120,
                        Distance = 2000,
                        Profile = "Car"
                    },
                    new Route.Meta()
                    {
                        Shape = 3,
                        Time = 180,
                        Distance = 3000,
                        Profile = "Car"
                    },
                    new Route.Meta()
                    {
                        Shape = 4,
                        Time = 240,
                        Distance = 4000,
                        Profile = "Car"
                    },
                    new Route.Meta()
                    {
                        Shape = 5,
                        Time = 300,
                        Distance = 5000,
                        Profile = "Car"
                    },
                    new Route.Meta()
                    {
                        Shape = 6,
                        Time = 360,
                        Distance = 6000,
                        Profile = "Car"
                    }
                },
                TotalDistance = 6000,
                TotalTime = 360
            };

            var aggregator = new RouteSegmentAggregator(route, RouteSegmentAggregator.ModalAggregator);
            aggregator.Run();

            Assert.IsTrue(aggregator.HasRun);
            Assert.IsTrue(aggregator.HasSucceeded);

            var result = aggregator.AggregatedRoute;

            Assert.IsNotNull(result);

            Assert.IsNotNull(result.Shape);
            Assert.AreEqual(7, result.Shape.Length);
            Assert.AreEqual(1, result.Shape[0].Latitude);
            Assert.AreEqual(2, result.Shape[0].Longitude);
            Assert.AreEqual(3, result.Shape[1].Latitude);
            Assert.AreEqual(4, result.Shape[1].Longitude);
            Assert.AreEqual(5, result.Shape[2].Latitude);
            Assert.AreEqual(6, result.Shape[2].Longitude);
            Assert.AreEqual(7, result.Shape[3].Latitude);
            Assert.AreEqual(8, result.Shape[3].Longitude);
            Assert.AreEqual(9, result.Shape[4].Latitude);
            Assert.AreEqual(10, result.Shape[4].Longitude);
            Assert.AreEqual(11, result.Shape[5].Latitude);
            Assert.AreEqual(12, result.Shape[5].Longitude);
            Assert.AreEqual(13, result.Shape[6].Latitude);
            Assert.AreEqual(14, result.Shape[6].Longitude);

            Assert.IsNotNull(result.ShapeMeta);
            Assert.AreEqual(2, result.ShapeMeta.Length);
            var meta = result.ShapeMeta[0];
            Assert.IsNull(meta.Attributes);
            Assert.AreEqual(0, meta.Shape);
            meta = result.ShapeMeta[1];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(6, meta.Shape);
            Assert.AreEqual(3, meta.Attributes.Count);
            Assert.AreEqual(6000, meta.Distance, 0.1);
            Assert.AreEqual(360, meta.Time, 0.1);
            Assert.AreEqual("Car", meta.Profile);

            Assert.AreEqual(result.ShapeMeta[result.ShapeMeta.Length - 1].Distance, result.TotalDistance);
            Assert.AreEqual(result.ShapeMeta[result.ShapeMeta.Length - 1].Time, result.TotalTime);

            route = new Route()
            {
                Shape = new Coordinate[]
                {
                    new Coordinate(1, 2),
                    new Coordinate(3, 4),
                    new Coordinate(5, 6),
                    new Coordinate(7, 8),
                    new Coordinate(9, 10)
                },
                ShapeMeta = new Route.Meta[]
                {
                    new Route.Meta()
                    {
                        Shape = 0
                    },
                    new Route.Meta()
                    {
                        Shape = 1,
                        Time = 60,
                        Distance = 1000,
                        Profile = "Car"
                    },
                    new Route.Meta()
                    {
                        Shape = 2,
                        Time = 120,
                        Distance = 2000,
                        Profile = "Car"
                    },
                    new Route.Meta()
                    {
                        Shape = 3,
                        Time = 180,
                        Distance = 3000,
                        Profile = "Bus.Transit"
                    },
                    new Route.Meta()
                    {
                        Shape = 4,
                        Time = 240,
                        Distance = 4000,
                        Profile = "Pedestrian"
                    }
                },
                TotalDistance = 4000,
                TotalTime = 240
            };

            aggregator = new RouteSegmentAggregator(route, RouteSegmentAggregator.ModalAggregator);
            aggregator.Run();

            Assert.IsTrue(aggregator.HasRun);
            Assert.IsTrue(aggregator.HasSucceeded);

            result = aggregator.AggregatedRoute;

            Assert.IsNotNull(result);

            Assert.IsNotNull(result.Shape);
            Assert.AreEqual(5, result.Shape.Length);
            Assert.AreEqual(1, result.Shape[0].Latitude);
            Assert.AreEqual(2, result.Shape[0].Longitude);
            Assert.AreEqual(3, result.Shape[1].Latitude);
            Assert.AreEqual(4, result.Shape[1].Longitude);
            Assert.AreEqual(5, result.Shape[2].Latitude);
            Assert.AreEqual(6, result.Shape[2].Longitude);
            Assert.AreEqual(7, result.Shape[3].Latitude);
            Assert.AreEqual(8, result.Shape[3].Longitude);
            Assert.AreEqual(9, result.Shape[4].Latitude);
            Assert.AreEqual(10, result.Shape[4].Longitude);

            Assert.IsNotNull(result.ShapeMeta);
            Assert.AreEqual(4, result.ShapeMeta.Length);
            meta = result.ShapeMeta[0];
            Assert.IsNull(meta.Attributes);
            Assert.AreEqual(0, meta.Shape);
            meta = result.ShapeMeta[1];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(2, meta.Shape);
            Assert.AreEqual(3, meta.Attributes.Count);
            Assert.AreEqual(2000, meta.Distance, 0.1);
            Assert.AreEqual(120, meta.Time, 0.1);
            Assert.AreEqual("Car", meta.Profile);
            meta = result.ShapeMeta[2];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(3, meta.Shape);
            Assert.AreEqual(3, meta.Attributes.Count);
            Assert.AreEqual(3000, meta.Distance, 0.1);
            Assert.AreEqual(180, meta.Time, 0.1);
            Assert.AreEqual("Bus.Transit", meta.Profile);
            meta = result.ShapeMeta[3];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(4, meta.Shape);
            Assert.AreEqual(3, meta.Attributes.Count);
            Assert.AreEqual(4000, meta.Distance, 0.1);
            Assert.AreEqual(240, meta.Time, 0.1);
            Assert.AreEqual("Pedestrian", meta.Profile);

            Assert.AreEqual(result.ShapeMeta[result.ShapeMeta.Length - 1].Distance, result.TotalDistance);
            Assert.AreEqual(result.ShapeMeta[result.ShapeMeta.Length - 1].Time, result.TotalTime);
        }
    }
}