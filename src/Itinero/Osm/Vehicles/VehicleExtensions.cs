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

using Itinero.Profiles;
using System.Reflection;

namespace Itinero.Osm.Vehicles
{
    /// <summary>
    /// Contains extension methods 
    /// </summary>
    public static class VehicleExtensions
    {
        /// <summary>
        /// Gets the itinero assembly to load embedded resources.
        /// </summary>
        /// <returns></returns>
        public static Assembly ItineroAssembly()
        {
#if NETFX_CORE
            return typeof(Vehicle).GetTypeInfo().Assembly;
#else
            return typeof(Vehicle).Assembly;
#endif
        }

        /// <summary>
        /// Loads a string from an embedded resource stream.
        /// </summary>
        public static string LoadEmbeddedResource(string name)
        {
            return VehicleExtensions.ItineroAssembly().GetManifestResourceStream(name).ReadToEnd();
        }
    }
}