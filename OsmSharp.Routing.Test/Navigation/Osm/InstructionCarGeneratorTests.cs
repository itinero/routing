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
using OsmSharp.Routing.Attributes;
using OsmSharp.Routing.Geo;
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
                Shape = new Coordinate[]
                {
                    new Coordinate(51.267819164340295f, 4.801352620124817f),
                    new Coordinate(51.268218575855880f, 4.801352620124817f)
                },
                ShapeMeta = new Route.Meta[]
                {
                    new Route.Meta()
                    {
                        Shape = 0
                    },
                    new Route.Meta()
                    {
                        Shape = 1
                    }
                },
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
                Shape = new Coordinate[]
                {
                    new Coordinate(51.267436531565470f, 4.8017871379852295f),
                    new Coordinate(51.267879579750364f, 4.8017871379852295f),
                    new Coordinate(51.267879579750364f, 4.8025381565093985f)
                },
                ShapeMeta = new Route.Meta[]
                {
                    new Route.Meta()
                    {
                        Shape = 0
                    },
                    new Route.Meta()
                    {
                        Shape = 1
                    },
                    new Route.Meta()
                    {
                        Shape = 2
                    }
                },
                Branches = new Route.Branch[]
                {
                    new Route.Branch()
                    {
                        Shape = 1,
                        Coordinate = new Coordinate(51.267879579750364f, 4.801073670387268f)
                    }
                },
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
                Shape = new Coordinate[]
                {
                    new Coordinate(51.267258639804595f, 4.801588654518127f),
                    new Coordinate(51.267718471813710f, 4.801631569862366f),
                    new Coordinate(51.267946707890620f, 4.802243113517761f),
                    new Coordinate(51.268309198153570f, 4.801674485206603f),
                    new Coordinate(51.268903273234820f, 4.801695942878722f),
                },
                ShapeMeta = new Route.Meta[]
                {
                    new Route.Meta()
                    {
                        Shape = 0
                    },
                    new Route.Meta()
                    {
                        Shape = 1
                    },
                    new Route.Meta()
                    {
                        Shape = 2,
                        Attributes = new AttributeCollection(
                            new Attribute("junction", "roundabout"))
                    },
                    new Route.Meta()
                    {
                        Shape = 3,
                        Attributes = new AttributeCollection(
                            new Attribute("junction", "roundabout"))
                    },
                    new Route.Meta()
                    {
                        Shape = 4
                    }
                },
                Branches = new Route.Branch[]
                {
                    new Route.Branch()
                    {
                        Shape = 2,
                        Coordinate = new Coordinate(51.26800712313301f, 4.801154136657715f)
                    },
                    new Route.Branch()
                    {
                        Shape = 3,
                        Coordinate = new Coordinate(51.26795006429507f, 4.803010225296020f)
                    }
                },
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

        /// <summary>
        /// Tets generating instructions for test route 1.
        /// </summary>
        [Test]
        public void TestRoute1()
        {
            var route = RouteExtensions.ReadXml(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "OsmSharp.Routing.Test.test_data.routes.route1.xml"));

            var generator = new InstructionCarGenerator(route, new MockLanguageReference());
            generator.Run();

            var instructions = generator.Instructions;
            Assert.IsNotNull(instructions);
            Assert.AreEqual(8, instructions.Count);
        }
    }
}