// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
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

using Itinero.Graphs;
using Itinero.Profiles;
using System;

namespace Itinero.Algorithms.Default
{
    /// <summary>
    /// Extensions related to the graph.
    /// </summary>
    public static class GraphExtensions
    {
        /// <summary>
        /// Adds a new edge.
        /// </summary>
        public static uint AddEdge(this Graph graph, uint vertex1, uint vertex2, float distance, ushort profile)
        {
            return graph.AddEdge(vertex1, vertex2, Data.Edges.EdgeDataSerializer.Serialize(distance, profile));
        }

        /// <summary>
        /// Gets the weight for the given path.
        /// </summary>
        public static float GetWeight(this Graph.EdgeEnumerator enumerator, Func<ushort, Factor> getFactor, uint vertex1, uint vertex2)
        {
            if (!enumerator.MoveTo(vertex1))
            {
                throw new System.Exception("vertex1 not found.");
            }
            enumerator.MoveToTargetVertex(vertex2);
            return enumerator.GetWeight(getFactor);
        }

        /// <summary>
        /// Gets the weight for the given path.
        /// </summary>
        public static float GetWeight(this Graph.EdgeEnumerator enumerator, Func<ushort, Factor> getFactor, uint[] path)
        {
            float weight = 0;
            for (var i = 0; i < path.Length - 1; i++)
            {
                weight += enumerator.GetWeight(getFactor, path[i], path[i + 1]);
            }
            return weight;
        }

        /// <summary>
        /// Gets the weight for the given path.
        /// </summary>
        public static float GetWeight(this Graph graph, Func<ushort, Factor> getFactor, uint vertex1, uint vertex2)
        {
            return graph.GetEdgeEnumerator().GetWeight(getFactor, vertex1, vertex2);
        }

        /// <summary>
        /// Gets the weight for the given path.
        /// </summary>
        public static float GetWeight(this Graph graph, Func<ushort, Factor> getFactor, uint[] path)
        {
            return graph.GetEdgeEnumerator().GetWeight(getFactor, path);
        }
    }
}