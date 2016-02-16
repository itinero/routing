// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
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

using Itinero.Graphs.Directed;
using Itinero.Profiles;
using System;
using System.Collections.Generic;

namespace Itinero.Algorithms.Contracted
{
    /// <summary>
    /// Builds a directed graph from a regular graph.
    /// </summary>
    public class DirectedGraphBuilder : AlgorithmBase
    {
        private readonly Itinero.Graphs.Graph _source;
        private readonly DirectedMetaGraph _target;
        private readonly Func<ushort, Factor> _getFactor;

        /// <summary>
        /// Creates anew graph builder.
        /// </summary>
        public DirectedGraphBuilder(Itinero.Graphs.Graph source, DirectedMetaGraph target, Func<ushort, Factor> getFactor)
        {
            _source = source;
            _target = target;
            _getFactor = getFactor;
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
                    Itinero.Data.EdgeDataSerializer.Deserialize(edgeEnumerator.Data0, 
                        out distance, out edgeProfile);
                    var factor = Factor.NoFactor;
                    if(!factors.TryGetValue(edgeProfile, out factor))
                    { // get from vehicle profile.
                        factor = _getFactor(edgeProfile);
                        factors[edgeProfile] = factor;
                    }

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
                        var data = Itinero.Data.Contracted.ContractedEdgeDataSerializer.Serialize(
                            distance * factor.Value, direction);

                        _target.AddEdge(edgeEnumerator.From, edgeEnumerator.To, data, Constants.NO_VERTEX);
                    }
                }
            }

            this.HasSucceeded = true;
        }
    }
}