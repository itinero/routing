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
        private readonly string _name;
        private readonly Func<TagsCollectionBase, Speed> _getSpeed;
        private readonly Func<TagsCollectionBase, bool> _canStop;
        private readonly HashSet<string> _vehicleTypes;

        /// <summary>
        /// Creates a new routing profile.
        /// </summary>
        public Profile(string name, Func<TagsCollectionBase, Speed> getSpeed, Func<TagsCollectionBase, bool> canStop,
            HashSet<string> vehicleTypes)
        {
            _getSpeed = getSpeed;
            _canStop = canStop;
            _vehicleTypes = vehicleTypes;
            _name = name;
        }

        /// <summary>
        /// Returns the multiplication factor for profile over a segment with the given attributes.
        /// </summary>
        /// <returns></returns>
        public virtual Factor Factor(TagsCollectionBase attributes)
        {
            var speed = _getSpeed(attributes);
            if(speed.Value == 0)
            {
                return new Factor()
                {
                    Value = 0,
                    Direction = 0
                };
            }
            return new Factor()
            {
                Value = 1.0f / speed.Value,
                Direction = speed.Direction
            };
        }

        /// <summary>
        /// Returns true if the vehicle represented by this profile can stop on the edge with the given attributes.
        /// </summary>
        /// <returns></returns>
        public virtual bool CanStopOn(TagsCollectionBase attributes)
        {
            return _canStop(attributes);
        }

        /// <summary>
        /// Returns the speed a vehicle with this profile would have over a segment with the given attributes.
        /// </summary>
        /// <returns></returns>
        public virtual Speed Speed(TagsCollectionBase attributes)
        {
            return _getSpeed(attributes);
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public virtual string Name
        {
            get
            {
                return _name;
            }
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
