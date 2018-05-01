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

using Itinero.Navigation.Language;
using Itinero.Profiles;
using Itinero.Profiles.Lua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
                lock (_script)
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
                }
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
            positionTable["relative_direction"] = (Func<Table>)(() =>
            {
                var table = new Table(_script);
                var direction = position.RelativeDirection();
                if (direction == null)
                {
                    table["direction"] = "unknown";
                }
                else
                {
                    table["angle"] = direction.Angle.ToInvariantString();
                    table["direction"] = direction.Direction.ToInvariantString().ToLowerInvariant();
                }
                return table;
            });
            positionTable["has_branches"] = (Func<bool>)(() =>
            {
                return position.HasBranches();
            });
            positionTable["next"] = (Func<Table>)(() =>
            {
                var next = position.Next();
                if (next == null)
                {
                    return null;
                }
                return this.BuildRoutePositionTable(next.Value);
            });
            positionTable["previous"] = (Func<Table>)(() =>
            {
                var previous = position.Previous();
                if (previous == null)
                {
                    return null;
                }
                return this.BuildRoutePositionTable(previous.Value);
            });

            return positionTable;
        }

        private void UpdateTable(RoutePosition position, Table positionTable)
        {
            positionTable["shape"] = position.Shape;
            positionTable["stop_index"] = position.StopIndex;
            positionTable["meta_index"] = position.MetaIndex;
            positionTable["branch_index"] = position.BranchIndex;
            if (position.HasBranches())
            {
                var branchesTable = new Table(_script);
                UpdateTable(position.Branches().ToList(), branchesTable);
                positionTable["branches"] = branchesTable;
            }
            else
            {
                positionTable["branches"] = DynValue.Nil;
            }
            var meta = position.Meta();
            if (meta != null)
            {
                positionTable["attributes"] = meta.Attributes.ToTable(_script);
            }
            else
            {
                positionTable["attributes"] = new Table(_script);
            }
        }

        private void UpdateTable(List<Route.Branch> branches, Table branchesTable)
        {
            var branchesItems = new Table(_script);
            for (var i = 0; i < branches.Count; i++)
            {
                var branchesItem = new Table(_script);
                branchesItem["attibutes"] = branches[i].Attributes.ToTable(_script);
                branchesItem["latitude"] = branches[i].Coordinate.Latitude;
                branchesItem["longitude"] = branches[i].Coordinate.Longitude;
                branchesItem["shape"] = branches[i].Shape;
                branchesItem["can_traverse"] = this._profile.FactorAndSpeed(branches[i].Attributes).SpeedFactor != 0;
                branchesItems[i] = branchesItem;
            }
            branchesTable["count"] = branches.Count;
            branchesTable["items"] = branchesItems;
            branchesTable["get_traversable"] = (Func<Table>)(() =>
            {
                var traversable = new List<Route.Branch>();
                foreach(var branch in branches)
                {
                    var factorAndSpeed = this._profile.FactorAndSpeed(branch.Attributes);
                    if (factorAndSpeed.SpeedFactor != 0)
                    {
                        if (factorAndSpeed.Direction == 0 ||
                            factorAndSpeed.Direction == 3)
                        {
                            traversable.Add(branch);
                        }
                        else
                        {
                            if (branch.AttributesDirection)
                            {
                                if (factorAndSpeed.Direction == 1 ||
                                    factorAndSpeed.Direction == 4)
                                {
                                    traversable.Add(branch);
                                }
                            }
                            else
                            {
                                if (factorAndSpeed.Direction == 2 ||
                                    factorAndSpeed.Direction == 5)
                                {
                                    traversable.Add(branch);
                                }
                            }
                        }
                    }
                }
                var table = new Table(_script);
                UpdateTable(traversable, table);
                return table;
            });
        }

        /// <summary>
        /// Generates instructions for the given route assuming it's using the profile in this generator.
        /// </summary>
        public IList<Instruction> Generate(Route route, ILanguageReference languageReference)
        {
            return this.Generate(route, languageReference, CancellationToken.None);
        }

        /// <summary>
        /// Generates instructions for the given route assuming it's using the profile in this generator.
        /// </summary>
        public IList<Instruction> Generate(Route route, ILanguageReference languageReference, CancellationToken cancellationToken)
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
            instructionGenerator.Run(cancellationToken);
            if (!instructionGenerator.HasSucceeded)
            {
                throw new Exception(string.Format("Failed to generate instructions: {0}", instructionGenerator.ErrorMessage));
            }
            return instructionGenerator.Instructions;
        }
    }
}
