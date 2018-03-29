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

using Itinero.Algorithms.Weights;
using Itinero.Data.Contracted.Edges;
using Itinero.Data.Edges;
using Itinero.Graphs.Directed;
using Itinero.Profiles;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Itinero.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// Builds a directed graph from a regular graph.
    /// </summary>
    public class DirectedGraphBuilder<T> : AlgorithmBase
        where T : struct
    {
        private readonly Itinero.Graphs.Graph _source;
        private readonly DirectedDynamicGraph _target;
        private readonly WeightHandler<T> _weightHandler;

        /// <summary>
        /// Creates a new graph builder.
        /// </summary>
        public DirectedGraphBuilder(Itinero.Graphs.Graph source, DirectedDynamicGraph target, WeightHandler<T> weightHandler)
        {
            weightHandler.CheckCanUse(target);

            _source = source;
            _target = target;
            _weightHandler = weightHandler;
        }

        /// <summary>
        /// Executes the actual run.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            float distance;
            ushort edgeProfile;
            bool? direction = null;

            var factors = new Dictionary<ushort, Factor>();
            var edgeEnumerator = _source.GetEdgeEnumerator();
            for (uint vertex = 0; vertex < _source.VertexCount; vertex++)
            {
                edgeEnumerator.MoveTo(vertex);
                edgeEnumerator.Reset();
                while (edgeEnumerator.MoveNext())
                {
                    EdgeDataSerializer.Deserialize(edgeEnumerator.Data0,
                        out distance, out edgeProfile);
                    Factor factor;
                    var weight = _weightHandler.Calculate(edgeProfile, distance, out factor);

                    if (factor.Value != 0)
                    {
                        direction = null;
                        if (factor.Direction == 1)
                        {
                            direction = true;
                            if (edgeEnumerator.DataInverted)
                            {
                                direction = false;
                            }
                        }
                        else if (factor.Direction == 2)
                        {
                            direction = false;
                            if (edgeEnumerator.DataInverted)
                            {
                                direction = true;
                            }
                        }

                        _weightHandler.AddEdge(_target, edgeEnumerator.From, edgeEnumerator.To, direction, weight);
                    }
                }
            }

            this.HasSucceeded = true;
        }
    }

    /// <summary>
    /// Builds a directed graph from a regular graph.
    /// </summary>
    public sealed class DirectedGraphBuilder : DirectedGraphBuilder<float>
    {
        /// <summary>
        /// Creates a new graph builder.
        /// </summary>
        public DirectedGraphBuilder(Itinero.Graphs.Graph source, DirectedDynamicGraph target, Func<ushort, Factor> getFactor)
            : base(source, target, new DefaultWeightHandler(getFactor))
        {

        }

        /// <summary>
        /// Creates a new graph builder.
        /// </summary>
        public DirectedGraphBuilder(Itinero.Graphs.Graph source, DirectedDynamicGraph target, DefaultWeightHandler weightHandler)
            : base(source, target, weightHandler)
        {

        }
    }
}