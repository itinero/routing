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

using Itinero.Attributes;
using Itinero.LocalGeo;
using Itinero.Profiles;

namespace Itinero.Algorithms.Shortcuts
{
    /// <summary>
    /// Represents shortcut specifications.
    /// </summary>
    public class ShortcutSpecs
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the locations.
        /// </summary>
        public Coordinate[] Locations { get; set; }

        /// <summary>
        /// Gets or sets the location meta-data.
        /// </summary>
        public IAttributeCollection[] LocationsMeta { get; set; }

        /// <summary>
        /// The minimum travel time. Below this no shortcuts will be added.
        /// </summary>
        public int MinTravelTime { get; set; }

        /// <summary>
        /// Gets or sets the time to start/stop a shortcut. For bike sharing system a measure on the time it takes to rent/leave bikes.
        /// </summary>
        public float TransferTime { get; set; }

        /// <summary>
        /// Gets or sets the max shortcut duration.
        /// </summary>
        public float MaxShortcutDuration { get; set; }

        /// <summary>
        /// Gets or sets the shortcut profile.
        /// </summary>
        public Profile Profile { get; set; }

        /// <summary>
        /// Gets or sets the transition profiles, the profiles that can be transferred to/from.
        /// </summary>
        public Profile[] TransitionProfiles { get; set; }
    }
}