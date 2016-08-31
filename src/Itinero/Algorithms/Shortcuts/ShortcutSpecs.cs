// Itinero - OpenStreetMap (OSM) SDK
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
        /// Gets or sets the shortcut profile.
        /// </summary>
        public Profile Profile { get; set; }

        /// <summary>
        /// Gets or sets the transition profiles, the profiles that can be transferred to/from.
        /// </summary>
        public Profile[] TransitionProfiles { get; set; }
    }
}