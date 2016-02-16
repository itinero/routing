// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
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

using OsmSharp.Routing.Navigation.Directions;
using OsmSharp.Routing.Navigation.Language;
using System.Collections.Generic;

namespace OsmSharp.Routing.Navigation.Osm
{
    /// <summary>
    /// An instruction car generator.
    /// </summary>
    public class InstructionCarGenerator : InstructionGenerator<Instruction>
    {
        /// <summary>
        /// Creates a new car instruction generator.
        /// </summary>
        public InstructionCarGenerator(Route route, ILanguageReference languageReference)
            : base(route, new TryGetDelegate[]
            {
                InstructionCarGenerator.GetStartInstruction,
                InstructionCarGenerator.GetStopInstruction,
                InstructionCarGenerator.GetRoundaboutInstruction,
                InstructionCarGenerator.GetTurnInstruction
            }, languageReference)
        {

        }

        /// <summary>
        /// Generates instructions for the given route.
        /// </summary>
        public static List<Instruction> Generate(Route route, ILanguageReference languageReference)
        {
            var algorithm = new InstructionCarGenerator(route, languageReference);
            algorithm.Run();

            if(algorithm.HasSucceeded)
            {
                return algorithm.Instructions;
            }
            return new List<Instruction>();
        }
        
        /// <summary>
        /// Gets the start instruction.
        /// </summary>
        public static int GetStartInstruction(RoutePosition position, ILanguageReference languageReference, 
            out Instruction instruction)
        {
            instruction = null;
            if (position.IsFirst())
            {
                instruction = new Instruction()
                {
                    Text = string.Format(languageReference["Start {0}."], 
                        languageReference[position.Direction().ToInvariantString()]),
                    Type = "start",
                    Shape = 0
                };
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Gets the stop instruction.
        /// </summary>
        public static int GetStopInstruction(RoutePosition position, ILanguageReference languageReference,
            out Instruction instruction)
        {
            instruction = null;
            if (position.IsLast())
            {
                instruction = new Instruction()
                {
                    Text = languageReference["Arrived at destination."],
                    Type = "stop",
                    Shape = position.Shape
                };
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Gets the turn instruction function.
        /// </summary>
        public static int GetTurnInstruction(RoutePosition position, ILanguageReference languageReference,
            out Instruction instruction)
        {
            var relative = position.RelativeDirection();

            var doInstruction = false;
            if (position.HasBranches())
            {
                var branches = new List<Route.Branch>(position.Branches());
                if (relative.Direction == RelativeDirectionEnum.StraightOn &&
                   branches.Count >= 2)
                { // straight-on at cross roads.
                    doInstruction = true;
                }
                else if (relative.Direction != RelativeDirectionEnum.StraightOn &&
                    relative.Direction != RelativeDirectionEnum.SlightlyLeft &&
                    relative.Direction != RelativeDirectionEnum.SlightlyRight &&
                    branches.Count > 0)
                { // an actual turn and there is at least on other street around.
                    doInstruction = true;
                }
            }
            if (doInstruction)
            {
                var name = position.GetMetaAttribute("name");
                if (!string.IsNullOrWhiteSpace(name))
                { // there is a name.
                    if (relative.Direction == RelativeDirectionEnum.StraightOn)
                    {
                        instruction = new Instruction()
                        {
                            Text = string.Format(languageReference["Go {0} on {1}."],
                                languageReference[relative.Direction.ToInvariantString()], name),
                            Type = "turn",
                            Shape = position.Shape
                        };
                        return 1;
                    }
                    instruction = new Instruction()
                    {
                        Text = string.Format(languageReference["Turn {0} on {1}."],
                            languageReference[relative.Direction.ToInvariantString()], name),
                        Type = "turn",
                        Shape = position.Shape
                    };
                    return 1;
                }
                else
                { // there is no name.
                    if (relative.Direction == RelativeDirectionEnum.StraightOn)
                    {
                        instruction = new Instruction()
                        {
                            Text = string.Format(languageReference["Go {0}."],
                                languageReference[relative.Direction.ToInvariantString()]),
                            Type = "turn",
                            Shape = position.Shape
                        };
                    }

                    instruction = new Instruction()
                    {
                        Text = string.Format(languageReference["Turn {0}."],
                            languageReference[relative.Direction.ToInvariantString()]),
                        Type = "turn",
                        Shape = position.Shape
                    };
                    return 1;
                }
            }
            instruction = null;
            return 0;
        }

        /// <summary>
        /// Gets the roundabout function.
        /// </summary>
        public static int GetRoundaboutInstruction(RoutePosition position, ILanguageReference languageReference,
            out Instruction instruction)
        {
            if (position.ContainsMetaAttribute("junction", "roundabout") &&
                !position.IsLast() &&
                !position.Next().Value.ContainsMetaAttribute("junction", "roundabout"))
            { // ok, it's a roundabout, if the next segment is not on the roundabout, generate an exit roundabout instruction.
                var exit = 1;
                var count = 1;
                while(position.MovePrevious() &&
                    position.ContainsMetaAttribute("junction", "roundabout"))
                {
                    if (position.HasBranches())
                    {
                        exit++;
                    }
                    count++;
                }

                if (exit == 1)
                {
                    instruction = new Instruction()
                    {
                        Text = string.Format(languageReference["Take the first exit at the next roundabout."],
                            exit),
                        Type = "roundabout",
                        Shape = position.Shape
                    };
                }
                else if (exit == 2)
                {
                    instruction = new Instruction()
                    {
                        Text = string.Format(languageReference["Take the second exit at the next roundabout."],
                            exit),
                        Type = "roundabout",
                        Shape = position.Shape
                    };
                }
                else if (exit == 3)
                {
                    instruction = new Instruction()
                    {
                        Text = string.Format(languageReference["Take the third exit at the next roundabout."],
                            exit),
                        Type = "roundabout",
                        Shape = position.Shape
                    };
                }
                else
                {
                    instruction = new Instruction()
                    {
                        Text = string.Format(languageReference["Take the {0}th exit at the next roundabout."],
                            exit),
                        Type = "roundabout",
                        Shape = position.Shape
                    };
                }
                return count;
            }
            instruction = null;
            return 0;
        }
    }
}