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

using System;
using Itinero.Attributes;
using Itinero.LocalGeo;
using NUnit.Framework;
using Attribute = Itinero.Attributes.Attribute;

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

            var instructions = route.GenerateInstructions();
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
                        Attributes = new AttributeCollection(new Attribute("highway", "residential"),
                            new Attribute("name", "Street 1")),
                        Shape = 2,
                        Distance = 100,
                        Time = 60
                    },
                    new Route.Meta()
                    {
                        Attributes = new AttributeCollection(new Attribute("highway", "residential"),
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
                        Attributes = new AttributeCollection(new Attribute("highway", "residential")),
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

            var instructions = route.GenerateInstructions();
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
        
        /// <summary>
        /// Tests the default instruction generation extension method when a non-matching profile is given.
        /// </summary>
        [Test]
        public void TestExtensionWithInvalidProfile()
        {
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
                        Attributes = new AttributeCollection(new Attribute("highway", "residential"),
                            new Attribute("name", "Street 1")),
                        Shape = 2,
                        Distance = 100,
                        Time = 60
                    },
                    new Route.Meta()
                    {
                        Attributes = new AttributeCollection(new Attribute("highway", "residential"),
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
                        Attributes = new AttributeCollection(new Attribute("highway", "residential")),
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

            Assert.Catch<Exception>(() => 
            { 
                var instructions = route.GenerateInstructions(Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest());
            });
        }

        /// <summary>
        /// A regression test for 'route2.json'. See issue:
        ///
        /// https://github.com/itinero/routing/issues/194
        /// </summary>
        [Test]
        public void TestRoute2_Regression_ThrowsWhenProfileNotRegistered()
        {
            var route = TestRouteBuilder.BuildRoute(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.routes.route2.json"));
            
            Assert.IsNotNull(route);
            Itinero.Profiles.Profile.ClearRegistered();
            Assert.Catch<Exception>(() =>
            {
                var instructions = route.GenerateInstructions();
            });
        }

        /// <summary>
        /// A regression test for 'route2.json'. See issue:
        ///
        /// https://github.com/itinero/routing/issues/194
        /// </summary>
        [Test]
        public void TestRoute2_Regression_WhenRegistered()
        {
            Itinero.Osm.Vehicles.Vehicle.RegisterVehicles();
            
            var route = TestRouteBuilder.BuildRoute(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.routes.route2.json"));
            
            Assert.IsNotNull(route);
            var instructions = route.GenerateInstructions();
        }

        /// <summary>
        /// A regression test for 'route2.json'. See issue:
        ///
        /// https://github.com/itinero/routing/issues/194
        /// </summary>
        [Test]
        public void TestRoute2_Regression_WhenExplicitlyProvided()
        {
            var route = TestRouteBuilder.BuildRoute(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.routes.route2.json"));
            
            Assert.IsNotNull(route);
            
            Itinero.Profiles.Profile.ClearRegistered();
            var instructions = route.GenerateInstructions(Itinero.Osm.Vehicles.Vehicle.Car.Fastest());
        }
    }
}
