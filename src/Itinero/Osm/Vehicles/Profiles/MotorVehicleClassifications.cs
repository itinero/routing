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
using Itinero.Osm.Vehicles;
using Itinero.Profiles;
using System;

namespace Itinero.Osm.Vehicles.Profiles
{
    /// <summary>
    /// A car profile that aggresively follows the road classifications. Motorway > Trunk > Primary > Secondary ... > Unclassified.
    /// </summary>
    internal class MotorVehicleClassifications : Profile
    {
        private static float CLASS_FACTOR = 4;
        private static float MOTORWAY = 10;
        private static float TRUNK = 9;
        private static float PRIMARY = 8;
        private static float SECONDARY = 7;
        private static float TERTIARY = 6;
        private static float RESIDENTIAL = 5;
        private static float REST = 4;

        internal MotorVehicleClassifications(MotorVehicle mv)
            : base(mv.UniqueName + ".Classifications", mv.GetGetSpeed(), mv.GetGetMinSpeed(),
                  mv.GetCanStop(), mv.GetEquals(), mv.VehicleTypes, InternalGetFactor(mv))
        {

        }

        /// <summary>
        /// Gets a custom factor for the given tags. 
        /// </summary>
        private static Func<IAttributeCollection, Factor> InternalGetFactor(MotorVehicle mv)
        {
            // adjusts to a hypothetical speed indicating preference.

            var getFactorDefault = mv.GetGetFactor();
            var getSpeedDefault = mv.GetGetSpeed();
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

                string highwayType;
                if (tags.TryGetValue("highway", out highwayType))
                {
                    switch (highwayType)
                    {
                        case "motorway":
                        case "motorway_link":
                            speed.Value = speed.Value * CLASS_FACTOR * MOTORWAY;
                            break;
                        case "trunk":
                        case "trunk_link":
                            speed.Value = speed.Value * CLASS_FACTOR * TRUNK;
                            break;
                        case "primary":
                        case "primary_link":
                            speed.Value = speed.Value * CLASS_FACTOR * PRIMARY;
                            break;
                        case "secondary":
                        case "secondary_link":
                            speed.Value = speed.Value * CLASS_FACTOR * SECONDARY;
                            break;
                        case "tertiary":
                        case "tertiary_link":
                            speed.Value = speed.Value * CLASS_FACTOR * TERTIARY;
                            break;
                        case "residential":
                            speed.Value = speed.Value * CLASS_FACTOR * RESIDENTIAL;
                            break;
                        default:
                            speed.Value = speed.Value * CLASS_FACTOR * REST;
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