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
using OsmSharp.Routing.Navigation.Osm;
using OsmSharp.Routing.Test.Navigation.Language;
using System.Collections.Generic;

namespace OsmSharp.Routing.Test.Navigation.Osm
{
    /// <summary>
    /// Contains tests for the instruction car generator.
    /// </summary>
    [TestFixture]
    public class InstructionCarGeneratorTests
    {
        /// <summary>
        /// Tests generating instructions for a route with one segment.
        /// </summary>
        [Test]
        public void TestOneSegment()
        {
            var route = new Route()
            {
                Segments = new List<RouteSegment>(new RouteSegment[]
                    {
                        new RouteSegment()
                        {
                            Latitude = 51.267819164340295f,
                            Longitude = 4.801352620124817f
                        },
                        new RouteSegment()
                        {
                            Latitude = 51.26821857585588f,
                            Longitude = 4.801352620124817f
                        }
                    }),
                Tags = new List<RouteTags>(),
                TotalDistance = 0,
                TotalTime = 0
            };

            var generator = new InstructionCarGenerator(route, new MockLanguageReference());
            generator.Run();

            var instructions = generator.Instructions;
            Assert.IsNotNull(instructions);
            Assert.AreEqual(2, instructions.Count);
            Assert.AreEqual("start", instructions[0].Type);
            Assert.AreEqual("stop", instructions[1].Type);
        }

        /// <summary>
        /// Tests generating instructions for a route with two segments and one right turn.
        /// </summary>
        [Test]
        public void TestRightTurn()
        {
            var route = new Route()
            {
                Segments = new List<RouteSegment>(new RouteSegment[]
                    {
                        new RouteSegment()
                        {
                            Latitude = 51.26743653156547f,
                            Longitude = 4.8017871379852295f
                        },
                        new RouteSegment()
                        {
                            Latitude = 51.267879579750364f,
                            Longitude = 4.8017871379852295f,
                            SideStreets = new RouteSegmentBranch[]
                            {
                                new RouteSegmentBranch()
                                {
                                    Latitude = 51.267879579750364f,
                                    Longitude = 4.801073670387268f,
                                    Tags = null
                                }
                            }
                        },
                        new RouteSegment()
                        {
                            Latitude = 51.267879579750364f,
                            Longitude = 4.8025381565093985f
                        }
                    }),
                Tags = new List<RouteTags>(),
                TotalDistance = 0,
                TotalTime = 0
            };

            var generator = new InstructionCarGenerator(route, new MockLanguageReference());
            generator.Run();

            var instructions = generator.Instructions;
            Assert.IsNotNull(instructions);
            Assert.AreEqual(3, instructions.Count);
            Assert.AreEqual("start", instructions[0].Type);
            Assert.AreEqual("turn", instructions[1].Type);
            Assert.AreEqual("stop", instructions[2].Type);
        }

        /// <summary>
        /// Tests generating instructions for a route with a roundabout.
        /// </summary>
        [Test]
        public void TestRoundabout()
        {
            var route = new Route()
            {
                Segments = new List<RouteSegment>(new RouteSegment[]
                    {
                        new RouteSegment()
                        {
                            Latitude = 51.267258639804595f,
                            Longitude = 4.801588654518127f
                        },
                        new RouteSegment()
                        {
                            Latitude = 51.26771847181371f,
                            Longitude = 4.801631569862366f,
                            SideStreets = new RouteSegmentBranch[]
                            {
                                new RouteSegmentBranch()
                                {
                                    Latitude = 51.26800712313301f,
                                    Longitude = 4.801154136657715f,
                                    Tags = null
                                }
                            }
                        },
                        new RouteSegment()
                        {
                            Latitude = 51.26794670789062f,
                            Longitude = 4.802243113517761f,
                            SideStreets = new RouteSegmentBranch[]
                            {
                                new RouteSegmentBranch()
                                {
                                    Latitude = 51.26795006429507f,
                                    Longitude = 4.80301022529602f,
                                    Tags = null
                                }
                            },
                            Tags = new RouteTags[]
                            {
                                new RouteTags()
                                {
                                    Key = "junction",
                                    Value = "roundabout"
                                }
                            }
                        },
                        new RouteSegment()
                        {
                            Latitude = 51.26830919815357f,
                            Longitude = 4.801674485206603f,
                            SideStreets = new RouteSegmentBranch[]
                            {
                                new RouteSegmentBranch()
                                {
                                    Latitude = 51.26800712313301f,
                                    Longitude = 4.801154136657715f,
                                    Tags = null
                                }
                            },
                            Tags = new RouteTags[]
                            {
                                new RouteTags()
                                {
                                    Key = "junction",
                                    Value = "roundabout"
                                }
                            }
                        },
                        new RouteSegment()
                        {
                            Latitude = 51.26890327323482f,
                            Longitude = 4.801695942878722f
                        }
                    }),
                Tags = new List<RouteTags>(),
                TotalDistance = 0,
                TotalTime = 0
            };

            var generator = new InstructionCarGenerator(route, new MockLanguageReference());
            generator.Run();

            var instructions = generator.Instructions;
            Assert.IsNotNull(instructions);
            Assert.AreEqual(3, instructions.Count);
            Assert.AreEqual("start", instructions[0].Type);
            Assert.AreEqual("roundabout", instructions[1].Type);
            Assert.AreEqual("stop", instructions[2].Type);
        }
    }
}