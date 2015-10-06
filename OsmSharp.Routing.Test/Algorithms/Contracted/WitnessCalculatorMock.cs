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

using OsmSharp.Routing.Algorithms.Contracted;
using OsmSharp.Routing.Graphs.Directed;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Test.Algorithms.Contracted
{
    /// <summary>
    /// A mock of a witness calculator.
    /// </summary>
    class WitnessCalculatorMock : IWitnessCalculator
    {
        private readonly Dictionary<uint, Dictionary<uint, bool>> _witnesses;

        public WitnessCalculatorMock()
        {

        }

        public WitnessCalculatorMock(Tuple<uint, Tuple<uint, bool>[]>[] witnesses)
        {
            _witnesses = new Dictionary<uint, Dictionary<uint, bool>>();
            foreach(var tuple in witnesses)
            {
                _witnesses[tuple.Item1] = new Dictionary<uint, bool>();
                foreach(var tuple2 in tuple.Item2)
                {
                    _witnesses[tuple.Item1].Add(tuple2.Item1, tuple2.Item2);
                }
            }
        }

        public void Calculate(DirectedGraph graph, uint source, List<uint> targets, List<float> weights, 
            ref bool[] forwardWitness, ref bool[] backwardWitness, uint vertexToSkip)
        {
            if (_witnesses != null)
            {
                Dictionary<uint, bool> sourceWitnesses;
                if(_witnesses.TryGetValue(source, out sourceWitnesses))
                {
                    for (var i = 0; i < targets.Count; i++)
                    {
                        var target = targets[i];
                        bool witness;
                        if(sourceWitnesses.TryGetValue(target, out witness))
                        {
                            if(witness)
                            {
                                forwardWitness[i] = true;
                            }
                            else
                            {
                                backwardWitness[i] = true;
                            }
                        }
                    }
                }
            }
        }
    }
}