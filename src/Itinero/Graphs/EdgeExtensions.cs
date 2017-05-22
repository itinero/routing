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

using Itinero.Profiles;
using Itinero.Algorithms;
using System;

namespace Itinero.Graphs
{
    /// <summary>
    /// Contains extensions related to edges, edge id's and directed edges.
    /// </summary>
    public static class EdgeExtensions
    {
        /// <summary>
        /// Returns a directed version of the edge-id. Smaller than 0 if inverted, as-is if not inverted.
        /// </summary>
        /// <remarks>
        /// The relationship between a regular edge id and a directed edge id:
        /// - 0 -> 1 forward, -1 backward.
        /// - all other id's are offset by 1 and postive when forward, negative when backward.
        /// </remarks>
        public static DirectedEdgeId DirectedEdgeId(this Graph.EdgeEnumerator enumerator)
        {
            return new DirectedEdgeId(enumerator.Id, !enumerator.DataInverted);
        }

        /// <summary>
        /// Returns a directed version of the edge-id. Smaller than 0 if inverted, as-is if not inverted.
        /// </summary>
        /// <remarks>
        /// The relationship between a regular edge id and a directed edge id:
        /// - 0 -> 1 forward, -1 backward.
        /// - all other id's are offset by 1 and postive when forward, negative when backward.
        /// </remarks>
        public static long IdDirected(this Graph.EdgeEnumerator enumerator)
        {
            if (enumerator.DataInverted)
            {
                return -(enumerator.Id + 1);
            }
            return (enumerator.Id + 1);
        }

        /// <summary>
        /// Returns a directed version of the edge-id. Smaller than 0 if inverted, as-is if not inverted.
        /// </summary>
        /// <remarks>
        /// The relationship between a regular edge id and a directed edge id:
        /// - 0 -> 1 forward, -1 backward.
        /// - all other id's are offset by 1 and postive when forward, negative when backward.
        /// </remarks>
        public static long IdDirected(this Edge edge)
        {
            if (edge.DataInverted)
            {
                return -(edge.Id + 1);
            }
            return (edge.Id + 1);
        }

        /// <summary>
        /// Moves to the given directed edge-id.
        /// </summary>
        public static void MoveToEdge(this Graph.EdgeEnumerator enumerator, long directedEdgeId)
        {
            if (directedEdgeId == 0) { throw new ArgumentOutOfRangeException("directedEdgeId"); }

            uint edgeId;
            if (directedEdgeId > 0)
            {
                edgeId = (uint)directedEdgeId - 1;
            }
            else
            {
                edgeId = (uint)((-directedEdgeId) - 1);
            }
            enumerator.MoveToEdge(edgeId);
        }

        /// <summary>
        /// Gets the target vertex.
        /// </summary>
        public static uint GetTargetVertex(this Graph.EdgeEnumerator enumerator, long directedEdgeId)
        {
            enumerator.MoveToEdge(directedEdgeId);

            if (directedEdgeId > 0)
            {
                if (!enumerator.DataInverted)
                {
                    return enumerator.To;
                }
                else
                {
                    return enumerator.From;
                }
            }
            else
            {
                if (enumerator.DataInverted)
                {
                    return enumerator.To;
                }
                else
                {
                    return enumerator.From;
                }
            }
        }

        /// <summary>
        /// Gets the source vertex.
        /// </summary>
        public static uint GetSourceVertex(this Graph.EdgeEnumerator enumerator, long directedEdgeId)
        {
            enumerator.MoveToEdge(directedEdgeId);

            if (directedEdgeId > 0)
            {
                if (!enumerator.DataInverted)
                {
                    return enumerator.From;
                }
                else
                {
                    return enumerator.To;
                }
            }
            else
            {
                if (enumerator.DataInverted)
                {
                    return enumerator.From;
                }
                else
                {
                    return enumerator.To;
                }
            }
        }

        /// <summary>
        /// Moves this enumerator to the target vertex of the given directed edge-id.
        /// </summary>
        public static void MoveToTargetVertex(this Graph.EdgeEnumerator enumerator, long directedEdgeId)
        {
            enumerator.MoveToEdge(directedEdgeId);

            if (directedEdgeId > 0)
            {
                if (!enumerator.DataInverted)
                {
                    enumerator.MoveTo(enumerator.To);
                }
                else
                {
                    enumerator.MoveTo(enumerator.From);
                }
            }
            else
            {
                if (enumerator.DataInverted)
                {
                    enumerator.MoveTo(enumerator.To);
                }
                else
                {
                    enumerator.MoveTo(enumerator.From);
                }
            }
        }

        /// <summary>
        /// Gets the weight for the given edge.
        /// </summary>
        public static float GetWeight(this Graph.EdgeEnumerator enumerator, long edge, Func<ushort, Factor> getFactor)
        {
            short direction;
            return enumerator.GetWeight(getFactor, out direction);
        }

        /// <summary>
        /// Gets the weight for the given edge.
        /// </summary>
        public static float GetWeight(this Graph.EdgeEnumerator enumerator, long directedEdgeId, Func<ushort, Factor> getFactor, out short direction)
        {
            enumerator.MoveToEdge(directedEdgeId);
            float distance;
            ushort edgeProfile;
            Data.Edges.EdgeDataSerializer.Deserialize(enumerator.Data0, out distance, out edgeProfile);
            var factor = getFactor(edgeProfile);
            direction = factor.Direction;
            return factor.Value * distance;
        }

        /// <summary>
        /// Gets the weight for the current edge.
        /// </summary>
        public static float GetWeight(this Graph.EdgeEnumerator enumerator, Func<ushort, Factor> getFactor)
        {
            short direction;
            return enumerator.GetWeight(getFactor, out direction);
        }

        /// <summary>
        /// Gets the weight for the current edge.
        /// </summary>
        public static float GetWeight(this Graph.EdgeEnumerator enumerator, Func<ushort, Factor> getFactor, out short direction)
        {
            float distance;
            ushort edgeProfile;
            Data.Edges.EdgeDataSerializer.Deserialize(enumerator.Data0, out distance, out edgeProfile);
            var factor = getFactor(edgeProfile);
            direction = factor.Direction;
            return factor.Value * distance;
        }
    }
}