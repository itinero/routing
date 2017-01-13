// Itinero.Optimization - Route optimization for .NET
// Copyright (C) 2017 Abelshausen Ben
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

using Itinero.Algorithms.Contracted.EdgeBased;
using Itinero.Algorithms.Search;
using Itinero.Algorithms.Weights;
using Itinero.Data.Contracted;
using Itinero.Graphs.Directed;
using Itinero.LocalGeo;
using Itinero.Profiles;
using System;
using System.Collections.Generic;

namespace Itinero.Algorithms.Matrices
{
    /// <summary>
    /// An algorithm to calculate a turn-aware weight matrix.
    /// </summary>
    public class DirectedWeightMatrixAlgorithm<T> : AlgorithmBase
        where T : struct
    {
        private readonly RouterBase _router;
        private readonly IProfileInstance _profile;
        private readonly WeightHandler<T> _weightHandler;
        private readonly MassResolvingAlgorithm _massResolver;

        private readonly Dictionary<uint, Dictionary<int, LinkedEdgePath<T>>> _buckets;
        private readonly DirectedDynamicGraph _graph;
        private readonly T _max;

        /// <summary>
        /// Creates a new weight-matrix algorithm.
        /// </summary>
        public DirectedWeightMatrixAlgorithm(RouterBase router, IProfileInstance profile, WeightHandler<T> weightHandler, Coordinate[] locations,
            T? max = null)
            : this(router, profile, weightHandler, new MassResolvingAlgorithm(
                router, new IProfileInstance[] { profile }, locations), max)
        {

        }

        /// <summary>
        /// Creates a new weight-matrix algorithm.
        /// </summary>
        public DirectedWeightMatrixAlgorithm(RouterBase router, IProfileInstance profile, WeightHandler<T> weightHandler, MassResolvingAlgorithm massResolver, T? max = null)
        {
            _router = router;
            _profile = profile;
            _weightHandler = weightHandler;
            _massResolver = massResolver;

            ContractedDb contractedDb;
            if (!router.Db.TryGetContracted(profile.Profile, out contractedDb))
            {
                throw new NotSupportedException(
                    "Contraction-based many-to-many calculates are not supported in the given router db for the given profile.");
            }
            if (!contractedDb.HasEdgeBasedGraph)
            {
                throw new NotSupportedException(
                    "Contraction-based edge-based many-to-many calculates are not supported in the given router db for the given profile.");
            }
            _graph = contractedDb.EdgeBasedGraph;
            weightHandler.CheckCanUse(contractedDb);
            if (max.HasValue)
            {
                _max = max.Value;
            }
            else
            {
                _max = weightHandler.Infinite;
            }

            _buckets = new Dictionary<uint, Dictionary<int, LinkedEdgePath<T>>>();
        }

        private Dictionary<int, RouterPointError> _errors; // all errors per routerpoint idx.
        private List<int> _resolvedPointsIndices; // the original routerpoint per resolved point index.
        private List<RouterPoint> _resolvedPoints; // only the valid resolved points.

        private T[][] _weights; // the weights between all valid resolved points.              
        private EdgePath<T>[] _sourcePaths;
        private EdgePath<T>[] _targetPaths;

        /// <summary>
        /// Executes the algorithm.
        /// </summary>
        protected sealed override void DoRun()
        {
            // run mass resolver if needed.
            if (!_massResolver.HasRun)
            {
                _massResolver.Run();
            }

            // create error and resolved point management data structures.
            _resolvedPoints = _massResolver.RouterPoints;
            _errors = new Dictionary<int, RouterPointError>(_resolvedPoints.Count);
            _resolvedPointsIndices = new List<int>(_resolvedPoints.Count);
            for (var i = 0; i < _resolvedPoints.Count; i++)
            {
                _resolvedPointsIndices.Add(i);
            }

            // convert sources into directed paths.
            _sourcePaths = new EdgePath<T>[_resolvedPoints.Count * 2];
            for (var i = 0; i < _resolvedPoints.Count; i++)
            {
                _resolvedPointsIndices.Add(i);

                var paths = _resolvedPoints[i].ToEdgePathsDirected(_router.Db, _weightHandler, true);
                if (paths.Length == 0)
                {
                    this.ErrorMessage = string.Format("Source at {0} could not be resolved properly.", i);
                    return;
                }

                _sourcePaths[i * 2 + 0] = paths[0];
                if (paths.Length == 2)
                {
                    _sourcePaths[i * 2 + 1] = paths[1];
                }
            }

            // convert targets into directed paths.
            _targetPaths = new EdgePath<T>[_resolvedPoints.Count * 2];
            for (var i = 0; i < _resolvedPoints.Count; i++)
            {
                var paths = _resolvedPoints[i].ToEdgePathsDirected(_router.Db, _weightHandler, false);
                if (paths.Length == 0)
                {
                    this.ErrorMessage = string.Format("Target at {0} could not be resolved properly.", i);
                    return;
                }

                // make sure paths are the opposive of the sources.
                if (paths[0].Edge == _sourcePaths[i * 2 + 0].Edge)
                { // switchs.
                    _targetPaths[i * 2 + 1] = paths[0];
                    if (paths.Length == 2)
                    {
                        _targetPaths[i * 2 + 0] = paths[1];
                    }
                }
                else
                { // keep.
                    _targetPaths[i * 2 + 0] = paths[0];
                    if (paths.Length == 2)
                    {
                        _targetPaths[i * 2 + 1] = paths[1];
                    }
                }
            }

            // put in default weights and weights for one-edge-paths.
            _weights = new T[_sourcePaths.Length][];
            for (var i = 0; i < _sourcePaths.Length; i++)
            {
                var source = _sourcePaths[i];
                if (source == null)
                {
                    continue;
                }

                _weights[i] = new T[_targetPaths.Length];
                for (var j = 0; j < _targetPaths.Length; j++)
                {
                    var target = _targetPaths[j];
                    if (target == null)
                    {
                        continue;
                    }

                    _weights[i][j] = _weightHandler.Infinite;

                    if (target.Edge == -source.Edge)
                    {
                        var s = i / 2;
                        var t = j / 2;
                        var sourcePoint = _resolvedPoints[s];
                        var targetPoint = _resolvedPoints[t];

                        EdgePath<T> newPath = null;
                        if (source.Edge > 0 &&
                            sourcePoint.Offset <= targetPoint.Offset)
                        {
                            newPath = sourcePoint.EdgePathTo(_router.Db, _weightHandler, targetPoint);
                        }
                        else if (source.Edge < 0 &&
                            sourcePoint.Offset >= targetPoint.Offset)
                        {
                            newPath = sourcePoint.EdgePathTo(_router.Db, _weightHandler, targetPoint);
                        }

                        if (newPath != null)
                        {
                            if (_weightHandler.IsLargerThan(_weights[i][j], newPath.Weight))
                            {
                                _weights[i][j] = newPath.Weight;
                            }
                        }
                    }
                }
            }

            // do forward searches into buckets.
            for (var i = 0; i < _sourcePaths.Length; i++)
            {
                var path = _sourcePaths[i];
                if (path != null)
                {
                    var forward = new Itinero.Algorithms.Contracted.EdgeBased.Dykstra<T>(_graph, _weightHandler, new EdgePath<T>[] { path }, (v) => null, false, _max);
                    forward.WasFound += (foundPath) =>
                    {
                        LinkedEdgePath<T> visits;
                        forward.TryGetVisits(foundPath.Vertex, out visits);
                        return this.ForwardVertexFound(i, foundPath.Vertex, visits);
                    };
                    forward.Run();
                }
            }

            // do backward searches into buckets.
            for (var i = 0; i < _targetPaths.Length; i++)
            {
                var path = _targetPaths[i];
                if (path != null)
                {
                    var backward = new Itinero.Algorithms.Contracted.EdgeBased.Dykstra<T>(_graph, _weightHandler, new EdgePath<T>[] { path }, (v) => null, true, _max);
                    backward.WasFound += (foundPath) =>
                    {
                        LinkedEdgePath<T> visits;
                        backward.TryGetVisits(foundPath.Vertex, out visits);
                        return this.BackwardVertexFound(i, foundPath.Vertex, visits);
                    };
                    backward.Run();
                }
            }

            // check for invalids.
            var invalidTargetCounts = new int[_sourcePaths.Length];
            var nonNullInvalids = new HashSet<int>();
            for (var s = 0; s < _weights.Length; s++)
            {
                var invalids = 0;
                if (_weights[s] != null)
                {
                    for (var t = 0; t < _weights[s].Length; t++)
                    {
                        if (t != s)
                        {
                            if (_weightHandler.GetMetric(_weights[s][t]) == float.MaxValue)
                            {
                                invalids++;
                                invalidTargetCounts[t]++;
                                if (invalidTargetCounts[t] > (_sourcePaths.Length - 1) / 2)
                                {
                                    nonNullInvalids.Add(t);
                                }
                            }
                        }
                    }
                }
                else
                {
                    invalids = _targetPaths.Length;

                    _weights[s] = new T[_targetPaths.Length];
                    for (var t = 0; t < _weights[s].Length; t++)
                    {
                        _weights[s][t] = _weightHandler.Infinite;
                    }
                }

                if (invalids > (_sourcePaths.Length - 1) / 2)
                {
                    nonNullInvalids.Add(s);
                }
            }

            // take into account the non-null invalids now.
            if (nonNullInvalids.Count > 0)
            { // shrink lists and add errors.
                // convert to original indices.
                var originalInvalids = new HashSet<int>();
                foreach (var invalid in nonNullInvalids)
                { // check if both are invalid for each router point.
                    if (invalid % 2 == 0)
                    {
                        if (originalInvalids.Contains(invalid + 1))
                        {
                            originalInvalids.Add(invalid / 2);
                        }
                    }
                    else
                    {
                        if (originalInvalids.Contains(invalid - 1))
                        {
                            originalInvalids.Add(invalid / 2);
                        }
                    }
                }

                _resolvedPoints = _resolvedPoints.ShrinkAndCopyList(originalInvalids);
                _resolvedPointsIndices = _resolvedPointsIndices.ShrinkAndCopyList(originalInvalids);

                // convert back to the path indexes.
                nonNullInvalids = new HashSet<int>();
                foreach (var invalid in originalInvalids)
                {
                    nonNullInvalids.Add(invalid * 2);
                    nonNullInvalids.Add(invalid * 2 + 1);
                }

                foreach (var invalid in nonNullInvalids)
                {
                    _errors[_resolvedPointsIndices[invalid / 2]] = new RouterPointError()
                    {
                        Code = RouterPointErrorCode.NotRoutable,
                        Message = "Location could not routed to or from."
                    };
                }

                _weights = _weights.SchrinkAndCopyMatrix(nonNullInvalids);
            }

            this.HasSucceeded = true;
        }

        /// <summary>
        /// Called when a forward vertex was found.
        /// </summary>
        /// <returns></returns>
        private bool ForwardVertexFound(int i, uint vertex, LinkedEdgePath<T> visit)
        {
            Dictionary<int, LinkedEdgePath<T>> bucket;
            if (!_buckets.TryGetValue(vertex, out bucket))
            {
                bucket = new Dictionary<int, LinkedEdgePath<T>>();
                _buckets.Add(vertex, bucket);
            }
            bucket[i] = visit;
            return false;
        }

        /// <summary>
        /// Called when a backward vertex was found.
        /// </summary>
        /// <returns></returns>
        private bool BackwardVertexFound(int i, uint vertex, LinkedEdgePath<T> backwardVisit)
        {
            Dictionary<int, LinkedEdgePath<T>> bucket;
            if (_buckets.TryGetValue(vertex, out bucket))
            {
                var edgeEnumerator = _graph.GetEdgeEnumerator();

                var originalBackwardVisit = backwardVisit;
                foreach (var pair in bucket)
                {
                    var best = _weights[pair.Key][i];

                    var forwardVisit = pair.Value;
                    while (forwardVisit != null)
                    {
                        var forwardCurrent = forwardVisit.Path;
                        if (_weightHandler.IsLargerThan(forwardCurrent.Weight, best))
                        {
                            forwardVisit = forwardVisit.Next;
                            continue;
                        }
                        backwardVisit = originalBackwardVisit;
                        while (backwardVisit != null)
                        {
                            var backwardCurrent = backwardVisit.Path;
                            var totalCurrentWeight = _weightHandler.Add(forwardCurrent.Weight, backwardCurrent.Weight);
                            if (_weightHandler.IsSmallerThan(totalCurrentWeight, best))
                            { // potentially a weight improvement.
                                var allowed = true;

                                // check u-turn.
                                var sequence2Forward = backwardCurrent.GetSequence2(edgeEnumerator);
                                var sequence2Current = forwardCurrent.GetSequence2(edgeEnumerator);
                                if (sequence2Current != null && sequence2Current.Length > 0 &&
                                    sequence2Forward != null && sequence2Forward.Length > 0)
                                {
                                    if (sequence2Current[sequence2Current.Length - 1] ==
                                        sequence2Forward[sequence2Forward.Length - 1])
                                    {
                                        allowed = false;
                                    }
                                }

                                //// check restrictions.
                                //if (restrictions != null && allowed)
                                //{
                                //    allowed = false;
                                //    var sequence = new List<uint>(
                                //        forwardCurrent.GetSequence2(edgeEnumerator));
                                //    sequence.Reverse();
                                //    sequence.Add(current.Vertex);
                                //    var s1 = backwardPath.Path.GetSequence2(edgeEnumerator);
                                //    sequence.AddRange(s1);

                                //    allowed = restrictions.IsSequenceAllowed(sequence);
                                //}

                                if (allowed)
                                {
                                    best = totalCurrentWeight;
                                }
                            }
                            backwardVisit = backwardVisit.Next;
                        }
                        forwardVisit = forwardVisit.Next;
                    }

                    _weights[pair.Key][i] = best;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the router.
        /// </summary>
        public RouterBase Router
        {
            get
            {
                return _router;
            }
        }

        /// <summary>
        /// Gets the profile.
        /// </summary>
        public IProfileInstance Profile
        {
            get
            {
                return _profile;
            }
        }

        /// <summary>
        /// Gets the mass resolver.
        /// </summary>
        public MassResolvingAlgorithm MassResolver
        {
            get
            {
                return _massResolver;
            }
        }

        /// <summary>
        /// Gets the weights between all valid router points.
        /// </summary>
        public T[][] Weights
        {
            get
            {
                this.CheckHasRunAndHasSucceeded();

                return _weights;
            }
        }

        /// <summary>
        /// Gets the valid router points.
        /// </summary>
        public List<RouterPoint> RouterPoints
        {
            get
            {
                this.CheckHasRunAndHasSucceeded();

                return _resolvedPoints;
            }
        }

        /// <summary>
        /// Gets the source paths.
        /// </summary>
        public EdgePath<T>[] SourcePaths
        {
            get
            {
                return _sourcePaths;
            }
        }

        /// <summary>
        /// Gets the target paths.
        /// </summary>
        public EdgePath<T>[] TargetPaths
        {
            get
            {
                return _targetPaths;
            }
        }

        /// <summary>
        /// Returns the index of the original router point in the list of routable routerpoint.
        /// </summary>
        /// <returns></returns>
        public int IndexOf(int originalRouterPointIndex)
        {
            this.CheckHasRunAndHasSucceeded();

            return _resolvedPointsIndices.IndexOf(originalRouterPointIndex);
        }

        /// <summary>
        /// Returns the index of the router point in the original router points list.
        /// </summary>
        /// <returns></returns>
        public int OriginalIndexOf(int routerPointIdx)
        {
            this.CheckHasRunAndHasSucceeded();

            return _resolvedPointsIndices[routerPointIdx];
        }

        /// <summary>
        /// Returns the errors indexed per original routerpoint index.
        /// </summary>
        public Dictionary<int, RouterPointError> Errors
        {
            get
            {
                this.CheckHasRunAndHasSucceeded();

                return _errors;
            }
        }
    }

    /// <summary>
    /// An algorithm to calculate a weight-matrix for a set of locations.
    /// </summary>
    public sealed class DirectedWeightMatrixAlgorithm : DirectedWeightMatrixAlgorithm<float>
    {
        /// <summary>
        /// Creates a new weight-matrix algorithm.
        /// </summary>
        public DirectedWeightMatrixAlgorithm(RouterBase router, IProfileInstance profile, MassResolvingAlgorithm massResolver, float max = float.MaxValue)
            : base(router, profile, profile.DefaultWeightHandler(router), massResolver, max)
        {

        }
        /// <summary>
        /// Creates a new weight-matrix algorithm.
        /// </summary>
        public DirectedWeightMatrixAlgorithm(RouterBase router, IProfileInstance profile, Coordinate[] locations, float max = float.MaxValue)
            : base(router, profile, profile.DefaultWeightHandler(router), locations, max)
        {

        }
    }
}