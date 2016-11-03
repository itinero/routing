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
            if (factorAndSpeed.Direction > 3)
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
            if (factorAndSpeed.Direction > 3)
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
        /// Returns true if the link defined by the given attributes can be stopped on.
        /// </summary>
        public static bool CanStopOn(this Profile profile, IAttributeCollection attributes)
        {
            return profile.FactorAndSpeed(attributes).Direction < 4;
        }
    }
}