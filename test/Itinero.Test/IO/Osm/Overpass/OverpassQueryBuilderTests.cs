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
using NUnit.Framework;

namespace Itinero.Test.IO.Osm.Overpass
{
    /// <summary>
    /// Tests the overpass query builder.
    /// </summary>
    [TestFixture]
    public class OverpassQueryBuilderTests
    {
        /// <summary>
        /// Tests building a query.
        /// </summary>
        [Test]
        public void TestBuildQuery()
        {
            var q = Itinero.IO.Osm.Overpass.OverpassQueryBuilder.BuildQueryForPolygon(
                new Coordinate(51.1725629311492f, 4.383201599121094f),
                new Coordinate(51.1408018208278f, 4.466800689697266f),
                new Coordinate(51.22860288655629f, 4.470577239990234f),
                new Coordinate(51.17256293114924f, 4.383201599121094f));

            Assert.AreEqual("<osm-script><union><query type=\"way\"><has-kv k=\"highway\"/><polygon-query bounds=\"51.17256 4.383202 51.1408 4.466801 51.2286 4.470577 51.17256 4.383202\"/></query><query type=\"relation\"><has-kv k=\"type=restriction\"/><polygon-query bounds=\"51.17256 4.383202 51.1408 4.466801 51.2286 4.470577 51.17256 4.383202\"/></query></union><print mode=\"body\"/><recurse type=\"down\"/><print mode=\"skeleton\"/></osm-script>", q);
        }
    }
}