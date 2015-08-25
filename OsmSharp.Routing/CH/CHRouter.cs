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

using OsmSharp.Collections.Coordinates.Collections;
using OsmSharp.Collections.PriorityQueues;
using OsmSharp.Collections.Tags;
using OsmSharp.Logging;
using OsmSharp.Math.Geo;
using OsmSharp.Math.Primitives;
using OsmSharp.Routing.CH.Preprocessing;
using OsmSharp.Routing.CH.Routing;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Routing;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Vehicles;
using OsmSharp.Units.Distance;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OsmSharp.Routing.CH
{
    /// <summary>
    /// A router for CH.
    /// </summary>
    public class CHRouter : IRoutingAlgorithm<CHEdgeData>
    {
        /// <summary>
        /// Creates a new CH router on the givend data.
        /// </summary>
        public CHRouter()
        {

        }

        /// <summary>
        /// Gets the weight type.
        /// </summary>
        public RouterWeightType WeightType
        {
            get
            {
                return RouterWeightType.Time;
            }
        }

        /// <summary>
        /// Calculates the shortest path from the given vertex to the given vertex given the weights in the graph.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public PathSegment<long> Calculate(IGraphReadOnly<CHEdgeData> graph, uint from, uint to)
        {
            var source = new PathSegmentVisitList();
            source.UpdateVertex(new PathSegment<long>(from));
            var target = new PathSegmentVisitList();
            target.UpdateVertex(new PathSegment<long>(to));

            // do the basic CH calculations.
            var result = this.DoCalculate(graph, source, target, float.MaxValue, int.MaxValue);

            // expand path.
            var expandedResult = this.ExpandBestResult(graph, result);

            // calculate weights along the path.
            if (expandedResult != null)
            { // expand path.
                expandedResult = this.AugmentWithWeights(graph, expandedResult);
            }
            return expandedResult;
        }

        /// <summary>
        /// Calculates the shortest path from the given vertex to the given vertex given the weights in the graph.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="max"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public PathSegment<long> Calculate(IRoutingAlgorithmData<CHEdgeData> graph, IRoutingInterpreter interpreter,
            Vehicle vehicle, PathSegmentVisitList source, PathSegmentVisitList target, double max, Dictionary<string, object> parameters)
        {
            // do the basic CH calculations.
            var result = this.DoCalculate(graph, source, target, max, int.MaxValue);

            // expand path.
            var expandedResult = this.ExpandBestResult(graph, result);

            // calculate weights along the path.
            if (expandedResult != null)
            { // expand path.
                expandedResult = this.AugmentWithWeights(graph, expandedResult);
            }
            return expandedResult;
        }

        /// <summary>
        /// Updates the weights along the path segment.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="expandedResult"></param>
        /// <returns></returns>
        private PathSegment<long> AugmentWithWeights(IGraphReadOnly<CHEdgeData> graph, PathSegment<long> expandedResult)
        {
            CHEdgeData edge;
            var current = expandedResult;
            while (current.From != null)
            { // keep updating weights.
                if (current.From.Weight == 0 &&
                    current.VertexId > 0 && current.From.VertexId > 0)
                { // this edge is in the graph and needs to be re-calculated.
                    if (!this.GetEdge(graph, Convert.ToUInt32(current.From.VertexId), Convert.ToUInt32(current.VertexId), out edge))
                    { // ok, an edge was found.
                        if (!this.GetEdge(graph, Convert.ToUInt32(current.VertexId), Convert.ToUInt32(current.From.VertexId), out edge))
                        {
                            throw new Exception(string.Format("Edge {0}->{1} or reverse not found!", current.From.VertexId, current.VertexId));
                        }
                        edge = (CHEdgeData)edge.Reverse();
                    }
                    current.From.Weight = System.Math.Max(current.Weight - edge.Weight, 0);
                }
                current = current.From;
            }

            // remove segments with length 0.
            current = expandedResult;
            while (current.From != null &&
                current.Weight == current.From.Weight)
            { // check the first one.                    
                if (current.From.VertexId > current.VertexId)
                { // choose the vertex with the lowest id.
                    current = current.From;
                }
                else
                { // choose the vertex with the lowest id.
                    var vertex = current.From.VertexId;
                    current = current.From;
                    current.VertexId = vertex;
                }
            }
            while (current.From != null && current.From.From != null)
            {
                if (current.From.Weight == current.From.From.Weight)
                {
                    if (current.From.VertexId > current.From.From.VertexId)
                    { // choose the vertex with the lowest id.
                        current.From = current.From.From;
                    }
                    else
                    { // choose the vertex with the lowest id.
                        var vertex = current.From.VertexId;
                        current.From = current.From.From;
                        current.From.VertexId = vertex;
                    }
                }
                current = current.From;
            }

            return expandedResult;
        }

        /// <summary>
        /// Calculates all routes between all sources and targets.
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="sources"></param>
        /// <param name="targets"></param>
        /// <param name="maxSearch"></param>
        /// <param name="graph"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public PathSegment<long>[][] CalculateManyToMany(IRoutingAlgorithmData<CHEdgeData> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            PathSegmentVisitList[] sources, PathSegmentVisitList[] targets, double maxSearch, Dictionary<string, object> parameters)
        {
            var results = new PathSegment<long>[sources.Length][];
            for (int sourceIdx = 0; sourceIdx < sources.Length; sourceIdx++)
            {
                results[sourceIdx] = new PathSegment<long>[targets.Length];
                for (int targetIdx = 0; targetIdx < targets.Length; targetIdx++)
                {
                    results[sourceIdx][targetIdx] =
                        this.Calculate(graph, interpreter, vehicle, sources[sourceIdx], targets[targetIdx], maxSearch, parameters);
                }
            }

            return results;
        }


        /// <summary>
        /// Calculates the weight of shortest path from the given vertex to the given vertex given the weights in the graph.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="max"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public double CalculateWeight(IRoutingAlgorithmData<CHEdgeData> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            PathSegmentVisitList source, PathSegmentVisitList target, double max, Dictionary<string, object> parameters)
        {
            // do the basic CH calculations.
            var result = this.DoCalculate(graph, source, target, max, int.MaxValue);

            if (result.Backward != null && result.Forward != null)
            {
                return result.Backward.Weight + result.Forward.Weight;
            }
            return double.MaxValue;
        }

        /// <summary>
        /// Calculate route to the closest.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="targets"></param>
        /// <param name="max"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public PathSegment<long> CalculateToClosest(IRoutingAlgorithmData<CHEdgeData> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            PathSegmentVisitList source, PathSegmentVisitList[] targets, double max, Dictionary<string, object> parameters)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Calculates all weights from one source to multiple targets.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="targets"></param>
        /// <param name="max"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public double[] CalculateOneToManyWeight(IRoutingAlgorithmData<CHEdgeData> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            PathSegmentVisitList source, PathSegmentVisitList[] targets, double max, Dictionary<string, object> parameters, HashSet<int> invalidSet)
        {
            var manyToManyResult = this.CalculateManyToManyWeight(
                graph, interpreter, vehicle, new PathSegmentVisitList[] { source }, targets, max, null, invalidSet);

            return manyToManyResult[0];
        }

        /// <summary>
        /// Calculates all weights from multiple sources to multiple targets.
        /// </summary>
        /// <returns></returns>
        public double[][] CalculateManyToManyWeight(IRoutingAlgorithmData<CHEdgeData> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            PathSegmentVisitList[] sources, PathSegmentVisitList[] targets, double max, Dictionary<string, object> parameters, HashSet<int> invalidSet)
        {
            return this.DoCalculateManyToMany(
                   graph, interpreter, sources, targets, max, int.MaxValue, invalidSet);
        }

        /// <summary>
        /// Returns false.
        /// </summary>
        public bool IsCalculateRangeSupported
        {
            get { return false; }
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="weight"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public HashSet<long> CalculateRange(IRoutingAlgorithmData<CHEdgeData> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            PathSegmentVisitList source, double weight, Dictionary<string, object> parameters)
        {
            throw new NotSupportedException("Check IsCalculateRangeSupported before using this functionality!");
        }

        /// <summary>
        /// Returns true if the search can move beyond the given weight.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="weight"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public bool CheckConnectivity(IRoutingAlgorithmData<CHEdgeData> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            PathSegmentVisitList source, double weight, Dictionary<string, object> parameters)
        {
            return this.DoCheckConnectivity(graph, source, weight, int.MaxValue);
        }

        #region Implementation

        #region Bi-directional Many-to-Many

        ///// <summary>
        ///// Calculates all the weights between all the vertices.
        ///// </summary>
        ///// <param name="froms"></param>
        ///// <param name="tos"></param>
        ///// <returns></returns>
        //public float[][] CalculateManyToManyWeights(uint[] froms, uint[] tos)
        //{
        //    // TODO: implement switching of from/to when to < from.

        //    // keep a list of distances to the given vertices while performance backward search.
        //    Dictionary<uint, Dictionary<uint, float>> buckets = new Dictionary<uint, Dictionary<uint, float>>();
        //    for (int idx = 0; idx < tos.Length; idx++)
        //    {
        //        this.SearchBackwardIntoBucket(buckets, tos[idx]);

        //        // report progress.
        //        OsmSharp.IO.Output.OutputStreamHost.ReportProgress(idx, tos.Length, "Router.CH.CalculateManyToManyWeights",
        //            "Calculating backward...");
        //    }

        //    // conduct a forward search from each source.
        //    float[][] weights = new float[froms.Length][];
        //    for (int idx = 0; idx < froms.Length; idx++)
        //    {
        //        uint from = froms[idx];

        //        // calculate all from's.
        //        Dictionary<uint, float> result =
        //            this.SearchForwardFromBucket(buckets, from, tos);

        //        float[] to_weights = new float[tos.Length];
        //        for (int to_idx = 0; to_idx < tos.Length; to_idx++)
        //        {
        //            to_weights[to_idx] = result[tos[to_idx]];
        //        }

        //        weights[idx] = to_weights;
        //        result.Clear();

        //        // report progress.
        //        OsmSharp.IO.Output.OutputStreamHost.ReportProgress(idx, tos.Length, "Router.CH.CalculateManyToManyWeights",
        //            "Calculating forward...");
        //    }
        //    return weights;
        //}

        /// <summary>
        /// Searches backwards and puts the weigths from the to-vertex into the buckets list.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="buckets"></param>
        /// <param name="toVisitList"></param>
        /// <returns></returns>
        private long SearchBackwardIntoBucket(IRoutingAlgorithmData<CHEdgeData> graph, Dictionary<long, Dictionary<long, double>> buckets,
            PathSegmentVisitList toVisitList)
        {
            long? to = null;
            var settledVertices = new Dictionary<long, PathSegment<long>>();
            IPriorityQueue<PathSegment<long>> queue = new BinaryHeap<PathSegment<long>>();
            foreach (long vertex in toVisitList.GetVertices())
            {
                PathSegment<long> path = toVisitList.GetPathTo(vertex);
                if (!to.HasValue)
                {
                    to = path.First().VertexId;
                }
                queue.Push(path, (float)path.Weight);

                // also add the from paths.
                path = path.From;
                while (path != null)
                { // keep adding paths.
                    Dictionary<long, double> bucket = null;
                    if (buckets.TryGetValue(to.Value, out bucket))
                    { // an existing bucket was found!
                        double existingWeight;
                        if (!bucket.TryGetValue(path.VertexId, out existingWeight) ||
                            existingWeight > path.Weight)
                        { // there already exists a weight 
                            bucket.Add(path.VertexId, path.Weight);
                        }
                    }
                    else
                    { // add new bucket.
                        bucket = new Dictionary<long, double>();
                        bucket.Add(path.VertexId, path.Weight);
                        buckets.Add(to.Value, bucket);
                    }
                    path = path.From; // get the next one.
                }
            }

            // get the current vertex with the smallest weight.
            while (queue.Count > 0) // TODO: work on a stopping condition?
            {
                //PathSegment<long> current = queue.Pop();
                PathSegment<long> current = queue.Pop();
                while (current != null && settledVertices.ContainsKey(current.VertexId))
                {
                    current = queue.Pop();
                }

                if (current != null)
                { // a next vertex was found!
                    // add to the settled vertices.
                    PathSegment<long> previousLinkedRoute;
                    if (settledVertices.TryGetValue(current.VertexId, out previousLinkedRoute))
                    {
                        if (previousLinkedRoute.Weight > current.Weight)
                        {
                            // settle the vertex again if it has a better weight.
                            settledVertices[current.VertexId] = current;
                        }
                    }
                    else
                    {
                        // settled the vertex.
                        settledVertices[current.VertexId] = current;
                    }

                    // add to bucket.
                    Dictionary<long, double> bucket;
                    if (!buckets.TryGetValue(current.VertexId, out bucket))
                    {
                        bucket = new Dictionary<long, double>();
                        buckets.Add(Convert.ToUInt32(current.VertexId), bucket);
                    }
                    bucket[to.Value] = current.Weight;

                    // get neighbours.
                    var neighbours = graph.GetEdges(Convert.ToUInt32(current.VertexId)).ToList();

                    // add the neighbours to the queue.
                    foreach (var neighbour in neighbours.Where(
                        a => a.EdgeData.CanMoveBackward))
                    {
                        if (!settledVertices.ContainsKey(neighbour.Neighbour))
                        {
                            // if not yet settled.
                            var routeToNeighbour = new PathSegment<long>(
                                neighbour.Neighbour, current.Weight + neighbour.EdgeData.Weight, current);
                            queue.Push(routeToNeighbour, (float)routeToNeighbour.Weight);
                        }
                    }
                }
            }

            return to.Value;
        }

        /// <summary>
        /// Searches forward and uses the bucket to calculate smallest weights.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="buckets"></param>
        /// <param name="fromVisitList"></param>
        /// <param name="tos"></param>
        private Dictionary<long, double> SearchForwardFromBucket(IRoutingAlgorithmData<CHEdgeData> graph, Dictionary<long, Dictionary<long, double>> buckets,
            PathSegmentVisitList fromVisitList, long[] tos)
        {
            long? from = null;
            // intialize weights.
            var results = new Dictionary<long, double>();
            //HashSet<long> permanent_results = new HashSet<long>();
            var tentativeResults = new Dictionary<long, double>();

            var settledVertices =
                new Dictionary<long, PathSegment<long>>();
            //CHPriorityQueue queue = new CHPriorityQueue();
            IPriorityQueue<PathSegment<long>> queue = new BinaryHeap<PathSegment<long>>();
            foreach (long vertex in fromVisitList.GetVertices())
            {
                PathSegment<long> path = fromVisitList.GetPathTo(vertex);
                if (!from.HasValue)
                {
                    from = path.First().VertexId;
                }
                queue.Push(path, (float)path.Weight);

                // also add the from paths.
                path = path.From;
                while (path != null)
                { // keep adding paths.
                    // search the bucket.
                    Dictionary<long, double> bucket;
                    if (buckets.TryGetValue(path.VertexId, out bucket))
                    {
                        // there is a bucket!
                        foreach (KeyValuePair<long, double> bucketEntry in bucket)
                        {
                            double foundDistance = bucketEntry.Value + path.Weight;
                            double tentativeDistance;
                            if (tentativeResults.TryGetValue(bucketEntry.Key, out tentativeDistance))
                            {
                                if (foundDistance < tentativeDistance)
                                {
                                    tentativeResults[bucketEntry.Key] = foundDistance;
                                }
                            }
                            else
                            { // there was no result yet!
                                tentativeResults[bucketEntry.Key] = foundDistance;
                            }
                        }
                    }

                    path = path.From; // get the next one.
                }
            }

            // get the current vertex with the smallest weight.
            int k = 0;
            while (queue.Count > 0) // TODO: work on a stopping condition?
            {
                //PathSegment<long> current = queue.Pop();
                PathSegment<long> current = queue.Pop();
                while (current != null && settledVertices.ContainsKey(current.VertexId))
                {
                    current = queue.Pop();
                }

                if (current != null)
                { // a next vertex was found!
                    k++;

                    // stop search if all results found.
                    if (results.Count == tos.Length)
                    {
                        break;
                    }
                    // add to the settled vertices.
                    PathSegment<long> previousLinkedRoute;
                    if (settledVertices.TryGetValue(current.VertexId, out previousLinkedRoute))
                    {
                        if (previousLinkedRoute.Weight > current.Weight)
                        {
                            // settle the vertex again if it has a better weight.
                            settledVertices[current.VertexId] = current;
                        }
                    }
                    else
                    {
                        // settled the vertex.
                        settledVertices[current.VertexId] = current;
                    }

                    // search the bucket.
                    Dictionary<long, double> bucket;
                    if (buckets.TryGetValue(current.VertexId, out bucket))
                    {
                        // there is a bucket!
                        foreach (KeyValuePair<long, double> bucketEntry in bucket)
                        {
                            //if (!permanent_results.Contains(bucket_entry.Key))
                            //{
                            double foundDistance = bucketEntry.Value + current.Weight;
                            double tentativeDistance;
                            if (tentativeResults.TryGetValue(bucketEntry.Key, out tentativeDistance))
                            {
                                if (foundDistance < tentativeDistance)
                                {
                                    tentativeResults[bucketEntry.Key] = foundDistance;
                                }

                                if (tentativeDistance < current.Weight)
                                {
                                    tentativeResults.Remove(bucketEntry.Key);
                                    results[bucketEntry.Key] = tentativeDistance;
                                }
                            }
                            else if (!results.ContainsKey(bucketEntry.Key))
                            { // there was no result yet!
                                tentativeResults[bucketEntry.Key] = foundDistance;
                            }
                            //}
                        }
                    }

                    // get neighbours.
                    var neighbours = graph.GetEdges(Convert.ToUInt32(current.VertexId)).ToList();

                    // add the neighbours to the queue.
                    foreach (var neighbour in neighbours.Where(
                        a => a.EdgeData.CanMoveForward))
                    {
                        if (!settledVertices.ContainsKey(neighbour.Neighbour))
                        {
                            // if not yet settled.
                            var routeToNeighbour = new PathSegment<long>(
                                neighbour.Neighbour, current.Weight + neighbour.EdgeData.Weight, current);
                            queue.Push(routeToNeighbour, (float)routeToNeighbour.Weight);
                        }
                    }
                }
            }

            foreach (long to in tos)
            {
                if (!results.ContainsKey(to) && tentativeResults.ContainsKey(to))
                {
                    results[to] = tentativeResults[to];
                }
            }

            return results;
        }

        #endregion

        #region Bi-directional Point-To-Point

        /// <summary>
        /// Calculates a shortest path between the two given vertices.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="max"></param>
        /// <param name="max_settles"></param>
        /// <returns></returns>
        private CHResult DoCalculate(IGraphReadOnly<CHEdgeData> graph,
            PathSegmentVisitList source, PathSegmentVisitList target, double max, int max_settles)
        {
            // keep settled vertices.
            var settledVertices = new CHQueue();

            // initialize the queues.
            var queueForward = new BinaryHeap<PathSegment<long>>();
            var queueBackward = new BinaryHeap<PathSegment<long>>();

            // add the sources to the forward queue.
            var resolvedSettles = new Dictionary<long, PathSegment<long>>();
            foreach (long sourceVertex in source.GetVertices())
            {
                var path = source.GetPathTo(sourceVertex);
                queueForward.Push(path, (float)path.Weight);
                path = path.From;
                while (path != null)
                { // keep looping.
                    PathSegment<long> existingSource = null;
                    if (!resolvedSettles.TryGetValue(path.VertexId, out existingSource) ||
                        existingSource.Weight > path.Weight)
                    { // the new path is better.
                        resolvedSettles[path.VertexId] = path;
                    }
                    path = path.From;
                }
            }

            // add the sources to the settled vertices.
            foreach (var resolvedSettled in resolvedSettles)
            {
                settledVertices.AddForward(resolvedSettled.Value);
            }

            // add the to(s) vertex to the backward queue.resolved_settles = 
            resolvedSettles = new Dictionary<long, PathSegment<long>>();
            foreach (long targetVertex in target.GetVertices())
            {
                PathSegment<long> path = target.GetPathTo(targetVertex);
                queueBackward.Push(path, (float)path.Weight);
                path = path.From;
                while (path != null)
                { // keep looping.
                    PathSegment<long> existingSource = null;
                    if (!resolvedSettles.TryGetValue(path.VertexId, out existingSource) ||
                        existingSource.Weight > path.Weight)
                    { // the new path is better.
                        resolvedSettles[path.VertexId] = path;
                    }
                    path = path.From;
                }
            }

            // add the sources to the settled vertices.
            foreach (KeyValuePair<long, PathSegment<long>> resolvedSettled
                in resolvedSettles)
            {
                settledVertices.AddBackward(resolvedSettled.Value);
            }

            // keep looping until stopping conditions are met.
            var best = this.CalculateBest(settledVertices);

            // calculate stopping conditions.
            double queueBackwardWeight = queueBackward.PeekWeight();
            double queueForwardWeight = queueForward.PeekWeight();
            while (true)
            { // keep looping until stopping conditions.
                if (queueBackward.Count == 0 && queueForward.Count == 0)
                { // stop the search; both queues are empty.
                    break;
                }
                if (max < queueBackwardWeight && max < queueForwardWeight)
                { // stop the search: the max search weight has been reached.
                    break;
                }
                if (max_settles < (settledVertices.Forward.Count + settledVertices.Backward.Count))
                { // stop the search: the max settles cound has been reached.
                    break;
                }
                if (best.Weight < queueForwardWeight && best.Weight < queueBackwardWeight)
                { // stop the search: it now became impossible to find a shorter route.
                    break;
                }

                // do a forward search.
                if (queueForward.Count > 0)
                {
                    this.SearchForward(graph, settledVertices, queueForward);
                }

                // do a backward search.
                if (queueBackward.Count > 0)
                {
                    this.SearchBackward(graph, settledVertices, queueBackward);
                }

                // calculate the new best if any.
                best = this.CalculateBest(settledVertices);

                // calculate stopping conditions.
                if (queueForward.Count > 0)
                {
                    queueForwardWeight = queueForward.PeekWeight();
                }
                if (queueBackward.Count > 0)
                {
                    queueBackwardWeight = queueBackward.PeekWeight();
                }
            }

            // return forward/backward routes.
            var result = new CHResult();
            if (!best.Found)
            {
                // no route was found!
            }
            else
            {
                // construct the existing route.
                result.Forward = settledVertices.Forward[best.VertexId];
                result.Backward = settledVertices.Backward[best.VertexId];
            }
            return result;
        }

        /// <summary>
        /// Calculates all shortest paths between the given vertices.
        /// </summary>
        /// <returns></returns>
        private double[][] DoCalculateManyToMany(IRoutingAlgorithmData<CHEdgeData> graph, IRoutingInterpreter interpreter,
            PathSegmentVisitList[] sources, PathSegmentVisitList[] targets, double max, int maxSettles, HashSet<int> invalidSet)
        {
            // TODO: implement switching of from/to when to < from.

            // keep a list of distances to the given vertices while doing backward search.
            var buckets = new Dictionary<long, Dictionary<long, double>>();
            var targetIds = new long[sources.Length];
            double latestProgress = 0;
            for (int idx = 0; idx < sources.Length; idx++)
            {
                targetIds[idx] =
                    this.SearchBackwardIntoBucket(graph, buckets, sources[idx]);

                float progress = (float)System.Math.Round((((double)idx / (double)targets.Length) * 100));
                if (progress != latestProgress)
                {
                    OsmSharp.Logging.Log.TraceEvent("CHPreProcessor", TraceEventType.Information,
                        "Calculating backward... {0}%", progress);
                    latestProgress = progress;
                }
            }

            // conduct a forward search from each source.
            var weights = new double[sources.Length][];
            latestProgress = 0;
            for (int idx = 0; idx < sources.Length; idx++)
            {
                // calculate all from's.
                var result = this.SearchForwardFromBucket(graph, buckets, sources[idx], targetIds);

                var toWeights = new double[targetIds.Length];
                for (int toIdx = 0; toIdx < targetIds.Length; toIdx++)
                {
                    if (result.ContainsKey(targetIds[toIdx]))
                    { // the the actual weight...
                        toWeights[toIdx] = result[targetIds[toIdx]];
                    }
                    else
                    { // ... or make sure that there is a way to recognize unset weights.
                        toWeights[toIdx] = double.MaxValue;
                        if (result.Count > targets.Length / 2)
                        { // ... assume to->from only when targets found.
                            invalidSet.Add(toIdx);
                        }
                    }
                }

                weights[idx] = toWeights;
                result.Clear();

                float progress = (float)System.Math.Round((((double)idx / (double)sources.Length) * 100));
                if (progress != latestProgress)
                {
                    OsmSharp.Logging.Log.TraceEvent("CHPreProcessor", TraceEventType.Information,
                        "Calculating forward... {0}%", progress);
                    latestProgress = progress;
                }
            }
            return weights;
        }

        /// <summary>
        /// Calculates the weight from from to to.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public double CalculateWeight(IRoutingAlgorithmData<CHEdgeData> graph, uint from, uint to, uint exception)
        {
            return this.CalculateWeight(graph, from, to, exception, double.MaxValue);
        }

        /// <summary>
        /// Calculates the weight from from to to.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="exception"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public double CalculateWeight(IRoutingAlgorithmData<CHEdgeData> graph, uint from, uint to, uint exception, double max)
        {
            // calculate the result.
            var result = this.CalculateInternal(graph, from, to, exception, max, int.MaxValue);

            // construct the route.
            if (result.Forward != null && result.Backward != null)
            {
                return result.Forward.Weight + result.Backward.Weight;
            }
            return double.MaxValue;
        }

        /// <summary>
        /// Calculates the weight from from to to.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="exception"></param>
        /// <param name="max"></param>
        /// <param name="maxSettles"></param>
        /// <returns></returns>
        public double CalculateWeight(IRoutingAlgorithmData<CHEdgeData> graph, uint from, uint to, uint exception, double max, int maxSettles)
        {
            // calculate the result.
            var result = this.CalculateInternal(graph, from, to, exception, max, maxSettles);

            // construct the route.
            if (result.Forward != null && result.Backward != null)
            {
                return result.Forward.Weight + result.Backward.Weight;
            }
            return double.MaxValue;
        }

        /// <summary>
        /// Checks connectivity of a vertex.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="source"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public bool CheckConnectivity(IRoutingAlgorithmData<CHEdgeData> graph, PathSegmentVisitList source, double max)
        {
            return this.DoCheckConnectivity(graph, source, max, int.MaxValue);
        }

        /// <summary>
        /// Checks connectivity of a vertex.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="source"></param>
        /// <param name="maxSettles"></param>
        /// <returns></returns>
        public bool CheckConnectivity(IRoutingAlgorithmData<CHEdgeData> graph, PathSegmentVisitList source, int maxSettles)
        {
            return this.DoCheckConnectivity(graph, source, double.MaxValue, maxSettles);
        }

        /// <summary>
        /// Calculates a shortest path between the two given vertices.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="exception"></param>
        /// <param name="max"></param>
        /// <param name="maxSettles"></param>
        /// <returns></returns>
        private CHResult CalculateInternal(IGraphReadOnly<CHEdgeData> graph, uint from, uint to, uint exception, double max, int maxSettles)
        {
            // keep settled vertices.
            var settledVertices = new CHQueue();

            // initialize the queues.
            var queueForward = new BinaryHeap<PathSegment<long>>();
            var queueBackward = new BinaryHeap<PathSegment<long>>();

            // add the from vertex to the forward queue.
            queueForward.Push(new PathSegment<long>(from), 0);

            // add the from vertex to the backward queue.
            queueBackward.Push(new PathSegment<long>(to), 0);

            // keep looping until stopping conditions are met.
            var best = this.CalculateBest(settledVertices);

            // calculate stopping conditions.
            double queueBackwardWeight = queueBackward.PeekWeight();
            double queueForwardWeight = queueForward.PeekWeight();
            while (true)
            { // keep looping until stopping conditions.
                if (queueBackward.Count == 0 && queueForward.Count == 0)
                { // stop the search; both queues are empty.
                    break;
                }
                if (max < queueBackwardWeight && max < queueForwardWeight)
                { // stop the search: the max search weight has been reached.
                    break;
                }
                if (maxSettles < (settledVertices.Forward.Count + settledVertices.Backward.Count))
                { // stop the search: the max settles cound has been reached.
                    break;
                }
                if (best.Weight < queueForwardWeight && best.Weight < queueBackwardWeight)
                { // stop the search: it now became impossible to find a shorter route.
                    break;
                }

                // do a forward search.
                if (queueForward.Count > 0)
                {
                    this.SearchForward(graph, settledVertices, queueForward);
                }

                // do a backward search.
                if (queueBackward.Count > 0)
                {
                    this.SearchBackward(graph, settledVertices, queueBackward);
                }

                // calculate the new best if any.
                best = this.CalculateBest(settledVertices);

                // calculate stopping conditions.
                if (queueForward.Count > 0)
                {
                    queueForwardWeight = queueForward.PeekWeight();
                }
                if (queueBackward.Count > 0)
                {
                    queueBackwardWeight = queueBackward.PeekWeight();
                }
            }

            // return forward/backward routes.
            var result = new CHResult();
            if (!best.Found)
            {
                // no route was found!
            }
            else
            {
                // construct the existing route.
                result.Forward = settledVertices.Forward[best.VertexId];
                result.Backward = settledVertices.Backward[best.VertexId];
            }
            return result;
        }

        /// <summary>
        /// Checks if the given vertex is connected to others.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="source"></param>
        /// <param name="max"></param>
        /// <param name="maxSettles"></param>
        /// <returns></returns>
        private bool DoCheckConnectivity(IGraphReadOnly<CHEdgeData> graph, PathSegmentVisitList source, double max, int maxSettles)
        {
            // keep settled vertices.
            var settledVertices = new CHQueue();

            // initialize the queues.
            var queueForward = new BinaryHeap<PathSegment<long>>();
            var queueBackward = new BinaryHeap<PathSegment<long>>();

            // add the sources to the forward queue.
            foreach (long sourceVertex in source.GetVertices())
            {
                var path = source.GetPathTo(sourceVertex);
                queueForward.Push(path, (float)path.Weight);
            }

            // add the to(s) vertex to the backward queue.
            foreach (long targetVertex in source.GetVertices())
            {
                var path = source.GetPathTo(targetVertex);
                queueBackward.Push(path, (float)path.Weight);
            }

            // calculate stopping conditions.
            double queueBackwardWeight = queueBackward.PeekWeight();
            double queueForwardWeight = queueForward.PeekWeight();
            while (true) // when the queue is empty the connectivity test fails!
            { // keep looping until stopping conditions.
                if (queueBackward.Count == 0 && queueForward.Count == 0)
                { // stop the search; both queues are empty.
                    break;
                }
                if (max < queueBackwardWeight && max < queueForwardWeight)
                { // stop the search: the max search weight has been reached.
                    break;
                }
                if (maxSettles < (settledVertices.Forward.Count + settledVertices.Backward.Count))
                { // stop the search: the max settles cound has been reached.
                    break;
                }

                // do a forward search.
                if (queueForward.Count > 0)
                {
                    this.SearchForward(graph, settledVertices, queueForward);
                }

                // do a backward search.
                if (queueBackward.Count > 0)
                {
                    this.SearchBackward(graph, settledVertices, queueBackward);
                }

                // calculate stopping conditions.
                if (queueForward.Count > 0)
                {
                    queueForwardWeight = queueForward.PeekWeight();
                }
                if (queueBackward.Count > 0)
                {
                    queueBackwardWeight = queueBackward.PeekWeight();
                }
            }
            return (max <= queueBackwardWeight && max <= queueForwardWeight) || // the search has continued until both weights exceed the maximum.
                maxSettles <= (settledVertices.Forward.Count + settledVertices.Backward.Count); // or until the max settled vertices have been reached.
        }

        /// <summary>
        /// Test stopping conditions and output the best tentative route.
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        private CHBest CalculateBest(CHQueue queue)
        {
            var best = new CHBest();
            best.VertexId = 0;
            best.Weight = double.MaxValue;

            // loop over all intersections.
            foreach (KeyValuePair<long, double> vertex in queue.Intersection)
            {
                double weight = vertex.Value;
                if (weight < best.Weight)
                {
                    best = new CHBest();
                    best.VertexId = vertex.Key;
                    best.Found = true;
                    best.Weight = weight;
                }
            }
            return best;
        }

        /// <summary>
        /// Holds the result.
        /// </summary>
        private struct CHBest
        {
            /// <summary>
            /// The vertex in the 'middle' of the best route yet.
            /// </summary>
            public long VertexId { get; set; }

            /// <summary>
            /// The weight of the best route yet.
            /// </summary>
            public double Weight { get; set; }

            /// <summary>
            /// The result that was found.
            /// </summary>
            public bool Found { get; set; }
        }

        /// <summary>
        /// Do one forward search step.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="settledQueue"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        private void SearchForward(IGraphReadOnly<CHEdgeData> graph, CHQueue settledQueue, IPriorityQueue<PathSegment<long>> queue)
        {
            // get the current vertex with the smallest weight.
            var current = queue.Pop();
            while (current != null && settledQueue.Forward.ContainsKey(
                current.VertexId))
            { // keep trying.
                current = queue.Pop();
            }

            if (current != null)
            { // there is a next vertex found.
                // get the edge enumerator.
                var edgeEnumerator = graph.GetEdgeEnumerator();

                // add to the settled vertices.
                PathSegment<long> previousLinkedRoute;
                if (settledQueue.Forward.TryGetValue(current.VertexId, out previousLinkedRoute))
                {
                    if (previousLinkedRoute.Weight > current.Weight)
                    {
                        // settle the vertex again if it has a better weight.
                        settledQueue.AddForward(current);
                    }
                }
                else
                {
                    // settled the vertex.
                    settledQueue.AddForward(current);
                }

                // get neighbours.
                //var neighbours = graph.GetEdges(Convert.ToUInt32(current.VertexId));
                edgeEnumerator.MoveTo(Convert.ToUInt32(current.VertexId));

                // add the neighbours to the queue.
                while (edgeEnumerator.MoveNext())
                {
                    var neighbourEdgeData = edgeEnumerator.EdgeData;
                    if (neighbourEdgeData.CanMoveForward)
                    { // the edge is forward, and is to higher or was not contracted at all.
                        var neighbourNeighbour = edgeEnumerator.Neighbour;
                        if (!settledQueue.Forward.ContainsKey(neighbourNeighbour))
                        { // if not yet settled.
                            var routeToNeighbour = new PathSegment<long>(
                                neighbourNeighbour, current.Weight + neighbourEdgeData.Weight, current);
                            queue.Push(routeToNeighbour, (float)routeToNeighbour.Weight);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Do one backward search step.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="settledQueue"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        private void SearchBackward(IGraphReadOnly<CHEdgeData> graph, CHQueue settledQueue, IPriorityQueue<PathSegment<long>> queue)
        {
            // get the current vertex with the smallest weight.
            var current = queue.Pop();
            while (current != null && settledQueue.Backward.ContainsKey(
                current.VertexId))
            { // keep trying.
                current = queue.Pop();
            }

            if (current != null)
            {
                // get the edge enumerator.
                var edgeEnumerator = graph.GetEdgeEnumerator();

                // add to the settled vertices.
                PathSegment<long> previousLinkedRoute;
                if (settledQueue.Backward.TryGetValue(current.VertexId, out previousLinkedRoute))
                {
                    if (previousLinkedRoute.Weight > current.Weight)
                    {
                        // settle the vertex again if it has a better weight.
                        settledQueue.AddBackward(current);
                    }
                }
                else
                {
                    // settled the vertex.
                    settledQueue.AddBackward(current);
                }

                // get neighbours.
                //var neighbours = graph.GetEdges(Convert.ToUInt32(current.VertexId));
                edgeEnumerator.MoveTo(Convert.ToUInt32(current.VertexId));

                // add the neighbours to the queue.
                while (edgeEnumerator.MoveNext())
                // foreach (var neighbour in neighbours)
                {
                    var neighbourEdgeData = edgeEnumerator.EdgeData;
                    if (neighbourEdgeData.CanMoveBackward)
                    { // the edge is backward, and is to higher or was not contracted at all.
                        var neighbourNeighbour = edgeEnumerator.Neighbour;
                        if (!settledQueue.Backward.ContainsKey(neighbourNeighbour))
                        { // if not yet settled.
                            var routeToNeighbour = new PathSegment<long>(
                                neighbourNeighbour, current.Weight + neighbourEdgeData.Weight, current);
                            queue.Push(routeToNeighbour, (float)routeToNeighbour.Weight);
                        }
                    }
                }
            }
        }

        #endregion

        #region Path Expansion

        /// <summary>
        /// Expands a ch results into an expanded path segment.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private PathSegment<long> ExpandBestResult(IGraphReadOnly<CHEdgeData> graph, CHResult result)
        {
            // construct the route.
            var forward = result.Forward;
            var backward = result.Backward;

            // check null.
            if (forward == null && backward == null)
            { // both null, should be no other possibilities.
                return null;
            }

            // invert backward.
            var invertedBackward = backward.Reverse();

            // concatenate.
            var route = invertedBackward.ConcatenateAfter(forward);

            // expand the CH path to a regular path.
            route = this.ExpandPath(graph, route);
            return route;
        }

        /// <summary>
        /// Converts the CH paths to complete paths in the orginal network.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private PathSegment<long> ExpandPath(IGraphReadOnly<CHEdgeData> graph, PathSegment<long> path)
        {
            // construct the full CH path.
            PathSegment<long> current = path;
            PathSegment<long> expandedPath = null;

            if (current != null && current.From == null)
            { // path contains just a single point.
                expandedPath = current;
            }
            else
            { // path containts at least two points or none at all.
                while (current != null && current.From != null)
                { // convert edges on-by-one.
                    var localPath = new PathSegment<long>(current.VertexId, current.Weight - current.From.Weight,
                        new PathSegment<long>(current.From.VertexId));

                    // expand edge recursively.
                    var expandedEdge = this.ExpandEdge(graph, localPath);
                    if (expandedPath != null)
                    { // there already is an expanded edge. add the new one.   
                        var oldExpandedEdge = expandedEdge.Clone();
                        var oldExpandedPath = expandedPath.Clone();

                        // update weights.
                        var first = expandedPath.First();
                        var last = expandedPath;
                        if (expandedPath.Weight > 0)
                        {
                            expandedPath.Weight = expandedEdge.Weight + expandedPath.Weight;
                        }
                        while (expandedPath.From != null)
                        {
                            if (expandedPath.From.Weight > 0)
                            {
                                expandedPath.From.Weight = expandedEdge.Weight + expandedPath.From.Weight;
                            }
                            expandedPath = expandedPath.From;
                        }

                        // concatenate.
                        first.From = expandedEdge.From;
                        first.Weight = expandedEdge.Weight;
                        expandedPath = last;
                    }
                    else
                    { // this is the first edge that was expanded.
                        expandedPath = expandedEdge;
                    }

                    current = current.From;
                }
            }
            return expandedPath;
        }

        /// <summary>
        /// Converts the given edge and expands it if needed.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private PathSegment<long> ExpandEdge(IGraphReadOnly<CHEdgeData> graph, PathSegment<long> path)
        {
            if (path.VertexId < 0 || path.From.VertexId < 0)
            { // these edges are not part of the regular network!
                return path;
            }

            // the from/to vertex.
            var fromVertex = (uint)path.From.VertexId;
            var toVertex = (uint)path.VertexId;

            // get the edge.
            CHEdgeData data;
            if (!this.GetEdge(graph, (uint)path.From.VertexId, (uint)path.VertexId, out data))
            { // there is an edge.
                if (!this.GetEdge(graph, (uint)path.VertexId, (uint)path.From.VertexId, out data))
                {
                    throw new Exception(string.Format("Edge {0} not found!", path.ToInvariantString()));
                }
                data = (CHEdgeData)data.Reverse();
            }
            var expandedEdge = path;
            if (data.IsContracted)
            { // there is nothing to expand.
                var contractedVertex = data.ContractedId;
                // arc is a shortcut.
                var firstPath = new PathSegment<long>(toVertex, path.Weight, new PathSegment<long>(contractedVertex));
                var firstPathExpanded = this.ExpandEdge(graph, firstPath);
                var secondPath = new PathSegment<long>(contractedVertex, 0, new PathSegment<long>(fromVertex));
                var secondPathExpanded = this.ExpandEdge(graph, secondPath);

                // link the two paths.
                firstPathExpanded = firstPathExpanded.ConcatenateAfter(secondPathExpanded);

                return firstPathExpanded;
            }
            return expandedEdge;
        }

        /// <summary>
        /// Returns an edge with a forward weight.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool GetEdge(IGraphReadOnly<CHEdgeData> graph, uint from, uint to, out CHEdgeData data)
        {
            var lowestWeight = float.MaxValue;
            data = new CHEdgeData();
            var edges =  graph.GetEdges(from, to);
            while(edges.MoveNext())
            {
                var edgeData = edges.EdgeData;
                if (edgeData.CanMoveForward &&
                    edgeData.Weight < lowestWeight)
                {
                    data = edgeData;
                    lowestWeight = edgeData.Weight;
                }
            }
            edges = graph.GetEdges(to, from);
            while (edges.MoveNext())
            {
                var edgeData = edges.EdgeData;
                if (edgeData.CanMoveBackward &&
                    edgeData.Weight < lowestWeight)
                {
                    data = edgeData;
                    lowestWeight = edgeData.Weight;
                }
            }
            return lowestWeight < float.MaxValue;
        }

        /// <summary>
        /// Returns an edge with a shape.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool GetEdgeShape(IGraphReadOnly<CHEdgeData> graph, uint from, uint to, out ICoordinateCollection data)
        {
            var lowestWeight = float.MaxValue;
            data = null;
            var edges =  graph.GetEdges(from, to);
            while (edges.MoveNext())
            {
                var edgeData = edges.EdgeData;
                if (edgeData.CanMoveForward &&
                    edgeData.RepresentsNeighbourRelations &&
                    edgeData.Weight < lowestWeight)
                {
                    data = edges.Intermediates;
                    lowestWeight = edgeData.Weight;
                }
            }
            edges = graph.GetEdges(to, from);
            while (edges.MoveNext())
            {
                var edgeData = edges.EdgeData;
                if (edgeData.CanMoveBackward &&
                    edgeData.RepresentsNeighbourRelations &&
                    edgeData.Weight < lowestWeight)
                {
                    if (edges.Intermediates != null)
                    {
                        data = edges.Intermediates.Reverse();
                    }
                    else
                    {
                        data = null;
                    }
                    lowestWeight = edgeData.Weight;
                }
            }
            return lowestWeight < float.MaxValue;
        }

        #endregion

        #region Notifications

        /// <summary>
        /// The delegate for arc notifications.
        /// </summary>
        /// <param name="route"></param>
        public delegate void NotifyPathSegmentDelegate(PathSegment<long> route);

        /// <summary>
        /// The event.
        /// </summary>
        public event NotifyPathSegmentDelegate NotifyPathSegmentEvent;

        /// <summary>
        /// Notifies the arc.
        /// </summary>
        /// <param name="route"></param>
        private void NotifyPathSegment(PathSegment<long> route)
        {
            if (this.NotifyPathSegmentEvent != null)
            {
                this.NotifyPathSegmentEvent(route);
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// Represents the result of a calculation.
        /// </summary>
        private struct CHResult
        {
            /// <summary>
            /// The shortest forward path.
            /// </summary>
            public PathSegment<long> Forward { get; set; }

            /// <summary>
            /// The shortest backward path.
            /// </summary>
            public PathSegment<long> Backward { get; set; }
        }

        #region Search Closest

        /// <summary>
        /// Searches the data for a point on an edge closest to the given coordinate.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="coordinate"></param>
        /// <param name="delta"></param>
        /// <param name="matcher"></param>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="pointTags"></param>
        /// <param name="parameters"></param>
        public SearchClosestResult<CHEdgeData> SearchClosest(IRoutingAlgorithmData<CHEdgeData> graph, IRoutingInterpreter interpreter,
            Vehicle vehicle, GeoCoordinate coordinate, float delta, IEdgeMatcher matcher, TagsCollectionBase pointTags, Dictionary<string, object> parameters)
        {
            return this.SearchClosest(graph, interpreter, vehicle, coordinate, delta, matcher, pointTags, false, null);
        }

        /// <summary>
        /// Searches the data for a point on an edge closest to the given coordinate.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="coordinate"></param>
        /// <param name="delta"></param>
        /// <param name="matcher"></param>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="pointTags"></param>
        /// <param name="verticesOnly"></param>
        /// <param name="parameters"></param>
        public SearchClosestResult<CHEdgeData> SearchClosest(IRoutingAlgorithmData<CHEdgeData> graph, IRoutingInterpreter interpreter,
            Vehicle vehicle, GeoCoordinate coordinate, float delta, IEdgeMatcher matcher, TagsCollectionBase pointTags, bool verticesOnly, Dictionary<string, object> parameters)
        {
            //// first try a very small area.
            //var result = this.DoSearchClosest(graph, interpreter,
            //    vehicle, coordinate, delta / 10, matcher, pointTags, verticesOnly);
            //if (result.Distance < double.MaxValue)
            //{ // success!
            //    return result;
            //}
            return this.DoSearchClosest(graph, interpreter, vehicle, coordinate, delta, matcher, pointTags, verticesOnly);
        }

        /// <summary>
        /// Searches the data for a point on an edge closest to the given coordinate.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="coordinate"></param>
        /// <param name="delta"></param>
        /// <param name="matcher"></param>
        /// <param name="pointTags"></param>
        /// <param name="verticesOnly"></param>
        /// <returns></returns>
        private SearchClosestResult<CHEdgeData> DoSearchClosest(IRoutingAlgorithmData<CHEdgeData> graph, IRoutingInterpreter interpreter,
            Vehicle vehicle, GeoCoordinate coordinate, float delta, IEdgeMatcher matcher, TagsCollectionBase pointTags, bool verticesOnly)
        {
            Meter distanceEpsilon = .1; // 10cm is the tolerance to distinguish points.

            var closestWithMatch = new SearchClosestResult<CHEdgeData>(double.MaxValue, 0);
            var closestWithoutMatch = new SearchClosestResult<CHEdgeData>(double.MaxValue, 0);

            //GeoCoordinateBox closestWithMatchBox = null;
            GeoCoordinateBox closestWithoutMatchBox = null;

            var emtpyCoordinates = new ICoordinate[0];

            double searchBoxSize = delta;
            // create the search box.
            var searchBox = new GeoCoordinateBox(new GeoCoordinate(
                coordinate.Latitude - searchBoxSize, coordinate.Longitude - searchBoxSize),
                                                               new GeoCoordinate(
                coordinate.Latitude + searchBoxSize, coordinate.Longitude + searchBoxSize));

            // get the arcs from the data source.
            var edges =  graph.GetEdges(searchBox);

            if (!verticesOnly)
            { // find both closest arcs and vertices.
                // loop over all.
                while(edges.MoveNext())
                {
                    if (!edges.EdgeData.RepresentsNeighbourRelations)
                    { // skip this edge, does not represent neighbours.
                        continue;
                    }
                    //if (!graph.TagsIndex.Contains(edges.EdgeData.Tags))
                    //{ // skip this edge, no valid tags found.
                    //    continue;
                    //}
                    TagsCollectionBase arcTags = null;
                    if (matcher != null)
                    {
                        arcTags = graph.TagsIndex.Get(edges.EdgeData.Tags);
                    }

                    // test the two points.
                    float fromLatitude, fromLongitude;
                    float toLatitude, toLongitude;
                    double distance;
                    if (graph.GetVertex(edges.Vertex1, out fromLatitude, out fromLongitude) &&
                        graph.GetVertex(edges.Vertex2, out toLatitude, out toLongitude))
                    { // return the vertex.
                        var fromCoordinate = new GeoCoordinate(fromLatitude, fromLongitude);
                        if (closestWithoutMatchBox == null ||
                            closestWithoutMatchBox.Contains(fromCoordinate))
                        { // the from coordinate is potentially closer.
                            distance = coordinate.DistanceReal(fromCoordinate).Value;

                            if (distance < distanceEpsilon.Value)
                            { // the distance is smaller than the tolerance value.
                                var diff = coordinate - fromCoordinate;
                                closestWithoutMatchBox = new GeoCoordinateBox(new GeoCoordinate(coordinate + diff),
                                    new GeoCoordinate(coordinate - diff));
                                closestWithoutMatch = new SearchClosestResult<CHEdgeData>(
                                    distance, edges.Vertex1);
                                if (matcher == null ||
                                    (pointTags == null || pointTags.Count == 0) ||
                                    matcher.MatchWithEdge(vehicle, pointTags, arcTags))
                                {
                                    closestWithMatch = new SearchClosestResult<CHEdgeData>(
                                        distance, edges.Vertex1);
                                    break;
                                }
                            }
                            if (distance < closestWithoutMatch.Distance)
                            { // the distance is smaller for the without match.
                                var diff = coordinate - fromCoordinate;
                                closestWithoutMatchBox = new GeoCoordinateBox(new GeoCoordinate(coordinate + diff),
                                    new GeoCoordinate(coordinate - diff));
                                closestWithoutMatch = new SearchClosestResult<CHEdgeData>(
                                    distance, edges.Vertex1);
                            }
                            if (distance < closestWithMatch.Distance)
                            { // the distance is smaller for the with match.
                                if (matcher == null ||
                                    (pointTags == null || pointTags.Count == 0) ||
                                    matcher.MatchWithEdge(vehicle, pointTags, graph.TagsIndex.Get(edges.EdgeData.Tags)))
                                {
                                    closestWithMatch = new SearchClosestResult<CHEdgeData>(
                                        distance, edges.Vertex1);
                                }
                            }
                        }
                        var toCoordinate = new GeoCoordinate(toLatitude, toLongitude);
                        if (closestWithoutMatchBox == null ||
                            closestWithoutMatchBox.Contains(toCoordinate))
                        { // the to coordinate is potentially closer.
                            distance = coordinate.DistanceReal(toCoordinate).Value;

                            if (distance < distanceEpsilon.Value)
                            { // the distance is smaller than the tolerance value.
                                var diff = coordinate - toCoordinate;
                                closestWithoutMatchBox = new GeoCoordinateBox(new GeoCoordinate(coordinate + diff),
                                    new GeoCoordinate(coordinate - diff));
                                closestWithoutMatch = new SearchClosestResult<CHEdgeData>(
                                    distance, edges.Vertex2);
                                if (matcher == null ||
                                    (pointTags == null || pointTags.Count == 0) ||
                                    matcher.MatchWithEdge(vehicle, pointTags, arcTags))
                                {
                                    closestWithMatch = new SearchClosestResult<CHEdgeData>(
                                        distance, edges.Vertex2);
                                    break;
                                }
                            }

                            if (distance < closestWithoutMatch.Distance)
                            { // the distance is smaller for the without match.
                                var diff = coordinate - toCoordinate;
                                closestWithoutMatchBox = new GeoCoordinateBox(new GeoCoordinate(coordinate + diff),
                                    new GeoCoordinate(coordinate - diff));
                                closestWithoutMatch = new SearchClosestResult<CHEdgeData>(
                                    distance, edges.Vertex2);
                            }
                            if (distance < closestWithMatch.Distance)
                            { // the distance is smaller for the with match.
                                if (matcher == null ||
                                    (pointTags == null || pointTags.Count == 0) ||
                                    matcher.MatchWithEdge(vehicle, pointTags, arcTags))
                                {
                                    closestWithMatch = new SearchClosestResult<CHEdgeData>(
                                        distance, edges.Vertex2);
                                }
                            }
                        }

                        // search along the line.
                        var previous = fromCoordinate;
                        var intermediatesBox = new GeoCoordinateBox(fromCoordinate, toCoordinate);
                        if (!edges.EdgeData.ShapeInBox ||
                            intermediatesBox.Overlaps(closestWithoutMatchBox))
                        { // only test when overlap or shape is larger.
                            var intermediates = edges.Intermediates;
                            var coordinatesArray = emtpyCoordinates;
                            if (intermediates != null)
                            { // calculate distance along all coordinates.
                                coordinatesArray = intermediates.ToArray();

                                foreach (var intermediateCoordinate in coordinatesArray)
                                {
                                    intermediatesBox.ExpandWith(
                                        new GeoCoordinate(intermediateCoordinate.Latitude, intermediateCoordinate.Longitude));
                                }
                            }

                            if (closestWithoutMatchBox == null ||
                                closestWithoutMatchBox.Overlaps(intermediatesBox) ||
                                intermediatesBox.Overlaps(closestWithoutMatchBox))
                            {
                                // loop over all edges that are represented by this arc (counting intermediate coordinates).
                                GeoCoordinateLine line;
                                var distanceToSegment = 0.0;
                                var distanceTotal = 0.0;
                                if (intermediates != null)
                                {
                                    for (int idx = 0; idx < coordinatesArray.Length; idx++)
                                    {
                                        var current = new GeoCoordinate(
                                            coordinatesArray[idx].Latitude, coordinatesArray[idx].Longitude);
                                        line = new GeoCoordinateLine(previous, current, true, true);
                                        if (closestWithoutMatchBox == null ||
                                            closestWithoutMatchBox.IntersectsPotentially(previous, current))
                                        { // potentially intersects.
                                            distance = line.DistanceReal(coordinate).Value;

                                            if (distance < closestWithoutMatch.Distance)
                                            { // the distance is smaller.
                                                var projectedPoint = line.ProjectOn(coordinate);

                                                // calculate the position.
                                                if (projectedPoint != null)
                                                { // calculate the distance
                                                    if (distanceTotal == 0)
                                                    { // calculate total distance.
                                                        var pCoordinate = fromCoordinate;
                                                        for (int cIdx = 0; cIdx < coordinatesArray.Length; cIdx++)
                                                        {
                                                            var cCoordinate = new GeoCoordinate(coordinatesArray[cIdx].Latitude, coordinatesArray[cIdx].Longitude);
                                                            distanceTotal = distanceTotal + cCoordinate.DistanceReal(pCoordinate).Value;
                                                            pCoordinate = cCoordinate;
                                                        }
                                                        distanceTotal = distanceTotal + toCoordinate.DistanceReal(pCoordinate).Value;
                                                    }

                                                    var distancePoint = previous.DistanceReal(new GeoCoordinate(projectedPoint)).Value + distanceToSegment;
                                                    var position = distancePoint / distanceTotal;

                                                    var diff = coordinate - projectedPoint;
                                                    closestWithoutMatchBox = new GeoCoordinateBox(new GeoCoordinate(coordinate + diff),
                                                        new GeoCoordinate(coordinate - diff));
                                                    closestWithoutMatch = new SearchClosestResult<CHEdgeData>(
                                                        distance, edges.Vertex1, edges.Vertex2, position, edges.EdgeData, coordinatesArray);
                                                }
                                            }
                                            if (distance < closestWithMatch.Distance)
                                            {
                                                var projectedPoint = line.ProjectOn(coordinate);

                                                // calculate the position.
                                                if (projectedPoint != null)
                                                { // calculate the distance
                                                    if (distanceTotal == 0)
                                                    { // calculate total distance.
                                                        var pCoordinate = fromCoordinate;
                                                        for (int cIdx = 0; cIdx < coordinatesArray.Length; cIdx++)
                                                        {
                                                            var cCoordinate = new GeoCoordinate(coordinatesArray[cIdx].Latitude, coordinatesArray[cIdx].Longitude);
                                                            distanceTotal = distanceTotal + cCoordinate.DistanceReal(pCoordinate).Value;
                                                            pCoordinate = cCoordinate;
                                                        }
                                                        distanceTotal = distanceTotal + toCoordinate.DistanceReal(pCoordinate).Value;
                                                    }

                                                    double distancePoint = previous.DistanceReal(new GeoCoordinate(projectedPoint)).Value + distanceToSegment;
                                                    double position = distancePoint / distanceTotal;

                                                    if (matcher == null ||
                                                        (pointTags == null || pointTags.Count == 0) ||
                                                        matcher.MatchWithEdge(vehicle, pointTags, arcTags))
                                                    {

                                                        closestWithMatch = new SearchClosestResult<CHEdgeData>(
                                                            distance, edges.Vertex1, edges.Vertex2, position, edges.EdgeData, coordinatesArray);
                                                    }
                                                }
                                            }
                                        }

                                        // add current segment distance to distanceToSegment for the next segment.
                                        distanceToSegment = distanceToSegment + line.LengthReal.Value;

                                        // set previous.
                                        previous = current;
                                    }
                                }

                                // check the last segment.
                                line = new GeoCoordinateLine(previous, toCoordinate, true, true);
                                if (closestWithoutMatchBox == null ||
                                    closestWithoutMatchBox.IntersectsPotentially(previous, toCoordinate))
                                { // potentially intersects.
                                    distance = line.DistanceReal(coordinate).Value;

                                    if (distance < closestWithoutMatch.Distance)
                                    { // the distance is smaller.
                                        var projectedPoint = line.ProjectOn(coordinate);

                                        // calculate the position.
                                        if (projectedPoint != null)
                                        { // calculate the distance
                                            if (distanceTotal == 0)
                                            { // calculate total distance.
                                                var pCoordinate = fromCoordinate;
                                                for (int cIdx = 0; cIdx < coordinatesArray.Length; cIdx++)
                                                {
                                                    var cCoordinate = new GeoCoordinate(coordinatesArray[cIdx].Latitude, coordinatesArray[cIdx].Longitude);
                                                    distanceTotal = distanceTotal + cCoordinate.DistanceReal(pCoordinate).Value;
                                                    pCoordinate = cCoordinate;
                                                }
                                                distanceTotal = distanceTotal + toCoordinate.DistanceReal(pCoordinate).Value;
                                            }

                                            var distancePoint = previous.DistanceReal(new GeoCoordinate(projectedPoint)).Value + distanceToSegment;
                                            var position = distancePoint / distanceTotal;

                                            var diff = coordinate - projectedPoint;
                                            closestWithoutMatchBox = new GeoCoordinateBox(new GeoCoordinate(coordinate + diff),
                                                new GeoCoordinate(coordinate - diff));
                                            closestWithoutMatch = new SearchClosestResult<CHEdgeData>(
                                                distance, edges.Vertex1, edges.Vertex2, position, edges.EdgeData, coordinatesArray);
                                        }
                                    }

                                    if (distance < closestWithMatch.Distance)
                                    {
                                        var projectedPoint = line.ProjectOn(coordinate);

                                        // calculate the position.
                                        if (projectedPoint != null)
                                        { // calculate the distance
                                            if (distanceTotal == 0)
                                            { // calculate total distance.
                                                var pCoordinate = fromCoordinate;
                                                for (int cIdx = 0; cIdx < coordinatesArray.Length; cIdx++)
                                                {
                                                    var cCoordinate = new GeoCoordinate(coordinatesArray[cIdx].Latitude, coordinatesArray[cIdx].Longitude);
                                                    distanceTotal = distanceTotal + cCoordinate.DistanceReal(pCoordinate).Value;
                                                    pCoordinate = cCoordinate;
                                                }
                                                distanceTotal = distanceTotal + toCoordinate.DistanceReal(pCoordinate).Value;
                                            }

                                            var distancePoint = previous.DistanceReal(new GeoCoordinate(projectedPoint)).Value + distanceToSegment;
                                            var position = distancePoint / distanceTotal;

                                            if (matcher == null ||
                                                (pointTags == null || pointTags.Count == 0) ||
                                                matcher.MatchWithEdge(vehicle, pointTags, arcTags))
                                            {
                                                closestWithMatch = new SearchClosestResult<CHEdgeData>(
                                                    distance, edges.Vertex1, edges.Vertex2, position, edges.EdgeData, coordinatesArray);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            { // only find closest vertices.
                // loop over all.
                while(edges.MoveNext())
                {
                    if (!edges.EdgeData.IsContracted)
                    {
                        float fromLatitude, fromLongitude;
                        float toLatitude, toLongitude;
                        if (graph.GetVertex(edges.Vertex1, out fromLatitude, out fromLongitude) &&
                            graph.GetVertex(edges.Vertex2, out toLatitude, out toLongitude))
                        {
                            var vertexCoordinate = new GeoCoordinate(fromLatitude, fromLongitude);
                            var distance = coordinate.DistanceReal(vertexCoordinate).Value;
                            if (distance < closestWithoutMatch.Distance)
                            { // the distance found is closer.
                                var diff = coordinate - vertexCoordinate;
                                closestWithoutMatchBox = new GeoCoordinateBox(new GeoCoordinate(coordinate + diff),
                                    new GeoCoordinate(coordinate - diff));
                                closestWithoutMatch = new SearchClosestResult<CHEdgeData>(
                                    distance, edges.Vertex1);
                            }

                            vertexCoordinate = new GeoCoordinate(toLatitude, toLongitude);
                            distance = coordinate.DistanceReal(vertexCoordinate).Value;
                            if (distance < closestWithoutMatch.Distance)
                            { // the distance found is closer.
                                var diff = coordinate - vertexCoordinate;
                                closestWithoutMatchBox = new GeoCoordinateBox(new GeoCoordinate(coordinate + diff),
                                    new GeoCoordinate(coordinate - diff));
                                closestWithoutMatch = new SearchClosestResult<CHEdgeData>(
                                    distance, edges.Vertex2);
                            }

                            var arcValueValueCoordinates = edges.Intermediates;
                            if (arcValueValueCoordinates != null)
                            { // search over intermediate points.
                                var arcValueValueCoordinatesArray = arcValueValueCoordinates.ToArray();
                                for (int idx = 0; idx < arcValueValueCoordinatesArray.Length; idx++)
                                {
                                    vertexCoordinate = new GeoCoordinate(
                                        arcValueValueCoordinatesArray[idx].Latitude,
                                        arcValueValueCoordinatesArray[idx].Longitude);
                                    distance = coordinate.DistanceReal(vertexCoordinate).Value;
                                    if (distance < closestWithoutMatch.Distance)
                                    { // the distance found is closer.
                                        var diff = coordinate - vertexCoordinate;
                                        closestWithoutMatchBox = new GeoCoordinateBox(new GeoCoordinate(coordinate + diff),
                                            new GeoCoordinate(coordinate - diff));
                                        closestWithoutMatch = new SearchClosestResult<CHEdgeData>(
                                            distance, edges.Vertex1, edges.Vertex2, idx, edges.EdgeData, arcValueValueCoordinatesArray);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // return the best result.
            if (closestWithMatch.Distance < double.MaxValue)
            {
                return closestWithMatch;
            }
            return closestWithoutMatch;
        }

        #endregion

    }
}