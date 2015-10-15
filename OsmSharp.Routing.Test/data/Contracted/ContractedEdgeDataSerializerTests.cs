// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
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
using OsmSharp.Routing.Data.Contracted;
using System;

namespace OsmSharp.Routing.Test.Data.Contracted
{
    /// <summary>
    /// Tests the edge data serializer.
    /// </summary>
    [TestFixture]
    public class ContractedEdgeDataSerializerTests
    {
        /// <summary>
        /// Tests serializing distance/profile id's.
        /// </summary>
        [Test]
        public void TestSerialize()
        {
            var edgeData = new ContractedEdgeData()
            {
                Weight = 100f,
                Direction = null,
                ContractedId = Constants.NO_VERTEX
            };

            var data = ContractedEdgeDataSerializer.Serialize(edgeData);
            Assert.IsNotNull(data);
            Assert.AreEqual(2, data.Length);
            Assert.AreEqual(((uint)(100.0f * 4 * 10)), data[0]);
            Assert.AreEqual(Constants.NO_VERTEX, data[1]);

            edgeData = new ContractedEdgeData()
            {
                Weight = ContractedEdgeDataSerializer.MAX_DISTANCE,
                Direction = true,
                ContractedId = Constants.NO_VERTEX
            };

            data = ContractedEdgeDataSerializer.Serialize(edgeData);
            Assert.IsNotNull(data);
            Assert.AreEqual(2, data.Length);
            Assert.AreEqual(((uint)1) + (((uint)(ContractedEdgeDataSerializer.MAX_DISTANCE * 4 * 10))), data[0]);
            Assert.AreEqual(Constants.NO_VERTEX, data[1]);

            edgeData = new ContractedEdgeData()
            {
                Weight = 100.25f,
                Direction = null,
                ContractedId = Constants.NO_VERTEX
            };

            data = ContractedEdgeDataSerializer.Serialize(edgeData);
            Assert.IsNotNull(data);
            Assert.AreEqual(2, data.Length);
            Assert.AreEqual(((uint)(100.25f * 10) * 4), data[0]);
            Assert.AreEqual(Constants.NO_VERTEX, data[1]);

            edgeData = new ContractedEdgeData()
            {
                Weight = 100,
                Direction = false,
                ContractedId = Constants.NO_VERTEX
            };

            data = ContractedEdgeDataSerializer.Serialize(edgeData);
            Assert.IsNotNull(data);
            Assert.AreEqual(2, data.Length);
            Assert.AreEqual(((uint)2) + (((uint)(100.0f * 4 * 10))), data[0]);
            Assert.AreEqual(Constants.NO_VERTEX, data[1]);

            edgeData = new ContractedEdgeData()
            {
                Weight = 100,
                Direction = true,
                ContractedId = Constants.NO_VERTEX
            };

            data = ContractedEdgeDataSerializer.Serialize(edgeData);
            Assert.IsNotNull(data);
            Assert.AreEqual(2, data.Length);
            Assert.AreEqual(((uint)1) + (((uint)(100.0f * 10) * 4)), data[0]);
            Assert.AreEqual(Constants.NO_VERTEX, data[1]);

            edgeData = new ContractedEdgeData()
            {
                Weight = ContractedEdgeDataSerializer.MAX_DISTANCE,
                Direction = false,
                ContractedId = Constants.NO_VERTEX
            };

            data = ContractedEdgeDataSerializer.Serialize(edgeData);
            Assert.IsNotNull(data);
            Assert.AreEqual(2, data.Length);
            Assert.AreEqual(((uint)2) + (((uint)(ContractedEdgeDataSerializer.MAX_DISTANCE * 4 * 10))), data[0]);
            Assert.AreEqual(Constants.NO_VERTEX, data[1]);

            edgeData = new ContractedEdgeData()
            {
                Weight = ContractedEdgeDataSerializer.MAX_DISTANCE + 100,
                Direction = true,
                ContractedId = Constants.NO_VERTEX
            };
            Assert.Catch<ArgumentOutOfRangeException>(() =>
            {
                ContractedEdgeDataSerializer.Serialize(edgeData);
            });
        }

        /// <summary>
        /// Test deserializing distance/profile id's.
        /// </summary>
        [Test]
        public void TestDeserialize()
        {
            var edge = ContractedEdgeDataSerializer.Deserialize(new uint[] { ((uint)(100.0f * 4)),
                Constants.NO_VERTEX });
            Assert.AreEqual(null, edge.Direction);
            Assert.AreEqual(10.0f, edge.Weight);
            Assert.AreEqual(Constants.NO_VERTEX, edge.ContractedId);

            edge = ContractedEdgeDataSerializer.Deserialize(new uint[] { (uint)1 + ((uint)(100.0f * 4)),
                Constants.NO_VERTEX });
            Assert.AreEqual(true, edge.Direction);
            Assert.AreEqual(10.0f, edge.Weight);
            Assert.AreEqual(Constants.NO_VERTEX, edge.ContractedId);

            edge = ContractedEdgeDataSerializer.Deserialize(new uint[] { (uint)2 + ((uint)(100.0f * 4)),
                Constants.NO_VERTEX });
            Assert.AreEqual(false, edge.Direction);
            Assert.AreEqual(10.0f, edge.Weight);
            Assert.AreEqual(Constants.NO_VERTEX, edge.ContractedId);

            edge = ContractedEdgeDataSerializer.Deserialize(new uint[] { ((uint)1 + (((uint)(ContractedEdgeDataSerializer.MAX_DISTANCE * 4 * 10)))),
                Constants.NO_VERTEX });
            Assert.AreEqual(true, edge.Direction);
            Assert.AreEqual(ContractedEdgeDataSerializer.MAX_DISTANCE, edge.Weight);
            Assert.AreEqual(Constants.NO_VERTEX, edge.ContractedId);

            edge = ContractedEdgeDataSerializer.Deserialize(new uint[] { ((uint)2 + (((uint)(ContractedEdgeDataSerializer.MAX_DISTANCE * 4 * 10)))),
                Constants.NO_VERTEX });
            Assert.AreEqual(false, edge.Direction);
            Assert.AreEqual(ContractedEdgeDataSerializer.MAX_DISTANCE, edge.Weight);
            Assert.AreEqual(Constants.NO_VERTEX, edge.ContractedId);
        }
    }
}