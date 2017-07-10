/*
 *  Licensed to SharpSoftware under one or more contributor
 *  license agreements. See the NOTICE file distributed with this work for 
 *  additional information regarding copyright ownership.
 * 
 *  SharpSoftware licenses this file to you under the Apache License, 
 *  Version 2.0 (the "License"); you may not use this file except in 
 *  compliance with the License. You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using Itinero.Attributes;
using Itinero.IO.Osm.Normalizer;
using NUnit.Framework;

namespace Itinero.Test.IO.Osm.Normalizer
{
    /// <summary>
    /// Contains tests for the attribute collection extensions.
    /// </summary>
    [TestFixture]
    public class AttributeCollectionExtensionsTests
    {
        /// <summary>
        /// Tests normalizing max speed.
        /// </summary>
        [Test]
        public void TestNormalizeMaxspeed()
        {
            var maxspeed = string.Empty;

            var tags = new AttributeCollection(
                new Attribute("maxspeed", "100"));
            var profile = new AttributeCollection();
            tags.NormalizeMaxspeed(profile);
            Assert.IsTrue(profile.TryGetValue("maxspeed", out maxspeed));
            Assert.AreEqual("100", maxspeed);

            tags = new AttributeCollection(
                new Attribute("maxspeed", "100 mph"));
            profile.Clear();
            tags.NormalizeMaxspeed(profile);
            Assert.IsTrue(profile.TryGetValue("maxspeed", out maxspeed));
            Assert.AreEqual("100 mph", maxspeed);

            tags = new AttributeCollection(
                new Attribute("maxspeed", "mph"));
            profile.Clear();
            tags.NormalizeMaxspeed(profile);
            Assert.IsFalse(profile.TryGetValue("maxspeed", out maxspeed));
        }
    }
}