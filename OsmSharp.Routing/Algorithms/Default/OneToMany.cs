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

using OsmSharp.Routing.Graphs;
using OsmSharp.Routing.Profiles;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Algorithms.Default
{
    /// <summary>
    /// An algorithm to calculate one-to-many weights/paths.
    /// </summary>
    public class OneToMany : AlgorithmBase
    {
        private readonly Graph _graph;
        private readonly IEnumerable<Path> _source;
        private readonly IList<IEnumerable<Path>> _targets;
        private readonly Func<ushort, Factor> _getFactor;
        private readonly float _maxSearch;

        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public OneToMany(Graph graph, Func<ushort, Factor> getFactor, 
            IEnumerable<Path> source, IList<IEnumerable<Path>> targets, float maxSearch)
        {
            _graph = graph;
            _source = source;
            _targets = targets;
            _getFactor = getFactor;
            _maxSearch = float.MaxValue;
        }

        private Tuple<uint, float>[] _best;
        private Dykstra _dykstra;

        /// <summary>
        /// Executes the actual run of the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            _best = new Tuple<uint, float>[_targets.Count];

            var targetIndexesPerVertex = new Dictionary<uint, LinkedTarget>();
            for (var i = 0; i < _targets.Count; i++)
            {
                foreach (var targetpath in _targets[i])
                {
                    var target = targetIndexesPerVertex.TryGetValueOrDefault(targetpath.Vertex);
                    targetIndexesPerVertex[targetpath.Vertex] = new LinkedTarget()
                    {
                        Target = i,
                        Next = target
                    };
                }
            }

            _dykstra = new Dykstra(_graph, _getFactor, _source, _maxSearch, false);
            _dykstra.WasFound += (vertex, weight) =>
            {
                LinkedTarget target;
                if(targetIndexesPerVertex.TryGetValue(vertex, out target))
                { // there is a target for this vertex.
                    while(target != null)
                    {
                        var bestWeight = float.MaxValue;
                        var best = uint.MaxValue;

                        var current = _best[target.Target];

                        var paths = _targets[target.Target];
                        foreach(var path in paths)
                        {
                            if(path.Vertex == vertex &&
                               path.Weight + weight < bestWeight)
                            {
                                best = path.Vertex;
                                bestWeight = path.Weight + weight;
                            }
                            if(current != null &&
                               current.Item1 == path.Vertex &&
                               current.Item2 + weight < bestWeight)
                            {
                                best = current.Item1;
                                bestWeight = current.Item2 + weight;
                            }
                        }

                        _best[target.Target] = new Tuple<uint, float>(
                            best, bestWeight);

                        target = target.Next;
                    }
                }
                return false;
            };
            _dykstra.Run();

            this.HasSucceeded = true;
        }

        /// <summary>
        /// Gets the best weight for the target at the given index.
        /// </summary>
        /// <returns></returns>
        public float GetBestWeight(uint target)
        {
            this.CheckHasRunAndHasSucceeded();

            Tuple<uint, float> best = _best[target];
            if(best != null)
            {
                return best.Item2;
            }
            return float.MaxValue;
        }

        /// <summary>
        /// Gets the best vertex for the target at the given index.
        /// </summary>
        /// <returns></returns>
        public uint GetBestVertex(uint target)
        {
            this.CheckHasRunAndHasSucceeded();

            var best = _best[target];
            if (best != null)
            {
                return best.Item1;
            }
            return uint.MaxValue;
        }

        /// <summary>
        /// Gets the path from source->target.
        /// </summary>
        /// <returns></returns>
        public List<uint> GetPath(uint target)
        {
            this.CheckHasRunAndHasSucceeded();

            var best = _best[target];

            if(best != null)
            {
                Path toTarget;
                if (_dykstra.TryGetVisit(best.Item1, out toTarget))
                {
                    var path = new List<uint>();
                    toTarget.AddToList(path);
                    return path;
                }
            }
            throw new InvalidOperationException("No path could be found to/from source/target.");
        }

        /// <summary>
        /// Returns true if the given vertex was visited and sets the visit output parameters with the actual visit data.
        /// </summary>
        /// <returns></returns>
        public bool TryGetVisit(uint vertex, out Path visit)
        {
            return _dykstra.TryGetVisit(vertex, out visit);
        }

        private class LinkedTarget
        {
            public int Target { get; set; }

            public LinkedTarget Next { get; set; }
        }
    }
}