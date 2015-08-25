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
using NUnit.Framework;
using OsmSharp.Math.Geo;
using OsmSharp.Routing;
using OsmSharp.Routing.Instructions;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Vehicles;

namespace OsmSharp.Test.Unittests.Routing.Instructions
{
    /// <summary>
    /// Holds a number of unittests for generating routing instructions for tiny pieces of routes.
    /// </summary>
    [TestFixture]
    public class InstructionTests
    {
        /// <summary>
        /// Tests a simple turn.
        /// </summary>
        [Test]
        public void TestSimpleTurn()
        {
            var route = new Route();
            route.Vehicle = Vehicle.Car.UniqueName;
            route.Segments = new RouteSegment[3];
            route.Segments[0] = new RouteSegment()
            {
                Distance = 0,
                Latitude = 50.999f,
                Longitude = 4,
                Points = new RoutePoint[] { 
                    new RoutePoint() 
                    {
                        Latitude = 50.999f,
                        Longitude = 4,
                        Name = "Start"
                    }},
                SideStreets = null,
                Type = RouteSegmentType.Start
            };
            route.Segments[1] = new RouteSegment()
            {
                Distance = 0,
                Latitude = 51,
                Longitude = 4,
                Tags = new RouteTags[] {
                    new RouteTags() { Key = "name", Value = "Street A" },
                    new RouteTags() { Key = "highway", Value = "residential" }
                },
                Type = RouteSegmentType.Along,
                SideStreets = new RouteSegmentBranch[] {
                    new RouteSegmentBranch() { 
                        Latitude = 51, 
                        Longitude = 3.999f,
                        Tags = new RouteTags[] {
                            new RouteTags() { Key = "name", Value = "Street B" },
                            new RouteTags() { Key = "highway", Value = "residential" }
                        },
                        Name = "Street B"
                    }
                }
            };
            route.Segments[2] = new RouteSegment()
            {
                Distance = 0,
                Latitude = 51,
                Longitude = 4.001f,
                Tags = new RouteTags[] {
                    new RouteTags() { Key = "name", Value = "Street B" },
                    new RouteTags() { Key = "highway", Value = "residential" }
                },
                Type = RouteSegmentType.Stop,
                Points = new RoutePoint[] { 
                    new RoutePoint() 
                    {
                        Latitude = 51,
                        Longitude = 4.001f,
                        Name = "Stop"
                    }},
            };

            // create the language generator.
            var languageGenerator = new LanguageTestGenerator();

            // generate instructions.
            List<Instruction> instructions = InstructionGenerator.Generate(route, new OsmRoutingInterpreter(), languageGenerator);
            Assert.AreEqual(3, instructions.Count);
            Assert.AreEqual("GenerateDirectTurn:0_Right_0", instructions[1].Text);
        }

        /// <summary>
        /// Tests a simple roundabout instruction.
        /// </summary>
        [Test]
        public void TestRoundabout()
        {
            var westWest = new GeoCoordinate(51, 3.998);
            var west = new GeoCoordinate(51, 3.999);
            var eastEast = new GeoCoordinate(51, 4.002);
            var east = new GeoCoordinate(51, 4.001);
            var north = new GeoCoordinate(51.001, 4);
            var northNorth = new GeoCoordinate(51.002, 4);
            var south = new GeoCoordinate(50.999, 4);
            var southSouth = new GeoCoordinate(50.998, 4);
            var center = new GeoCoordinate(51, 4);

            var route = new Route();
            route.Vehicle = Vehicle.Car.UniqueName;
            route.Segments = new RouteSegment[5];
            route.Segments[0] = new RouteSegment()
            {
                Distance = 0,
                Latitude = (float)southSouth.Latitude,
                Longitude = (float)southSouth.Longitude,
                Points = new RoutePoint[] { 
                    new RoutePoint() 
                    {
                        Latitude = (float)southSouth.Latitude,
                        Longitude = (float)southSouth.Longitude,
                        Name = "Start"
                    }},
                SideStreets = null,
                Type = RouteSegmentType.Start
            };
            route.Segments[1] = new RouteSegment()
            {
                Distance = 0,
                Latitude = (float)south.Latitude,
                Longitude = (float)south.Longitude,
                Type = RouteSegmentType.Along,
                Name = "SouthStreet",
                Tags = new RouteTags[] {
                    new RouteTags() { Key = "name", Value = "SouthStreet" },
                    new RouteTags() { Key = "highway", Value = "residential" }
                },
                SideStreets = new RouteSegmentBranch[] {
                    new RouteSegmentBranch() { 
                        Latitude = (float)west.Latitude,
                        Longitude = (float)west.Longitude,
                        Tags = new RouteTags[] {
                            new RouteTags() { Key = "junction", Value = "roundabout" },
                            new RouteTags() { Key = "highway", Value = "residential" }
                        },
                        Name = "Street B"
                    },
                    new RouteSegmentBranch() { 
                        Latitude = (float)east.Latitude,
                        Longitude = (float)east.Longitude,
                        Tags = new RouteTags[] {
                            new RouteTags() { Key = "junction", Value = "roundabout" },
                            new RouteTags() { Key = "highway", Value = "residential" }
                        },
                        Name = "Street B"
                    }
                }
            };
            route.Segments[2] = new RouteSegment()
            {
                Distance = 0,
                Latitude = (float)east.Latitude,
                Longitude = (float)east.Longitude,
                Type = RouteSegmentType.Along,
                Tags = new RouteTags[] {
                    new RouteTags() { Key = "junction", Value = "roundabout" },
                    new RouteTags() { Key = "highway", Value = "residential" }
                },
                SideStreets = new RouteSegmentBranch[] {
                    new RouteSegmentBranch() { 
                        Latitude = (float)eastEast.Latitude,
                        Longitude = (float)eastEast.Longitude,
                        Tags = new RouteTags[] {
                            new RouteTags() { Key = "name", Value = "EastStreet" },
                            new RouteTags() { Key = "highway", Value = "residential" }
                        },
                        Name = "EastStreet"
                    }
                }
            };
            route.Segments[3] = new RouteSegment()
            {
                Distance = 0,
                Latitude = (float)north.Latitude,
                Longitude = (float)north.Longitude,
                Type = RouteSegmentType.Along,
                Tags = new RouteTags[] {
                    new RouteTags() { Key = "junction", Value = "roundabout" },
                    new RouteTags() { Key = "highway", Value = "residential" }
                },
                SideStreets = new RouteSegmentBranch[] {
                    new RouteSegmentBranch() { 
                        Latitude = (float)west.Latitude,
                        Longitude = (float)west.Longitude,
                        Tags = new RouteTags[] {
                            new RouteTags() { Key = "junction", Value = "roundabout" },
                            new RouteTags() { Key = "highway", Value = "residential" }
                        }
                    }
                }
            };
            route.Segments[4] = new RouteSegment()
            {
                Distance = 0,
                Latitude = (float)northNorth.Latitude,
                Longitude = (float)northNorth.Longitude,
                Type = RouteSegmentType.Stop,
                Tags = new RouteTags[] {
                    new RouteTags() { Key = "name", Value = "NorthStreet" },
                    new RouteTags() { Key = "highway", Value = "residential" }
                },
                Points = new RoutePoint[] { 
                    new RoutePoint() 
                    {
                        Latitude = (float)north.Latitude,
                        Longitude = (float)north.Longitude,
                        Name = "Stop"
                    }}
            };

            // create the language generator.
            var languageGenerator = new LanguageTestGenerator();

            // generate instructions.
            var instructions = InstructionGenerator.Generate(route, new OsmRoutingInterpreter(), languageGenerator);
            Assert.AreEqual(3, instructions.Count);
            Assert.AreEqual("GenerateRoundabout:3", instructions[1].Text);
        }

        /// <summary>
        /// Tests a simple roundabout instruction but with an extra part of the route before the roundabout.
        /// </summary>
        [Test]
        public void TestRoundaboutExtended()
        {
            var westWest = new GeoCoordinate(51, 3.998);
            var west = new GeoCoordinate(51, 3.999);
            var eastEast = new GeoCoordinate(51, 4.002);
            var east = new GeoCoordinate(51, 4.001);
            var north = new GeoCoordinate(51.001, 4);
            var northNorth = new GeoCoordinate(51.002, 4);
            var south = new GeoCoordinate(50.999, 4);
            var southSouth = new GeoCoordinate(50.998, 4);
            var southSouthSouth = new GeoCoordinate(50.997, 4);
            var center = new GeoCoordinate(51, 4);

            var route = new Route();
            route.Vehicle = Vehicle.Car.UniqueName;
            route.Segments = new RouteSegment[6];
            route.Segments[0] = new RouteSegment()
            {
                Distance = 0,
                Latitude = (float)southSouth.Latitude,
                Longitude = (float)southSouth.Longitude,
                Points = new RoutePoint[] { 
                    new RoutePoint() 
                    {
                        Latitude = (float)southSouthSouth.Latitude,
                        Longitude = (float)southSouthSouth.Longitude,
                        Name = "Start"
                    }},
                SideStreets = null,
                Type = RouteSegmentType.Start
            };
            route.Segments[1] = new RouteSegment()
            {
                Distance = 0,
                Latitude = (float)southSouth.Latitude,
                Longitude = (float)southSouth.Longitude,
                Tags = new RouteTags[] {
                    new RouteTags() { Key = "name", Value = "SouthStreet" },
                    new RouteTags() { Key = "highway", Value = "residential" }
                },
                SideStreets = null,
                Type = RouteSegmentType.Along
            };
            route.Segments[2] = new RouteSegment()
            {
                Distance = 0,
                Latitude = (float)south.Latitude,
                Longitude = (float)south.Longitude,
                Type = RouteSegmentType.Along,
                Name = "SouthStreet",
                Tags = new RouteTags[] {
                    new RouteTags() { Key = "name", Value = "SouthStreet" },
                    new RouteTags() { Key = "highway", Value = "residential" }
                },
                SideStreets = new RouteSegmentBranch[] {
                    new RouteSegmentBranch() { 
                        Latitude = (float)west.Latitude,
                        Longitude = (float)west.Longitude,
                        Tags = new RouteTags[] {
                            new RouteTags() { Key = "junction", Value = "roundabout" },
                            new RouteTags() { Key = "highway", Value = "residential" }
                        },
                        Name = "Street B"
                    },
                    new RouteSegmentBranch() { 
                        Latitude = (float)east.Latitude,
                        Longitude = (float)east.Longitude,
                        Tags = new RouteTags[] {
                            new RouteTags() { Key = "junction", Value = "roundabout" },
                            new RouteTags() { Key = "highway", Value = "residential" }
                        },
                        Name = "Street B"
                    }
                }
            };
            route.Segments[3] = new RouteSegment()
            {
                Distance = 0,
                Latitude = (float)east.Latitude,
                Longitude = (float)east.Longitude,
                Type = RouteSegmentType.Along,
                Tags = new RouteTags[] {
                    new RouteTags() { Key = "junction", Value = "roundabout" },
                    new RouteTags() { Key = "highway", Value = "residential" }
                },
                SideStreets = new RouteSegmentBranch[] {
                    new RouteSegmentBranch() { 
                        Latitude = (float)eastEast.Latitude,
                        Longitude = (float)eastEast.Longitude,
                        Tags = new RouteTags[] {
                            new RouteTags() { Key = "name", Value = "EastStreet" },
                            new RouteTags() { Key = "highway", Value = "residential" }
                        },
                        Name = "EastStreet"
                    }
                }
            };
            route.Segments[4] = new RouteSegment()
            {
                Distance = 0,
                Latitude = (float)north.Latitude,
                Longitude = (float)north.Longitude,
                Type = RouteSegmentType.Along,
                Tags = new RouteTags[] {
                    new RouteTags() { Key = "junction", Value = "roundabout" },
                    new RouteTags() { Key = "highway", Value = "residential" }
                },
                SideStreets = new RouteSegmentBranch[] {
                    new RouteSegmentBranch() { 
                        Latitude = (float)west.Latitude,
                        Longitude = (float)west.Longitude,
                        Tags = new RouteTags[] {
                            new RouteTags() { Key = "junction", Value = "roundabout" },
                            new RouteTags() { Key = "highway", Value = "residential" }
                        }
                    }
                }
            };
            route.Segments[5] = new RouteSegment()
            {
                Distance = 0,
                Latitude = (float)northNorth.Latitude,
                Longitude = (float)northNorth.Longitude,
                Type = RouteSegmentType.Stop,
                Tags = new RouteTags[] {
                    new RouteTags() { Key = "name", Value = "NorthStreet" },
                    new RouteTags() { Key = "highway", Value = "residential" }
                },
                Points = new RoutePoint[] { 
                    new RoutePoint() 
                    {
                        Latitude = (float)north.Latitude,
                        Longitude = (float)north.Longitude,
                        Name = "Stop"
                    }}
            };

            // create the language generator.
            var languageGenerator = new LanguageTestGenerator();

            // generate instructions.
            List<Instruction> instructions = InstructionGenerator.Generate(route, new OsmRoutingInterpreter(), languageGenerator);
            Assert.AreEqual(3, instructions.Count);
            Assert.AreEqual("GenerateRoundabout:3", instructions[1].Text);
        }
    }
}