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

using NUnit.Framework;
using System;
using Itinero.Data.Edges;

namespace Itinero.Test.Data.Edges
{
    /// <summary>
    /// Tests the edge data serializer.
    /// </summary>
    [TestFixture]
    public class EdgeDataSerializerTests
    {
        /// <summary>
        /// Tests serializing distance/profile id's.
        /// </summary>
        [Test]
        public void TestSerialize()
        {
            var edgeData = new EdgeData()
            {
                Distance = 100.1f,
                Profile = 12
            };

            var data = EdgeDataSerializer.Serialize(edgeData);
            Assert.IsNotNull(data);
            Assert.AreEqual(1, data.Length);
            Assert.AreEqual((uint)16384 * (uint)(edgeData.Distance * 10) + (uint)edgeData.Profile,
                data[0]);

            edgeData = new EdgeData()
            {
                Distance = 0f,
                Profile = 12
            };

            data = EdgeDataSerializer.Serialize(edgeData);
            Assert.IsNotNull(data);
            Assert.AreEqual(1, data.Length);
            Assert.AreEqual((uint)16384 * (uint)(edgeData.Distance * 10) + (uint)edgeData.Profile,
                data[0]);

            edgeData = new EdgeData()
            {
                Distance = EdgeDataSerializer.MAX_DISTANCE,
                Profile = EdgeDataSerializer.MAX_PROFILE_COUNT - 1
            };

            data = EdgeDataSerializer.Serialize(edgeData);
            Assert.IsNotNull(data);
            Assert.AreEqual(1, data.Length);
            Assert.AreEqual((uint)16384 * (uint)(edgeData.Distance * 10) + (uint)edgeData.Profile,
                data[0]);

            edgeData = new EdgeData()
            {
                Distance = EdgeDataSerializer.MAX_DISTANCE + 0.1f,
                Profile = EdgeDataSerializer.MAX_PROFILE_COUNT - 1
            };

            Assert.Catch<ArgumentOutOfRangeException>(() => 
                {
                    EdgeDataSerializer.Serialize(edgeData);
                });
            edgeData = new EdgeData()
            {
                Distance = EdgeDataSerializer.MAX_DISTANCE,
                Profile = EdgeDataSerializer.MAX_PROFILE_COUNT
            };
            Assert.Catch<ArgumentOutOfRangeException>(() =>
            {
                EdgeDataSerializer.Serialize(edgeData);
            });
        }

        /// <summary>
        /// Test deserializing distance/profile id's.
        /// </summary>
        [Test]
        public void TestDeserialize()
        {
            var edge = EdgeDataSerializer.Deserialize(new uint[] { 0 });
            Assert.AreEqual(0, edge.Distance);
            Assert.AreEqual(0, edge.Profile);

            edge = EdgeDataSerializer.Deserialize(new uint[] { 10 });
            Assert.AreEqual(0, edge.Distance);
            Assert.AreEqual(10, edge.Profile);

            edge = EdgeDataSerializer.Deserialize(new uint[] { EdgeDataSerializer.MAX_PROFILE_COUNT - 1 });
            Assert.AreEqual(0, edge.Distance);
            Assert.AreEqual(EdgeDataSerializer.MAX_PROFILE_COUNT - 1, edge.Profile);

            edge = EdgeDataSerializer.Deserialize(new uint[] { 
                (uint)16384 * (uint)(EdgeDataSerializer.MAX_DISTANCE * 10) + 
                (uint)(EdgeDataSerializer.MAX_PROFILE_COUNT - 1)});
            Assert.AreEqual(EdgeDataSerializer.MAX_DISTANCE, edge.Distance, .1f);
            Assert.AreEqual(EdgeDataSerializer.MAX_PROFILE_COUNT - 1, edge.Profile);
        }
    }
}