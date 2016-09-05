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

namespace Itinero.IO.Shape.Vehicles
{
    /// <summary>
    /// Contains extension methods related to the vehicles.
    /// </summary>
    public static class VehicleExtensions
    {
        /// <summary>
        /// Returns true if an attribute with the given key is relevant for any the profiles.
        /// </summary>
        public static bool IsRelevantForProfileAny(this Vehicle[] vehicles, string key)
        {
            for (var i = 0; i < vehicles.Length; i++)
            {
                if (vehicles[i].IsRelevantForProfile(key))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if an attribute with the given key is relevant for any the profiles.
        /// </summary>
        public static bool IsRelevantAny(this Vehicle[] vehicles, string key)
        {
            for (var i = 0; i < vehicles.Length; i++)
            {
                if (vehicles[i].IsRelevant(key))
                {
                    return true;
                }
            }
            return false;
        }
    }
}