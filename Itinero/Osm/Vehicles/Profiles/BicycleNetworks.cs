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

using Itinero.Attributes;
using Itinero.Profiles;
using System;

namespace Itinero.Osm.Vehicles.Profiles
{
    /// <summary>
    /// A cycling network based bicycle profile.
    /// </summary>
    internal class BicycleNetworks : Profile
    {
        private const float HIGHEST_AVOID_FACTOR = 0.8f;
        private const float AVOID_FACTOR = 0.9f;
        private const float PREFER_FACTOR = 1.1f;
        private const float HIGHEST_PREFER_FACTOR = 1.2f;
        private const float CYCLE_NETWORK_PREFER_FACTOR = 40f; // force cyclists over the cycle network.

        internal BicycleNetworks(Bicycle bicycle)
            : base(bicycle.UniqueName + ".Networks", bicycle.GetGetSpeed(), bicycle.GetGetMinSpeed(),
                  bicycle.GetCanStop(), bicycle.GetEquals(), bicycle.VehicleTypes, InternalGetFactor(bicycle))
        {

        }

        /// <summary>
        /// Gets a custom factor for the given tags. 
        /// </summary>
        private static Func<IAttributeCollection, Factor> InternalGetFactor(Bicycle bicycle)
        {
            // adjusts to a hypothetical speed indicating preference.

            var getFactorDefault = bicycle.GetGetFactor();
            var getSpeedDefault = bicycle.GetGetSpeed();
            return (tags) =>
            {
                var speed = getSpeedDefault(tags);
                if (speed.Value == 0)
                {
                    return new Factor()
                    {
                        Value = 0,
                        Direction = 0
                    };
                }

                var highwayType = string.Empty;
                foreach (var tag in tags)
                {
                    if (tag.Key.StartsWith("cyclenetwork"))
                    { // use for all cycle networks the best factor.
                        // THIS IS THE PLACE TO IMPLEMENT NETWORK-SPECIFIC WEIGHTS.
                        speed.Value = speed.Value * CYCLE_NETWORK_PREFER_FACTOR;
                        return new Factor()
                        {
                            Value = 1.0f / speed.Value,
                            Direction = speed.Direction
                        };
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
                            speed.Value = speed.Value * HIGHEST_AVOID_FACTOR;
                            break;
                        case "tertiary":
                        case "tertiary_link":
                            speed.Value = speed.Value * AVOID_FACTOR;
                            break;
                        case "residential":
                            speed.Value = speed.Value * PREFER_FACTOR;
                            break;
                        case "path":
                        case "footway":
                        case "cycleway":
                        case "pedestrian":
                        case "steps":
                            speed.Value = speed.Value * HIGHEST_PREFER_FACTOR;
                            break;
                    }
                }
                return new Factor()
                {
                    Value = 1.0f / speed.Value,
                    Direction = speed.Direction
                };
            };
        }
    }
}