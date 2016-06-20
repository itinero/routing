// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
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

using Itinero.Graphs.Directed;
using System;
using System.Collections.Generic;
using Itinero.Algorithms.Contracted.EdgeBased.Witness;
using Itinero.Algorithms;

namespace Itinero.Test.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// A mock of a witness calculator.
    /// </summary>
    class WitnessCalculatorMock : IWitnessCalculator<float>
    {
        private readonly Func<uint, uint, Tuple<EdgePath<float>, EdgePath<float>>> _witnesses; // source, target, forward, result.

        public WitnessCalculatorMock()
        {

        }

        public WitnessCalculatorMock(Func<uint, uint, Tuple<EdgePath<float>, EdgePath<float>>> witnesses)
        {
            _witnesses = witnesses;
        }

        public void Calculate(DirectedDynamicGraph graph, Func<uint, IEnumerable<uint[]>> getRestrictions, uint source, List<uint> targets, List<float> weights, 
            ref EdgePath<float>[] forwardWitness, ref EdgePath<float>[] backwardWitness, uint vertexToSkip)
        {
            for(var i = 0; i < forwardWitness.Length; i++)
            {
                if (forwardWitness[i] == null)
                {
                    forwardWitness[i] = new EdgePath<float>();
                }
                if (backwardWitness[i] == null)
                {
                    backwardWitness[i] = new EdgePath<float>();
                }
            }
            if (_witnesses != null)
            {
                for (var i = 0; i < targets.Count; i++)
                {
                    var target = targets[i];
                    var witnesses = _witnesses(source, target);
                    if (witnesses != null)
                    {
                        forwardWitness[i] = witnesses.Item1;
                        backwardWitness[i] = witnesses.Item2;
                    }
                }
            }
        }
    }
}