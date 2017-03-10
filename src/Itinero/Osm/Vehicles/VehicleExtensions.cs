/*
 *  Licensed to SharpSoftware under one or more contributor
 *  license agreements. See the NOTICE file distributed with this work for 
 *  additional information regarding copyright ownership.
 * 
 *  SharpSoftware licenses this file to you under the Apache License, 
 *  Version 2.0 (the "License"); you may not use this file except in 
 *  compliance with the License. You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

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