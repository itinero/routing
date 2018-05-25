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

namespace Itinero.Algorithms.Networks.Preprocessing.Areas
{
    /// <summary>
    /// Abstract representation of an area.
    /// </summary>
    public interface IArea
    {
        /// <summary>
        /// Returns true if the given coordinate is inside the area.
        /// </summary>
        bool Overlaps(float latitude, float longitude);

        /// <summary>
        /// Returns the location(s) the given line intersects with the area's boundary. Returns null if there is no intersection.
        /// </summary>
        Coordinate[] Intersect(float latitude1, float longitude1, float latitude2, float longitude2);
    }
}