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
using System;

namespace Itinero.Osm.Vehicles
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
            : base(mv.Name + ".classifications", ProfileMetric.Custom, mv.VehicleTypes, mv, null)
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
            string highwayType;
            if (attributes.TryGetValue("highway", out highwayType))
            {
                switch (highwayType)
                {
                    case "motorway":
                    case "motorway_link":
                        factorAndSpeed.Value = factorAndSpeed.Value * CLASS_FACTOR * MOTORWAY;
                        break;
                    case "trunk":
                    case "trunk_link":
                        factorAndSpeed.Value = factorAndSpeed.Value * CLASS_FACTOR * TRUNK;
                        break;
                    case "primary":
                    case "primary_link":
                        factorAndSpeed.Value = factorAndSpeed.Value * CLASS_FACTOR * PRIMARY;
                        break;
                    case "secondary":
                    case "secondary_link":
                        factorAndSpeed.Value = factorAndSpeed.Value * CLASS_FACTOR * SECONDARY;
                        break;
                    case "tertiary":
                    case "tertiary_link":
                        factorAndSpeed.Value = factorAndSpeed.Value * CLASS_FACTOR * TERTIARY;
                        break;
                    case "residential":
                        factorAndSpeed.Value = factorAndSpeed.Value * CLASS_FACTOR * RESIDENTIAL;
                        break;
                    default:
                        factorAndSpeed.Value = factorAndSpeed.Value * CLASS_FACTOR * REST;
                        break;
                }
            }
            return factorAndSpeed;
        }
    }
}