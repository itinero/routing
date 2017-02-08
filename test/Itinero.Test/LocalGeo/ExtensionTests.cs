// Itinero - Routing for .NET
// Copyright (C) 2017 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.LocalGeo;
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