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
    public class Profile : IProfileInstance
    {
        private readonly string _name;
        private readonly ProfileMetric _metric;
        private readonly string[] _vehicleTypes;
        private readonly Vehicle _parent;
        private readonly Func<IAttributeCollection, FactorAndSpeed> _custom;
        private readonly Constraint[] _constrainedVariables;

        /// <summary>
        /// Creates a new profile.
        /// </summary>
        public Profile(string name, ProfileMetric metric, string[] vehicleTypes, Constraint[] constrainedVariables, Vehicle parent)
            : this(name, metric, vehicleTypes, constrainedVariables, parent, null)
        {

        }

        /// <summary>
        /// Creates a new profile.
        /// </summary>
        public Profile(string name, ProfileMetric metric, string[] vehicleTypes, Constraint[] constrainedVariables, Vehicle parent, Func<IAttributeCollection, FactorAndSpeed> custom)
        {
            _name = name;
            _metric = metric;
            _vehicleTypes = vehicleTypes;
            _constrainedVariables = constrainedVariables;
            _parent = parent;
            _custom = custom;
        }

        /// <summary>
        /// Gets the name used by this profile.
        /// </summary>
        public virtual string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the full name used by this profile.
        /// </summary>
        public virtual string FullName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_name))
                {
                    return _parent.Name;
                }
                return _parent.Name + "." + _name;
            }
        }

        /// <summary>
        /// The vehicle this profile is for.
        /// </summary>
        public Vehicle Parent
        {
            get
            {
                return _parent;
            }
        }

        /// <summary>
        /// Gets the metric used by this profile.
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
        public virtual string[] VehicleTypes
        {
            get
            {
                return _vehicleTypes;
            }
        }

        /// <summary>
        /// Gets the constrained variables.
        /// </summary>
        public virtual Constraint[] ConstrainedVariables
        {
            get
            {
                return _constrainedVariables;
            }
        }

        /// <summary>
        /// Gets the profile.
        /// </summary>
        Profile IProfileInstance.Profile
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// Gets the constraint variables.
        /// </summary>
        float[] IProfileInstance.Constraints
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Builds a constrained instance of this profile.
        /// </summary>
        public IProfileInstance BuildConstrained(float[] values)
        {
            return new ProfileInstance(this, values);
        }

        /// <summary>
        /// Get a function to calculate properties for a set given edge attributes.
        /// </summary>
        /// <returns></returns>
        public virtual FactorAndSpeed FactorAndSpeed(IAttributeCollection attributes)
        {
            FactorAndSpeed result;
            if (_custom != null)
            {
                result = _custom(attributes);
            }
            else
            {
                result = _parent.FactorAndSpeed(attributes, Whitelist.Dummy);
            }

            if (result.Value == 0)
            { // nothing to add when result is zero.
                return result;
            }

            if (_metric == ProfileMetric.TimeInSeconds)
            { // use 1/speed as factor.
                result.Value = result.SpeedFactor;
            }
            else if (_metric == ProfileMetric.DistanceInMeters)
            { // use 1 as factor.
                result.Value = 1;
            }
            return result;
        }

        /// <summary>
        /// Returns true if the two given edges are equals as far as this vehicle is concerned.
        /// </summary>
        public virtual bool Equals(IAttributeCollection attributes1, IAttributeCollection attributes2)
        {
            return _parent.Equals(attributes1, attributes2);
        }

        private static Dictionary<string, Profile> _profiles = new Dictionary<string, Profile>();

        /// <summary>
        /// Registers a profile.
        /// </summary>
        public static void Register(Profile profile)
        {
            _profiles[profile.FullName] = profile;
        }

        /// <summary>
        /// Gets a registered profiles.
        /// </summary>
        public static Profile GetRegistered(string name)
        {
            return _profiles[name.ToLowerInvariant()];
        }

        /// <summary>
        /// Tries to get a registred profile.
        /// </summary>
        public static bool TryGet(string name, out Profile value)
        {
            return _profiles.TryGetValue(name.ToLowerInvariant(), out value);
        }

        /// <summary>
        /// Gets all registered profiles.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Profile> GetRegistered()
        {
            return _profiles.Values;
        }
    }
}