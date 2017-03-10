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
    /// Contains extensions methods related to profiles.
    /// </summary>
    public static class ProfileExtensions
    {
        /// <summary>
        /// Gets the speed for the given profile on the link defined by the given attributes.
        /// </summary>
        public static Speed Speed(this Profile profile, IAttributeCollection attributes)
        {
            return profile.FactorAndSpeed(attributes).ToSpeed();
        }

        /// <summary>
        /// Converts a speed definition for the given factor and speed.
        /// </summary>
        public static Speed ToSpeed(this FactorAndSpeed factorAndSpeed)
        {
            if (factorAndSpeed.Direction >= 3)
            {
                return new Profiles.Speed()
                {
                    Direction = (short)(factorAndSpeed.Direction - 3),
                    Value = 1.0f / factorAndSpeed.SpeedFactor
                };
            }
            return new Profiles.Speed()
            {
                Direction = factorAndSpeed.Direction,
                Value = 1.0f / factorAndSpeed.SpeedFactor
            };
        }

        /// <summary>
        /// Gets a get factor function based on the given routerdb.
        /// </summary>
        public static Func<ushort, Factor> GetGetFactor(this Profile profile, RouterDb routerDb)
        {
            return (profileId) =>
            {
                var edgeProfile = routerDb.EdgeProfiles.Get(profileId);
                return profile.Factor(edgeProfile);
            };
        }

        /// <summary>
        /// Gets a get factor function based on the given routerdb.
        /// </summary>
        public static Func<ushort, FactorAndSpeed> GetGetFactorAndSpeed(this Profile profile, RouterDb routerDb)
        {
            return (profileId) =>
            {
                var edgeProfile = routerDb.EdgeProfiles.Get(profileId);
                return profile.FactorAndSpeed(edgeProfile);
            };
        }

        /// <summary>
        /// Gets the factor for the given profile on the link defined by the given attributes.
        /// </summary>
        public static Factor Factor(this Profile profile, IAttributeCollection attributes)
        {
            return profile.FactorAndSpeed(attributes).ToFactor();
        }

        /// <summary>
        /// Converts a factor definition for the given factor and speed.
        /// </summary>
        public static Factor ToFactor(this FactorAndSpeed factorAndSpeed)
        {
            if (factorAndSpeed.Direction >= 3)
            {
                return new Profiles.Factor()
                {
                    Direction = (short)(factorAndSpeed.Direction - 3),
                    Value = factorAndSpeed.Value
                };
            }
            return new Profiles.Factor()
            {
                Direction = factorAndSpeed.Direction,
                Value = factorAndSpeed.Value
            };
        }

        /// <summary>
        /// Converts a factor definition for the given factor and speed.
        /// </summary>
        public static bool CanStopOn(this FactorAndSpeed factorAndSpeed)
        {
            return factorAndSpeed.Direction < 3;
        }

        /// <summary>
        /// Returns true if the link defined by the given attributes can be stopped on.
        /// </summary>
        public static bool CanStopOn(this Profile profile, IAttributeCollection attributes)
        {
            return profile.FactorAndSpeed(attributes).Direction < 3;
        }
    }
}