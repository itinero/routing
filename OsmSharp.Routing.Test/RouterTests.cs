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
using OsmSharp.Routing.Test.Algorithms.Search;
using OsmSharp.Routing.Test.Profiles;

namespace OsmSharp.Routing.Test
{
    /// <summary>
    /// Contains tests for the router.
    /// </summary>
    [TestFixture]
    public class RouterTests
    {
        /// <summary>
        /// Tests setting the custom resolver delegate.
        /// </summary>
        [Test]
        public void TestCustomResolverDelegate()
        {
            var routerDb = new RouterDb();
            routerDb.AddSupportedProfile(MockProfile.CarMock());
            var router = new Router(routerDb);
            var called = false;
            router.CreateCustomResolver = (latitude, longitude) =>
                {
                    called = true;
                    return new MockResolver(new RouterPoint(latitude, longitude, 0, 0));
                };
            router.Resolve(new Routing.Profiles.Profile[] { MockProfile.CarMock() }, 0, 0);

            Assert.IsTrue(called);
        }
    }
}