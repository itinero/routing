// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
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