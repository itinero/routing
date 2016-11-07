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

using Itinero.Profiles;
using Itinero.Attributes;

namespace Itinero.Osm.Vehicles
{
    /// <summary>
    /// A profile that uses all shorcuts.
    /// </summary>
    internal class PedestrianShortcutsProfile : Profile
    {
        internal PedestrianShortcutsProfile(Pedestrian pedestrian)
            : base(pedestrian.Name + ".shortcuts", ProfileMetric.Custom, pedestrian.VehicleTypes, pedestrian, null)
        {

        }

        /// <summary>
        /// Get a function to calculate properties for a set given edge attributes.
        /// </summary>
        /// <returns></returns>
        public override FactorAndSpeed FactorAndSpeed(IAttributeCollection attributes)
        {
            if (attributes == null)
            {
                return Profiles.FactorAndSpeed.NoFactor;
            }

            string shortcuts;
            if (attributes.TryGetValue("shortcut", out shortcuts) &&
                !string.IsNullOrEmpty(shortcuts))
            {
                return new Profiles.FactorAndSpeed()
                {
                    Constraints = null,
                    Direction = 0,
                    SpeedFactor = 1,
                    Value = 1
                };
            }

            return base.FactorAndSpeed(attributes);
        }
    }
}