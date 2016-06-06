// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of Itinero.
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
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Algorithms.Contracted.EdgeBased;
using Itinero.Graphs.Directed;
using System;
using System.Linq;
using System.Collections.Generic;
using Itinero.Algorithms.Contracted.EdgeBased.Witness;

namespace Itinero.Test.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// A mock of a witness calculator.
    /// </summary>
    class WitnessCalculatorMock : IWitnessCalculator
    {
        private readonly uint[][] _witnesses;

        public WitnessCalculatorMock()
        {

        }

        public WitnessCalculatorMock(uint[][] witnesses)
        {
            _witnesses = witnesses;
        }

        public void Calculate(DirectedDynamicGraph graph, Func<uint, IEnumerable<uint[]>> getRestrictions, uint source, List<uint> targets, List<float> weights, 
            ref bool[] forwardWitness, ref bool[] backwardWitness, uint vertexToSkip)
        {
            if (_witnesses != null)
            {
                for (var i = 0; i < targets.Count; i++)
                {
                    var target = targets[i];
                    if(_witnesses.Any((x) => x[0] == vertexToSkip &&
                        x[1] == source && x[2] == target && (x[3] == 1 || x[3] == 0)))
                    {
                        forwardWitness[i] = true;
                    }
                    else if (_witnesses.Any((x) => x[0] == vertexToSkip &&
                        x[1] == source && x[2] == target && (x[3] == 2 || x[3] == 0)))
                    {
                        backwardWitness[i] = true;
                    }
                }
            }
        }
    }
}