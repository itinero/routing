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

using Itinero.LocalGeo.Elevation;
using System;

namespace Itinero.Elevation
{
    /// <summary>
    /// Contains extension methods related to elevation for the routerdb.
    /// </summary>
    public static class RouterDbExtensions
    {
        /// <summary>
        /// Adds elevation to the given routerdb using the default elevation provider.
        /// </summary>
        public static void AddElevation(this RouterDb routerDb)
        {
            if (routerDb == null) { throw new ArgumentNullException(nameof(routerDb)); }
            if (ElevationHandler.GetElevation == null)
            {
                throw new Exception("Cannot add elevation, there is no default elevation provider set.");
            }

            routerDb.AddElevation(ElevationHandler.GetElevation);
        }

        /// <summary>
        /// Adds elevation to the given routerdb using the given elevation provider.
        /// </summary>
        public static void AddElevation(this RouterDb routerDb, ElevationHandler.GetElevationDelegate getElevationFunc)
        {
            routerDb.Network.GeometricGraph.AddElevation(getElevationFunc);
        }
    }
}