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
using Itinero.Profiles;

namespace Itinero.Osm.Vehicles
{
    /// <summary>
    /// A balanced bicycle profile.
    /// </summary>
    internal class BicycleBalancedProfile : Profile
    {
        private const float HIGHEST_AVOID_FACTOR = 0.8f;
        private const float AVOID_FACTOR = 0.9f;
        private const float PREFER_FACTOR = 1.1f;
        private const float HIGHEST_PREFER_FACTOR = 1.2f;

        /// <summary>
        /// Creates a new profile.
        /// </summary>
        public BicycleBalancedProfile(Vehicle parent)
            : base(parent.Name + ".balanced", ProfileMetric.Custom, parent.VehicleTypes, parent, null)
        {

        }

        /// <summary>
        /// Gets a custom factor for the given tags. 
        /// </summary>
        public override FactorAndSpeed FactorAndSpeed(IAttributeCollection attributes)
        {
            var factorAndSpeed = base.FactorAndSpeed(attributes);

            if (factorAndSpeed.Value == 0)
            {
                return factorAndSpeed;
            }

            string cycleway;
            if (attributes.TryGetValue("cycleway", out cycleway))
            {
                factorAndSpeed.Value = factorAndSpeed.Value * HIGHEST_PREFER_FACTOR;
                return factorAndSpeed;
            }

            string highwayType;
            if (attributes.TryGetValue("highway", out highwayType))
            {
                switch (highwayType)
                {
                    case "trunk":
                    case "trunk_link":
                    case "primary":
                    case "primary_link":
                    case "secondary":
                    case "secondary_link":
                        factorAndSpeed.Value = factorAndSpeed.Value * HIGHEST_AVOID_FACTOR;
                        break;
                    case "tertiary":
                    case "tertiary_link":
                        factorAndSpeed.Value = factorAndSpeed.Value * AVOID_FACTOR;
                        break;
                    case "residential":
                        break;
                    case "path":
                    case "cycleway":
                        factorAndSpeed.Value = factorAndSpeed.Value * HIGHEST_PREFER_FACTOR;
                        break;
                    case "footway":
                    case "pedestrian":
                    case "steps":
                        factorAndSpeed.Value = factorAndSpeed.Value * PREFER_FACTOR;
                        break;
                }
            }
            return factorAndSpeed;
        }
    }
}
