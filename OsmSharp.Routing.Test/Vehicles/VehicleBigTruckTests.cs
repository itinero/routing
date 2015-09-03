// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
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

using NUnit.Framework;
using OsmSharp.Routing;
using OsmSharp.Routing.Vehicles;
using OsmSharp.Units.Speed;

namespace OsmSharp.Test.Unittests.Routing
{
    /// <summary>
    /// Contains test for the Vehicle.BigTruck class.
    /// </summary>
    [TestFixture]
    public class VehicleBigTruckTests : VehicleBaseTests
    {
        /// <summary>
        /// Tests the can traverse functionality.
        /// </summary>
        [Test]
        public void TestVehicleBigTruckCanTranverse()
        {
            var vehicle = Vehicle.BigTruck;

            // invalid highway types.
            base.TestVehicleCanTranverse(vehicle, false, "highwey", "road");

            // default highway types.
            base.TestVehicleCanTranverse(vehicle, false, "highway", "path");
            base.TestVehicleCanTranverse(vehicle, false, "highway", "footway");
            base.TestVehicleCanTranverse(vehicle, false, "highway", "pedestrian");
            base.TestVehicleCanTranverse(vehicle, false, "highway", "cycleway");

            base.TestVehicleCanTranverse(vehicle, true, "highway", "road");
            base.TestVehicleCanTranverse(vehicle, true, "highway", "living_street");
            base.TestVehicleCanTranverse(vehicle, true, "highway", "residential");
            base.TestVehicleCanTranverse(vehicle, true, "highway", "unclassified");
            base.TestVehicleCanTranverse(vehicle, true, "highway", "secondary");
            base.TestVehicleCanTranverse(vehicle, true, "highway", "secondary_link");
            base.TestVehicleCanTranverse(vehicle, true, "highway", "primary");
            base.TestVehicleCanTranverse(vehicle, true, "highway", "primary_link");
            base.TestVehicleCanTranverse(vehicle, true, "highway", "tertiary");
            base.TestVehicleCanTranverse(vehicle, true, "highway", "tertiary_link");
            base.TestVehicleCanTranverse(vehicle, true, "highway", "trunk");
            base.TestVehicleCanTranverse(vehicle, true, "highway", "trunk_link");
            base.TestVehicleCanTranverse(vehicle, true, "highway", "motorway");
            base.TestVehicleCanTranverse(vehicle, true, "highway", "motorway_link");

            // designated roads.
            base.TestVehicleCanTranverse(vehicle, true, "highway", "unclassified", "foot", "designated");
            base.TestVehicleCanTranverse(vehicle, true, "highway", "unclassified", "foot", "yes");
            base.TestVehicleCanTranverse(vehicle, true, "highway", "unclassified", "foot", "no");
            base.TestVehicleCanTranverse(vehicle, true, "highway", "unclassified", "bicycle", "designated");
            base.TestVehicleCanTranverse(vehicle, true, "highway", "unclassified", "bicycle", "yes");
            base.TestVehicleCanTranverse(vehicle, true, "highway", "unclassified", "bicycle", "no");
        }

        /// <summary>
        /// Tests the max speed functionality.
        /// </summary>
        [Test]
        public void TestVehicleBigTruckMaxSpeed()
        {
            Vehicle vehicle = Vehicle.BigTruck;

            //base.TextMaxSpeed(Vehicle.Car, 5, "highway", "pedestrian");

            base.TextMaxSpeed(vehicle, 5, "highway", "living_street");
            base.TextMaxSpeed(vehicle, 30, "highway", "road");
            base.TextMaxSpeed(vehicle, 30, "highway", "track");
            base.TextMaxSpeed(vehicle, 50, "highway", "unclassified");
            base.TextMaxSpeed(vehicle, 70, "highway", "tertiary");
            base.TextMaxSpeed(vehicle, 70, "highway", "tertiary_link");
            base.TextMaxSpeed(vehicle, 70, "highway", "secondary");
            base.TextMaxSpeed(vehicle, 70, "highway", "secondary_link");
            base.TextMaxSpeed(vehicle, 90, "highway", "trunk");
            base.TextMaxSpeed(vehicle, 90, "highway", "trunk_link");
            base.TextMaxSpeed(vehicle, 90, "highway", "primary");
            base.TextMaxSpeed(vehicle, 90, "highway", "primary_link");

            base.TextMaxSpeed(vehicle, 120, "highway", "motorway");
            base.TextMaxSpeed(vehicle, 120, "highway", "motorway_link");

            base.TextMaxSpeed(vehicle, 30, "highway", "primary", "maxspeed", "30");
            base.TextMaxSpeed(vehicle, ((KilometerPerHour)(MilesPerHour)20).Value, "highway", "primary", "maxspeed", "20 mph");
        }

        /// <summary>
        /// Tests the probable speed functionality.
        /// </summary>
        [Test]
        public void TestVehicleBigTruckProbableSpeed()
        {
            Vehicle vehicle = Vehicle.BigTruck;
            double max = 200;

            //base.TextMaxSpeed(Vehicle.Car, 5, "highway", "pedestrian");

            base.TextProbableSpeed(vehicle, System.Math.Min(5, max), "highway", "living_street");
            base.TextProbableSpeed(vehicle, System.Math.Min(30, max), "highway", "road");
            base.TextProbableSpeed(vehicle, System.Math.Min(30, max), "highway", "track");
            base.TextProbableSpeed(vehicle, System.Math.Min(50, max), "highway", "unclassified");
            base.TextProbableSpeed(vehicle, System.Math.Min(70, max), "highway", "tertiary");
            base.TextProbableSpeed(vehicle, System.Math.Min(70, max), "highway", "tertiary_link");
            base.TextProbableSpeed(vehicle, System.Math.Min(70, max), "highway", "secondary");
            base.TextProbableSpeed(vehicle, System.Math.Min(70, max), "highway", "secondary_link");
            base.TextProbableSpeed(vehicle, System.Math.Min(90, max), "highway", "trunk");
            base.TextProbableSpeed(vehicle, System.Math.Min(90, max), "highway", "trunk_link");
            base.TextProbableSpeed(vehicle, System.Math.Min(90, max), "highway", "primary");
            base.TextProbableSpeed(vehicle, System.Math.Min(90, max), "highway", "primary_link");

            base.TextProbableSpeed(vehicle, System.Math.Min(120, max), "highway", "motorway");
            base.TextProbableSpeed(vehicle, System.Math.Min(120, max), "highway", "motorway_link");

            base.TextProbableSpeed(vehicle, System.Math.Min(30, max), "highway", "primary", "maxspeed", "30");
            base.TextProbableSpeed(vehicle, System.Math.Min(((KilometerPerHour)(MilesPerHour)20).Value, max), "highway", "primary", "maxspeed", "20 mph");
        }
    }
}