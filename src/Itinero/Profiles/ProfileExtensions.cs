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
        /// Gets a get factor function for the given profile.
        /// </summary>
        public static Func<ushort, Factor> GetGetFactor(this Profile profile, RouterDb routerDb)
        {
            return (e) => profile.Factor(routerDb.EdgeProfiles.Get(e));
        }

        /// <summary>
        /// Gets a get factor and speed function for the given profile.
        /// </summary>
        public static Func<ushort, FactorAndSpeed> GetGetFactorAndSpeed(this Profile profile, RouterDb routerDb)
        {
            return (e) =>
            {
                var att = routerDb.EdgeProfiles.Get(e);
                var f = profile.Factor(att);
                var s = profile.Speed(att);
                return new FactorAndSpeed()
                {
                    SpeedFactor = 1 / s.Value,
                    Value = f.Value,
                    Direction = f.Direction
                };
            };
        }

        /// <summary>
        /// Converts a regular get factor function to a get factor supporting contraints.
        /// </summary>
        public static Func<IAttributeCollection, Tuple<Factor, float[]>> ToUnconstrainedGetFactor(this Func<IAttributeCollection, Factor> getFactor)
        {
            return (attributes) =>
            {
                return new Tuple<Factor, float[]>(getFactor(attributes), null);
            };
        }

        /// <summary>
        /// Converts a regular get speed function to a get speed supporting contraints.
        /// </summary>
        public static Func<IAttributeCollection, Tuple<Speed, float[]>> ToUnconstrainedGetSpeed(this Func<IAttributeCollection, Speed> getSpeed)
        {
            return (attributes) =>
            {
                return new Tuple<Speed, float[]>(getSpeed(attributes), null);
            };
        }

        /// <summary>
        /// Gets the factor.
        /// </summary>
        public static Factor Factor(this Profile profileInstance, IAttributeCollection attributes)
        {
            var speedAndConstraints = profileInstance.Definition.Factor(attributes);
            return profileInstance.Factor(speedAndConstraints.Item1, speedAndConstraints.Item2);
        }

        /// <summary>
        /// Gets the factor.
        /// </summary>
        public static bool CanStopOn(this Profile profileInstance, IAttributeCollection attributes)
        {
            if (!profileInstance.Definition.CanStopOn(attributes))
            {
                return false;
            }
            var factor = profileInstance.Factor(attributes);
            if (factor.Value <= 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the speed.
        /// </summary>
        public static Speed Speed(this Profile profileInstance, IAttributeCollection attributes)
        {
            var speedAndConstraints = profileInstance.Definition.Speed(attributes);
            return profileInstance.Speed(speedAndConstraints.Item1, speedAndConstraints.Item2);
        }
    }
}