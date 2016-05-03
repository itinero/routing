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

using Itinero.Graphs.Directed;
using System;

namespace Itinero.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// Contains extension methods for the directed dynamic graph.
    /// </summary>
    public static class DirectedDynamicGraphExtensions
    {
        /// <summary>
        /// Returns true if this edge is an original edge, not a shortcut.
        /// </summary>
        public static bool IsOriginal(this DynamicEdge edge)
        {
            if (edge.DynamicData == null || edge.DynamicData.Length < 1)
            {
                throw new ArgumentException("The given edge is not part of a contracted edge-based graph.");
            }
            return edge.DynamicData[1] == Constants.NO_VERTEX;
        }

        /// <summary>
        /// Gets the sequence at the source.
        /// </summary>
        public static uint[] GetSequence(this DynamicEdge edge)
        {
            if (edge.IsOriginal())
            {
                return new uint[] { edge.Neighbour };
            }

            if (edge.DynamicData == null || edge.DynamicData.Length < 1)
            {
                throw new ArgumentException("The given edge is not part of a contracted edge-based graph.");
            }
            if (edge.DynamicData == null || edge.DynamicData.Length < 2)
            {
                return new uint[0];
            }
            var size = edge.DynamicData[2];
            var sequence = new uint[size];
            for (var i = 0; i < size; i++)
            {
                sequence[i] = edge.DynamicData[i + 2];
            }
            return sequence;
        }

        /// <summary>
        /// Gets the sequence at the target.
        /// </summary>
        public static uint[] GetReverseSequence(this DynamicEdge edge, uint sourceVertex)
        {
            if (edge.IsOriginal())
            { // sequence is the source 
                return new uint[] { sourceVertex };
            }

            if (edge.DynamicData == null || edge.DynamicData.Length < 1)
            {
                throw new ArgumentException("The given edge is not part of a contracted edge-based graph.");
            }
            if (edge.DynamicData == null || edge.DynamicData.Length < 2)
            {
                return new uint[0];
            }
            var size = edge.DynamicData[2];
            var sequence = new uint[edge.DynamicData.Length - 2 - size];
            for (var i = 0; i < sequence.Length; i++)
            {
                sequence[i] = edge.DynamicData[size + 2];
            }
            return sequence;
        }
    }
}