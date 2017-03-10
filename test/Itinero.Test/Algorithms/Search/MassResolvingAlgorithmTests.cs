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

using Itinero.Algorithms.Search;
using Itinero.Attributes;
using Itinero.LocalGeo;
using NUnit.Framework;

namespace Itinero.Test.Algorithms.Search
{
    /// <summary>
    /// Contains tests for the mass resolving algorithm.
    /// </summary>
    [TestFixture]
    public class MassResolvingAlgorithmTests
    {
        /// <summary>
        /// Tests a simple two-point matrix with edge-matcher.
        /// </summary>
        [Test]
        public void Test1TwoPointsWithMatching()
        {
            // build test case.
            var router = new RouterMock(new AttributeCollection(new Itinero.Attributes.Attribute("highway", "residential")));
            var locations = new Coordinate[] {
                    new Coordinate(0, 0),
                    new Coordinate(1, 1) };
            var resolver = new MassResolvingAlgorithm(router, new Itinero.Profiles.Profile[] { Itinero.Osm.Vehicles.Vehicle.Car.Fastest() }, locations,
                (edge, i) =>
                {
                    return true;
                });

            // run.
            resolver.Run();

            // check result.
            Assert.IsNotNull(resolver);
            Assert.IsTrue(resolver.HasRun);
            Assert.IsTrue(resolver.HasSucceeded);
            Assert.AreEqual(0, resolver.Errors.Count);
            Assert.AreEqual(2, resolver.RouterPoints.Count);
            Assert.AreEqual(0, resolver.ResolvedIndexOf(0));
            Assert.AreEqual(1, resolver.ResolvedIndexOf(1));

            // build test case.
            router = new RouterMock(new AttributeCollection(new Itinero.Attributes.Attribute("highway", "primary")));
            locations = new Coordinate[] {
                    new Coordinate(0, 0),
                    new Coordinate(1, 1) };
            resolver = new MassResolvingAlgorithm(router, new Itinero.Profiles.Profile[] { Itinero.Osm.Vehicles.Vehicle.Car.Fastest() }, locations,
                (edge, i) =>
                {
                    return false;
                });

            // run.
            resolver.Run();

            Assert.IsNotNull(resolver);
            Assert.IsTrue(resolver.HasRun);
            Assert.IsTrue(resolver.HasSucceeded);
            Assert.AreEqual(2, resolver.Errors.Count);
            Assert.AreEqual(LocationErrorCode.NotResolved, resolver.Errors[0].Code);
            Assert.AreEqual(LocationErrorCode.NotResolved, resolver.Errors[1].Code);
        }
        
        /// <summary>
        /// Tests a simple two-point matrix but with one of the points not 'resolvable'.
        /// </summary>
        [Test]
        public void Test2TwoPoints()
        {
            // build test case.
            var router = new RouterMock();
            var locations = new Coordinate[] {
                    new Coordinate(0, 0),
                    new Coordinate(180, 1) };
            var resolver = new MassResolvingAlgorithm(router, new Itinero.Profiles.Profile[] { Itinero.Osm.Vehicles.Vehicle.Car.Fastest() }, locations);

            // run.
            resolver.Run();

            Assert.IsNotNull(resolver);
            Assert.IsTrue(resolver.HasRun);
            Assert.IsTrue(resolver.HasSucceeded);
            Assert.AreEqual(1, resolver.Errors.Count);
            Assert.IsTrue(resolver.Errors.ContainsKey(1));
            Assert.AreEqual(1, resolver.RouterPoints.Count);
            Assert.AreEqual(0, resolver.ResolvedIndexOf(0));
            Assert.AreEqual(0, resolver.LocationIndexOf(0));

            // build test case.
            locations = new Coordinate[] {
                    new Coordinate(180, 0),
                    new Coordinate(1, 1) };
            resolver = new MassResolvingAlgorithm(router, new Itinero.Profiles.Profile[] { Itinero.Osm.Vehicles.Vehicle.Car.Fastest() }, locations);

            // run.
            resolver.Run();

            Assert.IsNotNull(resolver);
            Assert.IsTrue(resolver.HasRun);
            Assert.IsTrue(resolver.HasSucceeded);
            Assert.AreEqual(1, resolver.Errors.Count);
            Assert.IsTrue(resolver.Errors.ContainsKey(0));
            Assert.AreEqual(0, resolver.ResolvedIndexOf(1));
            Assert.AreEqual(1, resolver.LocationIndexOf(0));
        }
    }
}