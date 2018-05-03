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

using Itinero.Algorithms.Restrictions;
using Itinero.Algorithms.Weights;
using Itinero.Graphs;
using System.Collections.Generic;
using System.Threading;

namespace Itinero.Algorithms.Default.EdgeBased
{
    /// <summary>
    /// An algorithm to calculate one-to-many directed weights.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DirectedOneToManyWeights<T> : AlgorithmBase
        where T : struct
    {
        private readonly Graph _graph;
        private readonly WeightHandler<T> _weightHandler;
        private readonly DirectedDykstraSource<T> _source;
        private readonly DirectedDykstraSource<T>[] _targets;
        private readonly T _maxSearch;
        private readonly RestrictionCollection _restrictions;

        /// <summary>
        /// Creates a new many-to-many algorithm instance.
        /// </summary>
        public DirectedOneToManyWeights(Graph graph, WeightHandler<T> weightHandler, RestrictionCollection restrictions,
            DirectedDykstraSource<T> source, DirectedDykstraSource<T>[] targets, T maxSearch)
        {
            _graph = graph;
            _source = source;
            _targets = targets;
            _weightHandler = weightHandler;
            _maxSearch = maxSearch;
            _restrictions = restrictions;
        }

        private T[] _weights;

        /// <summary>
        /// Executes the actual algorithm.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            _weights = new T[_targets.Length];
            for(var t = 0; t < _targets.Length; t++)
            {
                _weights[t] = _weightHandler.Infinite;
            }

            // index targets per edge id.
            var targets = new Dictionary<DirectedEdgeId, LinkedTarget>();
            for (var i = 0; i < _targets.Length; i++)
            {
                var t = _targets[i];
                if (!t.Edge1.IsNoEdge)
                {
                    LinkedTarget lt;
                    if (!targets.TryGetValue(t.Edge1, out lt))
                    {
                        lt = null;
                    }
                    targets[t.Edge1] = new LinkedTarget()
                    {
                        Next = lt,
                        Target = i
                    };
                }
                if (!t.Edge2.IsNoEdge)
                {
                    LinkedTarget lt;
                    if (!targets.TryGetValue(t.Edge2, out lt))
                    {
                        lt = null;
                    }
                    targets[t.Edge2] = new LinkedTarget()
                    {
                        Next = lt,
                        Target = i
                    };
                }
            }

            // start the search.
            var foundTargets = 0;
            var dykstra = new DirectedDykstra<T>(_graph, _weightHandler, _restrictions, _source, _maxSearch, false);
            dykstra.WasFound += (p, e, w) =>
            {
                LinkedTarget lt;
                if (targets.TryGetValue(e, out lt))
                { // a target exists at this edge.
                    while (lt != null)
                    {
                        // get the target.
                        var target = _targets[lt.Target];
                        T tWeight = default(T);
                        if (target.Edge1.Raw == e.Raw)
                        {
                            tWeight = target.Weight1;
                        }
                        else if (target.Edge2.Raw == e.Raw)
                        {
                            tWeight = target.Weight2;
                        }
                        else
                        {
                            lt = lt.Next;
                            continue;
                        }

                        if (_weightHandler.IsLargerThanOrEqual(_weights[lt.Target], _weightHandler.Infinite))
                        {
                            foundTargets++;
                            _weights[lt.Target] = _weightHandler.Add(w, tWeight);
                        }
                        else
                        {
                            // calculate the total weight.
                            tWeight = _weightHandler.Add(w, tWeight);

                            if (_weightHandler.IsSmallerThan(tWeight, _weights[lt.Target]))
                            {
                                _weights[lt.Target] = _weightHandler.Add(w, tWeight);
                            }
                        }

                        if (foundTargets == _targets.Length)
                        { // TODO: improve this stopping condition by updating the max search parameter of the dykstra search with the maxweight of any target instead of stopping here.
                            return true;
                        }

                        lt = lt.Next;
                    }
                }

                return false;
            };
            dykstra.Run(cancellationToken);

            this.HasSucceeded = true;
        }

        /// <summary>
        /// Gets the weights.
        /// </summary>
        public T[] Weights
        {
            get
            {
                return _weights;
            }
        }

        private class LinkedTarget
        {
            public int Target { get; set; }

            public LinkedTarget Next { get; set; }
        }
    }
}