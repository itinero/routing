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