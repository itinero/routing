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
    /// A pedestrian profile that uses all shorcuts.
    /// </summary>
    internal class PedestrianShortcuts : ProfileDefinition
    {
        internal PedestrianShortcuts(Pedestrian pedestrian)
            : base(pedestrian.UniqueName + ".Shortcuts", pedestrian.GetGetSpeed().ToUnconstrainedGetSpeed(), pedestrian.GetGetMinSpeed(),
                  pedestrian.GetCanStop(), pedestrian.GetEquals(), pedestrian.VehicleTypes, InternalGetFactor(pedestrian).ToUnconstrainedGetFactor(), ProfileMetric.TimeInSeconds)
        {

        }

        /// <summary>
        /// Gets a custom factor for the given tags. 
        /// </summary>
        private static Func<IAttributeCollection, Factor> InternalGetFactor(Pedestrian pedestrian)
        {
            var getSpeedDefault = pedestrian.GetGetSpeed();
            return (tags) =>
            {
                if (tags == null)
                {
                    return new Itinero.Profiles.Factor()
                    {
                        Direction = 0,
                        Value = 0
                    };
                }

                string shortcuts;
                if (tags.TryGetValue("shortcut", out shortcuts) &&
                    !string.IsNullOrEmpty(shortcuts))
                {
                    return new Factor()
                    {
                        Value = 1.0f,
                        Direction = 1
                    };
                }

                var speed = getSpeedDefault(tags);

                if (speed.Value == 0)
                {
                    return new Itinero.Profiles.Factor()
                    {
                        Direction = 0,
                        Value = 0
                    };
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