// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
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

using Itinero.IO.Osm.Relations;
using Itinero.IO.Osm.Streams;
using Itinero.LocalGeo;
using Itinero.Osm.Vehicles;
using NUnit.Framework;
using OsmSharp;
using OsmSharp.Streams;
using OsmSharp.Tags;

namespace Itinero.Test.IO.Osm.Relations
{
    /// <summary>
    /// Contains tests for the relation tag processor.
    /// </summary>
    [TestFixture]
    public class RelationTagsProcessorTests
    {
        /// <summary>
        /// The most basic positive tests of the relation tags processor.
        /// </summary>
        [Test]
        public void TestAddingOneRelation()
        {
            // build source stream.
            var location1 = new Coordinate() { Latitude = 51.265016473294075f, Longitude = 4.7835588455200195f };
            var location2 = new Coordinate() { Latitude = 51.265016473294075f, Longitude = 4.7907257080078125f };
            var source = new OsmGeo[] {
                new Node()
                {
                    Id = 1,
                    Latitude = location1.Latitude,
                    Longitude = location1.Longitude
                },
                new Node()
                {
                    Id = 2,
                    Latitude = location2.Latitude,
                    Longitude = location2.Longitude
                },
                new Way()
                {
                    Id = 1,
                    Tags = new TagsCollection(
                        new Tag("highway", "residential")),
                    Nodes = new long[] { 1, 2 }
                },
                new Relation()
                {
                    Id = 1,
                    Tags = new TagsCollection(
                        new Tag("type", "to_add")),
                    Members = new RelationMember[]
                    {
                        new RelationMember()
                        {
                            Id = 1,
                            Role = "some_role",
                            Type = OsmGeoType.Way
                        }
                    }
                }};

            // create processor.
            var processors = new ITwoPassProcessor[]
            {
                new RelationTagProcessor((r) => true, (w, t) => {
                    w.Tags.AddOrReplace(t);
                    Assert.AreEqual(1, w.Id.Value);
                })
            };

            // build db from stream.
            var routerDb = new RouterDb();
            var target = new RouterDbStreamTarget(
                routerDb, new Vehicle[] {
                    Vehicle.Car
                }, processors: processors);
            target.RegisterSource(source);
            target.Initialize();
            target.Pull();
        }
    }
}