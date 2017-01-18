// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2017 Abelshausen Ben
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