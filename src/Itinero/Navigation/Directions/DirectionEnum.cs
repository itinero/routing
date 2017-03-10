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

namespace Itinero.Navigation.Directions
{
    /// <summary>
    /// Direction types.
    /// </summary>
    public enum DirectionEnum
    {
        /// <summary>
        /// North.
        /// </summary>
        North = 0,
        /// <summary>
        /// Northeast.
        /// </summary>
        NorthEast = 45,
        /// <summary>
        /// East.
        /// </summary>
        East = 90,
        /// <summary>
        /// Southeast.
        /// </summary>
        SouthEast = 135,
        /// <summary>
        /// South.
        /// </summary>
        South = 180,
        /// <summary>
        /// Southwest.
        /// </summary>
        SouthWest = 225,
        /// <summary>
        /// West.
        /// </summary>
        West = 270,
        /// <summary>
        /// Northwest.
        /// </summary>
        NorthWest = 315,
    }
}