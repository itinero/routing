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

using NUnit.Framework;
using OsmSharp.Geo.Geometries;
using OsmSharp.Routing.Algorithms.Routes;
using System.Collections.Generic;

namespace OsmSharp.Routing.Test.Algorithms.Routes
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
                Segments = new List<RouteSegment>(new RouteSegment[]
                    {
                        new RouteSegment()
                        {
                            Latitude = 1,
                            Longitude = 2
                        },
                        new RouteSegment()
                        {
                            Latitude = 3,
                            Longitude = 4,
                            Time = 60,
                            Distance = 1000
                        }
                    }),
                Tags = new List<RouteTags>(),
                TotalDistance = 0,
                TotalTime = 0
            };

            var aggregator = new RouteSegmentAggregator(route, (x, y) => null);
            aggregator.Run();

            Assert.IsTrue(aggregator.HasRun);
            Assert.IsTrue(aggregator.HasSucceeded);

            Assert.IsNotNull(aggregator.Features);
            var features = aggregator.Features;
            Assert.AreEqual(1, features.Count);
            var lineFeature = features[0];
            Assert.IsNotNull(lineFeature);
            Assert.IsInstanceOf<LineString>(lineFeature.Geometry);
            var line = lineFeature.Geometry as LineString;
            Assert.IsNotNull(line);
            Assert.AreEqual(1, line.Coordinates[0].Latitude);
            Assert.AreEqual(2, line.Coordinates[0].Longitude);
            Assert.AreEqual(3, line.Coordinates[1].Latitude);
            Assert.AreEqual(4, line.Coordinates[1].Longitude);
            var attributes = lineFeature.Attributes;
            Assert.IsTrue(attributes.ContainsKeyValue("time", 60.0));
            Assert.IsTrue(attributes.ContainsKeyValue("distance", 1000.0));
        }

        /// <summary>
        /// Tests a route with two segments.
        /// </summary>
        [Test]
        public void Test2()
        {
            var route = new Route()
            {
                Segments = new List<RouteSegment>(new RouteSegment[]
                    {
                        new RouteSegment()
                        {
                            Latitude = 1,
                            Longitude = 2
                        },
                        new RouteSegment()
                        {
                            Latitude = 3,
                            Longitude = 4,
                            Time = 60,
                            Distance = 1000
                        },
                        new RouteSegment()
                        {
                            Latitude = 5,
                            Longitude = 6,
                            Time = 120,
                            Distance = 2000
                        }
                    }),
                Tags = new List<RouteTags>(),
                TotalDistance = 0,
                TotalTime = 0
            };

            var aggregator = new RouteSegmentAggregator(route, (x, y) => null);
            aggregator.Run();

            Assert.IsTrue(aggregator.HasRun);
            Assert.IsTrue(aggregator.HasSucceeded);

            Assert.IsNotNull(aggregator.Features);
            var features = aggregator.Features;
            Assert.AreEqual(2, features.Count);

            var lineFeature = features[0];
            Assert.IsNotNull(lineFeature);
            Assert.IsInstanceOf<LineString>(lineFeature.Geometry);
            var line = lineFeature.Geometry as LineString;
            Assert.IsNotNull(line);
            Assert.AreEqual(1, line.Coordinates[0].Latitude);
            Assert.AreEqual(2, line.Coordinates[0].Longitude);
            Assert.AreEqual(3, line.Coordinates[1].Latitude);
            Assert.AreEqual(4, line.Coordinates[1].Longitude);
            var attributes = lineFeature.Attributes;
            Assert.IsTrue(attributes.ContainsKeyValue("time", 60.0));
            Assert.IsTrue(attributes.ContainsKeyValue("distance", 1000.0));

            lineFeature = features[1];
            Assert.IsNotNull(lineFeature);
            Assert.IsInstanceOf<LineString>(lineFeature.Geometry);
            line = lineFeature.Geometry as LineString;
            Assert.IsNotNull(line);
            Assert.AreEqual(3, line.Coordinates[0].Latitude);
            Assert.AreEqual(4, line.Coordinates[0].Longitude);
            Assert.AreEqual(5, line.Coordinates[1].Latitude);
            Assert.AreEqual(6, line.Coordinates[1].Longitude);
            attributes = lineFeature.Attributes;
            Assert.IsTrue(attributes.ContainsKeyValue("time", 120.0));
            Assert.IsTrue(attributes.ContainsKeyValue("distance", 2000.0));

            aggregator = new RouteSegmentAggregator(route, (x, y) => y);
            aggregator.Run();

            Assert.IsTrue(aggregator.HasRun);
            Assert.IsTrue(aggregator.HasSucceeded);

            Assert.IsNotNull(aggregator.Features);
            features = aggregator.Features;
            Assert.AreEqual(1, features.Count);

            lineFeature = features[0];
            Assert.IsNotNull(lineFeature);
            Assert.IsInstanceOf<LineString>(lineFeature.Geometry);
            line = lineFeature.Geometry as LineString;
            Assert.IsNotNull(line);
            Assert.AreEqual(1, line.Coordinates[0].Latitude);
            Assert.AreEqual(2, line.Coordinates[0].Longitude);
            Assert.AreEqual(3, line.Coordinates[1].Latitude);
            Assert.AreEqual(4, line.Coordinates[1].Longitude);
            Assert.AreEqual(5, line.Coordinates[2].Latitude);
            Assert.AreEqual(6, line.Coordinates[2].Longitude);
            attributes = lineFeature.Attributes;
            Assert.IsTrue(attributes.ContainsKeyValue("time", 120.0));
            Assert.IsTrue(attributes.ContainsKeyValue("distance", 2000.0));
        }

        /// <summary>
        /// Tests a route with segments with different vehicle profiles and aggregated according to those profiles.
        /// </summary>
        [Test]
        public void Test2Modal()
        {
            var route = new Route()
            {
                Segments = new List<RouteSegment>(new RouteSegment[]
                    {
                        new RouteSegment()
                        {
                            Latitude = 1,
                            Longitude = 2
                        },
                        new RouteSegment()
                        {
                            Latitude = 3,
                            Longitude = 4,
                            Time = 60,
                            Distance = 1000,
                            Profile = "Car"
                        },
                        new RouteSegment()
                        {
                            Latitude = 5,
                            Longitude = 6,
                            Time = 120,
                            Distance = 2000,
                            Profile = "Car"
                        },
                        new RouteSegment()
                        {
                            Latitude = 7,
                            Longitude = 8,
                            Time = 180,
                            Distance = 3000,
                            Profile = "Transit.Bus"
                        },
                        new RouteSegment()
                        {
                            Latitude = 9,
                            Longitude = 10,
                            Time = 240,
                            Distance = 4000,
                            Profile = "Transit.Bus"
                        },
                        new RouteSegment()
                        {
                            Latitude = 11,
                            Longitude = 12,
                            Time = 300,
                            Distance = 5000,
                            Profile = "Pedestrian"
                        },
                        new RouteSegment()
                        {
                            Latitude = 13,
                            Longitude = 14,
                            Time = 360,
                            Distance = 6000,
                            Profile = "Pedestrian"
                        }
                    }),
                Tags = new List<RouteTags>(),
                TotalDistance = 6000,
                TotalTime = 360
            };

            var aggregator = new RouteSegmentAggregator(route, RouteSegmentAggregator.ModalAggregator);
            aggregator.Run();

            Assert.IsTrue(aggregator.HasRun);
            Assert.IsTrue(aggregator.HasSucceeded);

            Assert.IsNotNull(aggregator.Features);
            var features = aggregator.Features;
            Assert.AreEqual(3, features.Count);

            var lineFeature = features[0];
            Assert.IsNotNull(lineFeature);
            Assert.IsInstanceOf<LineString>(lineFeature.Geometry);
            var line = lineFeature.Geometry as LineString;
            Assert.IsNotNull(line);
            Assert.AreEqual(1, line.Coordinates[0].Latitude);
            Assert.AreEqual(2, line.Coordinates[0].Longitude);
            Assert.AreEqual(3, line.Coordinates[1].Latitude);
            Assert.AreEqual(4, line.Coordinates[1].Longitude);
            Assert.AreEqual(5, line.Coordinates[2].Latitude);
            Assert.AreEqual(6, line.Coordinates[2].Longitude);
            var attributes = lineFeature.Attributes;
            Assert.IsTrue(attributes.ContainsKeyValue("time", 120.0));
            Assert.IsTrue(attributes.ContainsKeyValue("distance", 2000.0));

            lineFeature = features[1];
            Assert.IsNotNull(lineFeature);
            Assert.IsInstanceOf<LineString>(lineFeature.Geometry);
            line = lineFeature.Geometry as LineString;
            Assert.IsNotNull(line);
            Assert.AreEqual(5, line.Coordinates[0].Latitude);
            Assert.AreEqual(6, line.Coordinates[0].Longitude);
            Assert.AreEqual(7, line.Coordinates[1].Latitude);
            Assert.AreEqual(8, line.Coordinates[1].Longitude);
            Assert.AreEqual(9, line.Coordinates[2].Latitude);
            Assert.AreEqual(10, line.Coordinates[2].Longitude);
            attributes = lineFeature.Attributes;
            Assert.IsTrue(attributes.ContainsKeyValue("time", 240.0));
            Assert.IsTrue(attributes.ContainsKeyValue("distance", 4000.0));

            lineFeature = features[2];
            Assert.IsNotNull(lineFeature);
            Assert.IsInstanceOf<LineString>(lineFeature.Geometry);
            line = lineFeature.Geometry as LineString;
            Assert.IsNotNull(line);
            Assert.AreEqual(9, line.Coordinates[0].Latitude);
            Assert.AreEqual(10, line.Coordinates[0].Longitude);
            Assert.AreEqual(11, line.Coordinates[1].Latitude);
            Assert.AreEqual(12, line.Coordinates[1].Longitude);
            Assert.AreEqual(13, line.Coordinates[2].Latitude);
            Assert.AreEqual(14, line.Coordinates[2].Longitude);
            attributes = lineFeature.Attributes;
            Assert.IsTrue(attributes.ContainsKeyValue("time", 360.0));
            Assert.IsTrue(attributes.ContainsKeyValue("distance", 6000.0));

            route = new Route()
            {
                Segments = new List<RouteSegment>(new RouteSegment[]
                    {
                        new RouteSegment()
                        {
                            Latitude = 1,
                            Longitude = 2
                        },
                        new RouteSegment()
                        {
                            Latitude = 5,
                            Longitude = 6,
                            Time = 120,
                            Distance = 2000,
                            Profile = "Car"
                        },
                        new RouteSegment()
                        {
                            Latitude = 9,
                            Longitude = 10,
                            Time = 240,
                            Distance = 4000,
                            Profile = "Transit.Bus"
                        },
                        new RouteSegment()
                        {
                            Latitude = 13,
                            Longitude = 14,
                            Time = 360,
                            Distance = 6000,
                            Profile = "Pedestrian"
                        }
                    }),
                Tags = new List<RouteTags>(),
                TotalDistance = 6000,
                TotalTime = 360
            };

            aggregator = new RouteSegmentAggregator(route, RouteSegmentAggregator.ModalAggregator);
            aggregator.Run();

            Assert.IsTrue(aggregator.HasRun);
            Assert.IsTrue(aggregator.HasSucceeded);

            Assert.IsNotNull(aggregator.Features);
            features = aggregator.Features;
            Assert.AreEqual(3, features.Count);

            lineFeature = features[0];
            Assert.IsNotNull(lineFeature);
            Assert.IsInstanceOf<LineString>(lineFeature.Geometry);
            line = lineFeature.Geometry as LineString;
            Assert.IsNotNull(line);
            Assert.AreEqual(1, line.Coordinates[0].Latitude);
            Assert.AreEqual(2, line.Coordinates[0].Longitude);
            Assert.AreEqual(5, line.Coordinates[1].Latitude);
            Assert.AreEqual(6, line.Coordinates[1].Longitude);
            attributes = lineFeature.Attributes;
            Assert.IsTrue(attributes.ContainsKeyValue("time", 120.0));
            Assert.IsTrue(attributes.ContainsKeyValue("distance", 2000.0));

            lineFeature = features[1];
            Assert.IsNotNull(lineFeature);
            Assert.IsInstanceOf<LineString>(lineFeature.Geometry);
            line = lineFeature.Geometry as LineString;
            Assert.IsNotNull(line);
            Assert.AreEqual(5, line.Coordinates[0].Latitude);
            Assert.AreEqual(6, line.Coordinates[0].Longitude);
            Assert.AreEqual(9, line.Coordinates[1].Latitude);
            Assert.AreEqual(10, line.Coordinates[1].Longitude);
            attributes = lineFeature.Attributes;
            Assert.IsTrue(attributes.ContainsKeyValue("time", 240.0));
            Assert.IsTrue(attributes.ContainsKeyValue("distance", 4000.0));

            lineFeature = features[2];
            Assert.IsNotNull(lineFeature);
            Assert.IsInstanceOf<LineString>(lineFeature.Geometry);
            line = lineFeature.Geometry as LineString;
            Assert.IsNotNull(line);
            Assert.AreEqual(9, line.Coordinates[0].Latitude);
            Assert.AreEqual(10, line.Coordinates[0].Longitude);
            Assert.AreEqual(13, line.Coordinates[1].Latitude);
            Assert.AreEqual(14, line.Coordinates[1].Longitude);
            attributes = lineFeature.Attributes;
            Assert.IsTrue(attributes.ContainsKeyValue("time", 360.0));
            Assert.IsTrue(attributes.ContainsKeyValue("distance", 6000.0));
        }
    }
}