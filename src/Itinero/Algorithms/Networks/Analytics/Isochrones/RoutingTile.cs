// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Paul Den Dulk, Abelshausen Ben
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

namespace Itinero.Algorithms.Networks.Analytics.Isochrones
{
    /// <summary>
    /// Represents a data point in the tile based isochrone builder.
    /// </summary>
    public struct RoutingTile
    {
        /// <summary>
        /// The sample weight.
        /// </summary>
        public double Weight { get; set; }

        /// <summary>
        /// The number of samples.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The tile this data is for.
        /// </summary>
        public TileIndex Index { get; set; }
    }
}
