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

using Itinero.Navigation.Language;
using Itinero.Profiles;
using Itinero.Profiles.Lua;
using System;
using System.Collections.Generic;

namespace Itinero.Navigation.Instructions
{
    /// <summary>
    /// A unimodal instruction generator based on a dynamic vehicle profile using Lua.
    /// </summary>
    public class DynamicUnimodalInstructionGenerator : IUnimodalInstructionGenerator
    {
        private readonly DynamicProfile _profile;
        private readonly Script _script;
        private readonly UnimodalInstructionGenerator.TryGetDelegate[] _getInstructionFunctions;

        /// <summary>
        /// Creates a new dynamic unimodal instruction generator.
        /// </summary>
        public DynamicUnimodalInstructionGenerator(DynamicProfile profile)
        {
            _profile = profile;
            _script = (_profile.Parent as DynamicVehicle).Script;

            var dynProfiles = _script.Globals.Get("instruction_generators");
            if (dynProfiles == null ||
                dynProfiles.Type == DataType.Nil)
            {
                throw new ArgumentException("No instruction_generators defined in lua script.");
            }
            var getInstructionFunctions = new List<Tuple<string, object>>();
            foreach (var dynProfile in dynProfiles.Table.Pairs)
            {
                var profileDefinition = dynProfile.Value;
                var appliesTo = profileDefinition.Table.Get("applies_to");
                var appliesToThis = false;
                if (appliesTo == null)
                {
                    appliesToThis = true;
                }
                else
                {
                    if (appliesTo.Type == DataType.String)
                    {
                        if (string.IsNullOrWhiteSpace(appliesTo.String) ||
                            appliesTo.String.ToLowerInvariant() == profile.Name.ToLowerInvariant())
                        {
                            appliesToThis = true;
                        }
                    }
                    else if (appliesTo.Type == DataType.Table)
                    {
                        foreach (var appliesToProfile in appliesTo.Table.Pairs)
                        {
                            if (appliesToProfile.Value.String.ToLowerInvariant() == profile.Name.ToLowerInvariant())
                            {
                                appliesToThis = true;
                            }
                        }
                    }
                }

                if (appliesToThis)
                {
                    var dynGenerators = profileDefinition.Table.Get("generators");
                    if (dynGenerators == null ||
                        dynGenerators.Type != DataType.Table)
                    {
                        throw new ArgumentException("Generators not defined correctly.");
                    }

                    foreach (var dynGenerator in dynGenerators.Table.Pairs)
                    {
                        var name = dynGenerator.Value.Table.Get("name");
                        if (name == null ||
                            name.Type != DataType.String)
                        {
                            throw new ArgumentException("One of the generator names not defined correctly.");
                        }
                        var functionName = dynGenerator.Value.Table.Get("function_name");
                        if (functionName == null ||
                            functionName.Type != DataType.String)
                        {
                            throw new ArgumentException("One of the generator functions not defined correctly.");
                        }
                        var function = _script.Globals[functionName.String];
                        if (function == null)
                        {
                            throw new ArgumentException(string.Format("One of the generator functions not found in lua script: {0}", functionName));
                        }
                        getInstructionFunctions.Add(new Tuple<string, object>(name.String, function));
                    }
                    break;
                }
            }

            _getInstructionFunctions = new UnimodalInstructionGenerator.TryGetDelegate[getInstructionFunctions.Count];
            for (var i=0; i < getInstructionFunctions.Count; i++)
            {
                _getInstructionFunctions[i] = this.BuildTryGetDelegate(getInstructionFunctions[i]);
            }
        }

        private UnimodalInstructionGenerator.TryGetDelegate BuildTryGetDelegate(Tuple<string, object> function)
        {
            return (RoutePosition position, ILanguageReference language, out Instruction instruction) =>
            {
                var positionTable = BuildRoutePositionTable(position);
                var languageReferenceTable = BuildLanguageReferenceTable(language);
                var instructionTable = new Table(_script);

                var result = _script.Call(function.Item2, positionTable, languageReferenceTable, instructionTable);
                if (result == null ||
                    result.Type != DataType.Number ||
                    result.Number == 0)
                {
                    instruction = null;
                    return 0;
                }

                instruction = BuildInstructionFromTable(instructionTable);
                instruction.Type = function.Item1;
                return (int)result.Number;
            };
        }

        private Instruction BuildInstructionFromTable(Table instructionTable)
        {
            var instruction = new Instruction();
            float shape;
            if (!instructionTable.TryGetFloat("shape", out shape))
            {
                throw new Exception("Error in lua script: Instruction doesn't contain a shape index.");
            }
            instruction.Shape = (int)shape;
            var dynText = instructionTable.Get("text");
            if (dynText == null ||
                dynText.Type != DataType.String)
            {
                throw new Exception("Error in lua script: Instruction doesn't contain a text.");
            }
            instruction.Text = dynText.String;
            return instruction;
        }

        private Table BuildLanguageReferenceTable(ILanguageReference language)
        {
            var table = new Table(_script);
            table["get"] = (Func<string, string>)((key) =>
            {
                return language[key];
            });
            return table;
        }

        private Table BuildRoutePositionTable(RoutePosition position)
        {
            var positionTable = new Table(_script);
            UpdateTable(position, positionTable);
            positionTable["move_next"] = (Func<bool>)(() =>
            {
                if (position.MoveNext())
                {
                    UpdateTable(position, positionTable);
                    return true;
                }
                UpdateTable(position, positionTable);
                return false;
            });
            positionTable["move_previous"] = (Func<bool>)(() =>
            {
                if (position.MovePrevious())
                {
                    UpdateTable(position, positionTable);
                    return true;
                }
                UpdateTable(position, positionTable);
                return false;
            });
            positionTable["has_stops"] = (Func<bool>)(() =>
            {
                return position.HasStops();
            });
            positionTable["is_first"] = (Func<bool>)(() =>
            {
                return position.IsFirst();
            });
            positionTable["is_last"] = (Func<bool>)(() =>
            {
                return position.IsLast();
            });
            positionTable["direction"] = (Func<string>)(() =>
            {
                return position.Direction().ToInvariantString().ToLowerInvariant();
            });
            return positionTable;
        }

        private static void UpdateTable(RoutePosition position, Table positionTable)
        {
            positionTable["shape"] = position.Shape;
            positionTable["stop_index"] = position.StopIndex;
            positionTable["meta_index"] = position.MetaIndex;
            positionTable["branch_index"] = position.BranchIndex;
        }

        /// <summary>
        /// Generates instructions for the given route assuming it's using the profile in this generator.
        /// </summary>
        public IList<Instruction> Generate(Route route, ILanguageReference languageReference)
        {
            if (route.IsMultimodal())
            {
                throw new ArgumentException("Cannot use a unimodal instruction generator on multimodal route.");
            }
            if (_profile.FullName.ToLowerInvariant() != route.Profile)
            {
                throw new ArgumentException(string.Format("Cannot generate instructions with a generator for profile {0} for a route with profile {1}.",
                    _profile.FullName, route.Profile));
            }

            var instructionGenerator = new UnimodalInstructionGenerator(route, _getInstructionFunctions, languageReference);
            instructionGenerator.Run();
            if (!instructionGenerator.HasSucceeded)
            {
                throw new Exception(string.Format("Failed to generate instructions: {0}", instructionGenerator.ErrorMessage));
            }
            return instructionGenerator.Instructions;
        }
    }
}
