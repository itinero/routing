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

using OsmSharp.Routing.Navigation.Language;
using System;
using System.Collections.Generic;
using System.Linq;

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
            : this(route, languageReference, new TryGetDelegateWithLanguageReference[]
            {
                InstructionCarGenerator.GetStartInstruction,
                InstructionCarGenerator.GetStopInstruction,
                InstructionCarGenerator.GetRoundaboutInstruction,
                InstructionCarGenerator.GetTurnInstruction
            })
        {

        }

        /// <summary>
        /// Creates a new car instruction generator.
        /// </summary>
        public InstructionCarGenerator(Route route, ILanguageReference languageReference,
            TryGetDelegateWithLanguageReference[] getInstructionDelegates)
            : base(route, InstructionCarGenerator.GetInstructionDelegatesFrom(getInstructionDelegates, languageReference))
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
        /// A delegate to construct an instruction for a given segment in a route.
        /// </summary>
        public delegate int TryGetDelegateWithLanguageReference(Route route, int i, ILanguageReference languageReference,
            out Instruction instruction);

        /// <summary>
        /// Gets an instruction delegate from an instruction delegate with a language reference.
        /// </summary>
        private static InstructionGenerator<Instruction>.TryGetDelegate GetInstructionDelegateFrom(TryGetDelegateWithLanguageReference tryGetDelegate,
            ILanguageReference languageReference)
        {
            return (Route r, int i, out Instruction instruction) =>
                {
                    return tryGetDelegate(r, i, languageReference, out instruction);
                };
        }

        /// <summary>
        /// Gets instruction delegates from instruction delegates with a language reference.
        /// </summary>
        private static InstructionGenerator<Instruction>.TryGetDelegate[] GetInstructionDelegatesFrom(
            TryGetDelegateWithLanguageReference[] tryGetDelegates,
            ILanguageReference languageReference)
        {
            var delegates = new InstructionGenerator<Instruction>.TryGetDelegate[tryGetDelegates.Length];
            for(var i = 0; i < delegates.Length; i++)
            {
                delegates[i] = InstructionCarGenerator.GetInstructionDelegateFrom(tryGetDelegates[i],
                    languageReference);
            }
            return delegates;
        }

        /// <summary>
        /// Gets the start instruction.
        /// </summary>
        public static int GetStartInstruction(Route r, int i, ILanguageReference languageReference, 
            out Instruction instruction)
        {
            instruction = null;
            if(i == 0 &&
               r.Segments.Count > 0)
            { // determine direction.
                var direction = Math.Geo.Meta.DirectionCalculator.Calculate(
                    new Math.Geo.GeoCoordinate(r.Segments[0].Latitude, r.Segments[0].Longitude),
                    new Math.Geo.GeoCoordinate(r.Segments[1].Latitude, r.Segments[1].Longitude));
                instruction = new Instruction()
                {
                    Text = languageReference[string.Format("Start {0}.", direction.ToInvariantString())],
                    Type = "start",
                    Segment = 0
                };
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Gets the start instruction.
        /// </summary>
        public static int GetStopInstruction(Route r, int i, ILanguageReference languageReference,
            out Instruction instruction)
        {
            instruction = null;
            if (i == r.Segments.Count - 1 &&
               r.Segments.Count > 0)
            {
                instruction = new Instruction()
                {
                    Text = languageReference["Arrived at destination."],
                    Type = "stop",
                    Segment = r.Segments.Count - 1
                };
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Gets the turn instruction function.
        /// </summary>
        public static int GetTurnInstruction(Route r, int i, ILanguageReference languageReference,
            out Instruction instruction)
        {
            var relative = r.RelativeDirectionAt(i);
            if (relative != null &&
                relative.Direction != Math.Geo.Meta.RelativeDirectionEnum.StraightOn &&
                r.Segments[i].SideStreets != null &&
                r.Segments[i].SideStreets.Length > 0)
            { // not straight on and at least one sidestreet.
                var name = string.Empty;
                if (i + 1 < r.Segments.Count &&
                    r.Segments[i + 1].Tags != null &&
                    r.Segments[i + 1].Tags.Any(x =>
                    {
                        if (x.Key == "name")
                        {
                            name = x.Value;
                            return true;
                        }
                        return false;
                    }))
                { // there is a name.
                    instruction = new Instruction()
                    {
                        Text = string.Format("Turn {0} on {1}.",
                            relative.Direction.ToInvariantString(), name),
                        Type = "turn",
                        Segment = i
                    };
                }
                else
                { // there is no name.
                    instruction = new Instruction()
                    {
                        Text = string.Format("Turn {0}.",
                            relative.Direction.ToInvariantString()),
                        Type = "turn",
                        Segment = i
                    };
                }
                return 1;
            }
            instruction = null;
            return 0;
        }

        /// <summary>
        /// Gets the roundabout function.
        /// </summary>
        public static int GetRoundaboutInstruction(Route r, int i, ILanguageReference languageReference, 
            out Instruction instruction)
        {
            if (r.Segments[i].Tags != null &&
                r.Segments[i].Tags.Any(x =>
                {
                    return x.Key == "junction" && x.Value == "roundabout";
                }))
            { // ok, it's a roundabout, is the next segment on the roundabout, if it's not generate an exit roundabout.
                if (i < r.Segments.Count &&
                    (r.Segments[i + 1].Tags == null ||
                    !r.Segments[i + 1].Tags.Any(x =>
                    {
                        return x.Key == "junction" && x.Value == "roundabout";
                    })))
                { // ok, the next one isn't a roundabout.
                    var exit = 1;
                    var count = 1;
                    for (var j = i - 1; j >= 0; j--)
                    {
                        count++;
                        if (r.Segments[j].Tags == null ||
                           !r.Segments[j].Tags.Any(x =>
                            {
                                return x.Key == "junction" && x.Value == "roundabout";
                            }))
                        { // not a roundabout anymore.
                            break;
                        }
                        if (r.Segments[j].SideStreets != null)
                        {
                            exit++;
                        }
                    }
                    if (exit == 1)
                    {
                        instruction = new Instruction()
                        {
                            Text = string.Format(languageReference["Take the first exit at the next roundabout."],
                                exit),
                            Type = "roundabout",
                            Segment = i
                        };
                    }
                    else if (exit == 2)
                    {
                        instruction = new Instruction()
                        {
                            Text = string.Format(languageReference["Take the second exit at the next roundabout."],
                                exit),
                            Type = "roundabout",
                            Segment = i
                        };
                    }
                    else if (exit == 3)
                    {
                        instruction = new Instruction()
                        {
                            Text = string.Format(languageReference["Take the third exit at the next roundabout."],
                                exit),
                            Type = "roundabout",
                            Segment = i
                        };
                    }
                    else
                    {
                        instruction = new Instruction()
                        {
                            Text = string.Format(languageReference["Take the {0}th exit at the next roundabout."],
                                exit),
                            Type = "roundabout",
                            Segment = i
                        };
                    }
                    return count;
                }
            }
            instruction = null;
            return 0;
        }
    }
}