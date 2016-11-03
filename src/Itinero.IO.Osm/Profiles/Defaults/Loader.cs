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

using System.IO;
using System.Reflection;
using System.Resources;

namespace Itinero.IO.Osm.Profiles.Defaults
{
    /// <summary>
    /// A loader loading embedded profiles.
    /// </summary>
    static class Loader
    {
        private static string PATH = "Itinero.IO.Osm.Profiles.Defaults.";

        /// <summary>
        /// Loads the given profile from embedded resources.
        /// </summary>
        public static string Load(string name)
        {
#if NETSTANDARD
            var assembly = typeof(Car).GetTypeInfo().Assembly;
#else
            var assembly = typeof(Car).Assembly;
#endif
            using (var stream = assembly.GetManifestResourceStream(PATH + name))
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}