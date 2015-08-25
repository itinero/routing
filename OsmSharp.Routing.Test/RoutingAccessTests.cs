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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OsmSharp.Routing;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Graph.Routing;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Routers;
using OsmSharp.Math.Geo;
using NUnit.Framework;
using OsmSharp.Routing.Vehicles;

namespace OsmSharp.Test.Unittests.Routing
{
    /// <summary>
    /// Generic tests to test access restrictions using different vehicles.
    /// </summary>
    public abstract class RoutingAccessTests<TEdgeData>
        where TEdgeData : IGraphEdgeData
    {
        /// <summary>
        /// Builds the router;
        /// </summary>
        /// <param name="data"></param>
        /// <param name="interpreter"></param>
        /// <param name="basicRouter"></param>
        /// <returns></returns>
        public abstract Router BuildRouter(IRoutingAlgorithmData<TEdgeData> data,
            IOsmRoutingInterpreter interpreter, IRoutingAlgorithm<TEdgeData> basicRouter);

        /// <summary>
        /// Builds the basic router.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract IRoutingAlgorithm<TEdgeData> BuildBasicRouter(IRoutingAlgorithmData<TEdgeData> data);

        /// <summary>
        /// Builds the data.
        /// </summary>
        /// <returns></returns>
        public abstract IRoutingAlgorithmData<TEdgeData> BuildData(IOsmRoutingInterpreter interpreter,
            string embeddedString, Vehicle vehicle);

        /// <summary>
        /// Tests access restrictions on all different highway times.
        /// </summary>
        protected void DoAccessTestsHighways()
        {
            var interpreter = new OsmRoutingInterpreter();

            const double longitudeLeft = 4.7696568;
            const double longitudeRight = 4.8283861;

            var footwayFrom = new GeoCoordinate(51.2, longitudeLeft);
            var footwayTo = new GeoCoordinate(51.2, longitudeRight);

            var cyclewayFrom = new GeoCoordinate(51.1, longitudeLeft);
            var cyclewayTo = new GeoCoordinate(51.1, longitudeRight);

            var bridlewayFrom = new GeoCoordinate(51.0, longitudeLeft);
            var bridlewayTo = new GeoCoordinate(51.0, longitudeRight);

            var pathFrom = new GeoCoordinate(50.9, longitudeLeft);
            var pathTo = new GeoCoordinate(50.9, longitudeRight);

            var pedestrianFrom = new GeoCoordinate(50.8, longitudeLeft);
            var pedestrianTo = new GeoCoordinate(50.8, longitudeRight);

            var roadFrom = new GeoCoordinate(50.7, longitudeLeft);
            var roadTo = new GeoCoordinate(50.7, longitudeRight);

            var livingStreetFrom = new GeoCoordinate(50.6, longitudeLeft);
            var livingStreetTo = new GeoCoordinate(50.6, longitudeRight);

            var residentialFrom = new GeoCoordinate(50.5, longitudeLeft);
            var residentialTo = new GeoCoordinate(50.5, longitudeRight);

            var unclassifiedFrom = new GeoCoordinate(50.4, longitudeLeft);
            var unclassifiedTo = new GeoCoordinate(50.4, longitudeRight);

            var tertiaryFrom = new GeoCoordinate(50.3, longitudeLeft);
            var tertiaryTo = new GeoCoordinate(50.3, longitudeRight);

            var secondaryFrom = new GeoCoordinate(50.2, longitudeLeft);
            var secondaryTo = new GeoCoordinate(50.2, longitudeRight);

            var primaryFrom = new GeoCoordinate(50.1, longitudeLeft);
            var primaryTo = new GeoCoordinate(50.1, longitudeRight);

            var trunkFrom = new GeoCoordinate(50.0, longitudeLeft);
            var trunkTo = new GeoCoordinate(50.0, longitudeRight);

            var motorwayFrom = new GeoCoordinate(49.9, longitudeLeft);
            var motorwayTo = new GeoCoordinate(49.9, longitudeRight);

            // pedestrian
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Pedestrian,
                footwayFrom, footwayTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Pedestrian,
                cyclewayFrom, cyclewayTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.Pedestrian,
                bridlewayFrom, bridlewayTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Pedestrian,
                pathFrom, pathTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Pedestrian,
                pedestrianFrom, pedestrianTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Pedestrian,
                roadFrom, roadTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Pedestrian,
                livingStreetFrom, livingStreetTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Pedestrian,
                residentialFrom, residentialTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Pedestrian,
                unclassifiedFrom, unclassifiedTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Pedestrian,
                tertiaryFrom, tertiaryTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Pedestrian,
                secondaryFrom, secondaryTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Pedestrian,
                primaryFrom, primaryTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.Pedestrian,
                trunkFrom, trunkTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.Pedestrian,
                motorwayFrom, motorwayTo, interpreter));

            // bicycle
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.Bicycle,
                footwayFrom, footwayTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Bicycle,
                cyclewayFrom, cyclewayTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.Bicycle,
                bridlewayFrom, bridlewayTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Bicycle,
                pathFrom, pathTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.Bicycle,
                pedestrianFrom, pedestrianTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Bicycle,
                roadFrom, roadTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Bicycle,
                livingStreetFrom, livingStreetTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Bicycle,
                residentialFrom, residentialTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Bicycle,
                unclassifiedFrom, unclassifiedTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Bicycle,
                tertiaryFrom, tertiaryTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Bicycle,
                secondaryFrom, secondaryTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Bicycle,
                primaryFrom, primaryTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.Bicycle,
                trunkFrom, trunkTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.Bicycle,
                motorwayFrom, motorwayTo, interpreter));

            // moped
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.Moped,
                footwayFrom, footwayTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.Moped,
                cyclewayFrom, cyclewayTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.Moped,
                bridlewayFrom, bridlewayTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.Moped,
                pathFrom, pathTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.Moped,
                pedestrianFrom, pedestrianTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Moped,
                roadFrom, roadTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Moped,
                livingStreetFrom, livingStreetTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Moped,
                residentialFrom, residentialTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Moped,
                unclassifiedFrom, unclassifiedTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Moped,
                tertiaryFrom, tertiaryTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Moped,
                secondaryFrom, secondaryTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Moped,
                primaryFrom, primaryTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Moped,
                trunkFrom, trunkTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.Moped,
                motorwayFrom, motorwayTo, interpreter));

            // moped
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.MotorCycle,
                footwayFrom, footwayTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.MotorCycle,
                cyclewayFrom, cyclewayTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.MotorCycle,
                bridlewayFrom, bridlewayTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.MotorCycle,
                pathFrom, pathTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.MotorCycle,
                pedestrianFrom, pedestrianTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.MotorCycle,
                roadFrom, roadTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.MotorCycle,
                livingStreetFrom, livingStreetTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.MotorCycle,
                residentialFrom, residentialTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.MotorCycle,
                unclassifiedFrom, unclassifiedTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.MotorCycle,
                tertiaryFrom, tertiaryTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.MotorCycle,
                secondaryFrom, secondaryTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.MotorCycle,
                primaryFrom, primaryTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.MotorCycle,
                trunkFrom, trunkTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.MotorCycle,
                motorwayFrom, motorwayTo, interpreter));

            // car
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.Car,
                footwayFrom, footwayTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.Car,
                cyclewayFrom, cyclewayTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.Car,
                bridlewayFrom, bridlewayTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.Car,
                pathFrom, pathTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.Car,
                pedestrianFrom, pedestrianTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Car,
                roadFrom, roadTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Car,
                livingStreetFrom, livingStreetTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Car,
                residentialFrom, residentialTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Car,
                unclassifiedFrom, unclassifiedTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Car,
                tertiaryFrom, tertiaryTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Car,
                secondaryFrom, secondaryTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Car,
                primaryFrom, primaryTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Car,
                trunkFrom, trunkTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Car,
                motorwayFrom, motorwayTo, interpreter));

            // small truck
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.SmallTruck,
                footwayFrom, footwayTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.SmallTruck,
                cyclewayFrom, cyclewayTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.SmallTruck,
                bridlewayFrom, bridlewayTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.SmallTruck,
                pathFrom, pathTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.SmallTruck,
                pedestrianFrom, pedestrianTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.SmallTruck,
                roadFrom, roadTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.SmallTruck,
                livingStreetFrom, livingStreetTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.SmallTruck,
                residentialFrom, residentialTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.SmallTruck,
                unclassifiedFrom, unclassifiedTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.SmallTruck,
                tertiaryFrom, tertiaryTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.SmallTruck,
                secondaryFrom, secondaryTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.SmallTruck,
                primaryFrom, primaryTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.SmallTruck,
                trunkFrom, trunkTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.SmallTruck,
                motorwayFrom, motorwayTo, interpreter));

            // big truck
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.BigTruck,
                footwayFrom, footwayTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.BigTruck,
                cyclewayFrom, cyclewayTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.BigTruck,
                bridlewayFrom, bridlewayTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.BigTruck,
                pathFrom, pathTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.BigTruck,
                pedestrianFrom, pedestrianTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.BigTruck,
                roadFrom, roadTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.BigTruck,
                livingStreetFrom, livingStreetTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.BigTruck,
                residentialFrom, residentialTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.BigTruck,
                unclassifiedFrom, unclassifiedTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.BigTruck,
                tertiaryFrom, tertiaryTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.BigTruck,
                secondaryFrom, secondaryTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.BigTruck,
                primaryFrom, primaryTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.BigTruck,
                trunkFrom, trunkTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.BigTruck,
                motorwayFrom, motorwayTo, interpreter));

            // bus
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.Bus,
                footwayFrom, footwayTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.Bus,
                cyclewayFrom, cyclewayTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.Bus,
                bridlewayFrom, bridlewayTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.Bus,
                pathFrom, pathTo, interpreter));
            Assert.IsFalse(this.DoTestForVehicle(Vehicle.Bus,
                pedestrianFrom, pedestrianTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Bus,
                roadFrom, roadTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Bus,
                livingStreetFrom, livingStreetTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Bus,
                residentialFrom, residentialTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Bus,
                unclassifiedFrom, unclassifiedTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Bus,
                tertiaryFrom, tertiaryTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Bus,
                secondaryFrom, secondaryTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Bus,
                primaryFrom, primaryTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Bus,
                trunkFrom, trunkTo, interpreter));
            Assert.IsTrue(this.DoTestForVehicle(Vehicle.Bus,
                motorwayFrom, motorwayTo, interpreter));
        }

        /// <summary>
        /// Tests access for a given vehicle type and for a given network between two given points.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="interpreter"></param>
        protected bool DoTestForVehicle(Vehicle vehicle, GeoCoordinate from, GeoCoordinate to,
            IOsmRoutingInterpreter interpreter)
        {
            IRoutingAlgorithmData<TEdgeData> data = 
                this.BuildData(interpreter, "OsmSharp.Test.Unittests.test_segments.osm", vehicle);
            IRoutingAlgorithm<TEdgeData> basicRouter = 
                this.BuildBasicRouter(data);
            Router router = 
                this.BuildRouter(data, interpreter, basicRouter);

            RouterPoint resolvedFrom = router.Resolve(vehicle, from);
            RouterPoint resolvedTo = router.Resolve(vehicle, to);

            if (resolvedFrom != null && resolvedTo != null)
            {
                Route route = router.Calculate(vehicle, resolvedFrom, resolvedTo);
                return route != null;
            }
            return false;
        }
    }
}