// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
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
        public static long IdDirected(this Graph.EdgeEnumerator enumerator)
        {
            if (enumerator.DataInverted)
            {
                return -(enumerator.Id + 1);
            }
            return (enumerator.Id + 1);
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
    }
}