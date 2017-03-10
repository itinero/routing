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

using Itinero.Algorithms.Weights;
using Itinero.Data.Contracted.Edges;
using Itinero.Graphs.Directed;
using System;
using System.Collections.Generic;

namespace Itinero.Algorithms.Contracted
{
    /// <summary>
    /// Directed graph extensions assuming it contains contracted data.
    /// </summary>
    public static class DirectedMetaGraphExtensions
    {
        /// <summary>
        /// Gets the shortest edge between two vertices.
        /// </summary>
        /// <returns></returns>
        public static MetaEdge GetShortestEdge(this DirectedMetaGraph graph, uint vertex1, uint vertex2, Func<uint[], float?> getWeight)
        {
            var minWeight = float.MaxValue;
            var edges = graph.GetEdgeEnumerator(vertex1);
            MetaEdge edge = null;
            while (edges.MoveNext())
            {
                if (edges.Neighbour == vertex2)
                { // the correct neighbour, get the weight.
                    var data = edges.Data;
                    var weight = getWeight(data);
                    if (weight.HasValue &&
                        weight.Value < minWeight)
                    { // weight is better.
                        edge = new MetaEdge()
                        {
                            Data = data,
                            Neighbour = vertex2,
                            Id = edges.Id,
                            MetaData = edges.MetaData
                        };
                        minWeight = weight.Value;
                    }
                }
            }
            return edge;
        }

        /// <summary>
        /// Gets the contracted id for the shortest edge between two vertices.
        /// </summary>
        /// <returns></returns>
        public static uint? GetShortestEdgeContractedId(this DirectedMetaGraph.EdgeEnumerator edgeEnumerator, uint vertex1, uint vertex2, bool forward)
        {
            var minWeight = float.MaxValue;
            edgeEnumerator.MoveTo(vertex1);
            uint? contractedId = null;
            while (edgeEnumerator.MoveNext())
            {
                if (edgeEnumerator.Neighbour == vertex2)
                { // the correct neighbour, get the weight.
                    float weight;
                    bool? direction;
                    ContractedEdgeDataSerializer.Deserialize(edgeEnumerator.Data0, out weight, out direction);
                    if (direction == null || direction.Value == forward)
                    {
                        if (weight < minWeight)
                        { // weight is better.
                            contractedId = edgeEnumerator.GetContractedId();
                            minWeight = weight;
                        }
                    }
                }
            }
            return contractedId;
        }

        /// <summary>
        /// Expands a the shortest edge between the two given vertices.
        /// </summary>
        public static void ExpandEdge(this DirectedMetaGraph graph, uint vertex1, uint vertex2, List<uint> vertices, bool inverted,
            bool forward)
        {
            var edgeEnumerator = graph.GetEdgeEnumerator();
            edgeEnumerator.ExpandEdge(vertex1, vertex2, vertices, inverted, forward);
        }

        /// <summary>
        /// Expands a the shortest edge between the two given vertices.
        /// </summary>
        public static void ExpandEdge(this DirectedMetaGraph.EdgeEnumerator edgeEnumerator, uint vertex1, uint vertex2, List<uint> vertices, bool inverted,
            bool forward)
        {
            // check if expansion is needed.
            var edgeContractedId = edgeEnumerator.GetShortestEdgeContractedId(vertex1, vertex2, forward);
            if(edgeContractedId == null)
            { // no edge found!
                throw new Exception(string.Format("No edge found from {0} to {1}.", vertex1, vertex2));
            }
            if (edgeContractedId != Constants.NO_VERTEX)
            { // further expansion needed.
                if (inverted)
                {
                    edgeEnumerator.ExpandEdge(edgeContractedId.Value, vertex1, vertices, false, !forward);
                    vertices.Add(edgeContractedId.Value);
                    edgeEnumerator.ExpandEdge(edgeContractedId.Value, vertex2, vertices, true, forward);
                }
                else
                {
                    edgeEnumerator.ExpandEdge(edgeContractedId.Value, vertex2, vertices, false, forward);
                    vertices.Add(edgeContractedId.Value);
                    edgeEnumerator.ExpandEdge(edgeContractedId.Value, vertex1, vertices, true, !forward);
                }
            }
        }

        /// <summary>
        /// Add edge.
        /// </summary>
        /// <returns></returns>
        public static void AddEdge(this DirectedMetaGraph graph, uint vertex1, uint vertex2, float weight, 
            bool? direction, uint contractedId)
        {
            graph.AddEdge(vertex1, vertex2, ContractedEdgeDataSerializer.Serialize(
                weight, direction), contractedId);
        }
        
        /// <summary>
        /// Add edge.
        /// </summary>
        /// <returns></returns>
        public static void AddEdge(this DirectedMetaGraph graph, uint vertex1, uint vertex2, float weight,
            bool? direction, uint contractedId, float distance, float time)
        {
            graph.AddEdge(vertex1, vertex2, new uint[] { ContractedEdgeDataSerializer.Serialize(
                weight, direction) }, ContractedEdgeDataSerializer.SerializeMetaAugmented(contractedId, distance, time));
        }

        /// <summary>
        /// Add or update edge.
        /// </summary>
        /// <returns></returns>
        public static void AddOrUpdateEdge(this DirectedMetaGraph graph, uint vertex1, uint vertex2, float weight, 
            bool? direction, uint contractedId)
        {
            var current = ContractedEdgeDataSerializer.Serialize(weight, direction);
            var hasExistingEdge = false;
            var hasExistingEdgeOnlySameDirection = true;
            if(graph.UpdateEdge(vertex1, vertex2, (data) => 
                {
                    hasExistingEdge = true;
                    if(ContractedEdgeDataSerializer.HasDirection(data[0], direction))
                    { // has the same direction.
                        if (weight < ContractedEdgeDataSerializer.DeserializeWeight(data[0]))
                        { // the weight is better, just update.
                            return true;
                        }
                        return false;
                    }
                    hasExistingEdgeOnlySameDirection = false;
                    return false;
                }, new uint[] { current }, contractedId) != Constants.NO_EDGE)
            { // updating the edge succeeded.
                return;
            }
            if (!hasExistingEdge)
            { // no edge exists yet.
                graph.AddEdge(vertex1, vertex2, current, contractedId);
                return;
            }
            else if (hasExistingEdgeOnlySameDirection)
            { // there is an edge already but it has a better weight.
                return;
            }
            else
            { // see what's there and update if needed.
                var forward = false;
                var forwardWeight = float.MaxValue;
                var forwardContractedId = uint.MaxValue;
                var backward = false;
                var backwardWeight = float.MaxValue;
                var backwardContractedId = uint.MaxValue;

                if(direction == null || direction.Value)
                {
                    forward = true;
                    forwardWeight = weight;
                    forwardContractedId = contractedId;
                }
                if(direction == null || !direction.Value)
                {
                    backward = true;
                    backwardWeight = weight;
                    backwardContractedId = contractedId;
                }

                var edgeEnumerator = graph.GetEdgeEnumerator(vertex1);
                while(edgeEnumerator.MoveNext())
                {
                    if(edgeEnumerator.Neighbour == vertex2)
                    {
                        float localWeight;
                        bool? localDirection;
                        uint localContractedId;
                        ContractedEdgeDataSerializer.Deserialize(edgeEnumerator.Data0, edgeEnumerator.MetaData0,
                            out localWeight, out localDirection, out localContractedId);
                        if(localDirection == null || localDirection.Value)
                        {
                            if(localWeight < forwardWeight)
                            {
                                forwardWeight = localWeight;
                                forward = true;
                                forwardContractedId = localContractedId;
                            }
                        }
                        if (localDirection == null || !localDirection.Value)
                        {
                            if (localWeight < backwardWeight)
                            {
                                backwardWeight = localWeight;
                                backward = true;
                                backwardContractedId = localContractedId;
                            }
                        }
                    }
                }

                graph.RemoveEdge(vertex1, vertex2);

                if(forward && backward &&
                    forwardWeight == backwardWeight &&
                    forwardContractedId == backwardContractedId)
                { // add one bidirectional edge.
                    graph.AddEdge(vertex1, vertex2,
                        ContractedEdgeDataSerializer.Serialize(forwardWeight, null), forwardContractedId);
                }
                else
                { // add two unidirectional edges if needed.
                    if(forward)
                    { // there is a forward edge.
                        graph.AddEdge(vertex1, vertex2,
                            ContractedEdgeDataSerializer.Serialize(forwardWeight, true), forwardContractedId);
                    }
                    if (backward)
                    { // there is a backward edge.
                        graph.AddEdge(vertex1, vertex2,
                            ContractedEdgeDataSerializer.Serialize(backwardWeight, false), backwardContractedId);
                    }
                }
            }
        }

        /// <summary>
        /// Add or update edge.
        /// </summary>
        /// <returns></returns>
        public static void AddOrUpdateEdge(this DirectedMetaGraph graph, uint vertex1, uint vertex2, float weight,
            bool? direction, uint contractedId, float distance, float time)
        {
            var current = ContractedEdgeDataSerializer.Serialize(weight, direction);
            var hasExistingEdge = false;
            var hasExistingEdgeOnlySameDirection = true;
            if (graph.UpdateEdge(vertex1, vertex2, (data) =>
            {
                hasExistingEdge = true;
                if (ContractedEdgeDataSerializer.HasDirection(data[0], direction))
                { // has the same direction.
                    if (weight < ContractedEdgeDataSerializer.DeserializeWeight(data[0]))
                    { // the weight is better, just update.
                        return true;
                    }
                    return false;
                }
                hasExistingEdgeOnlySameDirection = false;
                return false;
            }, new uint[] { current }, ContractedEdgeDataSerializer.SerializeMetaAugmented(contractedId, distance, time)) != Constants.NO_EDGE)
            { // updating the edge succeeded.
                return;
            }
            if (!hasExistingEdge)
            { // no edge exists yet.
                graph.AddEdge(vertex1, vertex2, new uint[] { current }, ContractedEdgeDataSerializer.SerializeMetaAugmented(
                    contractedId, distance, time));
                return;
            }
            else if (hasExistingEdgeOnlySameDirection)
            { // there is an edge already but it has a better weight.
                return;
            }
            else
            { // see what's there and update if needed.
                var forward = false;
                var forwardWeight = float.MaxValue;
                var forwardContractedId = uint.MaxValue;
                var forwardTime = float.MaxValue;
                var forwardDistance = float.MaxValue;
                var backward = false;
                var backwardWeight = float.MaxValue;
                var backwardContractedId = uint.MaxValue;
                var backwardTime = float.MaxValue;
                var backwardDistance = float.MaxValue;

                if (direction == null || direction.Value)
                {
                    forward = true;
                    forwardWeight = weight;
                    forwardContractedId = contractedId;
                    forwardTime = time;
                    forwardDistance = distance;
                }
                if (direction == null || !direction.Value)
                {
                    backward = true;
                    backwardWeight = weight;
                    backwardContractedId = contractedId;
                    backwardTime = time;
                    backwardDistance = distance;
                }

                var edgeEnumerator = graph.GetEdgeEnumerator(vertex1);
                while (edgeEnumerator.MoveNext())
                {
                    if (edgeEnumerator.Neighbour == vertex2)
                    {
                        float localWeight;
                        bool? localDirection;
                        ContractedEdgeDataSerializer.Deserialize(edgeEnumerator.Data0,
                            out localWeight, out localDirection);
                        uint localContractedId;
                        float localTime;
                        float localDistance;
                        ContractedEdgeDataSerializer.DeserializeMetaAgumented(edgeEnumerator.MetaData, out localContractedId, out localDistance, out localTime);
                        if (localDirection == null || localDirection.Value)
                        {
                            if (localWeight < forwardWeight)
                            {
                                forwardWeight = localWeight;
                                forward = true;
                                forwardContractedId = localContractedId;
                                forwardTime = localTime;
                                forwardDistance = localDistance;
                            }
                        }
                        if (localDirection == null || !localDirection.Value)
                        {
                            if (localWeight < backwardWeight)
                            {
                                backwardWeight = localWeight;
                                backward = true;
                                backwardContractedId = localContractedId;
                                backwardDistance = localDistance;
                                backwardTime = localTime;
                            }
                        }
                    }
                }

                graph.RemoveEdge(vertex1, vertex2);

                if (forward && backward &&
                    forwardWeight == backwardWeight &&
                    forwardContractedId == backwardContractedId)
                { // add one bidirectional edge.
                    graph.AddEdge(vertex1, vertex2, forwardWeight, null, forwardContractedId, forwardDistance, forwardTime);
                    //graph.AddEdge(vertex1, vertex2,
                    //    ContractedEdgeDataSerializer.Serialize(forwardWeight, null), forwardContractedId);
                }
                else
                { // add two unidirectional edges if needed.
                    if (forward)
                    { // there is a forward edge.
                        graph.AddEdge(vertex1, vertex2, forwardWeight, true, forwardContractedId, forwardDistance, forwardTime);
                        //graph.AddEdge(vertex1, vertex2,
                        //    ContractedEdgeDataSerializer.Serialize(forwardWeight, true), forwardContractedId);
                    }
                    if (backward)
                    { // there is a backward edge.
                        graph.AddEdge(vertex1, vertex2, backwardWeight, false, backwardContractedId, backwardDistance, backwardTime);
                    }
                }
            }
        }

        /// <summary>
        /// Tries adding or updating an edge and returns #added and #removed edges.
        /// </summary>
        /// <returns></returns>
        public static void TryAddOrUpdateEdge(this DirectedMetaGraph graph, uint vertex1, uint vertex2, float weight, bool? direction, uint contractedId,
            out int added, out int removed)
        {
            var hasExistingEdge = false;
            var hasExistingEdgeOnlySameDirection = true;
            var edgeCount = 0;
            var edgeEnumerator = graph.GetEdgeEnumerator(vertex1);
            while(edgeEnumerator.MoveNext())
            {
                if (edgeEnumerator.Neighbour == vertex2)
                {
                    edgeCount++;
                    hasExistingEdge = true;
                    if (ContractedEdgeDataSerializer.HasDirection(edgeEnumerator.Data0, direction))
                    { // has the same direction.
                        if (weight < ContractedEdgeDataSerializer.DeserializeWeight(edgeEnumerator.Data0))
                        { // the weight is better, just update.
                            added = 1;
                            removed = 1;
                            return;
                        }
                        hasExistingEdgeOnlySameDirection = false;
                    }
                }
            }

            if (!hasExistingEdge)
            { // no edge exists yet.
                added = 1;
                removed = 0;
                return;
            }
            else if (hasExistingEdgeOnlySameDirection)
            { // there is an edge already but it has a better weight.
                added = 0;
                removed = 0;
                return;
            }
            else
            { // see what's there and update if needed.
                var forward = false;
                var forwardWeight = float.MaxValue;
                var forwardContractedId = uint.MaxValue;
                var backward = false;
                var backwardWeight = float.MaxValue;
                var backwardContractedId = uint.MaxValue;

                if (direction == null || direction.Value)
                {
                    forward = true;
                    forwardWeight = weight;
                    forwardContractedId = contractedId;
                }
                if (direction == null || !direction.Value)
                {
                    backward = true;
                    backwardWeight = weight;
                    backwardContractedId = contractedId;
                }

                edgeEnumerator = graph.GetEdgeEnumerator(vertex1);
                while (edgeEnumerator.MoveNext())
                {
                    if (edgeEnumerator.Neighbour == vertex2)
                    {
                        float localWeight;
                        bool? localDirection;
                        uint localContractedId;
                        ContractedEdgeDataSerializer.Deserialize(edgeEnumerator.Data0, edgeEnumerator.MetaData0,
                            out localWeight, out localDirection, out localContractedId);
                        if (localDirection == null || localDirection.Value)
                        {
                            if (localWeight < forwardWeight)
                            {
                                forwardWeight = localWeight;
                                forward = true;
                                forwardContractedId = localContractedId;
                            }
                        }
                        if (localDirection == null || !localDirection.Value)
                        {
                            if (localWeight < backwardWeight)
                            {
                                backwardWeight = localWeight;
                                backward = true;
                                backwardContractedId = localContractedId;
                            }
                        }
                    }
                }

                removed = edgeCount;
                added = 0;

                if (forward && backward &&
                    forwardWeight == backwardWeight &&
                    forwardContractedId == backwardContractedId)
                { // add one bidirectional edge.
                    added++;
                }
                else
                { // add two unidirectional edges if needed.
                    if (forward)
                    { // there is a forward edge.
                        added++;
                    }
                    if (backward)
                    { // there is a backward edge.
                        added++;
                    }
                }
            }
        }
    }
}