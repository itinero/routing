// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Paul Den Dulk, Abelshausen Ben
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

using Itinero.Algorithms;
using Itinero.Algorithms.Default;
using Itinero.Graphs.Geometric;
using Itinero.Profiles;
using System;
using System.Collections.Generic;

namespace Itinero.Algorithms.Networks.Analytics
{
    /// <summary>
    /// Represents an edgevisitor based on the dykstra implementation.
    /// </summary>
    public class DykstraEdgeVisitor : AlgorithmBase, IEdgeVisitor
    {
        private readonly GeometricGraph _geometricGraph;
        private readonly Func<ushort, Factor> _getFactor;
        private readonly IEnumerable<EdgePath<float>> _source;
        private readonly float _limitInSeconds;

        /// <summary>
        /// Creates a new dykstra edge visitor.
        /// </summary>
        public DykstraEdgeVisitor(GeometricGraph geometricGraph,
            Func<ushort, Factor> getFactor, IEnumerable<EdgePath<float>> source, float limitInSeconds)
        {
            _geometricGraph = geometricGraph;
            _getFactor = getFactor;
            _source = source;
            _limitInSeconds = limitInSeconds;
        }

        /// <summary>
        /// Executes the actual algorithm.
        /// </summary>
        protected override void DoRun()
        {
            // create dykstra algorithm.
            // search forward starting from sourcePaths with no restrictions.
            var dykstra = new Dykstra(_geometricGraph.Graph, _getFactor, v => Constants.NO_VERTEX,
                _source, _limitInSeconds, false);

            dykstra.WasEdgeFound += (v1, v2, w1, w2, e, length) =>
            {
                if (Visit == null) return false;

                // Calculate weight at start vertex.
                uint edgeId;
                if (e > 0)
                {
                    edgeId = (uint)e - 1;
                }
                else
                {
                    edgeId = (uint)((-e) - 1);
                }
                var edge = _geometricGraph.GetEdge(edgeId);
                var shape = _geometricGraph.GetShape(edge);
                
                Visit?.Invoke(edgeId, w1, w2, shape);

                return false; // return false, returning true stops the search!
            };

            dykstra.Run();
        }

        /// <summary>
        /// Gets or sets the visit delegate.
        /// </summary>
        public VisitDelegate Visit { get; set; }
    }
}