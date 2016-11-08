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
    /// A cycling network based bicycle profile.
    /// </summary>
    internal class BicycleNetworksProfile : Profile
    {
        private const float HIGHEST_AVOID_FACTOR = 0.8f;
        private const float AVOID_FACTOR = 0.9f;
        private const float PREFER_FACTOR = 1.1f;
        private const float HIGHEST_PREFER_FACTOR = 1.2f;
        private const float CYCLE_NETWORK_PREFER_FACTOR = 5f; // force cyclists over the cycle network.

        /// <summary>
        /// Creates a new profile.
        /// </summary>
        public BicycleNetworksProfile(Vehicle parent)
            : base(parent.Name + ".networks", ProfileMetric.Custom, parent.VehicleTypes, null, parent, null)
        {

        }

        /// <summary>
        /// Get a function to calculate properties for a set given edge attributes.
        /// </summary>
        /// <returns></returns>
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

            var highwayType = string.Empty;
            foreach (var tag in attributes)
            {
                if (tag.Key.StartsWith("cyclenetwork"))
                { // use for all cycle networks the best factor.
                  // THIS IS THE PLACE TO IMPLEMENT NETWORK-SPECIFIC WEIGHTS.
                    factorAndSpeed.Value = factorAndSpeed.Value * CYCLE_NETWORK_PREFER_FACTOR;
                    return factorAndSpeed;
                }

                if (tag.Key == "highway")
                {
                    highwayType = tag.Value;
                }
            }

            if (!string.IsNullOrWhiteSpace(highwayType))
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
                        factorAndSpeed.Value = factorAndSpeed.Value * PREFER_FACTOR;
                        break;
                    case "path":
                    case "footway":
                    case "cycleway":
                    case "pedestrian":
                    case "steps":
                        factorAndSpeed.Value = factorAndSpeed.Value * HIGHEST_PREFER_FACTOR;
                        break;
                }
            }
            return factorAndSpeed;
        }
    }
}
