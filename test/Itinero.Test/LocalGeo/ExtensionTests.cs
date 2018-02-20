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

using Itinero.LocalGeo;
using Itinero.Navigation.Directions;
using NUnit.Framework;

namespace Itinero.Test.LocalGeo
{
    /// <summary>
    /// Contains tests for the local geo extension methods.
    /// </summary>
    [TestFixture]
    public class ExtensionTests
    {
        /// <summary>
        /// Tests simplification.
        /// </summary>
        [Test]
        public void TestSimplify()
        {
            var shape = new Coordinate[]
            {
                new Coordinate(51.16917253319145f, 4.476456642150879f),
                new Coordinate(51.16937434957071f, 4.477078914642334f),
                new Coordinate(51.16942143993214f, 4.477341771125793f),
                new Coordinate(51.16938444036650f, 4.477781653404236f),
                new Coordinate(51.16933734996729f, 4.478076696395874f)
            };

            var simplified = shape.Simplify(50);
            Assert.IsNotNull(simplified);
            Assert.AreEqual(2, simplified.Length);
            Assert.AreEqual(shape[0].Latitude, simplified[0].Latitude);
            Assert.AreEqual(shape[0].Longitude, simplified[0].Longitude);
            Assert.AreEqual(shape[shape.Length - 1].Latitude, simplified[simplified.Length - 1].Latitude);
            Assert.AreEqual(shape[shape.Length - 1].Longitude, simplified[simplified.Length - 1].Longitude);
            
            simplified = shape.Simplify(0.0000001f);
            Assert.IsNotNull(simplified);
            Assert.AreEqual(5, simplified.Length);
            Assert.AreEqual(shape[0].Latitude, simplified[0].Latitude);
            Assert.AreEqual(shape[0].Longitude, simplified[0].Longitude);
            Assert.AreEqual(shape[1].Latitude, simplified[1].Latitude);
            Assert.AreEqual(shape[1].Longitude, simplified[1].Longitude);
            Assert.AreEqual(shape[2].Latitude, simplified[2].Latitude);
            Assert.AreEqual(shape[2].Longitude, simplified[2].Longitude);
            Assert.AreEqual(shape[3].Latitude, simplified[3].Latitude);
            Assert.AreEqual(shape[3].Longitude, simplified[3].Longitude);
            Assert.AreEqual(shape[4].Latitude, simplified[4].Latitude);
            Assert.AreEqual(shape[4].Longitude, simplified[4].Longitude);
        }
    }
}