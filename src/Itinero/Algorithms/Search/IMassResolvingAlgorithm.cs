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

using Itinero.LocalGeo;
using Itinero.Profiles;
using System.Collections.Generic;

namespace Itinero.Algorithms.Search
{
    /// <summary>
    /// Abstract representation of a mass resolving algorithm.
    /// </summary>
    public interface IMassResolvingAlgorithm : IAlgorithm
    {
        /// <summary>
        /// Returns the errors indexed per location idx.
        /// </summary>
        Dictionary<int, LocationError> Errors { get; }

        /// <summary>
        /// Gets the original locations.
        /// </summary>
        Coordinate[] Locations { get; }

        /// <summary>
        /// Gets the valid router points.
        /// </summary>
        List<RouterPoint> RouterPoints { get; }
        
        /// <summary>
        /// Returns the index of the resolved point, given the original index of in the locations array.
        /// </summary>
        int ResolvedIndexOf(int locationIdx);

        /// <summary>
        /// Returns the index of the location in the original locations array, given the resolved point index..
        /// </summary>
        int LocationIndexOf(int resolvedIdx);

        /// <summary>
        /// Gets the router.
        /// </summary>
        RouterBase Router { get; }
    
        /// <summary>
        /// Gets the profiles.
        /// </summary>
        IProfileInstance[] Profiles { get; }
    }
}