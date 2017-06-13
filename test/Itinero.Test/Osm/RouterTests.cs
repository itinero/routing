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

namespace Itinero.Test.Osm
{
    /// <summary>
    /// Contains a series of integration/regression tests.
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
                new Coordinate(51.04963322083945f, 3.7196928262710570f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsFalse(route.IsError);
            route = router.TryCalculate(Vehicle.Car.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.7196928262710570f));
            Assert.IsTrue(route.IsError);

            // confirm oneway:bicycle=no is working for bicycles.
            route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.04963322083945f, 3.7196928262710570f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsFalse(route.IsError);
            route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.7196928262710570f));
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
                new Coordinate(51.04963322083945f, 3.7196928262710570f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsTrue(route.IsError);
            route = router.TryCalculate(Vehicle.Car.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.7196928262710570f));
            Assert.IsTrue(route.IsError);

            // confirm access=no combined with bicycle=yes is working for bicycles.
            route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.04963322083945f, 3.7196928262710570f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsFalse(route.IsError);
            route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.7196928262710570f));
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
                new Coordinate(51.04963322083945f, 3.7196928262710570f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsTrue(route.IsError);
            route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.7196928262710570f));
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
            routerDb.LoadOsmData(osmGeos, Vehicle.Car, Vehicle.Bicycle);

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
        /// An integration test that tests the vehicle access tags, vehicle=no.
        /// </summary>
        [Test]
        public void TestVehicleNo()
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
                        new Tag("vehicle", "no"))
                }
            };

            // build router db.
            var routerDb = new RouterDb();
            routerDb.LoadOsmData(osmGeos, Vehicle.Car, Vehicle.Bicycle, Vehicle.Pedestrian);

            // test some routes.
            var router = new Router(routerDb);

            // confirm it's working for pedestrians.
            var route = router.TryCalculate(Vehicle.Pedestrian.Fastest(),
                new Coordinate(51.04963322083945f, 3.719692826271057f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsFalse(route.IsError);
            route = router.TryCalculate(Vehicle.Pedestrian.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.719692826271057f));
            Assert.IsFalse(route.IsError);

            // confirm it's not working for bicycles.
            route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.04963322083945f, 3.719692826271057f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsTrue(route.IsError);
            route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.719692826271057f));
            Assert.IsTrue(route.IsError);

            // confirm it's not working for cars.
            route = router.TryCalculate(Vehicle.Car.Fastest(),
                new Coordinate(51.04963322083945f, 3.719692826271057f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsTrue(route.IsError);
            route = router.TryCalculate(Vehicle.Car.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.719692826271057f));
            Assert.IsTrue(route.IsError);
        }

        /// <summary>
        /// An integration test that tests the vehicle access tags, vehicle=no, foot=no.
        /// </summary>
        [Test]
        public void TestVehicleNoFootNo()
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
                        new Tag("vehicle", "no"),
                        new Tag("foot", "no"))
                }
            };

            // build router db.
            var routerDb = new RouterDb();
            routerDb.LoadOsmData(osmGeos, Vehicle.Car, Vehicle.Bicycle, Vehicle.Pedestrian);

            // test some routes.
            var router = new Router(routerDb);

            // confirm it's not working for pedestrians.
            var route = router.TryCalculate(Vehicle.Pedestrian.Fastest(),
                new Coordinate(51.04963322083945f, 3.719692826271057f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsTrue(route.IsError);
            route = router.TryCalculate(Vehicle.Pedestrian.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.719692826271057f));
            Assert.IsTrue(route.IsError);

            // confirm it's not working for bicycles.
            route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.04963322083945f, 3.719692826271057f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsTrue(route.IsError);
            route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.719692826271057f));
            Assert.IsTrue(route.IsError);

            // confirm it's not working for cars.
            route = router.TryCalculate(Vehicle.Car.Fastest(),
                new Coordinate(51.04963322083945f, 3.719692826271057f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsTrue(route.IsError);
            route = router.TryCalculate(Vehicle.Car.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.719692826271057f));
            Assert.IsTrue(route.IsError);
        }

        /// <summary>
        /// An integration test that tests the vehicle access tags, vehicle=no, (foot=yes), bicycle=yes.
        /// </summary>
        [Test]
        public void TestVehicleNoBicycleYesFootYes()
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
                        new Tag("vehicle", "no"),
                        new Tag("foot", "yes"),
                        new Tag("bicycle", "yes"))
                }
            };

            // build router db.
            var routerDb = new RouterDb();
            routerDb.LoadOsmData(osmGeos, Vehicle.Car, Vehicle.Bicycle, Vehicle.Pedestrian);

            // test some routes.
            var router = new Router(routerDb);

            // confirm it's working for pedestrians.
            var route = router.TryCalculate(Vehicle.Pedestrian.Fastest(),
                new Coordinate(51.04963322083945f, 3.719692826271057f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsFalse(route.IsError);
            route = router.TryCalculate(Vehicle.Pedestrian.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.719692826271057f));
            Assert.IsFalse(route.IsError);

            // confirm it's working for bicycles.
            route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.04963322083945f, 3.719692826271057f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsFalse(route.IsError);
            route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.719692826271057f));
            Assert.IsFalse(route.IsError);

            // confirm it's not working for cars.
            route = router.TryCalculate(Vehicle.Car.Fastest(),
                new Coordinate(51.04963322083945f, 3.719692826271057f),
                new Coordinate(51.05062804602733f, 3.7198376655578613f));
            Assert.IsTrue(route.IsError);
            route = router.TryCalculate(Vehicle.Car.Fastest(),
                new Coordinate(51.05062804602733f, 3.7198376655578613f),
                new Coordinate(51.04963322083945f, 3.719692826271057f));
            Assert.IsTrue(route.IsError);
        }

        /// <summary>
        /// An integration test to test handling the bicycle=use_sidepath tag. Expected is that this tag leads to the sidepath being used.
        /// </summary>
        [Test]
        public void TestBicycleUseSidepath()
        {
            // the input osm-data.
            var osmGeos = new OsmGeo[]
            {
                new Node()
                {
                    Id = 1,
                    Latitude = 12.608657753733688f,
                    Longitude = -7.966136634349823f
                },
                new Node()
                {
                    Id = 2,
                    Latitude = 12.608647283636317f,
                    Longitude = -7.967574298381805f
                },
                new Node()
                {
                    Id = 3,
                    Latitude = 12.609173405500497f,
                    Longitude = -7.966713309288025f
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
                        new Tag("bicycle", "use_sidepath"))
                },
                new Way()
                {
                    Id = 2,
                    Nodes = new long[]
                    {
                        1, 3, 2
                    },
                    Tags = new TagsCollection(
                        new Tag("highway", "cycleway"))
                }
            };

            // build router db.
            var routerDb = new RouterDb();
            routerDb.LoadOsmData(osmGeos, Vehicle.Car, Vehicle.Bicycle);

            // test some routes.
            var router = new Router(routerDb);

            // confirm it's working for bicycles but that the cycleway is used.
            var route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(12.608657753733688f, -7.966136634349823f),
                new Coordinate(12.608647283636317f, -7.967574298381805f));
            Assert.IsFalse(route.IsError);
            Assert.IsTrue(route.Value.TotalDistance > 180);
            route = router.TryCalculate(Vehicle.Bicycle.Fastest(),
                new Coordinate(12.608657753733688f, -7.966136634349823f),
                new Coordinate(12.608647283636317f, -7.967574298381805f));
            Assert.IsFalse(route.IsError);
            Assert.IsTrue(route.Value.TotalDistance > 180);

            // confirm it's not working for cars.
            route = router.TryCalculate(Vehicle.Car.Fastest(),
                new Coordinate(12.608657753733688f, -7.966136634349823f),
                new Coordinate(12.608647283636317f, -7.967574298381805f));
            Assert.IsFalse(route.IsError);
            Assert.IsTrue(route.Value.TotalDistance < 180);
            route = router.TryCalculate(Vehicle.Car.Fastest(),
                new Coordinate(12.608657753733688f, -7.966136634349823f),
                new Coordinate(12.608647283636317f, -7.967574298381805f));
            Assert.IsFalse(route.IsError);
            Assert.IsTrue(route.Value.TotalDistance < 180);
        }
        
        /// <summary>
        /// An integration test that loads two ways, on shorter with no cycle network and one longer with cycle network.
        /// </summary>
        [Test]
        public void TestBicycleWithCyclenetwork()
        {
            // the input osm-data.
            var osmGeos = new OsmGeo[]
            {
                new Node()
                {
                    Id = 1,
                    Latitude = 51.44979479062501f,
                    Longitude = 5.960791110992432f
                },
                new Node()
                {
                    Id = 2,
                    Latitude = 51.448825279548814f,
                    Longitude = 5.961284637451172f
                },
                new Node()
                {
                    Id = 3,
                    Latitude = 51.448577883770454f,
                    Longitude = 5.962904691696167f
                },
                new Way()
                {
                    Id = 1,
                    Nodes = new long[]
                    {
                        1, 3
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
                        new Tag("highway", "residential"))
                },
                new Way()
                {
                    Id = 3,
                    Nodes = new long[]
                    {
                        2, 3
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
                            Id = 2,
                            Type = OsmGeoType.Way,
                            Role = string.Empty
                        },
                        new RelationMember()
                        {
                            Id = 3,
                            Type = OsmGeoType.Way,
                            Role = string.Empty
                        }
                    },
                    Tags = new TagsCollection(
                        new Tag("type", "route"),
                        new Tag("route", "bicycle"))
                }
            };

            // build router db.
            var routerDb = new RouterDb();
            routerDb.LoadOsmData(osmGeos, Vehicle.Bicycle);

            // test some routes.
            var router = new Router(routerDb);

            // confirm it's not working for bicycles.
            var profile = Vehicle.Bicycle.Fastest() as Itinero.Profiles.IProfileInstance;
            var weightHandler = router.GetDefaultWeightHandler(profile);
            var route = router.TryCalculateRaw<float>(profile, weightHandler,
                router.Resolve(profile, new Coordinate(51.44979479062501f, 5.960791110992432f)),
                router.Resolve(profile, new Coordinate(51.44857788377045f, 5.962904691696167f)), null);
            Assert.IsFalse(route.IsError);
            Assert.IsNotNull(route.Value.From);
            Assert.IsNull(route.Value.From.From);
            profile = Vehicle.Bicycle.Profile("networks");
            weightHandler = router.GetDefaultWeightHandler(profile);
            route = router.TryCalculateRaw<float>(profile, weightHandler,
                router.Resolve(profile, new Coordinate(51.44979479062501f, 5.960791110992432f)),
                router.Resolve(profile, new Coordinate(51.44857788377045f, 5.962904691696167f)), null);
            Assert.IsFalse(route.IsError);
            Assert.IsNotNull(route.Value.From);
            Assert.IsNotNull(route.Value.From.From);
            Assert.IsNull(route.Value.From.From.From);
        }

        /// <summary>
        /// Tests a restriction that consists of a bollard on an uncontracted network with only simple restrictions.
        /// </summary>
        [Test]
        public void TestBollardUncontractedSimpleRestrictions()
        {
            var routerDb = new RouterDb();
            using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network10.osm"))
            {
                var source = new OsmSharp.Streams.XmlOsmStreamSource(stream);
                routerDb.LoadOsmData(source, Itinero.Osm.Vehicles.Vehicle.Car);
            }

            var profile = routerDb.GetSupportedProfile("car");

            var location1 = new Coordinate(51.22735657780183f, 4.834375977516174f);
            var location2 = new Coordinate(51.22702399914085f, 4.833759069442749f);
            var location3 = new Coordinate(51.22759509233135f, 4.833576679229736f);
            var router = new Router(routerDb);

            var resolved1 = router.Resolve(profile, location1);
            var resolved2 = router.Resolve(profile, location2);
            var resolved3 = router.Resolve(profile, location3);

            var route1 = router.TryCalculate(profile, resolved1, resolved2);
            Assert.IsTrue(route1.IsError);
            var route2 = router.TryCalculate(profile, resolved1, resolved3);
            Assert.IsFalse(route2.IsError);
        }

        /// <summary>
        /// Tests a restriction that consists of a bollard on a contracted network with only simple restrictions.
        /// </summary>
        [Test]
        public void TestBollardContractedSimpleRestrictions()
        {
            var routerDb = new RouterDb();
            using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network10.osm"))
            {
                var source = new OsmSharp.Streams.XmlOsmStreamSource(stream);
                routerDb.LoadOsmData(source, Itinero.Osm.Vehicles.Vehicle.Car);
            }

            var profile = routerDb.GetSupportedProfile("car");
            routerDb.AddContracted(profile, false);

            var location1 = new Coordinate(51.22735657780183f, 4.834375977516174f);
            var location2 = new Coordinate(51.22702399914085f, 4.833759069442749f);
            var location3 = new Coordinate(51.22759509233135f, 4.833576679229736f);
            var router = new Router(routerDb);

            var resolved1 = router.Resolve(profile, location1);
            var resolved2 = router.Resolve(profile, location2);
            var resolved3 = router.Resolve(profile, location3);

            var route1 = router.TryCalculate(profile, resolved1, resolved2);
            Assert.IsTrue(route1.IsError);
            var route2 = router.TryCalculate(profile, resolved1, resolved3);
            Assert.IsFalse(route2.IsError);
        }

        /// <summary>
        /// Tests a restriction that consists of a bollard on an uncontracted network with restrictions.
        /// </summary>
        [Test]
        public void TestBollardUncontractedRestrictions()
        {
            var routerDb = new RouterDb();
            using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network11.osm"))
            {
                var source = new OsmSharp.Streams.XmlOsmStreamSource(stream);
                routerDb.LoadOsmData(source, Itinero.Osm.Vehicles.Vehicle.Car);
            }

            var profile = routerDb.GetSupportedProfile("car");

            var location1 = new Coordinate(51.22735657780183f, 4.834375977516174f);
            var location2 = new Coordinate(51.22702399914085f, 4.833759069442749f);
            var location3 = new Coordinate(51.22759509233135f, 4.833576679229736f);
            var router = new Router(routerDb);

            var resolved1 = router.Resolve(profile, location1);
            var resolved2 = router.Resolve(profile, location2);
            var resolved3 = router.Resolve(profile, location3);

            var route1 = router.TryCalculate(profile, resolved1, resolved2);
            Assert.IsTrue(route1.IsError);
            var route2 = router.TryCalculate(profile, resolved1, resolved3);
            Assert.IsFalse(route2.IsError);
        }

        /// <summary>
        /// Tests a restriction that consists of a bollard on a contracted network with restrictions.
        /// </summary>
        [Test]
        public void TestBollardContractedRestrictions()
        {
            var routerDb = new RouterDb();
            using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network11.osm"))
            {
                var source = new OsmSharp.Streams.XmlOsmStreamSource(stream);
                routerDb.LoadOsmData(source, Itinero.Osm.Vehicles.Vehicle.Car);
            }

            var profile = routerDb.GetSupportedProfile("car");
            routerDb.AddContracted(profile);

            var location1 = new Coordinate(51.22735657780183f, 4.834375977516174f);
            var location2 = new Coordinate(51.22702399914085f, 4.833759069442749f);
            var location3 = new Coordinate(51.22759509233135f, 4.833576679229736f);
            var router = new Router(routerDb);

            var resolved1 = router.Resolve(profile, location1);
            var resolved2 = router.Resolve(profile, location2);
            var resolved3 = router.Resolve(profile, location3);

            var route1 = router.TryCalculate(profile, resolved1, resolved2);
            Assert.IsTrue(route1.IsError);
            var route2 = router.TryCalculate(profile, resolved1, resolved3);
            Assert.IsFalse(route2.IsError);
        }
    }
}