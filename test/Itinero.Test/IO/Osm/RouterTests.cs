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

using Itinero.IO.Osm;
using Itinero.LocalGeo;
using Itinero.Osm.Vehicles;
using NUnit.Framework;
using OsmSharp;
using OsmSharp.Tags;

namespace Itinero.Test.IO.Osm
{
    /// <summary>
    /// Contains a series of regression tests.
    /// </summary>
    [TestFixture]
    public class RouterTests
    {
        /// <summary>
        /// An integration test that loads one way with the oneway tags but bicycle allowed in two directions.
        /// </summary>
        [Test]
        public void TestOnewayBicycleNo()
        {
            // the input osm-data.
            var osmGeos = new OsmGeo[]
            {
                new Node()
                {
                    Id = 1,
                    Latitude = 51.04963322083945f,
                    Longitude = 3.719692826271057f
                },
                new Node()
                {
                    Id = 2,
                    Latitude = 51.05062804602733f,
                    Longitude = 3.7198376655578613f
                },
                new Way()
                {
                    Id = 1,
                    Nodes = new long[]
                    {
                        1, 2
                    },
                    Tags = new TagsCollection(
                        new Tag("highway", "residential"),
                        new Tag("oneway", "yes"),
                        new Tag("oneway:bicycle", "no"))
                }
            };

            // build router db.
            var routerDb = new RouterDb();
            routerDb.LoadOsmData(osmGeos, Vehicle.Car, Vehicle.Bicycle);

            // test some routes.
            var router = new Router(routerDb);

            // confirm oneway is working for cars.
            var route = router.TryCalculate(Vehicle.Car.Fastest(),
                new Coordinate(51.04963322083945f, 3.719692826271057f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsFalse(route.IsError);
            route = router.TryCalculate(Vehicle.Car.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.719692826271057f));
            Assert.IsTrue(route.IsError);

            // confirm oneway:bicycle=no is working for bicycles.
            route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.04963322083945f, 3.719692826271057f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsFalse(route.IsError);
            route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.719692826271057f));
            Assert.IsFalse(route.IsError);
        }

        /// <summary>
        /// An integration test that loads one way with access=no and bicycle=yes.
        /// </summary>
        [Test]
        public void TestBicycleYesAccessNo()
        {
            // the input osm-data.
            var osmGeos = new OsmGeo[]
            {
                new Node()
                {
                    Id = 1,
                    Latitude = 51.04963322083945f,
                    Longitude = 3.719692826271057f
                },
                new Node()
                {
                    Id = 2,
                    Latitude = 51.05062804602733f,
                    Longitude = 3.7198376655578613f
                },
                new Way()
                {
                    Id = 1,
                    Nodes = new long[]
                    {
                        1, 2
                    },
                    Tags = new TagsCollection(
                        new Tag("highway", "residential"),
                        new Tag("access", "no"),
                        new Tag("bicycle", "yes"))
                }
            };

            // build router db.
            var routerDb = new RouterDb();
            routerDb.LoadOsmData(osmGeos, Vehicle.Car, Vehicle.Bicycle);

            // test some routes.
            var router = new Router(routerDb);

            // confirm access=no is working for cars.
            var route = router.TryCalculate(Vehicle.Car.Fastest(),
                new Coordinate(51.04963322083945f, 3.719692826271057f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsTrue(route.IsError);
            route = router.TryCalculate(Vehicle.Car.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.719692826271057f));
            Assert.IsTrue(route.IsError);

            // confirm access=no combined with bicycle=yes is working for bicycles.
            route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.04963322083945f, 3.719692826271057f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsFalse(route.IsError);
            route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.719692826271057f));
            Assert.IsFalse(route.IsError);
        }

        /// <summary>
        /// An integration test that loads one way with bicycle=no.
        /// </summary>
        [Test]
        public void TestBicycleNo()
        {
            // the input osm-data.
            var osmGeos = new OsmGeo[]
            {
                new Node()
                {
                    Id = 1,
                    Latitude = 51.04963322083945f,
                    Longitude = 3.719692826271057f
                },
                new Node()
                {
                    Id = 2,
                    Latitude = 51.05062804602733f,
                    Longitude = 3.7198376655578613f
                },
                new Way()
                {
                    Id = 1,
                    Nodes = new long[]
                    {
                        1, 2
                    },
                    Tags = new TagsCollection(
                        new Tag("highway", "residential"),
                        new Tag("bicycle", "no"))
                }
            };

            // build router db.
            var routerDb = new RouterDb();
            routerDb.LoadOsmData(osmGeos, Vehicle.Car, Vehicle.Bicycle, Vehicle.Pedestrian);

            // test some routes.
            var router = new Router(routerDb);

            // confirm it's not working for bicycles.
            var route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.04963322083945f, 3.719692826271057f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsTrue(route.IsError);
            route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.719692826271057f));
            Assert.IsTrue(route.IsError);
        }

        /// <summary>
        /// An integration test that loads one way with highway=pedestrian.
        /// </summary>
        [Test]
        public void TestBicycleHighwayPedestrian()
        {
            // the input osm-data.
            var osmGeos = new OsmGeo[]
            {
                new Node()
                {
                    Id = 1,
                    Latitude = 51.04963322083945f,
                    Longitude = 3.719692826271057f
                },
                new Node()
                {
                    Id = 2,
                    Latitude = 51.05062804602733f,
                    Longitude = 3.7198376655578613f
                },
                new Way()
                {
                    Id = 1,
                    Nodes = new long[]
                    {
                        1, 2
                    },
                    Tags = new TagsCollection(
                        new Tag("highway", "pedestrian"))
                }
            };

            // build router db.
            var routerDb = new RouterDb();
            routerDb.LoadOsmData(osmGeos, Vehicle.Bicycle);

            // test some routes.
            var router = new Router(routerDb);

            // confirm it's not working for bicycles.
            var route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.04963322083945f, 3.719692826271057f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsTrue(route.IsError);
            route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.719692826271057f));
            Assert.IsTrue(route.IsError);
        }

        /// <summary>
        /// An integration test that loads one way with highway=pedestrian and bicycle=yes.
        /// </summary>
        [Test]
        public void TestBicycleHighwayPedestrianBicycleYes()
        {
            // the input osm-data.
            var osmGeos = new OsmGeo[]
            {
                new Node()
                {
                    Id = 1,
                    Latitude = 51.04963322083945f,
                    Longitude = 3.719692826271057f
                },
                new Node()
                {
                    Id = 2,
                    Latitude = 51.05062804602733f,
                    Longitude = 3.7198376655578613f
                },
                new Way()
                {
                    Id = 1,
                    Nodes = new long[]
                    {
                        1, 2
                    },
                    Tags = new TagsCollection(
                        new Tag("highway", "pedestrian"),
                        new Tag("bicycle", "yes"))
                }
            };

            // build router db.
            var routerDb = new RouterDb();
            routerDb.LoadOsmData(osmGeos, Vehicle.Bicycle);

            // test some routes.
            var router = new Router(routerDb);

            // confirm it's working for bicycles.
            var route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.04963322083945f, 3.719692826271057f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsFalse(route.IsError);
            route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.719692826271057f));
            Assert.IsFalse(route.IsError);
        }

        /// <summary>
        /// An integration test that loads two overlapping ways, highway=pedestrian, and highway=residential
        /// </summary>
        [Test]
        public void TestOverlappingWaysPedestrianResidential()
        {
            // the input osm-data.
            var osmGeos = new OsmGeo[]
            {
                new Node()
                {
                    Id = 1,
                    Latitude = 51.04963322083945f,
                    Longitude = 3.719692826271057f
                },
                new Node()
                {
                    Id = 2,
                    Latitude = 51.05062804602733f,
                    Longitude = 3.7198376655578613f
                },
                new Way()
                {
                    Id = 2,
                    Nodes = new long[]
                    {
                        1, 2
                    },
                    Tags = new TagsCollection(
                        new Tag("highway", "pedestrian"))
                },
                new Way()
                {
                    Id = 1,
                    Nodes = new long[]
                    {
                        1, 2
                    },
                    Tags = new TagsCollection(
                        new Tag("highway", "residential"))
                }
            };

            // build router db.
            var routerDb = new RouterDb();
            routerDb.LoadOsmData(osmGeos, Vehicle.Car, Vehicle.Bicycle, Vehicle.Pedestrian);

            // test some routes.
            var router = new Router(routerDb);

            // confirm it's working for cars.
            var route = router.TryCalculate(Vehicle.Car.Fastest(),
                new Coordinate(51.04963322083945f, 3.719692826271057f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsFalse(route.IsError);
            route = router.TryCalculate(Vehicle.Car.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.719692826271057f));
            Assert.IsFalse(route.IsError);

            // confirm it's working for pedestrians.
            route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.04963322083945f, 3.719692826271057f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsFalse(route.IsError);
            route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.719692826271057f));
            Assert.IsFalse(route.IsError);
        }

        /// <summary>
        /// An integration test that loads two overlapping ways, highway=pedestrian, and highway=residential
        /// </summary>
        [Test]
        public void TestOverlappingWaysResidentialPedestrian()
        {
            // the input osm-data.
            var osmGeos = new OsmGeo[]
            {
                new Node()
                {
                    Id = 1,
                    Latitude = 51.04963322083945f,
                    Longitude = 3.719692826271057f
                },
                new Node()
                {
                    Id = 2,
                    Latitude = 51.05062804602733f,
                    Longitude = 3.7198376655578613f
                },
                new Way()
                {
                    Id = 1,
                    Nodes = new long[]
                    {
                        1, 2
                    },
                    Tags = new TagsCollection(
                        new Tag("highway", "residential"))
                },
                new Way()
                {
                    Id = 2,
                    Nodes = new long[]
                    {
                        1, 2
                    },
                    Tags = new TagsCollection(
                        new Tag("highway", "pedestrian"))
                }
            };

            // build router db.
            var routerDb = new RouterDb();
            routerDb.LoadOsmData(osmGeos, Vehicle.Car, Vehicle.Bicycle, Vehicle.Pedestrian);

            // test some routes.
            var router = new Router(routerDb);

            // confirm it's working for cars.
            var route = router.TryCalculate(Vehicle.Car.Fastest(),
                new Coordinate(51.04963322083945f, 3.719692826271057f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsFalse(route.IsError);
            route = router.TryCalculate(Vehicle.Car.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.719692826271057f));
            Assert.IsFalse(route.IsError);

            // confirm it's working for pedestrians.
            route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.04963322083945f, 3.719692826271057f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsFalse(route.IsError);
            route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.719692826271057f));
            Assert.IsFalse(route.IsError);
        }

        /// <summary>
        /// An integration test that loads two overlapping ways, highway=cycleway, and highway=residential,bicycle=no
        /// </summary>
        [Test]
        public void TestOverlappingWaysResidentialCycleway()
        {
            // the input osm-data.
            var osmGeos = new OsmGeo[]
            {
                new Node()
                {
                    Id = 1,
                    Latitude = 51.04963322083945f,
                    Longitude = 3.719692826271057f
                },
                new Node()
                {
                    Id = 2,
                    Latitude = 51.05062804602733f,
                    Longitude = 3.7198376655578613f
                },
                new Way()
                {
                    Id = 1,
                    Nodes = new long[]
                    {
                        1, 2
                    },
                    Tags = new TagsCollection(
                        new Tag("highway", "residential"),
                        new Tag("bicycle", "no"))
                },
                new Way()
                {
                    Id = 2,
                    Nodes = new long[]
                    {
                        1, 2
                    },
                    Tags = new TagsCollection(
                        new Tag("highway", "cycleway"))
                }
            };

            // build router db.
            var routerDb = new RouterDb();
            routerDb.LoadOsmData(osmGeos, Vehicle.Car, Vehicle.Bicycle, Vehicle.Pedestrian);

            // test some routes.
            var router = new Router(routerDb);

            // confirm it's working for cars.
            var route = router.TryCalculate(Vehicle.Car.Fastest(),
                new Coordinate(51.04963322083945f, 3.719692826271057f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsFalse(route.IsError);
            route = router.TryCalculate(Vehicle.Car.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.719692826271057f));
            Assert.IsFalse(route.IsError);

            // confirm it's working for bicycle.
            route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.04963322083945f, 3.719692826271057f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsFalse(route.IsError);
            route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.719692826271057f));
            Assert.IsFalse(route.IsError);
        }

        /// <summary>
        /// Tests routing along a restricted via-node.
        /// </summary>
        [Test]
        public void TestRestrictionViaNode()
        {
            var location1 = new Coordinate(51.265137311403734f, 4.783644676208496f);
            var location2 = new Coordinate(51.264425704628850f, 4.784331321716309f);
            var location3 = new Coordinate(51.263975909757960f, 4.784958958625793f);
            var location4 = new Coordinate(51.264251157888160f, 4.785377383232116f);
            var location5 = new Coordinate(51.264758012937406f, 4.785699248313904f);

            var osmGeos = new OsmGeo[]
            {
                new Node()
                {
                    Id = 1,
                    Latitude = location1.Latitude,
                    Longitude = location1.Longitude
                },
                new Node()
                {
                    Id = 2,
                    Latitude = location2.Latitude,
                    Longitude = location2.Longitude
                },
                new Node()
                {
                    Id = 3,
                    Latitude = location3.Latitude,
                    Longitude = location3.Longitude
                },
                new Node()
                {
                    Id = 4,
                    Latitude = location4.Latitude,
                    Longitude = location4.Longitude
                },
                new Node()
                {
                    Id = 5,
                    Latitude = location5.Latitude,
                    Longitude = location5.Longitude
                },
                new Way()
                {
                    Id = 1,
                    Nodes = new long[]
                    {
                        1, 2
                    },
                    Tags = new TagsCollection(
                        new Tag("highway", "residential"))
                },
                new Way()
                {
                    Id = 2,
                    Nodes = new long[]
                    {
                        2, 3
                    },
                    Tags = new TagsCollection(
                        new Tag("highway", "residential"))
                },
                new Way()
                {
                    Id = 3,
                    Nodes = new long[]
                    {
                        3, 4
                    },
                    Tags = new TagsCollection(
                        new Tag("highway", "residential"))
                },
                new Way()
                {
                    Id = 4,
                    Nodes = new long[]
                    {
                        4, 5
                    },
                    Tags = new TagsCollection(
                        new Tag("highway", "residential"))
                },
                new Relation()
                {
                    Id = 1,
                    Members = new RelationMember[]
                    {
                        new RelationMember()
                        {
                            Id = 3,
                            Role = "via",
                            Type = OsmGeoType.Node
                        },
                        new RelationMember()
                        {
                            Id = 2,
                            Role = "from",
                            Type = OsmGeoType.Way
                        },
                        new RelationMember()
                        {
                            Id = 3,
                            Role = "to",
                            Type = OsmGeoType.Way
                        }
                    },
                    Tags = new TagsCollection(
                        new Tag("type", "restriction"),
                        new Tag("restriction", "no_right_turn"))
                }
            };
            
            // build router db.
            var routerDb = new RouterDb();
            routerDb.LoadOsmData(osmGeos, false, true, Vehicle.Car);

            // test some routes.
            var router = new Router(routerDb);

            // confirm it's working for cars only in one direction.
            var route = router.TryCalculate(Vehicle.Car.Fastest(),
                location1, location5);
            Assert.IsTrue(route.IsError);
            route = router.TryCalculate(Vehicle.Car.Fastest(),
                location5, location1);
            Assert.IsFalse(route.IsError);
        }
    }
}