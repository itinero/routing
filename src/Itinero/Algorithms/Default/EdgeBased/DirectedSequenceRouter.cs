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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Itinero.Algorithms.Matrices;
using Itinero.Algorithms.PriorityQueues;
using Itinero.Algorithms.Search;
using Itinero.Algorithms.Weights;
using Itinero.Data.Network;

[assembly: InternalsVisibleTo("Itinero.Test")]
namespace Itinero.Algorithms.Default.EdgeBased
{
    /// <summary>
    /// A directed sequence router.
    /// </summary>
    public class DirectedSequenceRouter : AlgorithmBase
    {
        private readonly IMassResolvingAlgorithm _massResolvingAlgorithm ;
        private readonly float _turnPenalty;
        private readonly Tuple<bool?, bool?>[] _fixedTurns = null;
        private readonly IDirectedWeightMatrixAlgorithm<float> _directedWeightMatrixAlgorithm;
        private const float NonFixedTurnPentalty = 100000f; // a 'big' number that is applied as a penalty to a turn that ignores the fixed turns array.

        /// <summary>
        /// Creates a new router.
        /// </summary>
        /// <param name="massResolvingAlgorithm">The mass resolving algorithm.</param>
        /// <param name="turnPenalty">The turn penalty.</param>
        /// <param name="fixedTurns">Turn that cannot change, if any.</param>
        public DirectedSequenceRouter(IMassResolvingAlgorithm massResolvingAlgorithm, float turnPenalty, Tuple<bool?, bool?>[] fixedTurns = null)
        {
            _massResolvingAlgorithm = massResolvingAlgorithm;
            _turnPenalty = turnPenalty;
            _fixedTurns = fixedTurns;

            _directedWeightMatrixAlgorithm = null;
        }

        /// <summary>
        /// Creates a new router.
        /// </summary>
        /// <param name="directedWeightMatrixAlgorithm">The directed weight matrix algorithm.</param>
        /// <param name="turnPenalty">The turn penalty.</param>
        /// <param name="fixedTurns">Turn that cannot change, if any.</param>
        public DirectedSequenceRouter(IDirectedWeightMatrixAlgorithm<float> directedWeightMatrixAlgorithm, float turnPenalty, Tuple<bool?, bool?>[] fixedTurns = null)
        {
            _massResolvingAlgorithm = directedWeightMatrixAlgorithm.MassResolver;
            _directedWeightMatrixAlgorithm = directedWeightMatrixAlgorithm;
            _turnPenalty = turnPenalty;
            _fixedTurns = fixedTurns;
        }

        protected override void DoRun(CancellationToken cancellationToken)
        {
            var router = _massResolvingAlgorithm.Router;
            var profile = _massResolvingAlgorithm.Profiles[0];
            var weightHandler = profile.DefaultWeightHandler(router);
            
            // make sure the resolve has run.
            if (!_massResolvingAlgorithm.HasRun)
            {
                _massResolvingAlgorithm.Run(cancellationToken);
            }
            
            // calculate directed weights.
            var directedWeightMatricAlgorithm = _directedWeightMatrixAlgorithm;
            if (directedWeightMatricAlgorithm == null)
            { // create weight matrix algorithm on-the-fly.
                directedWeightMatricAlgorithm = new DirectedWeightMatrixAlgorithm(_massResolvingAlgorithm.Router,
                    _massResolvingAlgorithm.Profiles[0], _massResolvingAlgorithm);
                directedWeightMatricAlgorithm.Run(cancellationToken);
                if (!directedWeightMatricAlgorithm.HasSucceeded)
                {
                    this.ErrorMessage =
                        $"Failed to calculate weight matrix: {directedWeightMatricAlgorithm.ErrorMessage}";
                    return;
                }
            }

            // calculate optimal turns.
            var turns = CalculateOptimimal(directedWeightMatricAlgorithm.Weights, _turnPenalty, _fixedTurns);
            
            // build route.
            var routes = new List<Result<Route>>();
            for (var p = 0; p < directedWeightMatricAlgorithm.RouterPoints.Count - 1; p++)
            {
                var point1 = directedWeightMatricAlgorithm.RouterPoints[p];
                var point2 = directedWeightMatricAlgorithm.RouterPoints[p + 1];

                var point1Turn = (turns[p] % 4);
                var point1Departure = (point1Turn == 1 || point1Turn == 3) ? 1 : 0;
                var point2Turn = (turns[p + 1] % 4);
                var point2Arrival = (point2Turn == 2 || point2Turn == 3) ? 1 : 0;

                var path1 = directedWeightMatricAlgorithm.SourcePaths[p * 2 + point1Departure];
                if (path1 == null)
                {
                    routes.Add(new Result<Route>(
                        $"Raw path was not found between {p}[{point1Departure}]->{p + 1}[{point2Arrival}]: Start location impossible."));
                    continue;
                }
                var path2 = directedWeightMatricAlgorithm.TargetPaths[(p + 1) * 2 + point2Arrival];
                if (path2 == null)
                {
                    routes.Add(new Result<Route>(
                        $"Raw path was not found between {p}[{point1Departure}]->{p + 1}[{point2Arrival}]: End location impossible."));
                    continue;
                }
                var pairFromEdgeId = router.Db.Network.GetEdges(path1.From.Vertex).First(x => x.To == path1.Vertex).IdDirected();
                var pairToEdgeId = router.Db.Network.GetEdges(path2.Vertex).First(x => x.To == path2.From.Vertex).IdDirected();
                
                var localRouteRawResult = router.TryCalculateRaw(profile, weightHandler, pairFromEdgeId, pairToEdgeId, null);
                if (localRouteRawResult.IsError)
                {
                    routes.Add(new Result<Route>(
                        $"Raw path was not found between {p}[{point1Departure}]->{p + 1}[{point2Arrival}]: {localRouteRawResult.ErrorMessage}"));
                    continue;
                }

                var localRouteRaw = localRouteRawResult.Value;
                localRouteRaw.StripSource();
                localRouteRaw.StripTarget();

                var localRoute = router.BuildRoute(profile, weightHandler, point1, point2, localRouteRaw);
                if (localRoute.IsError)
                {
                    routes.Add(new Result<Route>(
                        $"Route was not found between {p}[{point1Departure}]->{p+1}[{point2Arrival}]: {localRoute.ErrorMessage}"));
                    continue;
                }

                if (localRoute.Value.Stops != null &&
                    localRoute.Value.Stops.Length == 2)
                { // add extra information to stops, if present.
                    var point1Original = directedWeightMatricAlgorithm.OriginalLocationIndex(p);
                    var point2Original = directedWeightMatricAlgorithm.OriginalLocationIndex(p + 1);

                    localRoute.Value.Stops[0].Attributes.AddOrReplace("stop",
                        point1Original.ToInvariantString());
                    localRoute.Value.Stops[1].Attributes.AddOrReplace("stop",
                        point2Original.ToInvariantString());
                }

                routes.Add(new Result<Route>(localRoute.Value));
            }

            this.Routes = routes;
            this.HasSucceeded = true;
        }
        
        /// <summary>
        /// Gets the routes.
        /// </summary>
        public List<Result<Route>> Routes { get; private set; }

        /// <summary>
        /// Calculates optimal turns.
        /// </summary>
        /// <param name="directedWeights">The directed weight matrix.</param>
        /// <param name="turnPenalty">The turn penalty.</param>
        /// <param name="fixedTurns">Any turns that can't change.</param>
        /// <returns>A sequence of optimal turns.</returns>
        internal static int[] CalculateOptimimal(float[][] directedWeights, float turnPenalty, Tuple<bool?, bool?>[] fixedTurns = null)
        {
            // on the fixedTurns:
            // it's possible for the user of this algorithm to request impossible routes.
            // we respond to this by applying a big penalty to turns that are restricted but 
            // instad of making them impossible. This way this algorithm still doesn't return restricted
            // routes when possible but returns a restricted route when the request is impossible.
            // It also minizes the violated restrictions this way.
            
            var settled = new HashSet<int>();
            var queue = new BinaryHeap<int>();
            var paths = new Tuple<int, float>[directedWeights.Length * 2]; // (int previous, float cost)[capacity];
            for (var i = 0; i < paths.Length; i++)
            {
                paths[i] = new Tuple<int, float>(int.MaxValue, float.MaxValue);
            }
            
            // build path.
            var nextArray = new int[directedWeights.Length / 2];
            var p = 0;
            nextArray[0] = -1;
            var length = 1;
            for (var c = 1; c < directedWeights.Length / 2; c++)
            {
                nextArray[c] = -1;
                
                // check if p -> c is possible.
                if (!(directedWeights[p * 2 + 0][c * 2 + 0] < float.MaxValue) &&
                    !(directedWeights[p * 2 + 0][c * 2 + 1] < float.MaxValue) &&
                    !(directedWeights[p * 2 + 1][c * 2 + 0] < float.MaxValue) &&
                    !(directedWeights[p * 2 + 1][c * 2 + 1] < float.MaxValue)) continue;
                
                nextArray[p] = c;
                length++;
                p = c;
            }

            if (fixedTurns?[0].Item2 == null)
            { // there is no fixed departure turn, consider both options.
                queue.Push(0, 0);
                queue.Push(1, 0);
            }
            else
            { // there is fixed turn, only add the relevant turn.
                if (!fixedTurns[0].Item2.Value)
                { // turn is fixed departure forward.
                    queue.Push(0, 0);
                    queue.Push(1, NonFixedTurnPentalty);
                }
                else
                { // turn is fixed departure backward.
                    queue.Push(0, NonFixedTurnPentalty);
                    queue.Push(1, 0);
                }
            }
            var bestLast = -1;
            while (queue.Count > 0)
            {
                var currentWeight = queue.PeekWeight();
                var current = queue.Pop();
                if (settled.Contains(current))
                { // was already settled before.
                    continue;
                }
                settled.Add(current);
                
                // check if we're done.
                var currentTurn = current % 4;
                var currentVisit = (current - currentTurn) / 4;
                if (nextArray[currentVisit] == -1)
                { // current = last, done!
                    bestLast = current;
                    break;
                }
                var currentDeparture = (currentTurn == 1 || currentTurn == 3) ? 1 : 0;
                var nextVisit = nextArray[currentVisit];
                for (var nextTurn = 0; nextTurn < 4; nextTurn++)
                {
                    var next = nextVisit * 4 + nextTurn;
                    if (settled.Contains(next))
                    { // already settled, don't queue again.
                        continue;
                    }
                    var nextArrival =  (nextTurn == 2 || nextTurn == 3) ? 1 : 0;
                    
                    // figure out penalty if any.
                    var fixedTurnPenalty = 0f;
                    if (fixedTurns?[nextVisit].Item1 != null)
                    { // there is a fixed arrival turn for this visit.
                        if (!fixedTurns[nextVisit].Item1.Value &&
                            nextArrival == 1)
                        { // turn is fixed arrival forward and current turn is arrival backward.
                            fixedTurnPenalty += NonFixedTurnPentalty;
                        }
                        if (fixedTurns[nextVisit].Item1.Value &&
                            nextArrival == 0)
                        { // turn is fixed arrival backward and current turn is arrival forward.
                            fixedTurnPenalty += NonFixedTurnPentalty;
                        }
                    }

                    if (fixedTurns?[nextVisit].Item2 != null)
                    { // there is a fixed departure turn for this visit.
                        var nextDeparture =  (nextTurn == 1 || nextTurn == 3) ? 1 : 0;
                        if (!fixedTurns[nextVisit].Item2.Value &&
                            nextDeparture == 1)
                        { // turn is fixed departure forward and current turn is departure backward.
                            fixedTurnPenalty += NonFixedTurnPentalty;
                        }
                        if (fixedTurns[nextVisit].Item2.Value &&
                            nextDeparture == 0)
                        { // turn is fixed departure backward and current turn is departure forward.
                            fixedTurnPenalty += NonFixedTurnPentalty;
                        }
                    }

                    var w = directedWeights[currentVisit * 2 + currentDeparture][nextVisit * 2 + nextArrival];
                    if (w >= float.MaxValue)
                    {
                        continue;
                    }
                    if (nextTurn == 1 || nextTurn == 2)
                    { // a u-turn.
                        w += turnPenalty;
                    }
                    w += fixedTurnPenalty;

                    if (paths[next].Item2 > w)
                    {
                        paths[next] = new Tuple<int, float>(current, w);
                        queue.Push(next, w);
                    }
                }
            }

            if (bestLast == -1)
            { // could not find a decent path, assume a path without turns.
                // a choice was made here to do this because in the next step the failing route should be reported.
                // users will find it easier to fix one bad route instead of figuring out why this part fails.
                return new int[length];
            }

            // build the path with turns only.
            var path = new int[length];
            var h = path.Length - 1;
            path[h] = bestLast;
            var nextHop = paths[bestLast];
            while (nextHop.Item1 >= 4)
            {
                h--;
                path[h] = nextHop.Item1;
                nextHop = paths[nextHop.Item1];
            }
            path[0] = nextHop.Item1;
            
            return path;
        }
    }
}