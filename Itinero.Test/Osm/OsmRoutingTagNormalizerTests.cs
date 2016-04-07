// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
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
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using NUnit.Framework;
using Itinero.Attributes;
using Itinero.Osm;
using OsmSharp;

namespace Itinero.Test.Osm
{
    /// <summary>
    /// Contains tests for the routing tag normalizer.
    /// </summary>
    [TestFixture]
    public class OsmRoutingTagNormalizerTests
    {
        /// <summary>
        /// Test some default highway definitions.
        /// </summary>
        [Test]
        public void TestHighway()
        {
            Itinero.Osm.Vehicles.Vehicle.RegisterVehicles();

            var tags = new AttributeCollection();
            var profileTags = new AttributeCollection();
            var metaTags = new AttributeCollection();

            Assert.IsFalse(tags.Normalize(profileTags, metaTags, Itinero.Osm.Vehicles.Vehicle.GetAllRegistered()));

            tags.AddOrReplace(new Attribute("highway", "residential"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, Itinero.Osm.Vehicles.Vehicle.GetAllRegistered()));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "footway"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, Itinero.Osm.Vehicles.Vehicle.GetAllRegistered()));
            Assert.IsTrue(profileTags.Contains("highway", "footway"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "motorway"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, Itinero.Osm.Vehicles.Vehicle.GetAllRegistered()));
            Assert.IsTrue(profileTags.Contains("highway", "motorway"));
            profileTags.Clear();
            tags.Clear();
        }

        /// <summary>
        /// Tests bicycle restriction normalization.
        /// </summary>
        [Test]
        public void TestBicycleRestrictions()
        {
            Itinero.Osm.Vehicles.Vehicle.RegisterVehicles();
            var vehicles = new Itinero.Osm.Vehicles.Vehicle[] { Itinero.Osm.Vehicles.Vehicle.Bicycle };

            var tags = new AttributeCollection();
            var profileTags = new AttributeCollection();
            var metaTags = new AttributeCollection();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("bicycle", "yes"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, vehicles));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsFalse(profileTags.Contains("bicycle", "yes"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("bicycle", "no"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, vehicles));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsTrue(profileTags.Contains("bicycle", "no"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("bicycle", "mistake"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, vehicles));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsFalse(profileTags.Contains("bicycle", "mistake"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "footway"));
            tags.AddOrReplace(new Attribute("bicycle", "no"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, vehicles));
            Assert.IsTrue(profileTags.Contains("highway", "footway"));
            Assert.IsFalse(profileTags.Contains("bicycle", "no"));
            profileTags.Clear();
            tags.Clear();

            vehicles = new Itinero.Osm.Vehicles.Vehicle[] { Itinero.Osm.Vehicles.Vehicle.Car };

            tags.AddOrReplace("highway", "residential");
            tags.AddOrReplace("bicycle", "no");
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, vehicles));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsFalse(profileTags.Contains("bicycle", "no"));
            profileTags.Clear();
            tags.Clear();
        }

        /// <summary>
        /// Tests foot restriction normalization.
        /// </summary>
        [Test]
        public void TestFootRestrictions()
        {
            Itinero.Osm.Vehicles.Vehicle.RegisterVehicles();
            var vehicles = new Itinero.Osm.Vehicles.Vehicle[] { Itinero.Osm.Vehicles.Vehicle.Pedestrian };

            var tags = new AttributeCollection();
            var profileTags = new AttributeCollection();
            var metaTags = new AttributeCollection();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("foot", "yes"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, vehicles));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsFalse(profileTags.Contains("foot", "yes"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("foot", "no"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, vehicles));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsTrue(profileTags.Contains("foot", "no"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("foot", "mistake"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, vehicles));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsFalse(profileTags.Contains("foot", "mistake"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "cycleway"));
            tags.AddOrReplace(new Attribute("foot", "no"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, vehicles));
            Assert.IsTrue(profileTags.Contains("highway", "cycleway"));
            Assert.IsTrue(profileTags.Contains("foot", "no"));
            profileTags.Clear();
            tags.Clear();
        }

        /// <summary>
        /// Tests motorvehicle restriction normalization.
        /// </summary>
        [Test]
        public void TestMotorvehicleRestrictions()
        {
            Itinero.Osm.Vehicles.Vehicle.RegisterVehicles();
            var vehicles = new Itinero.Osm.Vehicles.Vehicle[] { Itinero.Osm.Vehicles.Vehicle.Car };

            var tags = new AttributeCollection();
            var profileTags = new AttributeCollection();
            var metaTags = new AttributeCollection();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("motor_vehicle", "yes"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, vehicles));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsFalse(profileTags.Contains("motor_vehicle", "yes"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("motorcar", "no"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, vehicles));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsTrue(profileTags.Contains("motorcar", "no"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("motorcar", "mistake"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, vehicles));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsFalse(profileTags.Contains("motorcar", "mistake"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "cycleway"));
            tags.AddOrReplace(new Attribute("motorcar", "no"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, vehicles));
            Assert.IsTrue(profileTags.Contains("highway", "cycleway"));
            Assert.IsFalse(profileTags.Contains("motorcar", "no"));
            profileTags.Clear();
            tags.Clear();
        }

        /// <summary>
        /// Tests oneway restriction normalization.
        /// </summary>
        [Test]
        public void TestOnewayRestrictions()
        {
            Itinero.Osm.Vehicles.Vehicle.RegisterVehicles();

            var tags = new AttributeCollection();
            var profileTags = new AttributeCollection();
            var metaTags = new AttributeCollection();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("oneway", "no"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, Itinero.Osm.Vehicles.Vehicle.GetAllRegistered()));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsFalse(profileTags.Contains("oneway", "no"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("oneway", "yes"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, Itinero.Osm.Vehicles.Vehicle.GetAllRegistered()));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsTrue(profileTags.Contains("oneway", "yes"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("oneway", "-1"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, Itinero.Osm.Vehicles.Vehicle.GetAllRegistered()));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsTrue(profileTags.Contains("oneway", "-1"));
            profileTags.Clear();
            tags.Clear();
        }

        /// <summary>
        /// Tests access restriction normalization.
        /// </summary>
        [Test]
        public void TestAccessRestrictions()
        {
            Itinero.Osm.Vehicles.Vehicle.RegisterVehicles();

            var tags = new AttributeCollection();
            var profileTags = new AttributeCollection();
            var metaTags = new AttributeCollection();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("access", "yes"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, Itinero.Osm.Vehicles.Vehicle.GetAllRegistered()));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsFalse(profileTags.Contains("access", "yes"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("access", "mistake"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, Itinero.Osm.Vehicles.Vehicle.GetAllRegistered()));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsFalse(profileTags.Contains("access", "mistake"));
            profileTags.Clear();
            tags.Clear();
        }

        /// <summary>
        /// Tests junction normalization.
        /// </summary>
        [Test]
        public void TestJunction()
        {
            Itinero.Osm.Vehicles.Vehicle.RegisterVehicles();

            var tags = new AttributeCollection();
            var profileTags = new AttributeCollection();
            var metaTags = new AttributeCollection();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("junction", "roundabout"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, Itinero.Osm.Vehicles.Vehicle.GetAllRegistered()));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsTrue(profileTags.Contains("junction", "roundabout"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("junction", "mistake"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, Itinero.Osm.Vehicles.Vehicle.GetAllRegistered()));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsFalse(profileTags.Contains("junction", "mistake"));
            profileTags.Clear();
            tags.Clear();
        }

        /// <summary>
        /// Tests maxspeed normalization.
        /// </summary>
        [Test]
        public void TestMaxspeed()
        {
            Itinero.Osm.Vehicles.Vehicle.RegisterVehicles();

            var tags = new AttributeCollection();
            var profileTags = new AttributeCollection();
            var metaTags = new AttributeCollection();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("maxspeed", "50"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, Itinero.Osm.Vehicles.Vehicle.GetAllRegistered()));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsTrue(profileTags.Contains("maxspeed", "50"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("maxspeed", "mistake"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, Itinero.Osm.Vehicles.Vehicle.GetAllRegistered()));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsFalse(profileTags.Contains("maxspeed", "mistake"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("maxspeed", "50 mph"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, Itinero.Osm.Vehicles.Vehicle.GetAllRegistered()));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsTrue(profileTags.Contains("maxspeed", "50 mph"));
            profileTags.Clear();
            tags.Clear();
        }

        /// <summary>
        /// Tests normalization with steps and ramp.
        /// </summary>
        [Test]
        public void TestRamp()
        {
            Itinero.Osm.Vehicles.Vehicle.RegisterVehicles();

            var tags = new AttributeCollection();
            var profileTags = new AttributeCollection();
            var metaTags = new AttributeCollection();

            tags.AddOrReplace("highway", "steps");
            tags.AddOrReplace("ramp", "yes");
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, Itinero.Osm.Vehicles.Vehicle.GetAllRegistered()));
            Assert.IsTrue(profileTags.Contains("highway", "steps"));
            Assert.IsTrue(profileTags.Contains("ramp", "yes"));
            profileTags.Clear();
            tags.Clear();
        }

        /// <summary>
        /// Tests motorway access tags.
        /// </summary>
        [Test]
        public void TestMotorwayAccess()
        {
            Itinero.Osm.Vehicles.Vehicle.RegisterVehicles();
            var vehicles = new Itinero.Osm.Vehicles.Vehicle[] {
                Itinero.Osm.Vehicles.Vehicle.Pedestrian,
                Itinero.Osm.Vehicles.Vehicle.Bicycle,
                Itinero.Osm.Vehicles.Vehicle.Car
            };

            var tags = new AttributeCollection();
            var profileTags = new AttributeCollection();
            var metaTags = new AttributeCollection();

            tags.AddOrReplace("highway", "motorway");
            tags.AddOrReplace("access", "no");
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, vehicles));
            Assert.AreEqual(2, profileTags.Count);
            Assert.IsTrue(profileTags.Contains("highway", "motorway"));
            Assert.IsTrue(profileTags.Contains("motorcar", "no"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace("highway", "motorway");
            tags.AddOrReplace("access", "yes");
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, vehicles));
            Assert.AreEqual(3, profileTags.Count);
            Assert.IsTrue(profileTags.Contains("highway", "motorway"));
            Assert.IsTrue(profileTags.Contains("bicycle", "yes"));
            Assert.IsTrue(profileTags.Contains("foot", "yes"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace("highway", "motorway");
            tags.AddOrReplace("access", "no");
            tags.AddOrReplace("vehicle", "yes");
            Assert.IsTrue(tags.Normalize(profileTags, metaTags, vehicles));
            Assert.AreEqual(2, profileTags.Count);
            Assert.IsTrue(profileTags.Contains("highway", "motorway"));
            Assert.IsTrue(profileTags.Contains("bicycle", "yes"));
            profileTags.Clear();
            tags.Clear();
        }
    }
}