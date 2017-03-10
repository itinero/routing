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
using Itinero.LocalGeo.IO;
using NUnit.Framework;
using System.Collections.Generic;

namespace Itinero.Test.LocalGeo.IO
{
    /// <summary>
    /// Holds tests for the GeoJSON extensions.
    /// </summary>
    [TestFixture]
    public class GeoJsonExtensionTests
    {
        /// <summary>
        /// Tests writing a polygon.
        /// </summary>
        [Test]
        public void TestWritePolygon()
        {
            var polygon = new Polygon()
            {
                ExteriorRing = new List<Coordinate>(new Coordinate[]
                {
                    new Coordinate(51.004249861455264f, 4.368438720703125f),
                    new Coordinate(50.900867668253730f, 4.2812347412109375f),
                    new Coordinate(50.898269339916050f, 4.5407867431640625f),
                    new Coordinate(51.004249861455264f, 4.368438720703125f)
                }),
                InteriorRings = new List<List<Coordinate>>(new List<Coordinate>[]
                {
                    new List<Coordinate>(new Coordinate[]
                    {
                        new Coordinate(50.946585333063865f, 4.3747901916503910f),
                        new Coordinate(50.938202617005570f, 4.3696832656860350f),
                        new Coordinate(50.938743483006796f, 4.3859052658081055f),
                        new Coordinate(50.946585333063865f, 4.3747901916503910f)
                    })
                })
            };

            var geoJson = polygon.ToGeoJson();
            Assert.AreEqual("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"name\":\"Shape\",\"properties\":{},\"geometry\":{\"type\":\"Polygon\",\"coordinates\":[[[4.368439,51.00425],[4.281235,50.90087],[4.540787,50.89827],[4.368439,51.00425]],[[4.37479,50.94659],[4.369683,50.9382],[4.385905,50.93874],[4.37479,50.94659]]]}}]}",
                geoJson);
            
            var polygons = new Polygon[] {
                new Polygon()
                {
                    ExteriorRing = new List<Coordinate>(new Coordinate[]
                    {
                        new Coordinate(51.004249861455264f, 4.368438720703125f),
                        new Coordinate(50.900867668253730f, 4.2812347412109375f),
                        new Coordinate(50.898269339916050f, 4.5407867431640625f),
                        new Coordinate(51.004249861455264f, 4.368438720703125f)
                    }),
                    InteriorRings = new List<List<Coordinate>>(new List<Coordinate>[]
                    {
                        new List<Coordinate>(new Coordinate[]
                        {
                            new Coordinate(50.946585333063865f, 4.3747901916503910f),
                            new Coordinate(50.938202617005570f, 4.3696832656860350f),
                            new Coordinate(50.938743483006796f, 4.3859052658081055f),
                            new Coordinate(50.946585333063865f, 4.3747901916503910f)
                        })
                    })
                },
                new Polygon()
                {
                    ExteriorRing = new List<Coordinate>(new Coordinate[]
                    {
                        new Coordinate(51.004249861455264f, 4.368438720703125f),
                        new Coordinate(50.900867668253730f, 4.2812347412109375f),
                        new Coordinate(50.898269339916050f, 4.5407867431640625f),
                        new Coordinate(51.004249861455264f, 4.368438720703125f)
                    }),
                    InteriorRings = new List<List<Coordinate>>(new List<Coordinate>[]
                    {
                        new List<Coordinate>(new Coordinate[]
                        {
                            new Coordinate(50.946585333063865f, 4.3747901916503910f),
                            new Coordinate(50.938202617005570f, 4.3696832656860350f),
                            new Coordinate(50.938743483006796f, 4.3859052658081055f),
                            new Coordinate(50.946585333063865f, 4.3747901916503910f)
                        })
                    })
                }
            };

            geoJson = polygons.ToGeoJson();
            Assert.AreEqual("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"name\":\"Shape\",\"properties\":{},\"geometry\":{\"type\":\"Polygon\",\"coordinates\":[[[4.368439,51.00425],[4.281235,50.90087],[4.540787,50.89827],[4.368439,51.00425]],[[4.37479,50.94659],[4.369683,50.9382],[4.385905,50.93874],[4.37479,50.94659]]]}},{\"type\":\"Feature\",\"name\":\"Shape\",\"properties\":{},\"geometry\":{\"type\":\"Polygon\",\"coordinates\":[[[4.368439,51.00425],[4.281235,50.90087],[4.540787,50.89827],[4.368439,51.00425]],[[4.37479,50.94659],[4.369683,50.9382],[4.385905,50.93874],[4.37479,50.94659]]]}}]}",
                geoJson);
        }
    }
}