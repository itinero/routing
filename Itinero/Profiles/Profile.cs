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
using System;
using System.Collections.Generic;

namespace Itinero.Profiles
{
    /// <summary>
    /// Represents a routing profile.
    /// </summary>
    public class Profile
    {
        private readonly string _name;
        private readonly Func<IAttributeCollection, Speed> _getSpeed;
        private readonly Func<IAttributeCollection, Factor> _getFactor;
        private readonly Func<IAttributeCollection, bool> _canStop;
        private readonly Func<IAttributeCollection, IAttributeCollection, bool> _equals;
        private readonly Func<Speed> _minSpeed;
        private readonly List<string> _vehicleTypes;
        private readonly ProfileMetric _metric;

        /// <summary>
        /// Creates a new routing profile.
        /// </summary>
        public Profile(string name, Func<IAttributeCollection, Speed> getSpeed, Func<Speed> minSpeed, Func<IAttributeCollection, bool> canStop,
            Func<IAttributeCollection, IAttributeCollection, bool> equals, List<string> vehicleTypes, ProfileMetric metric)
        {
            if (metric == ProfileMetric.Custom)
            {
                throw new ArgumentException("Cannot set a custom metric without a getFactor function.");
            }

            _minSpeed = minSpeed;
            _getSpeed = getSpeed;
            _canStop = canStop;
            _equals = equals;
            _vehicleTypes = vehicleTypes;
            _name = name;
            _metric = metric;
            _getFactor = null;
        }

        /// <summary>
        /// Creates a new routing profile.
        /// </summary>
        public Profile(string name, Func<IAttributeCollection, Speed> getSpeed, Func<Speed> minSpeed, Func<IAttributeCollection, bool> canStop,
            Func<IAttributeCollection, IAttributeCollection, bool> equals, List<string> vehicleTypes, Func<IAttributeCollection, Factor> getFactor)
        {
            _minSpeed = minSpeed;
            _getSpeed = getSpeed;
            _canStop = canStop;
            _equals = equals;
            _vehicleTypes = vehicleTypes;
            _name = name;
            _metric = ProfileMetric.Custom;
            _getFactor = getFactor;
        }

        /// <summary>
        /// Returns the multiplication factor for profile over a segment with the given attributes.
        /// </summary>
        public virtual Factor Factor(IAttributeCollection attributes)
        {
            if (_metric == ProfileMetric.Custom)
            { // use a custom factor.
                return _getFactor(attributes);
            }

            var speed = _getSpeed(attributes);
            if (_metric == ProfileMetric.DistanceInMeters)
            { // shortest, use a constant factor but take direction from speed.
                return new Profiles.Factor()
                {
                    Direction = speed.Direction,
                    Value = 1
                };
            }
            else if (_metric == ProfileMetric.TimeInSeconds)
            { // fastest, use the speed as the factor.
                if (speed.Value == 0)
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
            else
            {
                throw new Exception(string.Format("Unknown metric used in profile: {0}", _name));
            }
        }

        /// <summary>
        /// Returns true if the vehicle represented by this profile can stop on the edge with the given attributes.
        /// </summary>
        public virtual bool CanStopOn(IAttributeCollection attributes)
        {
            return _canStop(attributes);
        }

        /// <summary>
        /// Returns the speed a vehicle with this profile would have over a segment with the given attributes.
        /// </summary>
        public virtual Speed Speed(IAttributeCollection attributes)
        {
            return _getSpeed(attributes);
        }

        /// <summary>
        /// Returns the minimum speed.
        /// </summary>
        /// <returns></returns>
        public virtual Speed MinSpeed()
        {
            return _minSpeed();
        }

        /// <summary>
        /// Returns true if the two tag collections are equal relative to this profile.
        /// </summary>
        public virtual bool Equals(IAttributeCollection edge1, IAttributeCollection edge2)
        {
            return _equals(edge1, edge2);
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
        /// Gets the metric.
        /// </summary>
        public virtual ProfileMetric Metric
        {
            get
            {
                return _metric;
            }
        }

        /// <summary>
        /// Gets the vehicle types.
        /// </summary>
        public virtual List<string> VehicleType
        {
            get
            {
                return _vehicleTypes;
            }
        }

        #region Static profile management

        private static Dictionary<string, Profile> _staticProfiles = 
            new Dictionary<string,Profile>();

        /// <summary>
        /// Registers the given profile.
        /// </summary>
        public static void Register(Profile profile)
        {
            _staticProfiles[profile.Name] = profile;
            _staticProfiles[profile.Name.ToLowerInvariant()] = profile;
        }

        /// <summary>
        /// Gets all registered profiles.
        /// </summary>
        public static IEnumerable<Profile> GetAllRegistered()
        {
            return _staticProfiles.Values;
        }

        /// <summary>
        /// Tries to get a profile for the given name.
        /// </summary>
        public static bool TryGet(string name, out Profile profile)
        {
            return _staticProfiles.TryGetValue(name, out profile);
        }

        /// <summary>
        /// Gets the profile for the given name.
        /// </summary>
        public static Profile Get(string name)
        {
            Profile profile;
            if(!Profile.TryGet(name, out profile))
            {
                throw new Exception(string.Format("Profile {0} not found.", name));
            }
            return profile;
        }

        #endregion
    }
}