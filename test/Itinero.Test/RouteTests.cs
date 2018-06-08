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
using Itinero.LocalGeo;
using Itinero.Attributes;
using System.Collections.Generic;
using Itinero.Navigation.Directions;

namespace Itinero.Test
{
    /// <summary>
    /// Contains tests for the route class and route extensions.
    /// </summary>
    [TestFixture]
    public class RouteTests
    {
        /// <summary>
        /// Tests route concatenation.
        /// </summary>
        [Test]
        public void TestConcatenate()
        {
            var route1 = new Route()
            {
                Shape = new Coordinate[]
                {
                    new Coordinate()
                    {
                        Latitude = 51.267819164340295f,
                        Longitude = 4.801352620124817f
                    },
                    new Coordinate()
                    {
                        Latitude = 51.26821857585588f,
                        Longitude = 4.801352620124817f
                    }
                },
                ShapeMeta = new Route.Meta[]
                {
                    new Route.Meta()
                    {
                        Shape = 0
                    },
                    new Route.Meta()
                    {
                        Shape = 1,
                        Distance = 100,
                        Time = 60
                    }
                },
                Stops = new Route.Stop[]
                {
                    new Route.Stop()
                    {
                        Shape = 0
                    },
                    new Route.Stop()
                    {
                        Shape = 1,
                        Distance = 100,
                        Time = 60
                    }
                },
                Attributes = new AttributeCollection(),
                TotalDistance = 100,
                TotalTime = 60
            };
            var route2 = new Route()
            {
                Shape = new Coordinate[]
                {
                    new Coordinate()
                    {
                        Latitude = 51.26821857585588f,
                        Longitude = 4.801352620124817f
                    },
                    new Coordinate()
                    {
                        Latitude = 51.267819164340295f,
                        Longitude = 4.801352620124817f
                    }
                },
                ShapeMeta = new Route.Meta[]
                {
                    new Route.Meta()
                    {
                        Shape = 0
                    },
                    new Route.Meta()
                    {
                        Shape = 1,
                        Distance = 100,
                        Time = 60
                    }
                },
                Stops = new Route.Stop[]
                {
                    new Route.Stop()
                    {
                        Shape = 0
                    },
                    new Route.Stop()
                    {
                        Shape = 1,
                        Distance = 100,
                        Time = 60
                    }
                },
                Attributes = new AttributeCollection(),
                TotalDistance = 100,
                TotalTime = 60
            };

            var route = route1.Concatenate(route2);

            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(3, route.Shape.Length);
            Assert.AreEqual(3, route.ShapeMeta.Length);
            Assert.AreEqual(0, route.ShapeMeta[0].Distance);
            Assert.AreEqual(0, route.ShapeMeta[0].Time);
            Assert.AreEqual(100, route.ShapeMeta[1].Distance);
            Assert.AreEqual(60, route.ShapeMeta[1].Time);
            Assert.AreEqual(200, route.ShapeMeta[2].Distance);
            Assert.AreEqual(120, route.ShapeMeta[2].Time);
            Assert.AreEqual(0, route.Stops[0].Distance);
            Assert.AreEqual(0, route.Stops[0].Time);
            Assert.AreEqual(100, route.Stops[1].Distance);
            Assert.AreEqual(60, route.Stops[1].Time);
            Assert.AreEqual(200, route.Stops[2].Distance);
            Assert.AreEqual(120, route.Stops[2].Time);
            Assert.AreEqual(200, route.TotalDistance);
            Assert.AreEqual(120, route.TotalTime);
        }

        /// <summary>
        /// Tests the route enumeration.
        /// </summary>
        [Test]
        public void TestEnumerator()
        {
            var route = new Route()
            {
                Shape = new Coordinate[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, 0),
                    new Coordinate(0, 0)
                },
                ShapeMeta = new Route.Meta[]
                {
                    new Route.Meta()
                    {
                        Shape = 0
                    },
                    new Route.Meta()
                    {
                        Shape = 1
                    },
                    new Route.Meta()
                    {
                        Shape = 2
                    }
                },
                TotalDistance = 0,
                TotalTime = 0
            };

            var enumerator = route.GetEnumerator();
            var positions = new List<RoutePosition>();
            while(enumerator.MoveNext())
            {
                positions.Add(enumerator.Current);
            }

            Assert.AreEqual(3, positions.Count);
            var position = positions[0];
            Assert.AreEqual(-1, position.BranchIndex);
            Assert.AreEqual(0, position.MetaIndex);
            Assert.AreEqual(0, position.Shape);
            Assert.AreEqual(-1, position.StopIndex);
            Assert.IsFalse(position.HasBranches());
            Assert.IsFalse(position.HasStops());
            Assert.IsTrue(position.HasCurrentMeta());
            position = positions[1];
            Assert.AreEqual(-1, position.BranchIndex);
            Assert.AreEqual(1, position.MetaIndex);
            Assert.AreEqual(1, position.Shape);
            Assert.AreEqual(-1, position.StopIndex);
            Assert.IsFalse(position.HasBranches());
            Assert.IsFalse(position.HasStops());
            Assert.IsTrue(position.HasCurrentMeta());
            position = positions[2];
            Assert.AreEqual(-1, position.BranchIndex);
            Assert.AreEqual(2, position.MetaIndex);
            Assert.AreEqual(2, position.Shape);
            Assert.AreEqual(-1, position.StopIndex);
            Assert.IsFalse(position.HasBranches());
            Assert.IsFalse(position.HasStops());
            Assert.IsTrue(position.HasCurrentMeta());
            
            route = new Route()
            {
                Shape = new Coordinate[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, 0),
                    new Coordinate(0, 0)
                },
                ShapeMeta = new Route.Meta[]
                {
                    new Route.Meta()
                    {
                        Shape = 0
                    },
                    new Route.Meta()
                    {
                        Shape = 2
                    }
                },
                TotalDistance = 0,
                TotalTime = 0
            };

            enumerator = route.GetEnumerator();
            positions = new List<RoutePosition>();
            while (enumerator.MoveNext())
            {
                positions.Add(enumerator.Current);
            }

            Assert.AreEqual(3, positions.Count);
            position = positions[0];
            Assert.AreEqual(-1, position.BranchIndex);
            Assert.AreEqual(0, position.MetaIndex);
            Assert.AreEqual(0, position.Shape);
            Assert.AreEqual(-1, position.StopIndex);
            Assert.IsFalse(position.HasBranches());
            Assert.IsFalse(position.HasStops());
            Assert.IsTrue(position.HasCurrentMeta());
            position = positions[1];
            Assert.AreEqual(-1, position.BranchIndex);
            Assert.AreEqual(1, position.MetaIndex);
            Assert.AreEqual(1, position.Shape);
            Assert.AreEqual(-1, position.StopIndex);
            Assert.IsFalse(position.HasBranches());
            Assert.IsFalse(position.HasStops());
            Assert.IsFalse(position.HasCurrentMeta());
            position = positions[2];
            Assert.AreEqual(-1, position.BranchIndex);
            Assert.AreEqual(1, position.MetaIndex);
            Assert.AreEqual(2, position.Shape);
            Assert.AreEqual(-1, position.StopIndex);
            Assert.IsFalse(position.HasBranches());
            Assert.IsFalse(position.HasStops());
            Assert.IsTrue(position.HasCurrentMeta());

            route = new Route()
            {
                Shape = new Coordinate[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, 0),
                    new Coordinate(0, 0)
                },
                ShapeMeta = new Route.Meta[]
                {
                    new Route.Meta()
                    {
                        Shape = 0
                    },
                    new Route.Meta()
                    {
                        Shape = 2
                    }
                },
                Stops = new Route.Stop[]
                {
                    new Route.Stop()
                    {
                        Shape = 1
                    }
                },
                Branches = new Route.Branch[]
                {
                    new Route.Branch()
                    {
                        Shape = 1
                    }
                },
                TotalDistance = 0,
                TotalTime = 0
            };

            enumerator = route.GetEnumerator();
            positions = new List<RoutePosition>();
            while (enumerator.MoveNext())
            {
                positions.Add(enumerator.Current);
            }

            Assert.AreEqual(3, positions.Count);
            position = positions[0];
            Assert.AreEqual(0, position.BranchIndex);
            Assert.AreEqual(0, position.MetaIndex);
            Assert.AreEqual(0, position.Shape);
            Assert.AreEqual(0, position.StopIndex);
            Assert.IsFalse(position.HasBranches());
            Assert.IsFalse(position.HasStops());
            Assert.IsTrue(position.HasCurrentMeta());
            position = positions[1];
            Assert.AreEqual(0, position.BranchIndex);
            Assert.AreEqual(1, position.MetaIndex);
            Assert.AreEqual(1, position.Shape);
            Assert.AreEqual(0, position.StopIndex);
            Assert.IsTrue(position.HasBranches());
            Assert.IsTrue(position.HasStops());
            Assert.IsFalse(position.HasCurrentMeta());
            position = positions[2];
            Assert.AreEqual(1, position.BranchIndex);
            Assert.AreEqual(1, position.MetaIndex);
            Assert.AreEqual(2, position.Shape);
            Assert.AreEqual(1, position.StopIndex);
            Assert.IsFalse(position.HasBranches());
            Assert.IsFalse(position.HasStops());
            Assert.IsTrue(position.HasCurrentMeta());
        }

        /// <summary>
        /// Tests writing a route as json.
        /// </summary>
        [Test]
        public void TestWriteJson()
        {
            var route = new Route()
            {
                Shape = new Coordinate[]
                {
                    new Coordinate()
                    {
                        Latitude = 51.267819164340295f,
                        Longitude = 4.801352620124817f
                    },
                    new Coordinate()
                    {
                        Latitude = 51.26821857585588f,
                        Longitude = 4.801352620124817f
                    }
                },
                ShapeMeta = new Route.Meta[]
                {
                    new Route.Meta()
                    {
                        Shape = 0
                    },
                    new Route.Meta()
                    {
                        Shape = 1,
                        Distance = 100,
                        Time = 60,
                        Attributes = new AttributeCollection(
                            new Attribute("highway", "residential")),
                    }
                },
                Stops = new Route.Stop[]
                {
                    new Route.Stop()
                    {
                        Shape = 1,
                        Attributes = new AttributeCollection(
                            new Attribute("address", "Pastorijstraat 102, 2275 Wechelderzande")),
                        Coordinate = new Coordinate()
                        {
                            Latitude = 51.26821857585588f,
                            Longitude = 4.801352620124817f
                        }
                    }
                },
                Branches = new Route.Branch[]
                {
                    new Route.Branch()
                    {
                        Shape = 1,
                        Attributes = new AttributeCollection(
                            new Attribute("highway", "residential")),
                        Coordinate = new Coordinate()
                        {
                            Latitude = 51.26821857585588f,
                            Longitude = 4.801352620124817f
                        }
                    }
                },
                Attributes = new AttributeCollection(),
                TotalDistance = 100,
                TotalTime = 60
            };

            var json = route.ToJson();
            Assert.AreEqual("{\"Attributes\":{\"distance\":\"100\",\"time\":\"60\"},\"Shape\":[[4.801353,51.26782],[4.801353,51.26822]],\"ShapeMeta\":[{\"Shape\":0},{\"Shape\":1,\"Attributes\":{\"highway\":\"residential\"}}],\"Stops\":[{\"Shape\":1,\"Coordinates\":[4.801353,51.26822],\"Attributes\":{\"address\":\"Pastorijstraat 102, 2275 Wechelderzande\"}}],\"Branches\":[{\"Shape\":1,\"Coordinates\":[4.801353,51.26822],\"Attributes\":{\"highway\":\"residential\"}}]}",
                json);
        }

        /// <summary>
        /// Tests writing a route as xml.
        /// </summary>
        [Test]
        public void TestWriteXml()
        {
            var route = new Route()
            {
                Shape = new Coordinate[]
                {
                    new Coordinate()
                    {
                        Latitude = 51.267819164340295f,
                        Longitude = 4.801352620124817f
                    },
                    new Coordinate()
                    {
                        Latitude = 51.26821857585588f,
                        Longitude = 4.801352620124817f
                    }
                },
                ShapeMeta = new Route.Meta[]
                {
                    new Route.Meta()
                    {
                        Shape = 0
                    },
                    new Route.Meta()
                    {
                        Shape = 1,
                        Distance = 100,
                        Time = 60,
                        Attributes = new AttributeCollection(
                            new Attribute("highway", "residential")),
                    }
                },
                Stops = new Route.Stop[]
                {
                    new Route.Stop()
                    {
                        Shape = 1,
                        Attributes = new AttributeCollection(
                            new Attribute("address", "Pastorijstraat 102, 2275 Wechelderzande")),
                        Coordinate = new Coordinate()
                        {
                            Latitude = 51.26821857585588f,
                            Longitude = 4.801352620124817f
                        }
                    }
                },
                Branches = new Route.Branch[]
                {
                    new Route.Branch()
                    {
                        Shape = 1,
                        Attributes = new AttributeCollection(
                            new Attribute("highway", "residential")),
                        Coordinate = new Coordinate()
                        {
                            Latitude = 51.26821857585588f,
                            Longitude = 4.801352620124817f
                        }
                    }
                },
                Attributes = new AttributeCollection(),
                TotalDistance = 100,
                TotalTime = 60
            };

            var xml = route.ToXml();
            Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-16\"?><route><shape><c lat=\"51.26782\" lon=\"4.801353\" /><c lat=\"51.26822\" lon=\"4.801353\" /></shape><metas><meta shape=\"0\" /><meta shape=\"1\"><property k=\"highway\" v=\"residential\" /></meta></metas><branches><branch shape=\"1\"><property k=\"highway\" v=\"residential\" /></branch></branches><stops><stop shape=\"1\" lat=\"51.26822\" lon=\"4.801353\"><property k=\"address\" v=\"Pastorijstraat 102, 2275 Wechelderzande\" /></stop></stops></route>",
                xml);
        }

        /// <summary>
        /// Tests writing a route as geojson.
        /// </summary>
        [Test]
        public void TestWriteGeoJson()
        {
            var route = new Route()
            {
                Shape = new Coordinate[]
                {
                    new Coordinate()
                    {
                        Latitude = 51.267819164340295f,
                        Longitude = 4.801352620124817f
                    },
                    new Coordinate()
                    {
                        Latitude = 51.26821857585588f,
                        Longitude = 4.801352620124817f
                    }
                },
                ShapeMeta = new Route.Meta[]
                {
                    new Route.Meta()
                    {
                        Shape = 0
                    },
                    new Route.Meta()
                    {
                        Shape = 1,
                        Distance = 100,
                        Time = 60,
                        Attributes = new AttributeCollection(
                            new Attribute("highway", "residential")),
                    }
                },
                Stops = new Route.Stop[]
                {
                    new Route.Stop()
                    {
                        Shape = 1,
                        Attributes = new AttributeCollection(
                            new Attribute("address", "Pastorijstraat 102, 2275 Wechelderzande")),
                        Coordinate = new Coordinate()
                        {
                            Latitude = 51.26821857585588f,
                            Longitude = 4.801352620124817f
                        }
                    }
                },
                Branches = new Route.Branch[]
                {
                    new Route.Branch()
                    {
                        Shape = 1,
                        Attributes = new AttributeCollection(
                            new Attribute("highway", "residential")),
                        Coordinate = new Coordinate()
                        {
                            Latitude = 51.26821857585588f,
                            Longitude = 4.801352620124817f
                        }
                    }
                },
                Attributes = new AttributeCollection(),
                TotalDistance = 100,
                TotalTime = 60
            };

            var geojson = route.ToGeoJson();
            Assert.AreEqual("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"name\":\"ShapeMeta\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[4.801353,51.26782],[4.801353,51.26822]]},\"properties\":{\"highway\":\"residential\"}},{\"type\":\"Feature\",\"name\":\"Stop\",\"Shape\":\"1\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[4.801353,51.26822]},\"properties\":{\"address\":\"Pastorijstraat 102, 2275 Wechelderzande\"}}]}",
                geojson);
        }

        /// <summary>
        /// Tests writing a route as geojson with the raw callback option.
        /// </summary>
        [Test]
        public void TestWriteGeoJsonRaw()
        {
            var route = new Route()
            {
                Shape = new Coordinate[]
                {
                    new Coordinate()
                    {
                        Latitude = 51.267819164340295f,
                        Longitude = 4.801352620124817f
                    },
                    new Coordinate()
                    {
                        Latitude = 51.26821857585588f,
                        Longitude = 4.801352620124817f
                    }
                },
                ShapeMeta = new Route.Meta[]
                {
                    new Route.Meta()
                    {
                        Shape = 0
                    },
                    new Route.Meta()
                    {
                        Shape = 1,
                        Attributes = new AttributeCollection(
                            new Attribute("highway", "residential")),
                        Distance = 100,
                        Time = 60,
                    }
                },
                Stops = new Route.Stop[]
                {
                    new Route.Stop()
                    {
                        Shape = 1,
                        Attributes = new AttributeCollection(
                            new Attribute("address", "Pastorijstraat 102, 2275 Wechelderzande")),
                        Coordinate = new Coordinate()
                        {
                            Latitude = 51.26821857585588f,
                            Longitude = 4.801352620124817f
                        }
                    }
                },
                Branches = new Route.Branch[]
                {
                    new Route.Branch()
                    {
                        Shape = 1,
                        Attributes = new AttributeCollection(
                            new Attribute("highway", "residential")),
                        Coordinate = new Coordinate()
                        {
                            Latitude = 51.26821857585588f,
                            Longitude = 4.801352620124817f
                        }
                    }
                },
                Attributes = new AttributeCollection(),
                TotalDistance = 100,
                TotalTime = 60
            };

            var geojson = route.ToGeoJson(isRaw: (k, v) => k == "time" || k == "distance");
            Assert.AreEqual("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"name\":\"ShapeMeta\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[4.801353,51.26782],[4.801353,51.26822]]},\"properties\":{\"highway\":\"residential\",\"distance\":100,\"time\":60}},{\"type\":\"Feature\",\"name\":\"Stop\",\"Shape\":\"1\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[4.801353,51.26822]},\"properties\":{\"address\":\"Pastorijstraat 102, 2275 Wechelderzande\"}}]}",
                geojson);
        }

        /// <summary>
        /// Tests writing a route as geojson.
        /// </summary>
        [Test]
        public void TestWriteGeoJsonWithAttributesCallback()
        {
            var route = new Route()
            {
                Shape = new Coordinate[]
                {
                    new Coordinate()
                    {
                        Latitude = 51.267819164340295f,
                        Longitude = 4.801352620124817f
                    },
                    new Coordinate()
                    {
                        Latitude = 51.26821857585588f,
                        Longitude = 4.801352620124817f
                    }
                },
                ShapeMeta = new Route.Meta[]
                {
                    new Route.Meta()
                    {
                        Shape = 0
                    },
                    new Route.Meta()
                    {
                        Shape = 1,
                        Distance = 100,
                        Time = 60,
                        Attributes = new AttributeCollection(
                            new Attribute("highway", "residential")),
                    }
                },
                Stops = new Route.Stop[]
                {
                    new Route.Stop()
                    {
                        Shape = 1,
                        Attributes = new AttributeCollection(
                            new Attribute("address", "Pastorijstraat 102, 2275 Wechelderzande")),
                        Coordinate = new Coordinate()
                        {
                            Latitude = 51.26821857585588f,
                            Longitude = 4.801352620124817f
                        }
                    }
                },
                Branches = new Route.Branch[]
                {
                    new Route.Branch()
                    {
                        Shape = 1,
                        Attributes = new AttributeCollection(
                            new Attribute("highway", "residential")),
                        Coordinate = new Coordinate()
                        {
                            Latitude = 51.26821857585588f,
                            Longitude = 4.801352620124817f
                        }
                    }
                },
                Attributes = new AttributeCollection(),
                TotalDistance = 100,
                TotalTime = 60
            };

            var geojson = route.ToGeoJson(attributesCallback: (att) => { att.AddOrReplace("extra", "attributes"); });
            Assert.AreEqual("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"name\":\"ShapeMeta\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[4.801353,51.26782],[4.801353,51.26822]]},\"properties\":{\"highway\":\"residential\",\"extra\":\"attributes\"}},{\"type\":\"Feature\",\"name\":\"Stop\",\"Shape\":\"1\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[4.801353,51.26822]},\"properties\":{\"address\":\"Pastorijstraat 102, 2275 Wechelderzande\",\"extra\":\"attributes\"}}]}",
                geojson);
            geojson = route.ToGeoJson(attributesCallback: (att) => { att.AddOrReplace("extra", "attributes"); }, includeShapeMeta: false, groupByShapeMeta: false);
            Assert.AreEqual("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"name\":\"Shape\",\"properties\":{},\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[4.801353,51.26782],[4.801353,51.26822]]},\"properties\":{\"extra\":\"attributes\"}},{\"type\":\"Feature\",\"name\":\"Stop\",\"Shape\":\"1\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[4.801353,51.26822]},\"properties\":{\"address\":\"Pastorijstraat 102, 2275 Wechelderzande\",\"extra\":\"attributes\"}}]}",
                geojson);
        }

        /// <summary>
        /// Tests route concatenation with identical stops.
        /// </summary>
        [Test]
        public void TestConcatenateWithIdenticalStops()
        {
            var route1 = new Route()
            {
                Shape = new Coordinate[]
                {
                    new Coordinate()
                    {
                        Latitude = 51.267819164340295f,
                        Longitude = 4.801352620124817f
                    },
                    new Coordinate()
                    {
                        Latitude = 51.26821857585588f,
                        Longitude = 4.801352620124817f
                    }
                },
                ShapeMeta = new Route.Meta[]
                {
                    new Route.Meta()
                    {
                        Shape = 0
                    },
                    new Route.Meta()
                    {
                        Shape = 1,
                        Distance = 100,
                        Time = 60
                    }
                },
                Stops = new Route.Stop[]
                {
                    new Route.Stop()
                    {
                        Attributes = null,
                        Coordinate = new Coordinate(0, 0),
                        Shape = 1
                    }
                },
                Attributes = new AttributeCollection(),
                TotalDistance = 100,
                TotalTime = 60
            };
            var route2 = new Route()
            {
                Shape = new Coordinate[]
                {
                    new Coordinate()
                    {
                        Latitude = 51.26821857585588f,
                        Longitude = 4.801352620124817f
                    },
                    new Coordinate()
                    {
                        Latitude = 51.267819164340295f,
                        Longitude = 4.801352620124817f
                    }
                },
                ShapeMeta = new Route.Meta[]
                {
                    new Route.Meta()
                    {
                        Shape = 0
                    },
                    new Route.Meta()
                    {
                        Shape = 1,
                        Distance = 100,
                        Time = 60
                    }
                },
                Stops = new Route.Stop[]
                {
                    new Route.Stop()
                    {
                        Attributes = null,
                        Coordinate = new Coordinate(0, 0),
                        Shape = 0
                    }
                },
                Attributes = new AttributeCollection(),
                TotalDistance = 100,
                TotalTime = 60
            };

            var route = route1.Concatenate(route2);

            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(3, route.Shape.Length);
            Assert.AreEqual(3, route.ShapeMeta.Length);
            Assert.AreEqual(0, route.ShapeMeta[0].Distance);
            Assert.AreEqual(0, route.ShapeMeta[0].Time);
            Assert.AreEqual(100, route.ShapeMeta[1].Distance);
            Assert.AreEqual(60, route.ShapeMeta[1].Time);
            Assert.AreEqual(200, route.ShapeMeta[2].Distance);
            Assert.AreEqual(120, route.ShapeMeta[2].Time);
            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(1, route.Stops.Length);
            Assert.AreEqual(200, route.TotalDistance);
            Assert.AreEqual(120, route.TotalTime);
        }

        /// <summary>
        /// Test getting a segment.
        /// </summary>
        [Test]
        public void TestGetSegment()
        {
            var route = new Route()
            {
                Shape = new Coordinate[]
                {
                    new Coordinate()
                    {
                        Latitude = 51.267819164340295f,
                        Longitude = 4.801352620124817f
                    },
                    new Coordinate()
                    {
                        Latitude = 51.26821857585588f,
                        Longitude = 4.801352620124817f
                    },
                    new Coordinate()
                    {
                        Latitude = 51.26821857585588f,
                        Longitude = 4.801352620124817f
                    },
                    new Coordinate()
                    {
                        Latitude = 51.26821857585588f,
                        Longitude = 4.801352620124817f
                    },
                    new Coordinate()
                    {
                        Latitude = 51.26821857585588f,
                        Longitude = 4.801352620124817f
                    }
                },
                ShapeMeta = new Route.Meta[]
                {
                    new Route.Meta()
                    {
                        Shape = 0
                    },
                    new Route.Meta()
                    {
                        Shape = 2,
                        Distance = 100,
                        Time = 60
                    },
                    new Route.Meta()
                    {
                        Shape = 4,
                        Distance = 200,
                        Time = 120
                    }
                },
                Attributes = new AttributeCollection(),
                TotalDistance = 100,
                TotalTime = 60
            };

            int segmentStart, segmentEnd;
            route.SegmentFor(0, out segmentStart, out segmentEnd);
            Assert.AreEqual(0, segmentStart);
            Assert.AreEqual(2, segmentEnd);
            route.SegmentFor(1, out segmentStart, out segmentEnd);
            Assert.AreEqual(0, segmentStart);
            Assert.AreEqual(2, segmentEnd);
            route.SegmentFor(2, out segmentStart, out segmentEnd);
            Assert.AreEqual(2, segmentStart);
            Assert.AreEqual(4, segmentEnd);
            route.SegmentFor(3, out segmentStart, out segmentEnd);
            Assert.AreEqual(2, segmentStart);
            Assert.AreEqual(4, segmentEnd);
            route.SegmentFor(4, out segmentStart, out segmentEnd);
            Assert.AreEqual(2, segmentStart);
            Assert.AreEqual(4, segmentEnd);
        }

        /// <summary>
        /// Test projecting points on the route.
        /// </summary>
        [Test]
        public void TestProjectOne()
        {
            var route = new Route()
            {
                Shape = new Coordinate[]
                {
                    new Coordinate()
                    {
                        Latitude = 51.268977112538806f,
                        Longitude = 4.800424575805664f
                    },
                    new Coordinate()
                    {
                        Latitude = 51.26830584177533f,
                        Longitude = 4.8006391525268555f
                    },
                    new Coordinate()
                    {
                        Latitude = 51.267768818104585f,
                        Longitude = 4.801325798034667f
                    },
                    new Coordinate()
                    {
                        Latitude = 51.26674845584085f,
                        Longitude = 4.801068305969238f
                    },
                    new Coordinate()
                    {
                        Latitude = 51.26551325015766f,
                        Longitude = 4.801154136657715f
                    }
                },
                ShapeMeta = new Route.Meta[]
                {
                    new Route.Meta()
                    {
                        Shape = 0
                    },
                    new Route.Meta()
                    {
                        Shape = 2,
                        Distance = 100,
                        Time = 60
                    },
                    new Route.Meta()
                    {
                        Shape = 4,
                        Distance = 200,
                        Time = 120
                    }
                },
                Attributes = new AttributeCollection(),
                TotalDistance = 200,
                TotalTime = 120
            };

            // calculate actual distance/times.
            Route.Meta previousMeta = null;
            foreach (var meta in route.ShapeMeta)
            {
                meta.Distance = 0;
                meta.Time = 0;

                if (previousMeta != null)
                {
                    for (var s = previousMeta.Shape; s < meta.Shape; s++)
                    {
                        meta.Distance = meta.Distance + Coordinate.DistanceEstimateInMeter(
                            route.Shape[s], route.Shape[s + 1]);
                    }

                    meta.Time = meta.Distance / 16.6667f; // 60km/h
                }

                previousMeta = meta;
            }
            route.TotalDistance = route.ShapeMeta[route.ShapeMeta.Length - 1].Distance;
            route.TotalTime = route.ShapeMeta[route.ShapeMeta.Length - 1].Time;

            float time, distance;
            int shape;
            Coordinate projected;
            Assert.IsTrue(route.ProjectOn(new Coordinate(51.26856092582056f, 4.800623059272766f), out projected, out shape, out distance, out time));
            Assert.AreEqual(0, shape);
            Assert.IsTrue(time > route.ShapeMeta[0].Time);
            Assert.IsTrue(time < route.ShapeMeta[1].Time);

            Assert.IsTrue(route.ProjectOn(new Coordinate(51.26795342069926f, 4.801229238510132f), out projected, out shape, out distance, out time));
            Assert.AreEqual(1, shape);
            Assert.IsTrue(time > route.ShapeMeta[0].Time);
            Assert.IsTrue(time < route.ShapeMeta[1].Time);

            Assert.IsTrue(route.ProjectOn(new Coordinate(51.26712438141587f, 4.801207780838013f), out projected, out shape, out distance, out time));
            Assert.AreEqual(2, shape);
            Assert.IsTrue(time > route.ShapeMeta[1].Time);
            Assert.IsTrue(time < route.ShapeMeta[2].Time);

            Assert.IsTrue(route.ProjectOn(new Coordinate(51.26610064830449f, 4.801395535469055f), out projected, out shape, out distance, out time));
            Assert.AreEqual(3, shape);
            Assert.IsTrue(time > route.ShapeMeta[1].Time);
            Assert.IsTrue(time < route.ShapeMeta[2].Time);
        }

        /// <summary>
        /// Test distance and time at a shape.
        /// </summary>
        [Test]
        public void TestGetDistanceAndTimeAt()
        {
            var route = new Route()
            {
                Shape = new Coordinate[]
                {
                    new Coordinate()
                    {
                        Latitude = 49.76851543353109f,
                        Longitude = 5.912189483642578f
                    },
                    new Coordinate()
                    {
                        Latitude = 49.768522363042294f,
                        Longitude = 5.9135788679122925f
                    },
                    new Coordinate()
                    {
                        Latitude = 49.76852929255253f,
                        Longitude = 5.914962887763977f
                    },
                    new Coordinate()
                    {
                        Latitude = 49.76852929255253f,
                        Longitude = 5.916352272033691f
                    },
                    new Coordinate()
                    {
                        Latitude = 49.768546616323725f,
                        Longitude = 5.917741656303405f
                    }
                },
                ShapeMeta = new Route.Meta[]
                {
                    new Route.Meta()
                    {
                        Shape = 0
                    },
                    new Route.Meta()
                    {
                        Shape = 2,
                        Distance = 100,
                        Time = 60
                    },
                    new Route.Meta()
                    {
                        Shape = 4,
                        Distance = 200,
                        Time = 120
                    }
                },
                Attributes = new AttributeCollection(),
                TotalDistance = 200,
                TotalTime = 120
            };

            float time, distance;
            route.DistanceAndTimeAt(0, out distance, out time);
            Assert.AreEqual(0, distance);
            Assert.AreEqual(0, time);
            route.DistanceAndTimeAt(1, out distance, out time);
            Assert.AreEqual(50, distance, 1);
            Assert.AreEqual(30, time, 1);
            route.DistanceAndTimeAt(2, out distance, out time);
            Assert.AreEqual(100, distance, 1);
            Assert.AreEqual(60, time, 1);
            route.DistanceAndTimeAt(3, out distance, out time);
            Assert.AreEqual(150, distance, 1);
            Assert.AreEqual(90, time, 1);
            route.DistanceAndTimeAt(4, out distance, out time);
            Assert.AreEqual(200, distance, 1);
            Assert.AreEqual(120, time, 1);
        }

        /// <summary>
        /// Tests relative direction at.
        /// </summary>
        [Test]
        public void TestRelativeDirectionAt()
        {
            var offset = 0.001f;
            var middle = new Coordinate(51.16917253319145f, 4.476456642150879f);
            var top = new Coordinate(middle.Latitude + offset, middle.Longitude);
            var right = new Coordinate(middle.Latitude, middle.Longitude + offset);
            var bottom = new Coordinate(middle.Latitude - offset, middle.Longitude);
            var left = new Coordinate(middle.Latitude, middle.Longitude - offset);
            
            var route = new Route()
            {
                Shape = new Coordinate[]
                {
                    top,
                    middle, 
                    bottom
                }
            };
            
            var dir = route.RelativeDirectionAt(1);
            Assert.IsNotNull(dir);
            Assert.AreEqual(RelativeDirectionEnum.StraightOn, dir.Direction);
            Assert.AreEqual(180, dir.Angle, 1);
            
            route = new Route()
            {
                Shape = new Coordinate[]
                {
                    bottom,
                    middle, 
                    right
                }
            };
            
            dir = route.RelativeDirectionAt(1);
            Assert.IsNotNull(dir);
            Assert.AreEqual(RelativeDirectionEnum.Right, dir.Direction);
            Assert.AreEqual(90, dir.Angle, 1);
            
            route = new Route()
            {
                Shape = new Coordinate[]
                {
                    bottom,
                    middle, 
                    left
                }
            };
            
            dir = route.RelativeDirectionAt(1);
            Assert.IsNotNull(dir);
            Assert.AreEqual(RelativeDirectionEnum.Left, dir.Direction);
            Assert.AreEqual(270, dir.Angle, 1);
            
            route = new Route()
            {
                Shape = new Coordinate[]
                {
                    bottom,
                    middle, 
                    bottom
                }
            };
            
            dir = route.RelativeDirectionAt(1);
            Assert.IsNotNull(dir);
            Assert.AreEqual(RelativeDirectionEnum.TurnBack, dir.Direction);
            Assert.AreEqual(0, dir.Angle, 1);
        }

        /// <summary>
        /// Tests relative direction at.
        /// </summary>
        [Test]
        public void TestRelativeDirectionAtShapeWithIdenticalPoint()
        {
            var offset = 0.001f;
            var middle = new Coordinate(51.16917253319145f, 4.476456642150879f);
            var top = new Coordinate(middle.Latitude + offset, middle.Longitude);
            var right = new Coordinate(middle.Latitude, middle.Longitude + offset);
            var bottom = new Coordinate(middle.Latitude - offset, middle.Longitude);
            var left = new Coordinate(middle.Latitude, middle.Longitude - offset);
            
            var route = new Route()
            {
                Shape = new Coordinate[]
                {
                    top,
                    middle, 
                    middle,
                    bottom
                }
            };
            
            var dir = route.RelativeDirectionAt(1);
            Assert.IsNotNull(dir);
            Assert.AreEqual(RelativeDirectionEnum.StraightOn, dir.Direction);
            Assert.AreEqual(180, dir.Angle, 1);
            dir = route.RelativeDirectionAt(2);
            Assert.IsNotNull(dir);
            Assert.AreEqual(RelativeDirectionEnum.StraightOn, dir.Direction);
            Assert.AreEqual(180, dir.Angle, 1);
            
            // test a few impossible calculations.
            route = new Route()
            {
                Shape = new Coordinate[]
                {
                    middle, 
                    middle,
                    bottom
                }
            };
            
            dir = route.RelativeDirectionAt(1);
            Assert.IsNull(dir);
            
            route = new Route()
            {
                Shape = new Coordinate[]
                {
                    top,
                    middle, 
                    middle
                }
            };
            
            dir = route.RelativeDirectionAt(1);
            Assert.IsNull(dir);
        }
    }
}