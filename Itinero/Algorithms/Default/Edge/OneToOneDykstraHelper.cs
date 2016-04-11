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

using Itinero.Graphs;
using Itinero.Profiles;
using System;
using System.Collections.Generic;

namespace Itinero.Algorithms.Default.Edge
{
    /// <summary>
    /// A helper class that contains functionality to use the dykstra algorithm as a one-to-one router.
    /// </summary>
    public class OneToOneDykstraHelper : AlgorithmBase
    {
        private readonly IEnumerable<EdgePath> _targets;
        private readonly Dykstra _dykstra;

        /// <summary>
        /// Creates a new one-to-all dykstra algorithm instance.
        /// </summary>
        public OneToOneDykstraHelper(Graph graph, Func<ushort, Factor> getFactor, Func<uint, uint[]> getRestriction,
            IEnumerable<EdgePath> sources, IEnumerable<EdgePath> targets, float sourceMax, bool backward)
        {
            _dykstra = new Dykstra(graph, getFactor, getRestriction, sources, sourceMax, backward);
            _dykstra.WasEdgeFound = (vertex, weight) =>
            {
                _maxBackward = weight;
                return this.ReachedVertexBackward((uint)vertex, weight);
            };

            _targets = targets;
        }
        
        /// <summary>
        /// Called when an edge was found.
        /// </summary>
        private bool WasEdgeFound(uint vertex, uint edge, float weight)
        {

        }

        /// <summary>
        /// Executes the actual algorithm.
        /// </summary>
        protected override void DoRun()
        {
            throw new NotImplementedException();
        }
    }
}
