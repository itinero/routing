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

using OsmSharp.Routing.Algorithms;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Navigation
{
    /// <summary>
    /// An instruction generator.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InstructionGenerator<T> : AlgorithmBase
    {
        private readonly Route _route;
        private readonly TryGetDelegate[] _tryGetInstructions;
        private readonly MergeDelegate _merge;

        /// <summary>
        /// A delegate to construct an instruction for a given segment in a route.
        /// </summary>
        public delegate int TryGetDelegate(Route route, int i, out T instruction);

        /// <summary>
        /// A delegate to merge two instructions if needed.
        /// </summary>
        public delegate bool MergeDelegate(Route route, T i1, T i2, out T i);

        /// <summary>
        /// Creates a new instruction generator.
        /// </summary>
        public InstructionGenerator(Route route, TryGetDelegate[] tryGetInstructions)
            : this(route, tryGetInstructions, null)
        {

        }

        /// <summary>
        /// Creates a new instruction generator.
        /// </summary>
        public InstructionGenerator(Route route, TryGetDelegate[] tryGetInstructions,
            MergeDelegate merge)
        {
            _route = route;
            _tryGetInstructions = tryGetInstructions;
            _merge = merge;
        }

        private List<T> _instructions;
        private List<int> _instructionIndexes;
        private List<int> _instructionSizes;

        /// <summary>
        /// Executes the actual run of the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            _instructions = new List<T>();
            _instructionIndexes = new List<int>();
            _instructionSizes = new List<int>();

            for (var i = 0; i < _route.Segments.Count; i++)
            { // check each part of the route to check for an instruction.
                for (var j = 0; j < _tryGetInstructions.Length; j++)
                {
                    T instruction;
                    var count = _tryGetInstructions[j](_route, i, out instruction);
                    if (count > 0)
                    { // ok, some segments have been consumed.
                        var current = _instructions.Count - 1;
                        while (current >= 0 &&
                            _instructionIndexes[current] > i - count)
                        {
                            _instructions.RemoveAt(current);
                            _instructionIndexes.RemoveAt(current);
                            _instructionSizes.RemoveAt(current);

                            current--;
                        }

                        // add instructions, index and size.
                        _instructions.Add(instruction);
                        _instructionIndexes.Add(i);
                        _instructionSizes.Add(count);
                        this.HasSucceeded = true;
                        break;
                    }
                }
            }

            if (_merge != null)
            {
                for (var i = 1; i < _instructions.Count; i++)
                { // keep on merging until impossible.
                    T merged;
                    if(_merge(_route, _instructions[i - 1], _instructions[i], out merged))
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
        public List<T> Instructions
        {
            get
            {
                return _instructions;
            }
        }
    }
}