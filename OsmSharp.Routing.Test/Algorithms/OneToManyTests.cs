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
using OsmSharp.Routing.Algorithms;
using OsmSharp.Routing.Algorithms.Default;
using OsmSharp.Routing.Data;
using OsmSharp.Routing.Graphs;
using OsmSharp.Routing.Profiles;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Test.Algorithms
{
    /// <summary>
    /// Executes tests
    /// </summary>
    [TestFixture]
    class OneToManyTests
    {
        /// <summary>
        /// Tests shortest path calculations on just one edge.
        /// </summary>
        /// <remarks>
        /// Situation:
        ///  (0)---100m----(1) @ 100km/h
        /// </remarks>
        [Test]
        public void TestOneEdge()
        {
            // build graph.
            var graph = new Graph(EdgeDataSerializer.Size);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddEdge(0, 1, EdgeDataSerializer.Serialize(new EdgeData()
            {
                Distance = 100,
                Profile = 1
            }));

            // build speed profile function.
            var speed = 100f / 3.6f;
            Func<ushort, OsmSharp.Routing.Profiles.Factor> getFactor = (x) =>
            {
                return new OsmSharp.Routing.Profiles.Factor()
                {
                    Direction = 0,
                    Value = 1.0f / speed
                };
            };

            // run algorithm.
            var algorithm = new OneToMany(graph, getFactor, new Path[] { new Path(0) },
                new List<IEnumerable<Path>>(new Path[][] { new Path[] { new Path(1) } }), float.MaxValue);
            algorithm.Run();

            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.AreEqual(1, algorithm.GetBestVertex(0));
            Assert.AreEqual(getFactor(1).Value * 100, algorithm.GetBestWeight(0), 0.001f);
            Assert.AreEqual(new uint[] { 0, 1 }, algorithm.GetPath(0).ToArray());
        }

        /// <summary>
        /// Tests one to many calculations between vertices on a triangle.
        /// </summary>
        /// <remarks>
        /// Situation:
        ///  (0)----100m----(1)
        ///   \             /
        ///    \           /           
        ///     \         /
        ///     100m    100m
        ///       \     /
        ///        \   /
        ///         (2)
        /// 
        /// Result:
        /// 
        ///     [  0,100,100]
        ///     [100,  0,100]
        ///     [100,100,  0]
        ///     
        /// </remarks>
        [Test]
        public void TestThreeEdges()
        {
            // build graph.
            var graph = new Graph(EdgeDataSerializer.Size);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddEdge(0, 1, EdgeDataSerializer.Serialize(new EdgeData()
            {
                Distance = 100,
                Profile = 1
            }));
            graph.AddEdge(1, 2, EdgeDataSerializer.Serialize(new EdgeData()
            {
                Distance = 100,
                Profile = 1
            }));
            graph.AddEdge(0, 2, EdgeDataSerializer.Serialize(new EdgeData()
            {
                Distance = 100,
                Profile = 1
            }));

            // build speed profile function.
            var speed = 100f / 3.6f;
            Func<ushort, Factor> getFactor = (x) =>
            {
                return new Factor()
                {
                    Direction = 0,
                    Value = 1.0f / speed
                };
            };

            // run algorithm 0->(1, 2).
            var algorithm = new OneToMany(graph, getFactor, new Path[] { new Path(0) },
                new List<IEnumerable<Path>>(
                    new Path[][] { 
                        new Path[] { new Path(1) },
                        new Path[] { new Path(2) }
                    }), float.MaxValue);
            algorithm.Run();

            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.AreEqual(1, algorithm.GetBestVertex(0));
            Assert.AreEqual(getFactor(1).Value * 100, algorithm.GetBestWeight(0), 0.001f);
            Assert.AreEqual(new uint[] { 0, 1 }, algorithm.GetPath(0).ToArray());
            Assert.AreEqual(2, algorithm.GetBestVertex(1));
            Assert.AreEqual(getFactor(1).Value * 100, algorithm.GetBestWeight(1), 0.001f);
            Assert.AreEqual(new uint[] { 0, 2 }, algorithm.GetPath(1).ToArray());

            // run algorithm 1->(0, 2).
            algorithm = new OneToMany(graph, getFactor, new Path[] { new Path(1) },
                new List<IEnumerable<Path>>(
                    new Path[][] { 
                        new Path[] { new Path(0) },
                        new Path[] { new Path(2) }
                    }), float.MaxValue);
            algorithm.Run();

            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.AreEqual(0, algorithm.GetBestVertex(0));
            Assert.AreEqual(getFactor(1).Value * 100, algorithm.GetBestWeight(0), 0.001f);
            Assert.AreEqual(new uint[] { 1, 0 }, algorithm.GetPath(0).ToArray());
            Assert.AreEqual(2, algorithm.GetBestVertex(1));
            Assert.AreEqual(getFactor(1).Value * 100, algorithm.GetBestWeight(1), 0.001f);
            Assert.AreEqual(new uint[] { 1, 2 }, algorithm.GetPath(1).ToArray());

            // run algorithm 2->(0, 1).
            algorithm = new OneToMany(graph, getFactor, new Path[] { new Path(2) },
                new List<IEnumerable<Path>>(
                    new Path[][] { 
                        new Path[] { new Path(0) },
                        new Path[] { new Path(1) }
                    }), float.MaxValue);
            algorithm.Run();

            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.AreEqual(0, algorithm.GetBestVertex(0));
            Assert.AreEqual(getFactor(1).Value * 100, algorithm.GetBestWeight(0), 0.001f);
            Assert.AreEqual(new uint[] { 2, 0 }, algorithm.GetPath(0).ToArray());
            Assert.AreEqual(1, algorithm.GetBestVertex(1));
            Assert.AreEqual(getFactor(1).Value * 100, algorithm.GetBestWeight(1), 0.001f);
            Assert.AreEqual(new uint[] { 2, 1 }, algorithm.GetPath(1).ToArray());
        }
    }
}