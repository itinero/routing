// Itinero - Routing for .NET
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
    /// Represents a profile.
    /// </summary>
    public class ProfileDefinition
    {
        private readonly string _name;
        private readonly Func<IAttributeCollection, Tuple<Speed, float[]>> _getSpeed;
        private readonly Func<IAttributeCollection, Tuple<Factor, float[]>> _getFactor;
        private readonly Func<IAttributeCollection, bool> _canStop;
        private readonly Func<IAttributeCollection, IAttributeCollection, bool> _equals;
        private readonly Func<Speed> _minSpeed;
        private readonly List<string> _vehicleTypes;
        private readonly ProfileMetric _metric;

        /// <summary>
        /// Creates a new routing profile.
        /// </summary>
        public ProfileDefinition(string name, Func<IAttributeCollection, Tuple<Speed, float[]>> getSpeed, Func<Speed> minSpeed, Func<IAttributeCollection, bool> canStop,
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
        public ProfileDefinition(string name, Func<IAttributeCollection, Tuple<Speed, float[]>> getSpeed, Func<Speed> minSpeed, Func<IAttributeCollection, bool> canStop,
            Func<IAttributeCollection, IAttributeCollection, bool> equals, List<string> vehicleTypes, Func<IAttributeCollection, Tuple<Factor, float[]>> getFactor,
            ProfileMetric metric = ProfileMetric.Custom)
        {
            _minSpeed = minSpeed;
            _getSpeed = getSpeed;
            _canStop = canStop;
            _equals = equals;
            _vehicleTypes = vehicleTypes;
            _name = name;
            _metric = metric;
            _getFactor = getFactor;
        }

        /// <summary>
        /// Returns the multiplication factor for profile over a segment with the given attributes.
        /// </summary>
        public virtual Tuple<Factor, float[]> Factor(IAttributeCollection attributes)
        {
            if (_getFactor != null)
            { // use a custom factor.
                return _getFactor(attributes);
            }

            var speed = _getSpeed(attributes);
            if (_metric == ProfileMetric.DistanceInMeters)
            { // shortest, use a constant factor but take direction from speed.
                return new Tuple<Profiles.Factor, float[]>(new Profiles.Factor()
                {
                    Direction = speed.Item1.Direction,
                    Value = 1
                }, speed.Item2);
            }
            else if (_metric == ProfileMetric.TimeInSeconds)
            { // fastest, use the speed as the factor.
                if (speed.Item1.Value == 0)
                {
                    return new Tuple<Profiles.Factor, float[]>(new Factor()
                    {
                        Value = 0,
                        Direction = 0
                    }, speed.Item2);
                }
                return new Tuple<Profiles.Factor, float[]>(new Factor()
                {
                    Value = 1.0f / speed.Item1.Value,
                    Direction = speed.Item1.Direction
                }, speed.Item2);
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
        public virtual Tuple<Speed, float[]> Speed(IAttributeCollection attributes)
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

        /// <summary>
        /// Gets the default unconstrained profile for this profile definition.
        /// </summary>
        /// <returns></returns>
        public Profile Default()
        {
            return new Profile(this, null);
        }

        #region Static profile management

        private static Dictionary<string, ProfileDefinition> _staticProfiles =
            new Dictionary<string, ProfileDefinition>();

        /// <summary>
        /// Registers the given profile.
        /// </summary>
        public static void Register(ProfileDefinition profile)
        {
            _staticProfiles[profile.Name] = profile;
            _staticProfiles[profile.Name.ToLowerInvariant()] = profile;
        }

        /// <summary>
        /// Gets all registered profiles.
        /// </summary>
        public static IEnumerable<ProfileDefinition> GetAllRegistered()
        {
            return _staticProfiles.Values;
        }

        /// <summary>
        /// Tries to get a profile for the given name.
        /// </summary>
        public static bool TryGet(string name, out ProfileDefinition profile)
        {
            return _staticProfiles.TryGetValue(name, out profile);
        }

        /// <summary>
        /// Gets the profile for the given name.
        /// </summary>
        public static ProfileDefinition Get(string name)
        {
            ProfileDefinition profile;
            if (!ProfileDefinition.TryGet(name, out profile))
            {
                throw new Exception(string.Format("ProfileDefinition {0} not found.", name));
            }
            return profile;
        }

        #endregion
    }
}