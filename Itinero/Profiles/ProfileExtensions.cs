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
    }
}