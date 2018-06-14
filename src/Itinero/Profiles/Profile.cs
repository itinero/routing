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
using Itinero.Navigation.Instructions;
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

        /// <summary>
        /// Gets the instruction generator for this profile.
        /// </summary>
        public virtual IUnimodalInstructionGenerator InstructionGenerator
        {
            get
            {
                throw new NotImplementedException("No default instruction generators implement, use dynamic lua profiles or override InstructionGeneration getter.");
            }
        }

        private static Dictionary<string, Profile> _profiles = new Dictionary<string, Profile>();

        /// <summary>
        /// Registers a profile.
        /// </summary>
        [Obsolete]
        public static void Register(Profile profile)
        {
            _profiles[profile.FullName] = profile;
        }

        /// <summary>
        /// Gets a registered profiles.
        /// </summary>
        [Obsolete]
        public static Profile GetRegistered(string name)
        {
            return _profiles[name.ToLowerInvariant()];
        }

        /// <summary>
        /// Clears all registered profiles.
        /// </summary>
        [Obsolete]
        public static void ClearRegistered()
        {
            _profiles.Clear();
        }

        /// <summary>
        /// Tries to get a registred profile.
        /// </summary>
        [Obsolete]
        public static bool TryGet(string name, out Profile value)
        {
            return _profiles.TryGetValue(name.ToLowerInvariant(), out value);
        }

        /// <summary>
        /// Gets all registered profiles.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public static IEnumerable<Profile> GetRegistered()
        {
            return _profiles.Values;
        }

        /// <summary>
        /// Gets a description of this profile.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.FullName;
        }
    }
}