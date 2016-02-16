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
using OsmSharp.Routing.Geo;
using OsmSharp.Routing.Navigation;
using OsmSharp.Routing.Navigation.Language;
using OsmSharp.Routing.Test.Navigation.Language;

namespace OsmSharp.Routing.Test.Navigation
{
    /// <summary>
    /// Contains tests for the instruction generator.
    /// </summary>
    [TestFixture]
    public class InstructionGeneratorTests
    {
        /// <summary>
        /// Tests generating instructions.
        /// </summary>
        [Test]
        public void TestGenerateInstruction()
        {
            var route = new Route()
            {
                Shape = new Coordinate[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, 0),
                    new Coordinate(0, 0)
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
                TotalDistance = 0,
                TotalTime = 0
            };

            var generator = new InstructionGenerator<string>(route,
                new InstructionGenerator<string>.TryGetDelegate[]
                {
                    (RoutePosition pos, ILanguageReference langRef, out string instruction) =>
                        {
                            instruction = string.Format("Instruction {0}", pos.Shape);
                            return 1;
                        }
                }, new MockLanguageReference());
            generator.Run();

            var instructions = generator.Instructions;
            Assert.IsNotNull(instructions);
            Assert.AreEqual(3, instructions.Count);
            Assert.AreEqual(string.Format("Instruction {0}", 0), instructions[0]);
            Assert.AreEqual(string.Format("Instruction {0}", 1), instructions[1]);
            Assert.AreEqual(string.Format("Instruction {0}", 2), instructions[2]);
        }

        /// <summary>
        /// Tests overwrite previous instructions.
        /// </summary>
        [Test]
        public void TestOverwritingInstructions()
        {
            var route = new Route()
            {
                Shape = new Coordinate[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, 0),
                    new Coordinate(0, 0)
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
                TotalDistance = 0,
                TotalTime = 0
            };

            var generator = new InstructionGenerator<string>(route,
                new InstructionGenerator<string>.TryGetDelegate[]
                {
                    (RoutePosition pos, ILanguageReference langRef, out string instruction) =>
                        {
                            if(pos.Shape == 2)
                            {
                                instruction = "The one and only instruction!";
                                return 3;
                            }
                            instruction = string.Format("Instruction {0}", pos.Shape);
                            return 1;
                        }
                }, new MockLanguageReference());
            generator.Run();

            var instructions = generator.Instructions;
            Assert.IsNotNull(instructions);
            Assert.AreEqual(1, instructions.Count);
            Assert.AreEqual("The one and only instruction!", instructions[0]);
        }

        /// <summary>
        /// Tests merging instructions.
        /// </summary>
        [Test]
        public void TestMergeInstructions()
        {
            var route = new Route()
            {
                Shape = new Coordinate[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, 0),
                    new Coordinate(0, 0)
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
                TotalDistance = 0,
                TotalTime = 0
            };

            var generator = new InstructionGenerator<string>(route,
                new InstructionGenerator<string>.TryGetDelegate[]
                {
                    (RoutePosition pos, ILanguageReference langRef, out string instruction) =>
                        {
                            instruction = string.Format("Instruction {0}", pos.Shape);
                            return 1;
                        }
                },
                (Route r, ILanguageReference langRef, string i1, string i2, out string i) =>
                {
                    i = string.Format("Merged instruction: {0} -> {1}",
                        i1, i2);
                    return true;
                }, new MockLanguageReference());
            generator.Run();

            var instructions = generator.Instructions;
            Assert.IsNotNull(instructions);
            Assert.AreEqual(1, instructions.Count);
            Assert.AreEqual("Merged instruction: Merged instruction: Instruction 0 -> Instruction 1 -> Instruction 2",
                instructions[0]);
        }
    }
}