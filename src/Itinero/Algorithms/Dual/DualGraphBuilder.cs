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
using Itinero.Graphs.Directed;
using Itinero.Graphs;
using Itinero.Algorithms.Restrictions;
using Itinero.Data.Edges;
using Itinero.Profiles;
using System.Threading;

namespace Itinero.Algorithms.Dual
{
    /// <summary>
    /// An algorithm to build a dual graph, turning edges into vertices.
    /// </summary>
    public class DualGraphBuilder<T> : AlgorithmBase
        where T : struct
    {
        private readonly Graphs.Graph _source;
        private readonly DirectedMetaGraph _target;
        private readonly WeightHandler<T> _weightHandler;
        private readonly RestrictionCollection _restrictions;

        /// <summary>
        /// Creates a new dual graph builder.
        /// </summary>
        public DualGraphBuilder(Graphs.Graph source, DirectedMetaGraph target,
            WeightHandler<T> weightHandler, RestrictionCollection restrictions)
        {
            _source = source;
            _target = target;
            _weightHandler = weightHandler;
            _restrictions = restrictions;
        }

        /// <summary>
        /// Executes the actual algorithm.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            float distance;
            ushort edgeProfile;

            var enumerator1 = _source.GetEdgeEnumerator();
            var enumerator2 = _source.GetEdgeEnumerator();
            for (uint v = 0; v < _source.VertexCount; v++)
            {
                enumerator1.MoveTo(v);
                while (enumerator1.MoveNext())
                {
                    EdgeDataSerializer.Deserialize(enumerator1.Data0,
                        out distance, out edgeProfile);
                    var accessible1 = false;
                    var weight1 = _weightHandler.CalculateWeightAndDir(edgeProfile, distance, out accessible1);
                    if (enumerator1.DataInverted)
                    {
                        var dir = weight1.Direction;
                        dir.Reverse();
                        weight1.Direction = dir;
                    }
                    if (!accessible1)
                    { // not accessible.
                        continue;
                    }
                    var direction1 = weight1.Direction;
                    var edge1 = enumerator1.DirectedEdgeId();

                    // look at the neighbours of this edge.
                    enumerator2.MoveTo(enumerator1.To);
                    _restrictions.Update(enumerator1.To);
                    while (enumerator2.MoveNext())
                    {
                        var turn = new Turn(new OriginalEdge(v, enumerator1.To), Constants.NO_VERTEX);
                        EdgeDataSerializer.Deserialize(enumerator2.Data0,
                            out distance, out edgeProfile);
                        var accessible2 = false;
                        var weight2 = _weightHandler.CalculateWeightAndDir(edgeProfile, distance, out accessible2);
                        if (enumerator2.DataInverted)
                        {
                            var dir = weight2.Direction;
                            dir.Reverse();
                            weight2.Direction = dir;
                        }
                        if (!accessible2)
                        { // not accessible.
                            continue;
                        }

                        var direction2 = weight2.Direction;
                        turn.Vertex3 = enumerator2.To;
                        if (turn.IsUTurn)
                        { // is a u-turn, leave this out!
                            continue;
                        }

                        var direction = Dir.Combine(direction1, direction2);

                        if (direction.F &&
                            turn.IsRestrictedBy(_restrictions))
                        { // turn is restricted.
                            direction.F = false;
                        }

                        if (!direction.F)
                        { // there is no possible combination for these two edges.
                            continue;
                        }

                        // ok, we need to add this edge, it's a non-restricted turn, not a u-turn and edges are in correct direction.
                        var edge2 = enumerator2.DirectedEdgeId();

                        _weightHandler.AddOrUpdateEdge(_target, edge1.Raw, edge2.Raw, Constants.NO_VERTEX, true,
                            weight1.Weight);
                        //direction.Reverse();
                        _weightHandler.AddOrUpdateEdge(_target, edge2.Raw, edge1.Raw, Constants.NO_VERTEX, false,
                            weight1.Weight);
                    }
                }
            }
        }
    }
}