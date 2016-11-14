// Itinero - Routing for .NET
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

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
    }
}