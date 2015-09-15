// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using OsmSharp.Collections.Tags;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Profiles
{
    /// <summary>
    /// Represents a routing profile.
    /// </summary>
    public class Profile
    {
        private readonly Func<TagsCollectionBase, float> _getSpeed;
        private readonly HashSet<string> _vehicleTypes;

        /// <summary>
        /// Creates a new routing profile.
        /// </summary>
        public Profile(Func<TagsCollectionBase, float> getSpeed, 
            HashSet<string> vehicleTypes)
        {
            _getSpeed = getSpeed;
            _vehicleTypes = vehicleTypes;
        }

        /// <summary>
        /// Returns the speed this vehicle would have over a segment with the given attributes.
        /// </summary>
        /// <returns></returns>
        public virtual float Speed(TagsCollectionBase attributes)
        {
            return _getSpeed(attributes);
        }

        /// <summary>
        /// Gets the vehicle types.
        /// </summary>
        public virtual HashSet<string> VehicleType
        {
            get
            {
                return _vehicleTypes;
            }
        }
    }
}
