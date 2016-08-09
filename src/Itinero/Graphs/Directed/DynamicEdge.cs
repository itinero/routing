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

namespace Itinero.Graphs.Directed
{
    /// <summary>
    /// Abstract representation of an edge with a dynamic payload.
    /// </summary>
    public class DynamicEdge
    {
        /// <summary>
        /// Creates a new edge keeping the current state of the given enumerator.
        /// </summary>
        internal DynamicEdge(DirectedDynamicGraph.EdgeEnumerator enumerator)
        {
            this.Neighbour = enumerator.Neighbour;
            this.Data = enumerator.Data;
            this.DynamicData = enumerator.DynamicData;
            this.Id = enumerator.Id;
        }

        /// <summary>
        /// Returns the current neighbour.
        /// </summary>
        public uint Neighbour
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the edge data.
        /// </summary>
        public uint[] Data
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the edge dynamic-data.
        /// </summary>
        public uint[] DynamicData
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the id.
        /// </summary>
        public uint Id
        {
            get;
            set;
        }

        /// <summary>
        /// Returns a string representing this edge.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} -> {1}",
                this.Data.ToInvariantString(),
                this.Neighbour);
        }
    }
}