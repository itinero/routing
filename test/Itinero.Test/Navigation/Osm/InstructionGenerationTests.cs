// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
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
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Attributes;
using Itinero.LocalGeo;
using NUnit.Framework;

namespace Itinero.Test.Navigation.Osm
{
    /// <summary>
    /// Contains instruction generation tests.
    /// </summary>
    [TestFixture]
    public class InstructionGenerationTests
    {
        /// <summary>
        /// Tests the default instruction generation.
        /// </summary>
        [Test]
        public void TestNoMovements()
        {
            Itinero.Osm.Vehicles.Vehicle.RegisterVehicles();

            var route = new Route()
            {
                Shape = new Coordinate[]
                {
                    new Coordinate()
                    {
                        Latitude = 49.76851543353109f,
                        Longitude = 5.912189483642578f
                    },
                    new Coordinate()
                    {
                        Latitude = 49.768522363042294f,
                        Longitude = 5.9135788679122925f
                    },
                    new Coordinate()
                    {
                        Latitude = 49.76852929255253f,
                        Longitude = 5.914962887763977f
                    },
                    new Coordinate()
                    {
                        Latitude = 49.76852929255253f,
                        Longitude = 5.916352272033691f
                    },
                    new Coordinate()
                    {
                        Latitude = 49.768546616323725f,
                        Longitude = 5.917741656303405f
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
                        Shape = 2,
                        Distance = 100,
                        Time = 60
                    },
                    new Route.Meta()
                    {
                        Shape = 4,
                        Distance = 200,
                        Time = 120
                    }
                },
                Attributes = new AttributeCollection(),
                TotalDistance = 200,
                TotalTime = 120,
                Profile = "car"
            };

            var instructions = route.GenerateInstruction();
            Assert.IsNotNull(instructions);
            Assert.AreEqual(2, instructions.Count);
            Assert.AreEqual(0, instructions[0].Shape);
            Assert.AreEqual(4, instructions[1].Shape);
        }

        /// <summary>
        /// Tests the default instruction generation.
        /// </summary>
        [Test]
        public void TestTurnRight()
        {
            Itinero.Osm.Vehicles.Vehicle.RegisterVehicles();

            var route = new Route()
            {
                Shape = new Coordinate[]
                {
                    new Coordinate()
                    {
                        Latitude = 49.76851543353109f,
                        Longitude = 5.912189483642578f
                    },
                    new Coordinate()
                    {
                        Latitude = 49.768522363042294f,
                        Longitude = 5.9135788679122925f
                    },
                    new Coordinate()
                    {
                        Latitude = 49.76852929255253f,
                        Longitude = 5.914962887763977f
                    },
                    new Coordinate(49.767475995631294f, 5.915011167526245f)
                },
                ShapeMeta = new Route.Meta[]
                {
                    new Route.Meta()
                    {
                        Shape = 0
                    },
                    new Route.Meta()
                    {
                        Attributes = new AttributeCollection(new Attribute("highway", "residentail"),
                            new Attribute("name", "Street 1")),
                        Shape = 2,
                        Distance = 100,
                        Time = 60
                    },
                    new Route.Meta()
                    {
                        Attributes = new AttributeCollection(new Attribute("highway", "residentail"),
                            new Attribute("name", "Street 2")),
                        Shape = 3,
                        Distance = 200,
                        Time = 120
                    }
                },
                Branches = new Route.Branch[]
                {
                    new Route.Branch()
                    {
                        Attributes = new AttributeCollection(new Attribute("highway", "residentail")),
                        Coordinate = new Coordinate()
                        {
                            Latitude = 49.768546616323725f,
                            Longitude = 5.917741656303405f
                        },
                        Shape = 2
                    }
                },
                Attributes = new AttributeCollection(),
                TotalDistance = 200,
                TotalTime = 120,
                Profile = "car"
            };

            var instructions = route.GenerateInstruction();
            Assert.IsNotNull(instructions);
            Assert.AreEqual(3, instructions.Count);
            Assert.AreEqual(0, instructions[0].Shape);
            Assert.AreEqual("start", instructions[0].Type);
            Assert.AreEqual(2, instructions[1].Shape);
            Assert.AreEqual("turn", instructions[1].Type);
            Assert.AreEqual("Go right on Street 2.", instructions[1].Text);
            Assert.AreEqual(3, instructions[2].Shape);
            Assert.AreEqual("stop", instructions[2].Type);
        }
    }
}
