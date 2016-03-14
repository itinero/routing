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

using Itinero.Data.Network.Edges;
using Itinero.Graphs.Geometric.Shapes;

namespace Itinero.Data.Network
{
    /// <summary>
    /// Represents an edge in a routing network.
    /// </summary>
    public class RoutingEdge
    {
        /// <summary>
        /// Creates a new edge.
        /// </summary>
        internal RoutingEdge(uint id, uint from, uint to, EdgeData data, bool edgeDataInverted,
            ShapeBase shape)
        {
            this.Id = id;
            this.To = to;
            this.From = from;
            this.Data = data;
            this.DataInverted = edgeDataInverted;
            this.Shape = shape;
        }

        /// <summary>
        /// Creates a new edge keeping the current state of the given enumerator.
        /// </summary>
        internal RoutingEdge(RoutingNetwork.EdgeEnumerator enumerator)
        {
            this.Id = enumerator.Id;
            this.To = enumerator.To;
            this.From = enumerator.From;
            this.Data = enumerator.Data;
            this.DataInverted = enumerator.DataInverted;
            this.Shape = enumerator.Shape;
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        public uint Id { get; private set; }

        /// <summary>
        /// Gets the from.
        /// </summary>
        public uint From { get; private set; }

        /// <summary>
        /// Gets the to.
        /// </summary>
        public uint To { get; private set; }

        /// <summary>
        /// Gets the data.
        /// </summary>
        public EdgeData Data { get; private set; }

        /// <summary>
        /// Gets the inverted-flag.
        /// </summary>
        public bool DataInverted { get; set; }

        /// <summary>
        /// Gets the shape.
        /// </summary>
        public ShapeBase Shape
        {
            get;
            private set;
        }
    }
}