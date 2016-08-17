// Itinero - Routing for .NET
// Copyright (C) 2016 Abelshausen Ben
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

using Itinero.IO.Osm.Restrictions;
using NUnit.Framework;
using OsmSharp;
using OsmSharp.Tags;

namespace Itinero.Test.IO.Osm.Restrictions
{
    /// <summary>
    /// Contains tests for the restriction processor.
    /// </summary>
    [TestFixture]
    public class RestrictionProcessorTests
    {
        /// <summary>
        /// Tests a perfectly modelled restriction.
        /// </summary>
        [Test]
        public void TestPerfectRestriction()
        {
            var processor = new RestrictionProcessor(
                new string[] { }, (node) => (uint)node, (vehicleType, sequence) =>
                {
                    Assert.IsTrue(string.IsNullOrWhiteSpace(vehicleType));
                    Assert.IsNotNull(sequence);
                    Assert.AreEqual(3, sequence.Count);
                    Assert.AreEqual(1, sequence[0]);
                    Assert.AreEqual(2, sequence[1]);
                    Assert.AreEqual(3, sequence[2]);
                });

            var via = new Node()
            {
                Id = 2
            };
            var from = new Way()
            {
                Id = 1,
                Nodes = new long[] { 1, 2 }
            };
            var to = new Way()
            {
                Id = 2,
                Nodes = new long[] { 2, 3 }
            };
            var restriction = new Relation()
            {
                Members = new RelationMember[]
                {
                    new RelationMember()
                    {
                        Id = 2,
                        Role = "via",
                        Type = OsmGeoType.Node
                    },
                    new RelationMember()
                    {
                        Id = 1,
                        Role = "from",
                        Type = OsmGeoType.Way
                    },
                    new RelationMember()
                    {
                        Id = 2,
                        Role = "to",
                        Type = OsmGeoType.Way
                    }
                },
                Tags = new TagsCollection(
                    new Tag("type", "restriction"))
            };

            processor.FirstPass(restriction);
            processor.SecondPass(via);
            processor.SecondPass(from);
            processor.SecondPass(to);
            processor.SecondPass(restriction);
        }

        /// <summary>
        /// Tests a restriction with a from way in reverse.
        /// </summary>
        [Test]
        public void TestRestrictionFromWayReversed()
        {
            var processor = new RestrictionProcessor(
                new string[] { }, (node) => (uint)node, (vehicleType, sequence) =>
                {
                    Assert.IsTrue(string.IsNullOrWhiteSpace(vehicleType));
                    Assert.IsNotNull(sequence);
                    Assert.AreEqual(3, sequence.Count);
                    Assert.AreEqual(1, sequence[0]);
                    Assert.AreEqual(2, sequence[1]);
                    Assert.AreEqual(3, sequence[2]);
                });

            var via = new Node()
            {
                Id = 2
            };
            var from = new Way()
            {
                Id = 1,
                Nodes = new long[] { 2, 1 }
            };
            var to = new Way()
            {
                Id = 2,
                Nodes = new long[] { 2, 3 }
            };
            var restriction = new Relation()
            {
                Members = new RelationMember[]
                {
                    new RelationMember()
                    {
                        Id = 2,
                        Role = "via",
                        Type = OsmGeoType.Node
                    },
                    new RelationMember()
                    {
                        Id = 1,
                        Role = "from",
                        Type = OsmGeoType.Way
                    },
                    new RelationMember()
                    {
                        Id = 2,
                        Role = "to",
                        Type = OsmGeoType.Way
                    }
                },
                Tags = new TagsCollection(
                    new Tag("type", "restriction"))
            };

            processor.FirstPass(restriction);
            processor.SecondPass(via);
            processor.SecondPass(from);
            processor.SecondPass(to);
            processor.SecondPass(restriction);
        }

        /// <summary>
        /// Tests a restriction with a to way in reverse.
        /// </summary>
        [Test]
        public void TestRestrictionToWayReversed()
        {
            var processor = new RestrictionProcessor(
                new string[] { }, (node) => (uint)node, (vehicleType, sequence) =>
                {
                    Assert.IsTrue(string.IsNullOrWhiteSpace(vehicleType));
                    Assert.IsNotNull(sequence);
                    Assert.AreEqual(3, sequence.Count);
                    Assert.AreEqual(1, sequence[0]);
                    Assert.AreEqual(2, sequence[1]);
                    Assert.AreEqual(3, sequence[2]);
                });

            var via = new Node()
            {
                Id = 2
            };
            var from = new Way()
            {
                Id = 1,
                Nodes = new long[] { 1, 2 }
            };
            var to = new Way()
            {
                Id = 2,
                Nodes = new long[] { 3, 2 }
            };
            var restriction = new Relation()
            {
                Members = new RelationMember[]
                {
                    new RelationMember()
                    {
                        Id = 2,
                        Role = "via",
                        Type = OsmGeoType.Node
                    },
                    new RelationMember()
                    {
                        Id = 1,
                        Role = "from",
                        Type = OsmGeoType.Way
                    },
                    new RelationMember()
                    {
                        Id = 2,
                        Role = "to",
                        Type = OsmGeoType.Way
                    }
                },
                Tags = new TagsCollection(
                    new Tag("type", "restriction"))
            };

            processor.FirstPass(restriction);
            processor.SecondPass(via);
            processor.SecondPass(from);
            processor.SecondPass(to);
            processor.SecondPass(restriction);
        }

        /// <summary>
        /// Tests a perfectly modelled restriction with a via way.
        /// </summary>
        [Test]
        public void TestPerfectRestrictionViaWay()
        {
            var processor = new RestrictionProcessor(
                new string[] { }, (node) => (uint)node, (vehicleType, sequence) =>
                {
                    Assert.IsTrue(string.IsNullOrWhiteSpace(vehicleType));
                    Assert.IsNotNull(sequence);
                    Assert.AreEqual(4, sequence.Count);
                    Assert.AreEqual(1, sequence[0]);
                    Assert.AreEqual(2, sequence[1]);
                    Assert.AreEqual(3, sequence[2]);
                    Assert.AreEqual(4, sequence[3]);
                });

            var via = new Way()
            {
                Id = 3,
                Nodes = new long[] { 2, 3 }
            };
            var from = new Way()
            {
                Id = 1,
                Nodes = new long[] { 1, 2 }
            };
            var to = new Way()
            {
                Id = 2,
                Nodes = new long[] { 3, 4 }
            };
            var restriction = new Relation()
            {
                Members = new RelationMember[]
                {
                    new RelationMember()
                    {
                        Id = 3,
                        Role = "via",
                        Type = OsmGeoType.Way
                    },
                    new RelationMember()
                    {
                        Id = 1,
                        Role = "from",
                        Type = OsmGeoType.Way
                    },
                    new RelationMember()
                    {
                        Id = 2,
                        Role = "to",
                        Type = OsmGeoType.Way
                    }
                },
                Tags = new TagsCollection(
                    new Tag("type", "restriction"))
            };

            processor.FirstPass(restriction);
            processor.SecondPass(via);
            processor.SecondPass(from);
            processor.SecondPass(to);
            processor.SecondPass(restriction);
        }

        /// <summary>
        /// Tests a perfectly modelled restriction for a motorcar.
        /// </summary>
        [Test]
        public void TestPerfectRestrictionMotorcar()
        {
            var processor = new RestrictionProcessor(
                new string[] { }, (node) => (uint)node, (vehicleType, sequence) =>
                {
                    Assert.AreEqual("motorcar", vehicleType);
                    Assert.IsNotNull(sequence);
                    Assert.AreEqual(3, sequence.Count);
                    Assert.AreEqual(1, sequence[0]);
                    Assert.AreEqual(2, sequence[1]);
                    Assert.AreEqual(3, sequence[2]);
                });

            var via = new Node()
            {
                Id = 2
            };
            var from = new Way()
            {
                Id = 1,
                Nodes = new long[] { 1, 2 }
            };
            var to = new Way()
            {
                Id = 2,
                Nodes = new long[] { 2, 3 }
            };
            var restriction = new Relation()
            {
                Members = new RelationMember[]
                {
                    new RelationMember()
                    {
                        Id = 2,
                        Role = "via",
                        Type = OsmGeoType.Node
                    },
                    new RelationMember()
                    {
                        Id = 1,
                        Role = "from",
                        Type = OsmGeoType.Way
                    },
                    new RelationMember()
                    {
                        Id = 2,
                        Role = "to",
                        Type = OsmGeoType.Way
                    }
                },
                Tags = new TagsCollection(
                    new Tag("type", "restriction:motorcar"))
            };

            processor.FirstPass(restriction);
            processor.SecondPass(via);
            processor.SecondPass(from);
            processor.SecondPass(to);
            processor.SecondPass(restriction);
        }
    }
}