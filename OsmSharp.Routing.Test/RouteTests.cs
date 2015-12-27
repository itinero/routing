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
using System.Linq;
using System.Collections.Generic;

namespace OsmSharp.Routing.Test
{
    /// <summary>
    /// Contains tests for the route class and route extensions.
    /// </summary>
    [TestFixture]
    public class RouteTests
    {
        /// <summary>
        /// Tests route concatenation.
        /// </summary>
        [Test]
        public void TestConcatenate()
        {
            var route1 = new Route()
            {
                Segments = new List<RouteSegment>(new RouteSegment[]
                    {
                        new RouteSegment()
                        {
                            Latitude = 51.267819164340295f,
                            Longitude = 4.801352620124817f,
                            Distance = 0,
                            Time = 0
                        },
                        new RouteSegment()
                        {
                            Latitude = 51.26821857585588f,
                            Longitude = 4.801352620124817f,
                            Distance = 100,
                            Time = 60
                        }
                    }),
                Tags = new List<RouteTags>(),
                TotalDistance = 100,
                TotalTime = 60
            };
            var route2 = new Route()
            {
                Segments = new List<RouteSegment>(new RouteSegment[]
                    {
                        new RouteSegment()
                        {
                            Latitude = 51.26821857585588f,
                            Longitude = 4.801352620124817f,
                            Distance = 0,
                            Time = 0
                        },
                        new RouteSegment()
                        {
                            Latitude = 51.267819164340295f,
                            Longitude = 4.801352620124817f,
                            Distance = 100,
                            Time = 60
                        }
                    }),
                Tags = new List<RouteTags>(),
                TotalDistance = 100,
                TotalTime = 60
            };

            var route = route1.Concatenate(route2);

            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(3, route.Segments.Count);
            Assert.AreEqual(0, route.Segments[0].Distance);
            Assert.AreEqual(0, route.Segments[0].Time);
            Assert.AreEqual(100, route.Segments[1].Distance);
            Assert.AreEqual(60, route.Segments[1].Time);
            Assert.AreEqual(200, route.Segments[2].Distance);
            Assert.AreEqual(120, route.Segments[2].Time);
            Assert.AreEqual(200, route.TotalDistance);
            Assert.AreEqual(120, route.TotalTime);
        }

        /// <summary>
        /// Tests add or replace.
        /// </summary>
        [Test]
        public void TestAddOrReplace()
        {
            var tags1 = new RouteTags[] {
                new RouteTags()
                {
                    Key = "key1",
                    Value = "value1"
                },
                new RouteTags()
                {
                    Key = "key2",
                    Value = "value2"
                }
            };
            var tags2 = new RouteTags[] {
                new RouteTags()
                {
                    Key = "key1",
                    Value = "value10"
                },
                new RouteTags()
                {
                    Key = "key2",
                    Value = "value20"
                }
            };

            RouteTagsExtensions.AddOrReplace(ref tags1, tags2);
            var result = tags1.ConvertToTagsCollection();

            Assert.IsTrue(result.ContainsKeyValue("key1", "value10"));
            Assert.IsTrue(result.ContainsKeyValue("key2", "value20"));

            tags1 = new RouteTags[] {
                new RouteTags()
                {
                    Key = "key1",
                    Value = "value1"
                },
                new RouteTags()
                {
                    Key = "key2",
                    Value = "value2"
                }
            };
            tags2 = new RouteTags[] {
                new RouteTags()
                {
                    Key = "key1",
                    Value = "value10"
                },
                new RouteTags()
                {
                    Key = "key3",
                    Value = "value3"
                }
            };

            RouteTagsExtensions.AddOrReplace(ref tags1, tags2);
            result = tags1.ConvertToTagsCollection();

            Assert.IsTrue(result.ContainsKeyValue("key1", "value10"));
            Assert.IsTrue(result.ContainsKeyValue("key2", "value2"));
            Assert.IsTrue(result.ContainsKeyValue("key3", "value3"));

            tags1 = new RouteTags[] {
                new RouteTags()
                {
                    Key = "key1",
                    Value = "value1"
                },
                new RouteTags()
                {
                    Key = "key2",
                    Value = "value2"
                }
            };
            tags2 = new RouteTags[] {
                new RouteTags()
                {
                    Key = "key4",
                    Value = "value4"
                },
                new RouteTags()
                {
                    Key = "key3",
                    Value = "value3"
                }
            };

            RouteTagsExtensions.AddOrReplace(ref tags1, tags2);
            result = tags1.ConvertToTagsCollection();

            Assert.IsTrue(result.ContainsKeyValue("key1", "value1"));
            Assert.IsTrue(result.ContainsKeyValue("key2", "value2"));
            Assert.IsTrue(result.ContainsKeyValue("key3", "value3"));
            Assert.IsTrue(result.ContainsKeyValue("key4", "value4"));
        }
    }
}