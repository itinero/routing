// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Algorithms.Weights;
using Itinero.Data.Edges;
using Itinero.Graphs.Directed;
using Itinero.Profiles;
using System;
using System.Collections.Generic;

namespace Itinero.Algorithms.Contracted
{
    /// <summary>
    /// Builds a directed graph from a regular graph.
    /// </summary>
    public class DirectedGraphBuilder<T> : AlgorithmBase
        where T : struct
    {
        private readonly Graphs.Graph _source;
        private readonly DirectedMetaGraph _target;
        private readonly WeightHandler<T> _weightHandler;

        /// <summary>
        /// Creates a new graph builder.
        /// </summary>
        public DirectedGraphBuilder(Itinero.Graphs.Graph source, DirectedMetaGraph target, WeightHandler<T> weightHandler)
        {
            _source = source;
            _target = target;
            _weightHandler = weightHandler;
        }

        /// <summary>
        /// Executes the actual run.
        /// </summary>
        protected override void DoRun()
        {
            float distance;
            ushort edgeProfile;
            bool? direction = null;
            
            var factors = new Dictionary<ushort, Factor>();
            var edgeEnumerator = _source.GetEdgeEnumerator();
            for(uint vertex = 0; vertex < _source.VertexCount; vertex++)
            {
                edgeEnumerator.MoveTo(vertex);
                edgeEnumerator.Reset();
                while(edgeEnumerator.MoveNext())
                {
                    EdgeDataSerializer.Deserialize(edgeEnumerator.Data0, 
                        out distance, out edgeProfile);
                    Factor factor;
                    var weight = _weightHandler.Calculate(edgeProfile, distance, out factor);

                    if(factor.Value != 0)
                    {
                        direction = null;
                        if (factor.Direction == 1)
                        {
                            direction = true;
                            if(edgeEnumerator.DataInverted)
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

                        _weightHandler.AddEdge(_target, edgeEnumerator.From, edgeEnumerator.To, Constants.NO_VERTEX, direction, weight);
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
        public DirectedGraphBuilder(Itinero.Graphs.Graph source, DirectedMetaGraph target, Func<ushort, Factor> getFactor)
            : base(source, target, new DefaultWeightHandler(getFactor))
        {

        }

        /// <summary>
        /// Creates a new graph builder.
        /// </summary>
        public DirectedGraphBuilder(Itinero.Graphs.Graph source, DirectedMetaGraph target, DefaultWeightHandler weightHandler)
            : base(source, target, weightHandler)
        {

        }
    }
}