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
        private readonly Dictionary<long, List<EdgePath>> _targets;
        private readonly Dykstra _dykstra;

        /// <summary>
        /// Creates a new one-to-all dykstra algorithm instance.
        /// </summary>
        public OneToOneDykstraHelper(Graph graph, Func<ushort, Factor> getFactor, Func<uint, uint[]> getRestriction,
            IEnumerable<EdgePath> sources, IEnumerable<EdgePath> targets, float sourceMax, bool backward)
        {
            _dykstra = new Dykstra(graph, getFactor, getRestriction, sources, sourceMax, backward);
            _dykstra.WasEdgeFound = (directedEdgeId, weight) =>
            {
                return this.WasEdgeFound(directedEdgeId, weight);
            };

            _targets = new Dictionary<long, List<EdgePath>>();
            foreach(var target in targets)
            {
                List<EdgePath> paths;
                if (!_targets.TryGetValue(target.DirectedEdge, out paths))
                {
                    paths = new List<EdgePath>();
                    _targets.Add(target.DirectedEdge, paths);
                }
                paths.Add(target);
            }
        }

        private EdgePath _best = null;

        /// <summary>
        /// Called when an edge was found.
        /// </summary>
        private bool WasEdgeFound(long directedEdgeId, float weight)
        {
            EdgePath visit;
            List<EdgePath> paths;
            if (_targets.TryGetValue(directedEdgeId, out paths) &&
                _dykstra.TryGetVisit(directedEdgeId, out visit))
            {
                if (visit.From == null)
                {
                    throw new NotSupportedException("One-edge paths are not supported.");
                }
                foreach (var target in paths)
                {
                    var pathToTarget = new EdgePath(Constants.NO_EDGE, target.Weight + visit.From.Weight,
                        visit.From);
                    if (_best == null || pathToTarget.Weight < _best.Weight)
                    {
                        _best = pathToTarget;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Executes the actual algorithm.
        /// </summary>
        protected override void DoRun()
        {
            _dykstra.Run();

            this.HasRun = true;
            this.HasSucceeded = _best != null;
        }

        /// <summary>
        /// Gets the best path.
        /// </summary>
        public EdgePath BestPath
        {
            get
            {
                this.CheckHasRunAndHasSucceeded();

                return _best;
            }
        }

        /// <summary>
        /// Gets the path from source->target.
        /// </summary>
        /// <returns></returns>
        public List<uint> GetPath(out float weight)
        {
            this.CheckHasRunAndHasSucceeded();
            
            var path = new List<uint>();
            weight = _best.Weight;
            _best.ToPath(_dykstra.Graph).AddToList(path);
            return path;
        }
    }
}