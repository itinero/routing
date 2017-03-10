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

using NUnit.Framework;
using Itinero.LocalGeo;
using Itinero.Navigation;
using Itinero.Navigation.Language;
using Itinero.Test.Navigation.Language;
using Itinero.Navigation.Instructions;

namespace Itinero.Test.Navigation
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

            var generator = new UnimodalInstructionGenerator(route,
                new UnimodalInstructionGenerator.TryGetDelegate[]
                {
                    (RoutePosition pos, ILanguageReference langRef, out Instruction instruction) =>
                        {
                            instruction = new Instruction() {
                                Shape = pos.Shape,
                                Text = string.Format("Instruction {0}", pos.Shape)
                            };
                            return 1;
                        }
                }, new MockLanguageReference());
            generator.Run();

            var instructions = generator.Instructions;
            Assert.IsNotNull(instructions);
            Assert.AreEqual(3, instructions.Count);
            Assert.AreEqual(string.Format("Instruction {0}", 0), instructions[0].Text);
            Assert.AreEqual(string.Format("Instruction {0}", 1), instructions[1].Text);
            Assert.AreEqual(string.Format("Instruction {0}", 2), instructions[2].Text);
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

            var generator = new UnimodalInstructionGenerator(route,
                new UnimodalInstructionGenerator.TryGetDelegate[]
                {
                    (RoutePosition pos, ILanguageReference langRef, out Instruction instruction) =>
                        {
                            if(pos.Shape == 2)
                            {
                                instruction = new Instruction()
                                {
                                    Text = "The one and only instruction!",
                                    Shape = pos.Shape
                                };
                                return 3;
                            }
                            instruction = instruction = new Instruction()
                            {
                                    Text = string.Format("Instruction {0}", pos.Shape),
                                    Shape = pos.Shape
                            };
                            return 1;
                        }
                }, new MockLanguageReference());
            generator.Run();

            var instructions = generator.Instructions;
            Assert.IsNotNull(instructions);
            Assert.AreEqual(1, instructions.Count);
            Assert.AreEqual("The one and only instruction!", instructions[0].Text);
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

            var generator = new UnimodalInstructionGenerator(route,
                new UnimodalInstructionGenerator.TryGetDelegate[]
                {
                    (RoutePosition pos, ILanguageReference langRef, out Instruction instruction) =>
                        {
                            instruction = new Instruction()
                            {
                                Text = string.Format("Instruction {0}", pos.Shape),
                                Shape = pos.Shape
                            };
                            return 1;
                        }
                },
                (Route r, ILanguageReference langRef, Instruction i1, Instruction i2, out Instruction i) =>
                {
                    i = new Instruction()
                    {
                        Text = string.Format("Merged instruction: {0} -> {1}", i1.Text, i2.Text)
                    };
                    return true;
                }, new MockLanguageReference());
            generator.Run();

            var instructions = generator.Instructions;
            Assert.IsNotNull(instructions);
            Assert.AreEqual(1, instructions.Count);
            Assert.AreEqual("Merged instruction: Merged instruction: Instruction 0 -> Instruction 1 -> Instruction 2",
                instructions[0].Text);
        }
    }
}