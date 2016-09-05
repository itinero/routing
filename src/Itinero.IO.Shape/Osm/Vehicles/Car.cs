// Itinero - Routing for .NET
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

using Itinero.IO.Shape.Vehicles;

namespace Itinero.IO.Shape.Osm.Vehicles
{
    /// <summary>
    /// A vehicle profile for OSM-based shapefiles.
    /// </summary>
    public class Car : Vehicle
    {
        /// <summary>
        /// Returns true if an attribute with the given key is relevant for the profile.
        /// </summary>
        public override bool IsRelevantForProfile(string key)
        {
            return key == "highway" || key == "oneway";
        }
    }
}