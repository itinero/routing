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

using NUnit.Framework;
using OsmSharp.Routing.Attributes;
using OsmSharp.Routing.Osm;

namespace OsmSharp.Routing.Test.Osm
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
            var tags = new AttributeCollection();
            var profileTags = new AttributeCollection();
            var metaTags = new AttributeCollection();

            Assert.IsFalse(tags.Normalize(profileTags, metaTags));

            tags.AddOrReplace(new Attribute("highway", "residential"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "footway"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags));
            Assert.IsTrue(profileTags.Contains("highway", "footway"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "motorway"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags));
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
            var tags = new AttributeCollection();
            var profileTags = new AttributeCollection();
            var metaTags = new AttributeCollection();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("bicycle", "yes"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsFalse(profileTags.Contains("bicycle", "yes"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("bicycle", "no"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsTrue(profileTags.Contains("bicycle", "no"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("bicycle", "mistake"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsFalse(profileTags.Contains("bicycle", "mistake"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "footway"));
            tags.AddOrReplace(new Attribute("bicycle", "no"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags));
            Assert.IsTrue(profileTags.Contains("highway", "footway"));
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
            var tags = new AttributeCollection();
            var profileTags = new AttributeCollection();
            var metaTags = new AttributeCollection();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("foot", "yes"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsFalse(profileTags.Contains("foot", "yes"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("foot", "no"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsTrue(profileTags.Contains("foot", "no"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("foot", "mistake"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsFalse(profileTags.Contains("foot", "mistake"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "cycleway"));
            tags.AddOrReplace(new Attribute("foot", "no"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags));
            Assert.IsTrue(profileTags.Contains("highway", "cycleway"));
            Assert.IsFalse(profileTags.Contains("foot", "no"));
            profileTags.Clear();
            tags.Clear();
        }

        /// <summary>
        /// Tests motorvehicle restriction normalization.
        /// </summary>
        [Test]
        public void TestMotorvehicleRestrictions()
        {
            var tags = new AttributeCollection();
            var profileTags = new AttributeCollection();
            var metaTags = new AttributeCollection();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("motorvehicle", "yes"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsFalse(profileTags.Contains("motorvehicle", "yes"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("motorvehicle", "no"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsTrue(profileTags.Contains("motorvehicle", "no"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("motorvehicle", "mistake"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsFalse(profileTags.Contains("motorvehicle", "mistake"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "cycleway"));
            tags.AddOrReplace(new Attribute("motorvehicle", "no"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags));
            Assert.IsTrue(profileTags.Contains("highway", "cycleway"));
            Assert.IsFalse(profileTags.Contains("motorvehicle", "no"));
            profileTags.Clear();
            tags.Clear();
        }

        /// <summary>
        /// Tests oneway restriction normalization.
        /// </summary>
        [Test]
        public void TestOnewayRestrictions()
        {
            var tags = new AttributeCollection();
            var profileTags = new AttributeCollection();
            var metaTags = new AttributeCollection();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("oneway", "no"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsFalse(profileTags.Contains("oneway", "no"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("oneway", "yes"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsTrue(profileTags.Contains("oneway", "yes"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("oneway", "-1"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags));
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
            var tags = new AttributeCollection();
            var profileTags = new AttributeCollection();
            var metaTags = new AttributeCollection();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("access", "no"));
            Assert.IsFalse(tags.Normalize(profileTags, metaTags));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("access", "yes"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsFalse(profileTags.Contains("access", "yes"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("access", "mistake"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags));
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
            var tags = new AttributeCollection();
            var profileTags = new AttributeCollection();
            var metaTags = new AttributeCollection();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("junction", "roundabout"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsTrue(profileTags.Contains("junction", "roundabout"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("junction", "mistake"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags));
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
            var tags = new AttributeCollection();
            var profileTags = new AttributeCollection();
            var metaTags = new AttributeCollection();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("maxspeed", "50"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsTrue(profileTags.Contains("maxspeed", "50"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("maxspeed", "mistake"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsFalse(profileTags.Contains("maxspeed", "mistake"));
            profileTags.Clear();
            tags.Clear();

            tags.AddOrReplace(new Attribute("highway", "residential"));
            tags.AddOrReplace(new Attribute("maxspeed", "50 mph"));
            Assert.IsTrue(tags.Normalize(profileTags, metaTags));
            Assert.IsTrue(profileTags.Contains("highway", "residential"));
            Assert.IsTrue(profileTags.Contains("maxspeed", "50 mph"));
            profileTags.Clear();
            tags.Clear();
        }
    }
}
