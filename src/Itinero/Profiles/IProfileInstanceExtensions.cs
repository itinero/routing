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
using System;

namespace Itinero.Profiles
{
    /// <summary>
    /// Contains extension methods 
    /// </summary>
    public static class IProfileInstanceExtensions
    {
        /// <summary>
        /// Checks the given edge values against the contraints in the profile.
        /// </summary>
        public static bool IsConstrained(this IProfileInstance profileInstance, float[] edgeValues)
        {
            if (profileInstance.Constraints == null)
            {
                return false;
            }

            if (profileInstance.Profile.ConstrainedVariables == null)
            {
                return false;
            }

            if (edgeValues == null)
            {
                return false;
            }

            for (var i = 0; i < profileInstance.Profile.ConstrainedVariables.Length && i < profileInstance.Constraints.Length; i++)
            {
                var constraint = profileInstance.Profile.ConstrainedVariables[i];
                if (constraint == null)
                {
                    continue;
                }
                var profileValue = profileInstance.Constraints[i];
                if (profileValue == constraint.DefaultValue)
                {
                    continue;
                }
                var edgeValue = edgeValues[i];
                if (edgeValue == constraint.DefaultValue)
                {
                    continue;
                }

                if (constraint.IsMax && profileValue > edgeValue)
                { // the constraint is a maximum and the profile value is larger than the edge value.
                    // the edge value for example maxweight 1.5T but vehicle weight is 2T.
                    return true;
                }
                else if (!constraint.IsMax && profileValue < edgeValue)
                { // the constraint is a minimum and the profile value is smaller than the edge value.
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a the factor and speed for an edge with the given attributes.
        /// </summary>
        public static FactorAndSpeed FactorAndSpeed(this IProfileInstance profileInstance, IAttributeCollection attributes)
        {
            var factorAndSpeed = profileInstance.Profile.FactorAndSpeed(attributes);
            if (profileInstance.IsConstrained(factorAndSpeed.Constraints))
            {
                return Profiles.FactorAndSpeed.NoFactor;
            }
            return factorAndSpeed;
        }

        /// <summary>
        /// Gets a function that gets a factor and speed for an edge with the given attributes.
        /// </summary>
        public static Func<ushort, FactorAndSpeed> GetGetFactorAndSpeed(this IProfileInstance profileInstance, RouterDb routerDb)
        {
            return (profileId) =>
            {
                var profile = routerDb.EdgeProfiles.Get(profileId);
                return profileInstance.FactorAndSpeed(profile);
            };
        }

        /// <summary>
        /// Gets a the factor for an edge with the given attributes.
        /// </summary>
        public static Factor Factor(this IProfileInstance profileInstance, IAttributeCollection attributes)
        {
            return profileInstance.FactorAndSpeed(attributes).ToFactor();
        }

        /// <summary>
        /// Gets a function that gets a factor for an edge with the given attributes.
        /// </summary>
        public static Func<ushort, Factor> GetGetFactor(this IProfileInstance profileInstance, RouterDb routerDb)
        {
            return (profileId) =>
            {
                var profile = routerDb.EdgeProfiles.Get(profileId);
                return profileInstance.Factor(profile);
            };
        }
    }
}