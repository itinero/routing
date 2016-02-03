// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using OsmSharp.Collections.Tags;
using OsmSharp.Routing.Profiles;
using System;

namespace OsmSharp.Routing.Osm.Vehicles.Profiles
{
    /// <summary>
    /// A balanced bicycle profile.
    /// </summary>
    internal class BicycleBalanced : Profile
    {
        private const float HIGHEST_AVOID_FACTOR = 0.5f;
        private const float AVOID_FACTOR = 0.85f;
        private const float PREFER_FACTOR = 1.15f;
        private const float HIGHEST_PREFER_FACTOR = 2f;

        internal BicycleBalanced(Bicycle bicycle)
            : base(bicycle.UniqueName + ".Balanced", bicycle.GetGetSpeed(), bicycle.GetGetMinSpeed(), 
                  bicycle.GetCanStop(), bicycle.GetEquals(), bicycle.VehicleTypes, InternalGetFactor(bicycle))
        {

        }

        /// <summary>
        /// Gets a custom factor for the given tags. 
        /// </summary>
        private static Func<TagsCollectionBase, Factor> InternalGetFactor(Bicycle bicycle)
        {
            // adjusts to a hypothetical speed indicating preference.

            var getFactorDefault = bicycle.GetGetFactor();
            var getSpeedDefault = bicycle.GetGetSpeed();
            return (tags) =>
            {
                var speed = getSpeedDefault(tags);
                if (speed.Value == 0)
                {
                    return new Routing.Profiles.Factor()
                    {
                        Value = 0,
                        Direction = 0
                    };
                }

                string highwayType;
                if (tags.TryGetValue("highway", out highwayType))
                {
                    switch(highwayType)
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
                return new Routing.Profiles.Factor()
                {
                    Value = 1.0f / speed.Value,
                    Direction = speed.Direction
                };
            };
        }
    }
}