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
using Itinero.Data.Contracted.Edges;

namespace Itinero.Test.Data.Contracted.Edges
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

            var data = ContractedEdgeDataSerializer.SerializeMeta(edgeData);
            Assert.IsNotNull(data);
            Assert.AreEqual(2, data.Length);
            Assert.AreEqual(((uint)(100.0f * 4 * ContractedEdgeDataSerializer.PRECISION_FACTOR)), data[0]);
            Assert.AreEqual(Constants.NO_VERTEX, data[1]);

            edgeData = new ContractedEdgeData()
            {
                Weight = ContractedEdgeDataSerializer.MAX_DISTANCE,
                Direction = true,
                ContractedId = Constants.NO_VERTEX
            };

            data = ContractedEdgeDataSerializer.SerializeMeta(edgeData);
            Assert.IsNotNull(data);
            Assert.AreEqual(2, data.Length);
            Assert.AreEqual(((uint)1) + (((uint)(ContractedEdgeDataSerializer.MAX_DISTANCE * 4 * ContractedEdgeDataSerializer.PRECISION_FACTOR))), data[0]);
            Assert.AreEqual(Constants.NO_VERTEX, data[1]);

            edgeData = new ContractedEdgeData()
            {
                Weight = 100.25f,
                Direction = null,
                ContractedId = Constants.NO_VERTEX
            };

            data = ContractedEdgeDataSerializer.SerializeMeta(edgeData);
            Assert.IsNotNull(data);
            Assert.AreEqual(2, data.Length);
            Assert.AreEqual(((uint)(100.25f * ContractedEdgeDataSerializer.PRECISION_FACTOR) * 4), data[0]);
            Assert.AreEqual(Constants.NO_VERTEX, data[1]);

            edgeData = new ContractedEdgeData()
            {
                Weight = 100,
                Direction = false,
                ContractedId = Constants.NO_VERTEX
            };

            data = ContractedEdgeDataSerializer.SerializeMeta(edgeData);
            Assert.IsNotNull(data);
            Assert.AreEqual(2, data.Length);
            Assert.AreEqual(((uint)2) + (((uint)(100.0f * 4 * ContractedEdgeDataSerializer.PRECISION_FACTOR))), data[0]);
            Assert.AreEqual(Constants.NO_VERTEX, data[1]);

            edgeData = new ContractedEdgeData()
            {
                Weight = 100,
                Direction = true,
                ContractedId = Constants.NO_VERTEX
            };

            data = ContractedEdgeDataSerializer.SerializeMeta(edgeData);
            Assert.IsNotNull(data);
            Assert.AreEqual(2, data.Length);
            Assert.AreEqual(((uint)1) + (((uint)(100.0f * ContractedEdgeDataSerializer.PRECISION_FACTOR) * 4)), data[0]);
            Assert.AreEqual(Constants.NO_VERTEX, data[1]);

            edgeData = new ContractedEdgeData()
            {
                Weight = ContractedEdgeDataSerializer.MAX_DISTANCE,
                Direction = false,
                ContractedId = Constants.NO_VERTEX
            };

            data = ContractedEdgeDataSerializer.SerializeMeta(edgeData);
            Assert.IsNotNull(data);
            Assert.AreEqual(2, data.Length);
            Assert.AreEqual(((uint)2) + (((uint)(ContractedEdgeDataSerializer.MAX_DISTANCE * 4 * ContractedEdgeDataSerializer.PRECISION_FACTOR))), data[0]);
            Assert.AreEqual(Constants.NO_VERTEX, data[1]);

            edgeData = new ContractedEdgeData()
            {
                Weight = ContractedEdgeDataSerializer.MAX_DISTANCE + 100,
                Direction = true,
                ContractedId = Constants.NO_VERTEX
            };
            Assert.Catch<ArgumentOutOfRangeException>(() =>
            {
                ContractedEdgeDataSerializer.SerializeMeta(edgeData);
            });
        }

        /// <summary>
        /// Test deserializing distance/profile id's.
        /// </summary>
        [Test]
        public void TestDeserialize()
        {
            var edge = ContractedEdgeDataSerializer.Deserialize(((uint)(10.0f * 4 * ContractedEdgeDataSerializer.PRECISION_FACTOR)),
                Constants.NO_VERTEX);
            Assert.AreEqual(null, edge.Direction);
            Assert.AreEqual(10.0f, edge.Weight);
            Assert.AreEqual(Constants.NO_VERTEX, edge.ContractedId);

            edge = ContractedEdgeDataSerializer.Deserialize((uint)1 + ((uint)(10.0f * 4 * ContractedEdgeDataSerializer.PRECISION_FACTOR)),
                Constants.NO_VERTEX);
            Assert.AreEqual(true, edge.Direction);
            Assert.AreEqual(10.0f, edge.Weight);
            Assert.AreEqual(Constants.NO_VERTEX, edge.ContractedId);

            edge = ContractedEdgeDataSerializer.Deserialize((uint)2 + ((uint)(10.0f * 4 * ContractedEdgeDataSerializer.PRECISION_FACTOR)),
                Constants.NO_VERTEX);
            Assert.AreEqual(false, edge.Direction);
            Assert.AreEqual(10.0f, edge.Weight);
            Assert.AreEqual(Constants.NO_VERTEX, edge.ContractedId);

            edge = ContractedEdgeDataSerializer.Deserialize(((uint)1 + (((uint)(ContractedEdgeDataSerializer.MAX_DISTANCE * 4 * ContractedEdgeDataSerializer.PRECISION_FACTOR)))),
                Constants.NO_VERTEX);
            Assert.AreEqual(true, edge.Direction);
            Assert.AreEqual(ContractedEdgeDataSerializer.MAX_DISTANCE, edge.Weight);
            Assert.AreEqual(Constants.NO_VERTEX, edge.ContractedId);

            edge = ContractedEdgeDataSerializer.Deserialize(((uint)2 + (((uint)(ContractedEdgeDataSerializer.MAX_DISTANCE * 4 * ContractedEdgeDataSerializer.PRECISION_FACTOR)))),
                Constants.NO_VERTEX);
            Assert.AreEqual(false, edge.Direction);
            Assert.AreEqual(ContractedEdgeDataSerializer.MAX_DISTANCE, edge.Weight);
            Assert.AreEqual(Constants.NO_VERTEX, edge.ContractedId);
        }
    }
}