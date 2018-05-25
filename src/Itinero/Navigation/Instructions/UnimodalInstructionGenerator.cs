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

using Itinero.Algorithms;
using Itinero.Navigation.Language;
using System.Collections.Generic;
using System.Threading;

namespace Itinero.Navigation.Instructions
{
    /// <summary>
    /// A unimodal instruction generator, assumes only one vehicle profile used for the entire route.
    /// </summary>
    public class UnimodalInstructionGenerator : AlgorithmBase
    { 
        private readonly ILanguageReference _languageReference;
        private readonly Route _route;
        private readonly TryGetDelegate[] _tryGetInstructions;
        private readonly MergeDelegate _merge;

        /// <summary>
        /// A delegate to construct an instruction for a given position in a route.
        /// </summary>
        public delegate int TryGetDelegate(RoutePosition position, ILanguageReference languageRefence, out Instruction instruction);

        /// <summary>
        /// A delegate to merge two instructions if needed.
        /// </summary>
        public delegate bool MergeDelegate(Route route, ILanguageReference languageRefence, Instruction i1, Instruction i2, out Instruction i);

        /// <summary>
        /// Creates a new instruction generator.
        /// </summary>
        public UnimodalInstructionGenerator(Route route, TryGetDelegate[] tryGetInstructions, ILanguageReference languageReference)
            : this(route, tryGetInstructions, null, languageReference)
        {

        }

        /// <summary>
        /// Creates a new instruction generator.
        /// </summary>
        public UnimodalInstructionGenerator(Route route, TryGetDelegate[] tryGetInstructions,
            MergeDelegate merge, ILanguageReference languageReference)
        {
            _route = route;
            _tryGetInstructions = tryGetInstructions;
            _merge = merge;
            _languageReference = languageReference;
        }

        private List<Instruction> _instructions;
        private List<int> _instructionIndexes;
        private List<int> _instructionSizes;

        /// <summary>
        /// Executes the actual run of the algorithm.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            _instructions = new List<Instruction>();
            _instructionIndexes = new List<int>();
            _instructionSizes = new List<int>();

            if (_route.ShapeMeta == null)
            {
                return;
            }

            var enumerator = _route.GetEnumerator();
            enumerator.Reset();
            if (!enumerator.MoveNext())
            {
                return;
            }

            do
            {
                for (var j = 0; j < _tryGetInstructions.Length; j++)
                {
                    Instruction instruction;
                    var count = _tryGetInstructions[j](enumerator.Current, _languageReference, out instruction);
                    if (count > 0)
                    { // ok, some points have been consumed and it may overridde other instructions.
                        var current = _instructions.Count - 1;
                        while (current >= 0 &&
                            _instructionIndexes[current] > enumerator.Current.Shape - count)
                        {
                            _instructions.RemoveAt(current);
                            _instructionIndexes.RemoveAt(current);
                            _instructionSizes.RemoveAt(current);

                            current--;
                        }

                        // add instructions, index and size.
                        _instructions.Add(instruction);
                        _instructionIndexes.Add(enumerator.Current.Shape);
                        _instructionSizes.Add(count);
                        this.HasSucceeded = true;
                        break;
                    }
                }
            } while (enumerator.MoveNextUntil(x => x.HasBranches() || x.HasCurrentMeta() || x.HasStops()));

            if (_merge != null)
            {
                for (var i = 1; i < _instructions.Count; i++)
                { // keep on merging until impossible.
                    Instruction merged;
                    if (_merge(_route, _languageReference, _instructions[i - 1], _instructions[i], out merged))
                    { // ok, an instruction got merged.
                        _instructions[i - 1] = merged;
                        _instructions.RemoveAt(i);
                        i = i - 1; // retry again.
                    }
                }
            }
        }

        /// <summary>
        /// Gets the generated list of instructions.
        /// </summary>
        public List<Instruction> Instructions
        {
            get
            {
                return _instructions;
            }
        }
    }
}