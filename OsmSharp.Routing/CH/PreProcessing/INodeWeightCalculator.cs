// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
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

namespace OsmSharp.Routing.CH.Preprocessing
{
    /// <summary>
    /// A weight calculator for the node ordering.
    /// </summary>
    public interface INodeWeightCalculator
    {
        /// <summary>
        /// Calculates the priority of the given vertex.
        /// </summary>
        /// <param name="vertex">The vertex to calculate the priority for.</param>
        float Calculate(uint vertex);

        /// <summary>
        /// Calculates the priority of the given vertex.
        /// </summary>
        /// <param name="vertex">The vertex to calculate the priority for.</param>
        /// <param name="newEdges">The number of new edges that would be added.</param>
        /// <param name="removedEdges">The number of edges that would be removed.</param>
        /// <param name="depth">The depth of the vertex.</param>
        /// <param name="contracted">The number of contracted neighours.</param>
        float Calculate(uint vertex, out int newEdges, out int removedEdges, out int depth, out int contracted);

        /// <summary>
        /// Notifies this calculator that the vertex was contracted.
        /// </summary>
        /// <param name="vertex"></param>
        void NotifyContracted(uint vertex);
    }
}
