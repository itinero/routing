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

using OsmSharp.Collections.PriorityQueues;
using OsmSharp.Logging;
using OsmSharp.Routing.Constraints;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using OsmSharp.Routing.Vehicles;

namespace OsmSharp.Routing.Graph.Routing
{
    /// <summary>
    /// A class containing a dykstra implementation suitable for a simple graph.
    /// </summary>
    public class Dykstra : DykstraBase<Edge>, IRoutingAlgorithm<Edge>
    {
        /// <summary>
        /// Creates a new dykstra routing object.
        /// </summary>
        public Dykstra()
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
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public PathSegment<long> Calculate(IRoutingAlgorithmData<Edge> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            uint from, uint to)
        {
            var source = new PathSegmentVisitList();
            source.UpdateVertex(new PathSegment<long>(from));
            var target = new PathSegmentVisitList();
            target.UpdateVertex(new PathSegment<long>(to));

            // do the basic CH calculations.
            return this.Calculate(graph, interpreter, vehicle, source, target, float.MaxValue, null);
        }

        /// <summary>
        /// Calculates the shortest path from the given vertex to the given vertex given the weights in the graph.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="max"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public PathSegment<long> Calculate(IRoutingAlgorithmData<Edge> graph, IRoutingInterpreter interpreter,
            Vehicle vehicle, PathSegmentVisitList from, PathSegmentVisitList to, double max, Dictionary<string, object> parameters)
        {
            return this.CalculateToClosest(graph, interpreter, vehicle, from,
                new PathSegmentVisitList[] { to }, max, null);
        }

        /// <summary>
        /// Calculates the shortest path from all sources to all targets.
        /// </summary>
        /// <returns></returns>
        public PathSegment<long>[][] CalculateManyToMany(IRoutingAlgorithmData<Edge> graph, IRoutingInterpreter interpreter,
            Vehicle vehicle, PathSegmentVisitList[] sources, PathSegmentVisitList[] targets, double maxSearch, Dictionary<string, object> parameters)
        {
            var results = new PathSegment<long>[sources.Length][];
            for (int sourceIdx = 0; sourceIdx < sources.Length; sourceIdx++)
            {
                results[sourceIdx] = this.DoCalculation(graph, interpreter, vehicle,
                   sources[sourceIdx], targets, maxSearch, false, false, parameters);
            }
            return results;
        }

        /// <summary>
        /// Calculates the shortest path from the given vertex to the given vertex given the weights in the graph.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="max"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public double CalculateWeight(IRoutingAlgorithmData<Edge> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            PathSegmentVisitList from, PathSegmentVisitList to, double max, Dictionary<string, object> parameters)
        {
            PathSegment<long> closest = this.CalculateToClosest(graph, interpreter, vehicle, from,
                new PathSegmentVisitList[] { to }, max, null);
            if (closest != null)
            {
                return closest.Weight;
            }
            return double.MaxValue;
        }

        /// <summary>
        /// Calculates a shortest path between the source vertex and any of the targets and returns the shortest.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="from"></param>
        /// <param name="targets"></param>
        /// <param name="max"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public PathSegment<long> CalculateToClosest(IRoutingAlgorithmData<Edge> graph, IRoutingInterpreter interpreter,
            Vehicle vehicle, PathSegmentVisitList from, PathSegmentVisitList[] targets, double max, Dictionary<string, object> parameters)
        {
            var result = this.DoCalculation(graph, interpreter, vehicle,
                from, targets, max, false, false, parameters);
            if (result != null && result.Length == 1)
            {
                return result[0];
            }
            return null;
        }

        /// <summary>
        /// Calculates all routes from a given source to all given targets.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="targets"></param>
        /// <param name="max"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public double[] CalculateOneToManyWeight(IRoutingAlgorithmData<Edge> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            PathSegmentVisitList source, PathSegmentVisitList[] targets, double max, Dictionary<string, object> parameters, HashSet<int> invalidSet)
        {
            var many = this.DoCalculation(graph, interpreter, vehicle,
                   source, targets, max, false, false, null);

            var weights = new double[many.Length];
            for (int idx = 0; idx < many.Length; idx++)
            {
                if (many[idx] != null)
                {
                    weights[idx] = many[idx].Weight;
                }
                else
                {
                    weights[idx] = double.MaxValue;
                    invalidSet.Add(idx);
                }
            }
            return weights;
        }

        /// <summary>
        /// Calculates all routes from a given sources to all given targets.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="sources"></param>
        /// <param name="targets"></param>
        /// <param name="max"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public double[][] CalculateManyToManyWeight(IRoutingAlgorithmData<Edge> graph, IRoutingInterpreter interpreter,
            Vehicle vehicle, PathSegmentVisitList[] sources, PathSegmentVisitList[] targets, double max, Dictionary<string, object> parameters, 
            HashSet<int> invalidSet)
        {
            var results = new double[sources.Length][];
            for (int idx = 0; idx < sources.Length; idx++)
            {
                results[idx] = this.CalculateOneToManyWeight(graph, interpreter, vehicle, sources[idx], targets, max, parameters, invalidSet);

                OsmSharp.Logging.Log.TraceEvent("Dykstra", TraceEventType.Information, "Calculating weights... {0}%",
                    (int)(((float)idx / (float)sources.Length) * 100));
            }
            return results;
        }

        /// <summary>
        /// Returns true, range calculation is supported.
        /// </summary>
        public bool IsCalculateRangeSupported
        {
            get
            {
                return true;
            }
        }


        /// <summary>
        /// Calculates all points that are at or close to the given weight.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="weight"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public HashSet<long> CalculateRange(IRoutingAlgorithmData<Edge> graph, IRoutingInterpreter interpreter,
            Vehicle vehicle, PathSegmentVisitList source, double weight, Dictionary<string, object> parameters)
        {
            return this.CalculateRange(graph, interpreter, vehicle, source, weight, true, null);
        }

        /// <summary>
        /// Calculates all points that are at or close to the given weight.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="weight"></param>
        /// <param name="forward"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public HashSet<long> CalculateRange(IRoutingAlgorithmData<Edge> graph, IRoutingInterpreter interpreter,
            Vehicle vehicle, PathSegmentVisitList source, double weight, bool forward, Dictionary<string, object> parameters)
        {
            PathSegment<long>[] result = this.DoCalculation(graph, interpreter, vehicle,
                   source, new PathSegmentVisitList[0], weight, false, true, forward, parameters);

            var resultVertices = new HashSet<long>();
            for (int idx = 0; idx < result.Length; idx++)
            {
                resultVertices.Add(result[idx].VertexId);
            }
            return resultVertices;
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
        public bool CheckConnectivity(IRoutingAlgorithmData<Edge> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            PathSegmentVisitList source, double weight, Dictionary<string, object> parameters)
        {
            HashSet<long> range = this.CalculateRange(graph, interpreter, vehicle, source, weight, true, null);

            if (range.Count > 0)
            {
                range = this.CalculateRange(graph, interpreter, vehicle, source, weight, false, null);
                if (range.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        #region Implementation

        /// <summary>
        /// Does forward dykstra calculation(s) with several options.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="targets"></param>
        /// <param name="weight"></param>
        /// <param name="stopAtFirst"></param>
        /// <param name="returnAtWeight"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private PathSegment<long>[] DoCalculation(IRoutingAlgorithmData<Edge> graph, IRoutingInterpreter interpreter,
            Vehicle vehicle, PathSegmentVisitList source, PathSegmentVisitList[] targets, double weight,
            bool stopAtFirst, bool returnAtWeight, Dictionary<string, object> parameters)
        {
            return this.DoCalculation(graph, interpreter, vehicle, source, targets, weight, stopAtFirst, returnAtWeight, true, parameters);
        }

        /// <summary>
        /// Does dykstra calculation(s) with several options.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="sourceList"></param>
        /// <param name="targetList"></param>
        /// <param name="weight"></param>
        /// <param name="stopAtFirst"></param>
        /// <param name="returnAtWeight"></param>
        /// <param name="forward"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private PathSegment<long>[] DoCalculation(IRoutingAlgorithmData<Edge> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            PathSegmentVisitList sourceList, PathSegmentVisitList[] targetList, double weight,
            bool stopAtFirst, bool returnAtWeight, bool forward, Dictionary<string, object> parameters)
        {
            // intialize dykstra data structures.
            var heap = new BinaryHeap<DykstraVisit>(100);
            var visits = new Dictionary<long, DykstraVisit>();

            // initialize a dictionary of speeds per profile.
            var speeds = new Dictionary<uint, Speed>();

            // make copies of the target and source visitlist.
            var source = sourceList.Clone() as PathSegmentVisitList;
            var targets = new PathSegmentVisitList[targetList.Length];
            var targetsCount = new int[targetList.Length];
            for (int targetIdx = 0; targetIdx < targetList.Length; targetIdx++)
            {
                targets[targetIdx] = targetList[targetIdx].Clone() as PathSegmentVisitList;
                targetsCount[targetIdx] = targetList[targetIdx].Count;
            }

            //  initialize the result data structures.
            var segmentsAtWeight = new List<PathSegment<long>>();
            var segmentsToTarget = new PathSegment<long>[targets.Length]; // the resulting target segments.

            var labels = new Dictionary<long, IList<RoutingLabel>>();
            foreach (long vertex in source.GetVertices())
            {
                labels[vertex] = new List<RoutingLabel>();

                var path = source.GetPathTo(vertex);
                heap.Push(new DykstraVisit(path), (float)path.Weight);
            }

            // set the from node as the current node and put it in the correct data structures.
            // initialize the source's neighbors.
            var current = heap.Pop();
            while (current != null && visits.ContainsKey(current.Vertex))
            { // keep dequeuing.
                current = heap.Pop();
            }

            if (current == null)
            {
                return null;
            }

            // test each target for the source.
            // test each source for any of the targets.
            var pathsFromSource = new Dictionary<long, PathSegment<long>>();
            foreach (long sourceVertex in source.GetVertices())
            { // get the path to the vertex.
                // get the source path.
                var sourcePath = source.GetPathTo(sourceVertex);
                sourcePath = sourcePath.From;
                while (sourcePath != null)
                { // add the path to the paths from source.
                    // add to visits.
                    var visit = new DykstraVisit(sourcePath);
                    visits[visit.Vertex] = visit;

                    pathsFromSource[sourcePath.VertexId] = sourcePath;
                    sourcePath = sourcePath.From;
                }
            }
            // loop over all targets, check for source.
            for (int idx = 0; idx < targets.Length; idx++)
            { // loop over each vertex in the targets.
                foreach (long targetVertex in new List<long>(targets[idx].GetVertices()))
                {
                    // get the target path.
                    var targetPath = targets[idx].GetPathTo(targetVertex);

                    targetPath = targetPath.From;
                    while (targetPath != null)
                    { // add the path to the paths from source.
                        PathSegment<long> pathFromSource;
                        if (pathsFromSource.TryGetValue(targetPath.VertexId, out pathFromSource))
                        { // a path is found.
                            // get the existing path if any.
                            var existing = segmentsToTarget[idx];
                            if (existing == null)
                            { // a path did not exist yet!
                                segmentsToTarget[idx] = targetPath.Reverse().ConcatenateAfter(pathFromSource);
                                targets[idx].Remove(targetVertex);
                            }
                            else if (existing.Weight > targetPath.Weight + pathFromSource.Weight)
                            { // a new path is found with a lower weight.
                                segmentsToTarget[idx] = targetPath.Reverse().ConcatenateAfter(pathFromSource);
                            }
                        }
                        targetPath = targetPath.From;
                    }
                }
            }
            if (targets.Length > 0 && targets.All(x => x.Count == 0))
            { // routing is finished!
                return segmentsToTarget.ToArray();
            }

            if (stopAtFirst)
            { // only one entry is needed.
                var oneFound = false;
                for (int idx = 0; idx < targets.Length; idx++)
                {
                    if (targets[idx].Count < targetsCount[idx])
                    {
                        oneFound = true;
                        break;
                    }
                }

                if (oneFound)
                { // targets found, return the shortest!
                    PathSegment<long> shortest = null;
                    foreach (PathSegment<long> foundTarget in segmentsToTarget)
                    {
                        if (shortest == null)
                        {
                            shortest = foundTarget;
                        }
                        else if (foundTarget != null &&
                            shortest.Weight > foundTarget.Weight)
                        {
                            shortest = foundTarget;
                        }
                    }
                    segmentsToTarget = new PathSegment<long>[1];
                    segmentsToTarget[0] = shortest;
                    return segmentsToTarget;
                }
                else
                { // not targets found yet!
                    segmentsToTarget = new PathSegment<long>[1];
                }
            }

            // test for identical start/end point.
            for (int idx = 0; idx < targets.Length; idx++)
            {
                var target = targets[idx];
                if (returnAtWeight)
                { // add all the reached vertices larger than weight to the results.
                    if (current.Weight > weight)
                    {
                        var toPath = target.GetPathTo(current.Vertex);
                        toPath.Reverse();
                        toPath = toPath.ConcatenateAfter(current.ToPath(visits));
                        segmentsAtWeight.Add(toPath);
                    }
                }
                else if (target.Contains(current.Vertex))
                { // the current is a target!
                    var toPath = target.GetPathTo(current.Vertex);
                    toPath = toPath.Reverse();
                    toPath = toPath.ConcatenateAfter(current.ToPath(visits));

                    if (stopAtFirst)
                    { // stop at the first occurrence.
                        segmentsToTarget[0] = toPath;
                        return segmentsToTarget;
                    }
                    else
                    { // normal one-to-many; add to the result.
                        // check if routing is finished.
                        if (segmentsToTarget[idx] == null)
                        { // make sure only the first route is set.
                            segmentsToTarget[idx] = toPath;
                            if (targets.All(x => x.Count == 0))
                            { // routing is finished!
                                return segmentsToTarget.ToArray();
                            }
                        }
                        else if (segmentsToTarget[idx].Weight > toPath.Weight)
                        { // check if the second, third or later is shorter.
                            segmentsToTarget[idx] = toPath;
                        }
                    }
                }
            }

            // start OsmSharp.Routing.
            var edges =  graph.GetEdges(Convert.ToUInt32(current.Vertex));
            visits[current.Vertex] = current;

            // loop until target is found and the route is the shortest!
            var noSpeed = new Speed() { Direction = null, MeterPerSecond = 0 };
            while (true)
            {
                // get the current labels list (if needed).
                IList<RoutingLabel> currentLabels = null;
                if (interpreter.Constraints != null)
                { // there are constraints, get the labels.
                    currentLabels = labels[current.Vertex];
                    labels.Remove(current.Vertex);
                }

                // check turn-restrictions.
                //List<uint[]> restrictions = null;
                bool isRestricted = false;
                //if (current.From != null &&
                //    current.From.Vertex > 0 &&
                //    graph.TryGetRestrictionAsStart(vehicle, (uint)current.From.Vertex, out restrictions))
                //{ // there are restrictions!
                //    // search for a restriction that ends in the currently selected vertex.
                //    for(int idx = 0; idx < restrictions.Count; idx++)
                //    {
                //        var restriction = restrictions[idx];
                //        if(restriction[restriction.Length - 1] == current.VertexId)
                //        { // oeps, do not consider the neighbours of this vertex.
                //            isRestricted = true;
                //            break;
                //        }

                //        for(int restrictedIdx = 0; restrictedIdx < restriction.Length; restrictedIdx++)
                //        { // make sure the restricted vertices can be choosen multiple times.
                //            // restrictedVertices.Add(restriction[restrictedIdx]);
                //            visitList.SetRestricted(restriction[restrictedIdx]);
                //        }
                //    }
                //}
                if (!isRestricted)
                {
                    // update the visited nodes.
                    while (edges.MoveNext())
                    {
                        var edge = edges;
                        var neighbour = edge.Neighbour;

                        if (current.From == neighbour)
                        { // don't go back!
                            continue;
                        }

                        if (visits.ContainsKey(neighbour))
                        { // has already been choosen.
                            continue;
                        }

                        //// prevent u-turns.
                        //if(current.From != null)
                        //{ // a possible u-turn.
                        //    if(current.From.VertexId == neighbour.Neighbour)
                        //    { // a u-turn, don't do this please!
                        //        continue;
                        //    }
                        //}

                        // get the speed from cache or calculate.
                        var edgeData = edge.EdgeData;
                        var speed = noSpeed;
                        if (!speeds.TryGetValue(edgeData.Tags, out speed))
                        { // speed not there, calculate speed.
                            var tags = graph.TagsIndex.Get(edgeData.Tags);
                            speed = noSpeed;
                            if (vehicle.CanTraverse(tags))
                            { // can traverse, speed not null!
                                speed = new Speed()
                                {
                                    MeterPerSecond = ((OsmSharp.Units.Speed.MeterPerSecond)vehicle.ProbableSpeed(tags)).Value,
                                    Direction = vehicle.IsOneWay(tags)
                                };
                            }
                            speeds.Add(edgeData.Tags, speed);
                        }

                        // check the tags against the interpreter.
                        if (speed.MeterPerSecond > 0 && (!speed.Direction.HasValue || speed.Direction.Value == edgeData.Forward))
                        { // it's ok; the edge can be traversed by the given vehicle.
                            if ((current.From == 0 || interpreter.CanBeTraversed(current.From, current.Vertex, neighbour)))
                            { // the neighbour is forward and is not settled yet!
                                bool restrictionsOk = true;
                                //if (restrictions != null)
                                //{ // search for a restriction that ends in the currently selected neighbour and check if it's via-vertex matches.
                                //    for (int idx = 0; idx < restrictions.Count; idx++)
                                //    {
                                //        var restriction = restrictions[idx];
                                //        if (restriction[restriction.Length - 1] == neighbour.Neighbour)
                                //        { // oeps, do not consider the neighbours of this vertex.
                                //            if (restriction[restriction.Length - 2] == current.VertexId)
                                //            { // damn this route-part is restricted!
                                //                restrictionsOk = false;
                                //                break;
                                //            }
                                //        }
                                //    }
                                //}

                                // check the labels (if needed).
                                bool constraintsOk = true;
                                if (restrictionsOk && interpreter.Constraints != null)
                                { // check if the label is ok.
                                    var neighbourLabel = interpreter.Constraints.GetLabelFor(
                                        graph.TagsIndex.Get(edgeData.Tags));

                                    // only test labels if there is a change.
                                    if (currentLabels.Count == 0 || !neighbourLabel.Equals(currentLabels[currentLabels.Count - 1]))
                                    { // labels are different, test them!
                                        constraintsOk = interpreter.Constraints.ForwardSequenceAllowed(currentLabels,
                                            neighbourLabel);

                                        if (constraintsOk)
                                        { // update the labels.
                                            var neighbourLabels = new List<RoutingLabel>(currentLabels);
                                            neighbourLabels.Add(neighbourLabel);

                                            labels[neighbour] = neighbourLabels;
                                        }
                                    }
                                    else
                                    { // set the same label(s).
                                        labels[neighbour] = currentLabels;
                                    }
                                }

                                if (constraintsOk && restrictionsOk)
                                { // all constraints are validated or there are none.
                                    // calculate neighbors weight.
                                    double totalWeight = current.Weight + (edgeData.Distance / speed.MeterPerSecond);
                                    //double totalWeight = current.Weight + edgeData.Distance;

                                    // update the visit list.
                                    var neighbourVisit = new DykstraVisit(neighbour, current.Vertex, (float)totalWeight);// new PathSegment<long>(neighbour, totalWeight, current);
                                    heap.Push(neighbourVisit, neighbourVisit.Weight);
                                }
                            }
                        }
                    }
                }

                // while the visit list is not empty.
                current = null;
                if (heap.Count > 0)
                { // choose the next vertex.
                    current = heap.Pop();
                    while (current != null && visits.ContainsKey(current.Vertex))
                    { // keep dequeuing.
                        current = heap.Pop();
                    }
                }
                while (current != null && current.Weight > weight)
                {
                    if (returnAtWeight)
                    { // add all the reached vertices larger than weight to the results.
                        segmentsAtWeight.Add(current.ToPath(visits));
                    }

                    // choose the next vertex.
                    current = heap.Pop();
                    while (current != null && visits.ContainsKey(current.Vertex))
                    { // keep dequeuing.
                        current = heap.Pop();
                    }
                }
                if (current != null)
                { // we visit this one, set visit.
                    visits[current.Vertex] = current;
                }
                else
                { // route is not found, there are no vertices left
                    // or the search went outside of the max bounds.
                    break;
                }

                // check target.
                for (int idx = 0; idx < targets.Length; idx++)
                {
                    PathSegmentVisitList target = targets[idx];
                    if (target.Contains(current.Vertex))
                    { // the current is a target!
                        var toPath = target.GetPathTo(current.Vertex);
                        toPath = toPath.Reverse();
                        toPath = toPath.ConcatenateAfter(current.ToPath(visits));

                        if (stopAtFirst)
                        { // stop at the first occurrence.
                            segmentsToTarget[0] = toPath;
                            return segmentsToTarget;
                        }
                        else
                        { // normal one-to-many; add to the result.
                            // check if routing is finished.
                            if (segmentsToTarget[idx] == null)
                            { // make sure only the first route is set.
                                segmentsToTarget[idx] = toPath;
                            }
                            else if (segmentsToTarget[idx].Weight > toPath.Weight)
                            { // check if the second, third or later is shorter.
                                segmentsToTarget[idx] = toPath;
                            }

                            // remove this vertex from this target's paths.
                            target.Remove(current.Vertex);

                            // if this target is empty it's optimal route has been found.
                            if (target.Count == 0)
                            { // now the shortest route has been found for sure!
                                if (targets.All(x => x.Count == 0))
                                { // routing is finished!
                                    // OsmSharp.Logging.Log.TraceEvent("Dykstra", TraceEventType.Information, string.Format("Finished with {0} visits.", visits.Count));
                                    return segmentsToTarget.ToArray();
                                }
                            }
                        }
                    }
                }

                // get the neighbors of the current node.
                edges = graph.GetEdges(Convert.ToUInt32(current.Vertex));
            }

            // return the result.
            if (!returnAtWeight)
            {
                // OsmSharp.Logging.Log.TraceEvent("Dykstra", TraceEventType.Information, string.Format("Finished with {0} visits.", visits.Count));
                return segmentsToTarget.ToArray();
            }
            // OsmSharp.Logging.Log.TraceEvent("Dykstra", TraceEventType.Information, string.Format("Finished with {0} visits.", visits.Count));
            return segmentsAtWeight.ToArray();
        }

        private struct Speed
        {
            public double MeterPerSecond { get; set; }

            public bool? Direction { get; set; }
        }

        #endregion
    }

    /// <summary>
    /// Represents a dykstra edge.
    /// </summary>
    public class DykstraVisit
    {
        /// <summary>
        /// Creates a new dykstra vertex state for the last vertex in the given path.
        /// </summary>
        /// <param name="path"></param>
        public DykstraVisit(PathSegment<long> path)
        {
            this.Vertex = path.VertexId;
            this.Weight = (float)path.Weight;
            if (path.From != null)
            {
                this.From = path.From.VertexId;
            }
        }

        /// <summary>
        /// Creates a new dykstra vertex state.
        /// </summary>
        /// <param name="vertex">The vertex id.</param>
        public DykstraVisit(uint vertex)
        {
            this.Vertex = vertex;
            this.From = 0;
            this.Weight = 0;
        }

        /// <summary>
        /// Creates a new dykstra vertex state.
        /// </summary>
        /// <param name="vertex">The vertex id.</param>
        /// <param name="from">The from vertex id.</param>
        /// <param name="weight">The weight.</param>
        public DykstraVisit(long vertex, long from, float weight)
        {
            this.Vertex = vertex;
            this.From = from;
            this.Weight = weight;
        }

        /// <summary>
        /// The id of this vertex.
        /// </summary>
        public long Vertex;

        /// <summary>
        /// The if of the vertex right before this vertex.
        /// </summary>
        public long From;

        /// <summary>
        /// The weight to the current vertex.
        /// </summary>
        public float Weight;

        /// <summary>
        /// Returns the path to this vertex given the visits.
        /// </summary>
        /// <param name="visits"></param>
        /// <returns></returns>
        public PathSegment<long> ToPath(Dictionary<long, DykstraVisit> visits)
        {
            if (this.From == 0)
            {
                return new PathSegment<long>(this.Vertex);
            }
            return new PathSegment<long>(this.Vertex, this.Weight, visits[this.From].ToPath(visits));
        }
    }
}