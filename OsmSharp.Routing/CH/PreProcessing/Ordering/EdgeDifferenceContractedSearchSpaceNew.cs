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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Routing;

namespace OsmSharp.Routing.CH.Preprocessing.Ordering
{
    /// <summary>
    /// The edge difference calculator.
    /// </summary>
    public class EdgeDifferenceContractedSearchSpace : INodeWeightCalculator
    {
        /// <summary>
        /// Holds the graph.
        /// </summary>
        private INodeWitnessCalculator _witnessCalculator;

        /// <summary>
        /// Holds the data.
        /// </summary>
        private GraphBase<CHEdgeData> _data;

        /// <summary>
        /// Holds the contracted count.
        /// </summary>
        private Dictionary<uint, int> _contractionCount;

        /// <summary>
        /// Holds the depth.
        /// </summary>
        private Dictionary<long, int> _depth;

        /// <summary>
        /// Creates a new edge difference calculator.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="witnessCalculator"></param>
        public EdgeDifferenceContractedSearchSpace(GraphBase<CHEdgeData> data, INodeWitnessCalculator witnessCalculator)
        {
            _data = data;
            _witnessCalculator = witnessCalculator;
            _contractionCount = new Dictionary<uint, int>();
            _depth = new Dictionary<long, int>();
        }

        /// <summary>
        /// Calculates the priority of the given vertex.
        /// </summary>
        /// <param name="vertex">The vertex to calculate the priority for.</param>
        public float Calculate(uint vertex)
        {
            int newEdges, removedEdges, depth, contracted;
            return this.Calculate(vertex, out newEdges, out removedEdges, out depth, out contracted);
        }

        /// <summary>
        /// Calculates the priority of the given vertex.
        /// </summary>
        /// <param name="vertex">The vertex to calculate the priority for.</param>
        /// <param name="newEdges">The number of new edges that would be added.</param>
        /// <param name="removedEdges">The number of edges that would be removed.</param>
        /// <param name="depth">The depth of the vertex.</param>
        /// <param name="contracted">The number of contracted neighours.</param>
        public float Calculate(uint vertex, out int newEdges, out int removedEdges, out int depth, out int contracted)
        {
            newEdges = 0; 
            removedEdges = 0;
            _contractionCount.TryGetValue(vertex, out contracted);

            // get all information from the source.
            var edges =  _data.GetEdges(vertex).ToList();

            // build the list of edges to replace.
            var edgesForContractions = new List<Edge<CHEdgeData>>(edges.Count);
            var tos = new List<uint>(edges.Count);
            var tosSet = new HashSet<uint>();
            foreach (var edge in edges)
            {
                // use this edge for contraction.
                edgesForContractions.Add(edge);
                tos.Add(edge.Neighbour);
                tosSet.Add(edge.Neighbour);
                removedEdges++;
            }

            var toRequeue = new HashSet<uint>();

            var forwardEdges = new CHEdgeData?[2];
            var backwardEdges = new CHEdgeData?[2];
            var existingEdgesToRemove = new HashSet<CHEdgeData>();

            // loop over each combination of edges just once.
            var forwardWitnesses = new bool[edgesForContractions.Count];
            var backwardWitnesses = new bool[edgesForContractions.Count];
            var weights = new List<float>(edgesForContractions.Count);
            var edgesToY = new Dictionary<uint, Tuple<CHEdgeData?, CHEdgeData?, CHEdgeData?, float, float>>(edgesForContractions.Count);
            for (int x = 1; x < edgesForContractions.Count; x++)
            { // loop over all elements first.
                var xEdge = edgesForContractions[x];

                // get edges.
                edgesToY.Clear();
                var rawEdgesToY = _data.GetEdges(xEdge.Neighbour);
                while (rawEdgesToY.MoveNext())
                {
                    var rawEdgeNeighbour = rawEdgesToY.Neighbour;
                    if (tosSet.Contains(rawEdgeNeighbour))
                    {
                        var rawEdgeData = rawEdgesToY.EdgeData;
                        var rawEdgeForwardWeight = rawEdgeData.CanMoveForward ? rawEdgeData.Weight : float.MaxValue;
                        var rawEdgeBackwardWeight = rawEdgeData.CanMoveBackward ? rawEdgeData.Weight : float.MaxValue;
                        Tuple<CHEdgeData?, CHEdgeData?, CHEdgeData?, float, float> edgeTuple;
                        if (!edgesToY.TryGetValue(rawEdgeNeighbour, out edgeTuple))
                        {
                            edgeTuple = new Tuple<CHEdgeData?, CHEdgeData?, CHEdgeData?, float, float>(rawEdgeData, null, null,
                                rawEdgeForwardWeight, rawEdgeBackwardWeight);
                            edgesToY.Add(rawEdgeNeighbour, edgeTuple);
                        }
                        else if (!edgeTuple.Item2.HasValue)
                        {
                            edgesToY[rawEdgeNeighbour] = new Tuple<CHEdgeData?, CHEdgeData?, CHEdgeData?, float, float>(
                                edgeTuple.Item1, rawEdgeData, null,
                                rawEdgeForwardWeight < edgeTuple.Item4 ? rawEdgeForwardWeight : edgeTuple.Item4,
                                rawEdgeBackwardWeight < edgeTuple.Item5 ? rawEdgeBackwardWeight : edgeTuple.Item5);
                        }
                        else
                        {
                            edgesToY[rawEdgeNeighbour] = new Tuple<CHEdgeData?, CHEdgeData?, CHEdgeData?, float, float>(
                                edgeTuple.Item1, edgeTuple.Item2, rawEdgeData,
                                rawEdgeForwardWeight < edgeTuple.Item4 ? rawEdgeForwardWeight : edgeTuple.Item4,
                                rawEdgeBackwardWeight < edgeTuple.Item5 ? rawEdgeBackwardWeight : edgeTuple.Item5);
                        }
                    }
                }

                // calculate max weight.
                weights.Clear();
                var forwardUnknown = false;
                var backwardUnknown = false;
                for (int y = 0; y < x; y++)
                {
                    // update maxWeight.
                    var yEdge = edgesForContractions[y];
                    if (xEdge.Neighbour != yEdge.Neighbour)
                    {
                        // reset witnesses.
                        var forwardWeight = (float)xEdge.EdgeData.Weight + (float)yEdge.EdgeData.Weight;
                        forwardWitnesses[y] = !xEdge.EdgeData.CanMoveBackward || !yEdge.EdgeData.CanMoveForward;
                        backwardWitnesses[y] = !xEdge.EdgeData.CanMoveForward || !yEdge.EdgeData.CanMoveBackward;
                        weights.Add(forwardWeight);

                        Tuple<CHEdgeData?, CHEdgeData?, CHEdgeData?, float, float> edgeTuple;
                        if (edgesToY.TryGetValue(yEdge.Neighbour, out edgeTuple))
                        {
                            if (!forwardWitnesses[y])
                            { // check 1-hop witnesses.
                                if (edgeTuple.Item4 <= forwardWeight)
                                {
                                    forwardWitnesses[y] = true;
                                }
                            }
                            if (!backwardWitnesses[y])
                            { // check 1-hop witnesses.
                                if (edgeTuple.Item5 <= forwardWeight)
                                {
                                    backwardWitnesses[y] = true;
                                }
                            }
                        }
                        forwardUnknown = !forwardWitnesses[y] || forwardUnknown;
                        backwardUnknown = !backwardWitnesses[y] || backwardUnknown;
                    }
                    else
                    { // already set this to true, not use calculating it's witness.
                        forwardWitnesses[y] = true;
                        backwardWitnesses[y] = true;
                        weights.Add(0);
                    }
                }

                // calculate witnesses.
                if (_witnessCalculator.HopLimit > 1)
                { // 1-hops already checked.
                    if (forwardUnknown || backwardUnknown)
                    {
                        _witnessCalculator.Exists(_data, xEdge.Neighbour, tos, weights, int.MaxValue,
                            ref forwardWitnesses, ref backwardWitnesses, vertex);
                    }
                }

                for (int y = 0; y < x; y++)
                { // loop over all elements.
                    var yEdge = edgesForContractions[y];

                    // add the combinations of these edges.
                    if (xEdge.Neighbour != yEdge.Neighbour)
                    { // there is a connection from x to y and there is no witness path.
                        // create x-to-y data and edge.
                        var canMoveForward = !forwardWitnesses[y] && (xEdge.EdgeData.CanMoveBackward && yEdge.EdgeData.CanMoveForward);
                        var canMoveBackward = !backwardWitnesses[y] && (xEdge.EdgeData.CanMoveForward && yEdge.EdgeData.CanMoveBackward);

                        if (canMoveForward || canMoveBackward)
                        { // add the edge if there is usefull info or if there needs to be a neighbour relationship.
                            // add contracted edges like normal. // calculate the total weights.
                            var weight = (float)xEdge.EdgeData.Weight + (float)yEdge.EdgeData.Weight;

                            // there are a few options now:
                            //  1) No edges yet between xEdge.Neighbour and yEdge.Neighbour.
                            //  1) There is no other contracted edge: just add as a duplicate.
                            //  2) There is at least on other contracted edge: optimize information because there can only be 4 case between two vertices:
                            //     - One bidirectional edge.
                            //     - Two directed edges with different weights.
                            //     - One forward edge.
                            //     - One backward edge.
                            //    =>  all available information needs to be combined.

                            // check existing data.
                            var existingCanMoveForward = false;
                            var existingCanMoveBackward = false;
                            var existingForwardWeight = float.MaxValue;
                            var existingBackwardWeight = float.MaxValue;
                            uint existingForwardContracted = 0;
                            uint existingBackwardContracted = 0;
                            Tuple<CHEdgeData?, CHEdgeData?, CHEdgeData?, float, float> edgeTuple;
                            if (edgesToY.TryGetValue(yEdge.Neighbour, out edgeTuple))
                            {
                                //var existingEdges = _data.GetEdges(xEdge.Neighbour, yEdge.Neighbour);
                                existingEdgesToRemove.Clear();
                                // remove all existing stuff.
                                var existingEdge = new CHEdgeData();
                                for (int idx = 0; idx < 3; idx++)
                                {
                                    switch (idx)
                                    {
                                        case 0:
                                            existingEdge = edgeTuple.Item1.Value;
                                            break;
                                        case 1:
                                            if (!edgeTuple.Item2.HasValue)
                                            {
                                                idx = 2;
                                                break;
                                            }
                                            existingEdge = edgeTuple.Item2.Value;
                                            break;
                                        case 2:
                                            if (!edgeTuple.Item3.HasValue)
                                            {
                                                idx = 2;
                                                break;
                                            }
                                            existingEdge = edgeTuple.Item3.Value;
                                            break;
                                    }

                                    var existingEdgeData = existingEdge;
                                    if (existingEdgeData.IsContracted)
                                    { // this edge is contracted, collect it's information.
                                        existingEdgesToRemove.Add(existingEdgeData);
                                        if (existingEdgeData.CanMoveForward)
                                        { // can move forward, so at least one edge that can move forward.
                                            existingCanMoveForward = true;
                                            if (existingForwardWeight > existingEdgeData.Weight)
                                            { // update forward weight.
                                                existingForwardWeight = existingEdgeData.Weight;
                                                existingForwardContracted = existingEdgeData.ContractedId;
                                            }
                                        }
                                        if (existingEdgeData.CanMoveBackward)
                                        { // can move backward, so at least one edge that can move backward.
                                            existingCanMoveBackward = true;
                                            if (existingBackwardWeight > existingEdgeData.Weight)
                                            { // update backward weight.
                                                existingBackwardWeight = existingEdgeData.Weight;
                                                existingBackwardContracted = existingEdgeData.ContractedId;
                                            }
                                        }
                                    }
                                }
                            }

                            if (existingCanMoveForward || existingCanMoveBackward)
                            { // there is already another contraced edge.
                                uint forwardContractedId = vertex;
                                float forwardWeight = weight;
                                // merge with existing data.
                                if (existingCanMoveForward &&
                                    ((weight > existingForwardWeight) || !canMoveForward))
                                { // choose the smallest weight.
                                    canMoveForward = true;
                                    forwardContractedId = existingForwardContracted;
                                    forwardWeight = existingForwardWeight;
                                }

                                uint backwardContractedId = vertex;
                                float backwardWeight = weight;
                                // merge with existing data.
                                if (existingCanMoveBackward &&
                                    ((weight > existingBackwardWeight) || !canMoveBackward))
                                { // choose the smallest weight.
                                    canMoveBackward = true;
                                    backwardContractedId = existingBackwardContracted;
                                    backwardWeight = existingBackwardWeight;
                                }

                                // add one of the 4 above case.
                                forwardEdges[0] = null;
                                forwardEdges[1] = null;
                                backwardEdges[0] = null;
                                backwardEdges[1] = null;
                                if (canMoveForward && canMoveBackward && forwardWeight == backwardWeight && forwardContractedId == backwardContractedId)
                                { // just add one edge.
                                    forwardEdges[0] = new CHEdgeData(forwardContractedId, true, true, forwardWeight);
                                    //_target.AddEdge(xEdge.Neighbour, yEdge.Neighbour, new CHEdgeData(forwardContractedId, true, true, forwardWeight));
                                    backwardEdges[0] = new CHEdgeData(backwardContractedId, true, true, backwardWeight);
                                    //_target.AddEdge(yEdge.Neighbour, xEdge.Neighbour, new CHEdgeData(backwardContractedId, true, true, backwardWeight));
                                }
                                else if (canMoveBackward && canMoveForward)
                                { // add two different edges.
                                    forwardEdges[0] = new CHEdgeData(forwardContractedId, true, false, forwardWeight);
                                    //_target.AddEdge(xEdge.Neighbour, yEdge.Neighbour, new CHEdgeData(forwardContractedId, true, false, forwardWeight));
                                    backwardEdges[0] = new CHEdgeData(forwardContractedId, false, true, forwardWeight);
                                    //_target.AddEdge(yEdge.Neighbour, xEdge.Neighbour, new CHEdgeData(forwardContractedId, false, true, forwardWeight));
                                    forwardEdges[1] = new CHEdgeData(backwardContractedId, false, true, backwardWeight);
                                    //_target.AddEdge(xEdge.Neighbour, yEdge.Neighbour, new CHEdgeData(backwardContractedId, false, true, backwardWeight));
                                    backwardEdges[1] = new CHEdgeData(backwardContractedId, true, false, backwardWeight);
                                    //_target.AddEdge(yEdge.Neighbour, xEdge.Neighbour, new CHEdgeData(backwardContractedId, true, false, backwardWeight));
                                }
                                else if (canMoveForward)
                                { // only add one forward edge.
                                    forwardEdges[0] = new CHEdgeData(forwardContractedId, true, false, forwardWeight);
                                    //_target.AddEdge(xEdge.Neighbour, yEdge.Neighbour, new CHEdgeData(forwardContractedId, true, false, forwardWeight));
                                    backwardEdges[0] = new CHEdgeData(forwardContractedId, false, true, forwardWeight);
                                    //_target.AddEdge(yEdge.Neighbour, xEdge.Neighbour, new CHEdgeData(forwardContractedId, false, true, forwardWeight));
                                }
                                else if (canMoveBackward)
                                { // only add one backward edge.
                                    forwardEdges[0] = new CHEdgeData(backwardContractedId, false, true, backwardWeight);
                                    //_target.AddEdge(xEdge.Neighbour, yEdge.Neighbour, new CHEdgeData(backwardContractedId, false, true, backwardWeight));
                                    backwardEdges[0] = new CHEdgeData(backwardContractedId, true, false, backwardWeight);
                                    //_target.AddEdge(yEdge.Neighbour, xEdge.Neighbour, new CHEdgeData(backwardContractedId, true, false, backwardWeight));
                                }

                                // remove all existing stuff.
                                foreach (var existingEdgeToRemove in existingEdgesToRemove)
                                {
                                    if (forwardEdges[0].Equals(existingEdgeToRemove))
                                    { // this forward edge is to be kept.
                                        forwardEdges[0] = null; // it's already there.
                                    }
                                    else if (forwardEdges[1] != null &&
                                        !forwardEdges[1].Equals(existingEdgeToRemove))
                                    { // this forward edge is to be kept.
                                        forwardEdges[1] = null; // it's already there.
                                    }
                                    else
                                    { // yup, just remove it now.
                                        removedEdges++;
                                    }
                                    var existingEdgeToRemoveBackward = (CHEdgeData)existingEdgeToRemove.Reverse();
                                    if (backwardEdges[0].Equals(existingEdgeToRemoveBackward))
                                    { // this backward edge is to be kept.
                                        backwardEdges[0] = null; // it's already there.
                                    }
                                    else if (backwardEdges[1] != null &&
                                        !backwardEdges[1].Equals(existingEdgeToRemoveBackward))
                                    { // this backward edge is to be kept.
                                        backwardEdges[1] = null; // it's already there.
                                    }
                                    else
                                    { // yup, just remove it now.
                                        removedEdges++;
                                    }
                                }

                                // add remaining edges.
                                if (forwardEdges[0].HasValue) { newEdges++; }
                                if (forwardEdges[1].HasValue) { newEdges++; }
                                if (backwardEdges[0].HasValue) { newEdges++; }
                                if (backwardEdges[1].HasValue) { newEdges++; }
                            }
                            else
                            { // there is no edge, just add the data.
                                newEdges = newEdges + 2;
                            }
                        }
                    }
                }
            }

            // get the depth.
            _depth.TryGetValue(vertex, out depth);
            return 1 * (newEdges - removedEdges) + (2 * depth) + (1 * contracted);
        }

        /// <summary>
        /// Notifies this calculator that the vertex was contracted.
        /// </summary>
        /// <param name="vertex"></param>
        public void NotifyContracted(uint vertex)
        {
            // removes the contractions count.
            _contractionCount.Remove(vertex);

            // loop over all neighbours.
            var neighbours = _data.GetEdges(vertex);
            foreach (var neighbour in neighbours)
            {
                int count;
                if (!_contractionCount.TryGetValue(neighbour.Neighbour, out count))
                {
                    _contractionCount[neighbour.Neighbour] = 1;
                }
                else
                {
                    _contractionCount[neighbour.Neighbour] = count++;
                }
            }

            int vertex_depth = 0;
            _depth.TryGetValue(vertex, out vertex_depth);
            _depth.Remove(vertex);
            vertex_depth++;

            // store the depth.
            foreach (var neighbour in neighbours)
            {
                int depth = 0;
                _depth.TryGetValue(neighbour.Neighbour, out depth);
                if (vertex_depth < depth)
                {
                    // _depth[neighbour.Neighbour] = depth;
                }
                else
                {
                    _depth[neighbour.Neighbour] = vertex_depth;
                }
            }
        }
    }
}