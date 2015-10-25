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
        private readonly Func<Route, int, T> _getInstruction;
        private readonly Func<Route, T, T, T> _merge;

        /// <summary>
        /// Creates a new instruction generator.
        /// </summary>
        public InstructionGenerator(Route route, Func<Route, int, T> getInstruction)
            : this(route, getInstruction, null)
        {

        }

        /// <summary>
        /// Creates a new instruction generator.
        /// </summary>
        public InstructionGenerator(Route route, Func<Route, int, T> getInstruction,
            Func<Route, T, T, T> merge)
        {
            _route = route;
            _getInstruction = getInstruction;
            _merge = merge;
        }

        private List<T> _instructions;

        /// <summary>
        /// Executes the actual run of the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            _instructions = new List<T>();

            for (var i = 0; i < _route.Segments.Count; i++)
            { // check each part of the route to check for an instruction.
                var instruction = _getInstruction(_route, i);
                if (instruction != null)
                {
                    _instructions.Add(instruction);
                }
            }

            if (_merge != null)
            {
                for (var i = 1; i < _instructions.Count; i++)
                { // keep on merging until impossible.
                    var merged = _merge(_route, _instructions[i - 1], _instructions[i]);
                    if (merged != null)
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