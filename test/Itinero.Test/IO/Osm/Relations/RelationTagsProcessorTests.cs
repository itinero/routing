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

using System;
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
                new SimpleRelationTagProcessor()
            };

            // build db from stream.
            var routerDb = new RouterDb();
            var target = new RouterDbStreamTarget(
                routerDb, new Itinero.Profiles.Vehicle[] {
                    Vehicle.Car
                }, processors: processors);
            target.RegisterSource(source);
            target.Initialize();
            target.Pull();
        }

        /// <summary>
        /// The most basic positive tests of the relation tags processor with multiple levels.
        /// </summary>
        [Test]
        public void TestAddingMultipleLevelRelations()
        {
            var way = new Way()
            {
                Id = 1,
                Tags = new TagsCollection(
                        new Tag("highway", "residential")),
                Nodes = new long[] { 1, 2 }
            };
            var relation1 = new Relation()
            {
                Id = 1,
                Tags = new TagsCollection(
                        new Tag("type", "level1")),
                Members = new RelationMember[]
                    {
                        new RelationMember()
                        {
                            Id = 1,
                            Role = "some_role",
                            Type = OsmGeoType.Way
                        }
                    }
            };
            var relation2 = new Relation()
            {
                Id = 2,
                Tags = new TagsCollection(
                        new Tag("type", "level2")),
                Members = new RelationMember[]
                    {
                        new RelationMember()
                        {
                            Id = 1,
                            Role = "some_role",
                            Type = OsmGeoType.Relation
                        }
                    }
            };

            // create processor.
            var processor = new SimpleRelationTagProcessor(true);
            Assert.AreEqual(false, processor.FirstPass(relation1));
            Assert.AreEqual(true, processor.FirstPass(relation2));
            Assert.AreEqual(false, processor.FirstPass(relation1));
            Assert.AreEqual(false, processor.FirstPass(relation2));

            processor.SecondPass(way);

            string typeValue;
            if (way.Tags.TryGetValue("type", out typeValue))
            {
                Assert.AreEqual("level1,level2", typeValue);
            }
        }

        class SimpleRelationTagProcessor : RelationTagProcessor
        {
            public SimpleRelationTagProcessor(bool processMemberRelations = false)
                : base(processMemberRelations)
            {

            }

            public override void AddTags(Way w, TagsCollectionBase t)
            {
                w.Tags.AddOrReplace(t);
                Assert.AreEqual(1, w.Id.Value);
            }

            public override bool IsRelevant(Relation relation)
            {
                return true;
            }
        }
    }
}