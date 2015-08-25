using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OsmSharp.Routing.Osm.Interpreter;
using NUnit.Framework;
using OsmSharp.Routing;
using OsmSharp.Collections.Tags;
using OsmSharp.Routing.Vehicles;

namespace OsmSharp.Test.Unittests.Routing.Interpreter
{
    /// <summary>
    /// Tests the OsmRoutingInterpreter.
    /// </summary>
    [TestFixture]
    public class OsmRoutingInterpreterTests
    {
        /// <summary>
        /// Tests the CanBeTraversedBy function.
        /// </summary>
        [Test]
        public void TestOsmRoutingInterpreterCanBeTraversedBy()
        {
            var interpreter = new OsmRoutingInterpreter();

            TagsCollectionBase tags = new TagsCollection();
            tags["highway"] = "footway";
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Pedestrian));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bicycle));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Moped));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.MotorCycle));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Car));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.SmallTruck));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.BigTruck));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bus));

            tags["highway"] = "cycleway";
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Pedestrian));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bicycle));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Moped));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.MotorCycle));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Car));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.SmallTruck));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.BigTruck));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bus));

            tags["highway"] = "bridleway";
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Pedestrian));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bicycle));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Moped));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.MotorCycle));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Car));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.SmallTruck));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.BigTruck));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bus));

            tags["highway"] = "path";
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Pedestrian));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bicycle));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Moped));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.MotorCycle));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Car));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.SmallTruck));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.BigTruck));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bus));

            tags["highway"] = "pedestrian";
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Pedestrian));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bicycle));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Moped));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.MotorCycle));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Car));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.SmallTruck));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.BigTruck));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bus));

            tags["highway"] = "road";
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Pedestrian));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bicycle));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Moped));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.MotorCycle));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Car));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.SmallTruck));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.BigTruck));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bus));

            tags["highway"] = "living_street";
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Pedestrian));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bicycle));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Moped));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.MotorCycle));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Car));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.SmallTruck));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.BigTruck));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bus));

            tags["highway"] = "residential";
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Pedestrian));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bicycle));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Moped));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.MotorCycle));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Car));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.SmallTruck));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.BigTruck));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bus));

            tags["highway"] = "unclassified";
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Pedestrian));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bicycle));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Moped));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.MotorCycle));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Car));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.SmallTruck));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.BigTruck));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bus));

            tags["highway"] = "tertiary";
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Pedestrian));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bicycle));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Moped));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.MotorCycle));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Car));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.SmallTruck));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.BigTruck));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bus));

            tags["highway"] = "secondary";
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Pedestrian));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bicycle));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Moped));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.MotorCycle));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Car));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.SmallTruck));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.BigTruck));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bus));

            tags["highway"] = "primary";
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Pedestrian));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bicycle));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Moped));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.MotorCycle));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Car));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.SmallTruck));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.BigTruck));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bus));

            tags["highway"] = "trunk";
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Pedestrian));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bicycle));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Moped));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.MotorCycle));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Car));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.SmallTruck));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.BigTruck));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bus));

            tags["highway"] = "motorway";
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Pedestrian));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bicycle));
            Assert.IsFalse(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Moped));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.MotorCycle));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Car));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.SmallTruck));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.BigTruck));
            Assert.IsTrue(interpreter.EdgeInterpreter.CanBeTraversedBy(tags, Vehicle.Bus));
        }
    }
}
