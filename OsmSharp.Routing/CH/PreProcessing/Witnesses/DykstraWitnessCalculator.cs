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
using OsmSharp.Routing.CH.Routing;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Routing;
using OsmSharp.Collections.PriorityQueues;
using OsmSharp.Collections;

namespace OsmSharp.Routing.CH.Preprocessing.Witnesses
{
    /// <summary>
    /// A simple dykstra witness calculator.
    /// </summary>
    public class DykstraWitnessCalculator : INodeWitnessCalculator
    {
        /// <summary>
        /// Holds the current hop limit.
        /// </summary>
        private int _hopLimit;

        /// <summary>
        /// Creates a new witness calculator.
        /// </summary>
        public DykstraWitnessCalculator()
        {
            _hopLimit = int.MaxValue;
        }

        /// <summary>
        /// Creates a new witness calculator.
        /// </summary>
        public DykstraWitnessCalculator(int hopLimit)
        {
            _hopLimit = hopLimit;
        }

        /// <summary>
        /// Holds a reusable heap.
        /// </summary>
        private BinaryHeap<SettledVertex> _reusableHeap = new BinaryHeap<SettledVertex>();

        /// <summary>
        /// Returns true if the given vertex has a witness calculator.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="maxWeight"></param>
        /// <param name="maxSettles"></param>
        public bool Exists(GraphBase<CHEdgeData> graph, uint from, uint to, float maxWeight, int maxSettles)
        {
            return this.Exists(graph, from, to, maxWeight, maxSettles, uint.MaxValue);
        }

        /// <summary>
        /// Returns true if the given vertex has a witness calculator.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="maxWeight"></param>
        /// <param name="maxSettles"></param>
        /// <param name="toSkip"></param>
        /// <returns></returns>
        public bool Exists(GraphBase<CHEdgeData> graph, uint from, uint to, float maxWeight, int maxSettles, uint toSkip)
        {
            var tos = new List<uint>(1);
            tos.Add(to);
            var tosWeights = new List<float>(1);
            tosWeights.Add(maxWeight);
            var forwardExists = new bool[1];
            var backwardExists = new bool[1];
            this.Exists(graph, from, tos, tosWeights, maxSettles,
                ref forwardExists, ref backwardExists, toSkip);
            return forwardExists[0];
        }

        /// <summary>
        /// Calculates witnesses from on source to multiple targets at once.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="from"></param>
        /// <param name="tos"></param>
        /// <param name="tosWeights"></param>
        /// <param name="maxSettles"></param>
        /// <param name="forwardExists"></param>
        /// <param name="backwardExists"></param>
        public void Exists(GraphBase<CHEdgeData> graph, uint from, List<uint> tos, List<float> tosWeights, int maxSettles,
            ref bool[] forwardExists, ref bool[] backwardExists)
        {
            this.Exists(graph, from, tos, tosWeights, maxSettles, ref forwardExists, ref backwardExists, uint.MaxValue);
        }

        /// <summary>
        /// Calculates witnesses from on source to multiple targets at once.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="from"></param>
        /// <param name="tos"></param>
        /// <param name="tosWeights"></param>
        /// <param name="maxSettles"></param>
        /// <param name="forwardExists"></param>
        /// <param name="backwardExists"></param>
        /// <param name="toSkip"></param>
        public void Exists(GraphBase<CHEdgeData> graph, uint from, List<uint> tos, List<float> tosWeights, int maxSettles,
            ref bool[] forwardExists, ref bool[] backwardExists, uint toSkip)
        {
            int maxHops = _hopLimit;

            if (maxHops == 1)
            {
                this.ExistsOneHop(graph, from, tos, tosWeights, maxSettles, ref forwardExists, ref backwardExists);
                return;
            }

            // creates the settled list.
            var backwardSettled = new HashSet<uint>();
            var forwardSettled = new HashSet<uint>();
            var backwardToSet = new HashSet<uint>();
            var forwardToSet = new HashSet<uint>();
            float forwardMaxWeight = 0, backwardMaxWeight = 0;
            for (int idx = 0; idx < tosWeights.Count; idx++)
            {
                if (!forwardExists[idx])
                {
                    forwardToSet.Add(tos[idx]);
                    if (forwardMaxWeight < tosWeights[idx])
                    {
                        forwardMaxWeight = tosWeights[idx];
                    }
                }
                if (!backwardExists[idx])
                {
                    backwardToSet.Add(tos[idx]);
                    if (backwardMaxWeight < tosWeights[idx])
                    {
                        backwardMaxWeight = tosWeights[idx];
                    }
                }
            }
            if (forwardMaxWeight == 0 && backwardMaxWeight == 0)
            { // no need to search!
                return;
            }

            // creates the priorty queue.
            var forwardMinWeight = new Dictionary<uint, float>();
            var backwardMinWeight = new Dictionary<uint, float>();
            var heap = _reusableHeap;
            heap.Clear();
            heap.Push(new SettledVertex(from, 0, 0, forwardMaxWeight > 0, backwardMaxWeight > 0), 0);

            // keep looping until the queue is empty or the target is found!
            while (heap.Count > 0)
            { // pop the first customer.
                var current = heap.Pop();
                if (current.Hops + 1 < maxHops)
                { // the current vertex has net been settled.
                    if(current.VertexId == toSkip)
                    {
                        continue;
                    }
                    bool forwardWasSettled = forwardSettled.Contains(current.VertexId);
                    bool backwardWasSettled = backwardSettled.Contains(current.VertexId);
                    if (forwardWasSettled && backwardWasSettled)
                    {
                        continue;
                    }

                    if (current.Forward)
                    { // this is a forward settle.
                        forwardSettled.Add(current.VertexId);
                        forwardMinWeight.Remove(current.VertexId);
                        if (forwardToSet.Contains(current.VertexId))
                        {
                            int index = tos.IndexOf(current.VertexId);
                            forwardExists[index] = current.Weight <= tosWeights[index];
                            //if (forwardExists[index])
                            //{
                            forwardToSet.Remove(current.VertexId);
                            //}
                        }
                    }
                    if (current.Backward)
                    { // this is a backward settle.
                        backwardSettled.Add(current.VertexId);
                        backwardMinWeight.Remove(current.VertexId);
                        if (backwardToSet.Contains(current.VertexId))
                        {
                            int index = tos.IndexOf(current.VertexId);
                            backwardExists[index] = current.Weight <= tosWeights[index];
                            //if (backwardExists[index])
                            //{
                            backwardToSet.Remove(current.VertexId);
                            //}
                        }
                    }

                    if (forwardToSet.Count == 0 &&
                        backwardToSet.Count == 0)
                    { // there is nothing left to check.
                        break;
                    }

                    if (forwardSettled.Count >= maxSettles &&
                        backwardSettled.Count >= maxSettles)
                    { // do not continue searching.
                        break;
                    }

                    bool doForward = current.Forward && forwardToSet.Count > 0 && !forwardWasSettled;
                    bool doBackward = current.Backward && backwardToSet.Count > 0 && !backwardWasSettled;
                    if (doForward || doBackward)
                    { // get the neighbours.
                        var neighbours = graph.GetEdges(current.VertexId);
                        while (neighbours.MoveNext())
                        { // move next.
                            var edgeData = neighbours.EdgeData;
                            var neighbourWeight = current.Weight + edgeData.Weight;
                            var doNeighbourForward = doForward && edgeData.CanMoveForward && neighbourWeight <= forwardMaxWeight &&
                                !forwardSettled.Contains(neighbours.Neighbour);
                            var doNeighbourBackward = doBackward && edgeData.CanMoveBackward && neighbourWeight <= backwardMaxWeight &&
                                !backwardSettled.Contains(neighbours.Neighbour);
                            if (doNeighbourBackward || doNeighbourForward)
                            {
                                float existingWeight;
                                if (doNeighbourForward)
                                {
                                    if (forwardMinWeight.TryGetValue(neighbours.Neighbour, out existingWeight))
                                    {
                                        if(existingWeight <= neighbourWeight)
                                        {
                                            doNeighbourForward = false;
                                        }
                                        else
                                        {
                                            forwardMinWeight[neighbours.Neighbour] = neighbourWeight;
                                        }
                                    }
                                    else
                                    {
                                        forwardMinWeight[neighbours.Neighbour] = neighbourWeight;
                                    }
                                }
                                if (doNeighbourBackward)
                                {
                                    if (backwardMinWeight.TryGetValue(neighbours.Neighbour, out existingWeight))
                                    {
                                        if (existingWeight <= neighbourWeight)
                                        {
                                            doNeighbourBackward = false;
                                        }
                                        else
                                        {
                                            backwardMinWeight[neighbours.Neighbour] = neighbourWeight;
                                        }
                                    }
                                    else
                                    {
                                        backwardMinWeight[neighbours.Neighbour] = neighbourWeight;
                                    }
                                }

                                if (doNeighbourBackward || doNeighbourForward)
                                {
                                    var neighbour = new SettledVertex(neighbours.Neighbour,
                                        neighbourWeight, current.Hops + 1, doNeighbourForward, doNeighbourBackward);
                                    heap.Push(neighbour, neighbour.Weight);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculates witnesses from one source to multiple targets at once but using only one hop.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="from"></param>
        /// <param name="tos"></param>
        /// <param name="tosWeights"></param>
        /// <param name="maxSettles"></param>
        /// <param name="forwardExists"></param>
        /// <param name="backwardExists"></param>
        private void ExistsOneHop(GraphBase<CHEdgeData> graph, uint from, List<uint> tos, List<float> tosWeights, int maxSettles,
            ref bool[] forwardExists, ref bool[] backwardExists)
        {
            var toSet = new HashSet<uint>();
            float maxWeight = 0;
            for (int idx = 0; idx < tosWeights.Count; idx++)
            {
                if (!forwardExists[idx] || !backwardExists[idx])
                {
                    toSet.Add(tos[idx]);
                    if (maxWeight < tosWeights[idx])
                    {
                        maxWeight = tosWeights[idx];
                    }
                }
            }

            if (toSet.Count > 0)
            {
                var neighbours = graph.GetEdges(from);
                while (neighbours.MoveNext())
                {
                    if (toSet.Contains(neighbours.Neighbour))
                    { // ok, this is a to-edge.
                        int index = tos.IndexOf(neighbours.Neighbour);
                        toSet.Remove(neighbours.Neighbour);

                        var edgeData = neighbours.EdgeData;
                        if (edgeData.CanMoveForward &&
                            edgeData.Weight < tosWeights[index])
                        {
                            forwardExists[index] = true;
                        }
                        if (edgeData.CanMoveBackward &&
                            edgeData.Weight < tosWeights[index])
                        {
                            backwardExists[index] = true;
                        }

                        if (toSet.Count == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Represents a settled vertex.
        /// </summary>
        private class SettledVertex
        {
            /// <summary>
            /// Creates a new settled vertex.
            /// </summary>
            /// <param name="vertex"></param>
            /// <param name="weight"></param>
            /// <param name="hops"></param>
            /// <param name="forward"></param>
            /// <param name="backward"></param>
            public SettledVertex(uint vertex, float weight, uint hops, bool forward, bool backward)
            {
                this.VertexId = vertex;
                this.Weight = weight;
                this.Hops = hops;
                this.Forward = forward;
                this.Backward = backward;
            }

            /// <summary>
            /// The vertex that was settled.
            /// </summary>
            public uint VertexId { get; set; }

            /// <summary>
            /// The weight this vertex was settled at.
            /// </summary>
            public float Weight { get; set; }

            /// <summary>
            /// The hop-count of this vertex.
            /// </summary>
            public uint Hops { get; set; }

            /// <summary>
            /// Holds the forward flag.
            /// </summary>
            public bool Forward { get; set; }

            /// <summary>
            /// Holds the backward flag.
            /// </summary>
            public bool Backward { get; set; }
        }

        /// <summary>
        /// Gets or sets the hop limit.
        /// </summary>
        public int HopLimit
        {
            get
            {
                return _hopLimit;
            }
            set
            {
                _hopLimit = value;
            }
        }
    }
}
