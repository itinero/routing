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
using Itinero.Graphs.Geometric;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Itinero.Test.Graphs.Geometric
{
    /// <summary>
    /// Tests the graph implementation.
    /// </summary>
    [TestFixture]
    public class GeometricGraphTests
    {
        /// <summary>
        /// Tests argument exceptions.
        /// </summary>
        [Test]
        public void TestArgumentExceptions()
        {
            // create graph with one vertex and start adding 2.
            var graph = new GeometricGraph(1, 2);

            // make sure to add 1 and 2.
            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 1, 1);
            graph.AddEdge(0, 1, new uint[] { 1 }, null);

            Assert.Catch<ArgumentOutOfRangeException>(() =>
            {
                graph.GetEdgeEnumerator(2);
            });
        }

        /// <summary>
        /// Tests adding an edge.
        /// </summary>
        [Test]
        public void TestAddEdge()
        {
            var graph = new GeometricGraph(1, 100);
            graph.Graph.MarkAsMulti();

            // add edge.
            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 1, 1);
            var edgeId1 = graph.AddEdge(0, 1, new uint[] { 10 }, null);
            Assert.AreEqual(0, edgeId1);

            // verify all edges.
            var edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(10, edges.First().Data[0]);
            Assert.AreEqual(edgeId1, edges.First().Id);
            Assert.AreEqual(0, edges.First().From);
            Assert.AreEqual(1, edges.First().To);

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(true, edges.First().DataInverted);
            Assert.AreEqual(10, edges.First().Data[0]);
            Assert.AreEqual(edgeId1, edges.First().Id);
            Assert.AreEqual(1, edges.First().From);
            Assert.AreEqual(0, edges.First().To);

            // add another edge.
            graph.AddVertex(2, 2, 2);
            var edgeId2 = graph.AddEdge(1, 2, new uint[] { 20 }, null);

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(10, edges.First().Data[0]);
            Assert.AreEqual(edgeId1, edges.First().Id);
            Assert.AreEqual(0, edges.First().From);
            Assert.AreEqual(1, edges.First().To);

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.To == 0));
            Assert.AreEqual(true, edges.First(x => x.To == 0).DataInverted);
            Assert.AreEqual(10, edges.First(x => x.To == 0).Data[0]);
            Assert.AreEqual(edgeId1, edges.First(x => x.To == 0).Id);
            Assert.AreEqual(1, edges.First(x => x.To == 0).From);
            Assert.IsTrue(edges.Any(x => x.To == 2));
            Assert.AreEqual(20, edges.First(x => x.To == 2).Data[0]);
            Assert.AreEqual(edgeId2, edges.First(x => x.To == 2).Id);
            Assert.AreEqual(1, edges.First(x => x.To == 2).From);

            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(true, edges.First().DataInverted);
            Assert.AreEqual(20, edges.First().Data[0]);
            Assert.AreEqual(edgeId2, edges.First().Id);
            Assert.AreEqual(2, edges.First().From);
            Assert.AreEqual(1, edges.First().To);

            // add another edge.
            graph.AddVertex(3, 3, 3);
            var edgeId3 = graph.AddEdge(1, 3, new uint[] { 30 }, null);

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(10, edges.First().Data[0]);
            Assert.AreEqual(edgeId1, edges.First().Id);
            Assert.AreEqual(0, edges.First().From);
            Assert.AreEqual(1, edges.First().To);

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(3, edges.Count());
            Assert.IsTrue(edges.Any(x => x.To == 0));
            Assert.AreEqual(true, edges.First(x => x.To == 0).DataInverted);
            Assert.AreEqual(10, edges.First(x => x.To == 0).Data[0]);
            Assert.AreEqual(edgeId1, edges.First(x => x.To == 0).Id);
            Assert.AreEqual(1, edges.First(x => x.To == 0).From);
            Assert.IsTrue(edges.Any(x => x.To == 2));
            Assert.AreEqual(20, edges.First(x => x.To == 2).Data[0]);
            Assert.AreEqual(edgeId2, edges.First(x => x.To == 2).Id);
            Assert.AreEqual(1, edges.First(x => x.To == 2).From);
            Assert.IsTrue(edges.Any(x => x.To == 3));
            Assert.AreEqual(30, edges.First(x => x.To == 3).Data[0]);
            Assert.AreEqual(edgeId3, edges.First(x => x.To == 3).Id);
            Assert.AreEqual(1, edges.First(x => x.To == 3).From);

            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(edgeId2, edges.First().Id);
            Assert.AreEqual(true, edges.First().DataInverted);
            Assert.AreEqual(20, edges.First().Data[0]);
            Assert.AreEqual(2, edges.First().From);
            Assert.AreEqual(1, edges.First().To);

            edges = graph.GetEdgeEnumerator(3);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(true, edges.First().DataInverted);
            Assert.AreEqual(30, edges.First().Data[0]);
            Assert.AreEqual(edgeId3, edges.First().Id);
            Assert.AreEqual(3, edges.First().From);
            Assert.AreEqual(1, edges.First().To);

            // add another edge but in reverse.
            var edgeId4 = graph.AddEdge(3, 1, new uint[] { 31 }, null);

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(10, edges.First().Data[0]);
            Assert.AreEqual(edgeId1, edges.First().Id);
            Assert.AreEqual(0, edges.First().From);
            Assert.AreEqual(1, edges.First().To);

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(4, edges.Count());
            Assert.IsTrue(edges.Any(x => x.To == 0));
            Assert.AreEqual(true, edges.First(x => x.To == 0).DataInverted);
            Assert.AreEqual(10, edges.First(x => x.To == 0).Data[0]);
            Assert.AreEqual(edgeId1, edges.First(x => x.To == 0).Id);
            Assert.AreEqual(1, edges.First(x => x.To == 0).From);
            Assert.IsTrue(edges.Any(x => x.To == 2));
            Assert.AreEqual(20, edges.First(x => x.To == 2).Data[0]);
            Assert.AreEqual(edgeId2, edges.First(x => x.To == 2).Id);
            Assert.AreEqual(1, edges.First(x => x.To == 2).From);
            Assert.IsTrue(edges.Any(x => x.To == 3 && x.Data[0] == 30));
            Assert.AreEqual(false, edges.First(x => x.To == 3 && x.Data[0] == 30).DataInverted);
            Assert.AreEqual(30, edges.First(x => x.To == 3 && x.Data[0] == 30).Data[0]);
            Assert.AreEqual(edgeId3, edges.First(x => x.To == 3 && x.Data[0] == 30).Id);
            Assert.AreEqual(1, edges.First(x => x.To == 3 && x.Data[0] == 30).From);
            Assert.IsTrue(edges.Any(x => x.To == 3 && x.Data[0] == 31));
            Assert.AreEqual(true, edges.First(x => x.To == 3 && x.Data[0] == 31).DataInverted);
            Assert.AreEqual(31, edges.First(x => x.To == 3 && x.Data[0] == 31).Data[0]);
            Assert.AreEqual(edgeId4, edges.First(x => x.To == 3 && x.Data[0] == 31).Id);
            Assert.AreEqual(1, edges.First(x => x.To == 3 && x.Data[0] == 31).From);

            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(edgeId2, edges.First().Id);
            Assert.AreEqual(true, edges.First().DataInverted);
            Assert.AreEqual(20, edges.First().Data[0]);
            Assert.AreEqual(2, edges.First().From);
            Assert.AreEqual(1, edges.First().To);

            edges = graph.GetEdgeEnumerator(3);
            Assert.AreEqual(2, edges.Count());
            Assert.AreEqual(30, edges.First(x => x.To == 1 && x.Data[0] == 30).Data[0]);
            Assert.AreEqual(edgeId3, edges.First(x => x.To == 1 && x.Data[0] == 30).Id);
            Assert.AreEqual(3, edges.First(x => x.To == 1 && x.Data[0] == 30).From);
            Assert.AreEqual(1, edges.First(x => x.To == 1 && x.Data[0] == 30).To);
            Assert.AreEqual(31, edges.First(x => x.To == 1 && x.Data[0] == 31).Data[0]);
            Assert.AreEqual(edgeId4, edges.First(x => x.To == 1 && x.Data[0] == 31).Id);
            Assert.AreEqual(3, edges.First(x => x.To == 1 && x.Data[0] == 31).From);
            Assert.AreEqual(1, edges.First(x => x.To == 1 && x.Data[0] == 31).To);

            // add another edge and start a new island.
            uint vertex4 = 4;
            uint vertex5 = 5;
            graph.AddVertex(vertex4, 4, 4);
            graph.AddVertex(vertex5, 5, 5);
            var edge5Id = graph.AddEdge(vertex4, vertex5, new uint[] { 40 }, null);

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(10, edges.First().Data[0]);
            Assert.AreEqual(edgeId1, edges.First().Id);
            Assert.AreEqual(0, edges.First().From);
            Assert.AreEqual(1, edges.First().To);

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(4, edges.Count());
            Assert.IsTrue(edges.Any(x => x.To == 0));
            Assert.AreEqual(true, edges.First(x => x.To == 0).DataInverted);
            Assert.AreEqual(10, edges.First(x => x.To == 0).Data[0]);
            Assert.AreEqual(edgeId1, edges.First(x => x.To == 0).Id);
            Assert.AreEqual(1, edges.First(x => x.To == 0).From);
            Assert.IsTrue(edges.Any(x => x.To == 2));
            Assert.AreEqual(20, edges.First(x => x.To == 2).Data[0]);
            Assert.AreEqual(edgeId2, edges.First(x => x.To == 2).Id);
            Assert.AreEqual(1, edges.First(x => x.To == 2).From);
            Assert.IsTrue(edges.Any(x => x.To == 3 && x.Data[0] == 30));
            Assert.AreEqual(false, edges.First(x => x.To == 3 && x.Data[0] == 30).DataInverted);
            Assert.AreEqual(30, edges.First(x => x.To == 3 && x.Data[0] == 30).Data[0]);
            Assert.AreEqual(edgeId3, edges.First(x => x.To == 3 && x.Data[0] == 30).Id);
            Assert.AreEqual(1, edges.First(x => x.To == 3 && x.Data[0] == 30).From);
            Assert.IsTrue(edges.Any(x => x.To == 3 && x.Data[0] == 31));
            Assert.AreEqual(true, edges.First(x => x.To == 3 && x.Data[0] == 31).DataInverted);
            Assert.AreEqual(31, edges.First(x => x.To == 3 && x.Data[0] == 31).Data[0]);
            Assert.AreEqual(edgeId4, edges.First(x => x.To == 3 && x.Data[0] == 31).Id);
            Assert.AreEqual(1, edges.First(x => x.To == 3 && x.Data[0] == 31).From);

            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(edgeId2, edges.First().Id);
            Assert.AreEqual(true, edges.First().DataInverted);
            Assert.AreEqual(20, edges.First().Data[0]);
            Assert.AreEqual(2, edges.First().From);
            Assert.AreEqual(1, edges.First().To);

            edges = graph.GetEdgeEnumerator(3);
            Assert.AreEqual(2, edges.Count());
            Assert.AreEqual(30, edges.First(x => x.To == 1 && x.Data[0] == 30).Data[0]);
            Assert.AreEqual(edgeId3, edges.First(x => x.To == 1 && x.Data[0] == 30).Id);
            Assert.AreEqual(3, edges.First(x => x.To == 1 && x.Data[0] == 30).From);
            Assert.AreEqual(1, edges.First(x => x.To == 1 && x.Data[0] == 30).To);
            Assert.AreEqual(31, edges.First(x => x.To == 1 && x.Data[0] == 31).Data[0]);
            Assert.AreEqual(edgeId4, edges.First(x => x.To == 1 && x.Data[0] == 31).Id);
            Assert.AreEqual(3, edges.First(x => x.To == 1 && x.Data[0] == 31).From);
            Assert.AreEqual(1, edges.First(x => x.To == 1 && x.Data[0] == 31).To);

            edges = graph.GetEdgeEnumerator(vertex4);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(40, edges.First().Data[0]);
            Assert.AreEqual(edge5Id, edges.First().Id);
            Assert.AreEqual(vertex4, edges.First().From);
            Assert.AreEqual(vertex5, edges.First().To);

            edges = graph.GetEdgeEnumerator(vertex5);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(true, edges.First().DataInverted);
            Assert.AreEqual(40, edges.First().Data[0]);
            Assert.AreEqual(edge5Id, edges.First().Id);
            Assert.AreEqual(vertex5, edges.First().From);
            Assert.AreEqual(vertex4, edges.First().To);

            // connect the islands.
            var edgeId5 = graph.AddEdge(vertex5, 3, new uint[] { 50 }, null);

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(10, edges.First().Data[0]);
            Assert.AreEqual(edgeId1, edges.First().Id);
            Assert.AreEqual(0, edges.First().From);
            Assert.AreEqual(1, edges.First().To);

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(4, edges.Count());
            Assert.IsTrue(edges.Any(x => x.To == 0));
            Assert.AreEqual(true, edges.First(x => x.To == 0).DataInverted);
            Assert.AreEqual(10, edges.First(x => x.To == 0).Data[0]);
            Assert.AreEqual(edgeId1, edges.First(x => x.To == 0).Id);
            Assert.AreEqual(1, edges.First(x => x.To == 0).From);
            Assert.IsTrue(edges.Any(x => x.To == 2));
            Assert.AreEqual(20, edges.First(x => x.To == 2).Data[0]);
            Assert.AreEqual(edgeId2, edges.First(x => x.To == 2).Id);
            Assert.AreEqual(1, edges.First(x => x.To == 2).From);
            Assert.IsTrue(edges.Any(x => x.To == 3 && x.Data[0] == 30));
            Assert.AreEqual(false, edges.First(x => x.To == 3 && x.Data[0] == 30).DataInverted);
            Assert.AreEqual(30, edges.First(x => x.To == 3 && x.Data[0] == 30).Data[0]);
            Assert.AreEqual(edgeId3, edges.First(x => x.To == 3 && x.Data[0] == 30).Id);
            Assert.AreEqual(1, edges.First(x => x.To == 3 && x.Data[0] == 30).From);
            Assert.IsTrue(edges.Any(x => x.To == 3 && x.Data[0] == 31));
            Assert.AreEqual(true, edges.First(x => x.To == 3 && x.Data[0] == 31).DataInverted);
            Assert.AreEqual(31, edges.First(x => x.To == 3 && x.Data[0] == 31).Data[0]);
            Assert.AreEqual(edgeId4, edges.First(x => x.To == 3 && x.Data[0] == 31).Id);
            Assert.AreEqual(1, edges.First(x => x.To == 3 && x.Data[0] == 31).From);

            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(edgeId2, edges.First().Id);
            Assert.AreEqual(true, edges.First().DataInverted);
            Assert.AreEqual(20, edges.First().Data[0]);
            Assert.AreEqual(2, edges.First().From);
            Assert.AreEqual(1, edges.First().To);

            edges = graph.GetEdgeEnumerator(3);
            Assert.AreEqual(3, edges.Count());
            Assert.AreEqual(30, edges.First(x => x.To == 1 && x.Data[0] == 30).Data[0]);
            Assert.AreEqual(edgeId3, edges.First(x => x.To == 1 && x.Data[0] == 30).Id);
            Assert.AreEqual(3, edges.First(x => x.To == 1 && x.Data[0] == 30).From);
            Assert.AreEqual(1, edges.First(x => x.To == 1 && x.Data[0] == 30).To);
            Assert.AreEqual(31, edges.First(x => x.To == 1 && x.Data[0] == 31).Data[0]);
            Assert.AreEqual(edgeId4, edges.First(x => x.To == 1 && x.Data[0] == 31).Id);
            Assert.AreEqual(3, edges.First(x => x.To == 1 && x.Data[0] == 31).From);
            Assert.AreEqual(1, edges.First(x => x.To == 1 && x.Data[0] == 31).To);
            Assert.IsTrue(edges.Any(x => x.To == vertex5));
            Assert.AreEqual(true, edges.First(x => x.To == vertex5).DataInverted);
            Assert.AreEqual(50, edges.First(x => x.To == vertex5).Data[0]);
            Assert.AreEqual(edgeId5, edges.First(x => x.To == vertex5).Id);
            Assert.AreEqual(3, edges.First(x => x.To == vertex5).From);

            edges = graph.GetEdgeEnumerator(vertex4);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(40, edges.First().Data[0]);
            Assert.AreEqual(edge5Id, edges.First().Id);
            Assert.AreEqual(vertex4, edges.First().From);
            Assert.AreEqual(vertex5, edges.First().To);

            edges = graph.GetEdgeEnumerator(vertex5);
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.To == vertex4));
            Assert.AreEqual(true, edges.First(x => x.To == vertex4).DataInverted);
            Assert.AreEqual(40, edges.First(x => x.To == vertex4).Data[0]);
            Assert.AreEqual(edge5Id, edges.First(x => x.To == vertex4).Id);
            Assert.AreEqual(vertex5, edges.First(x => x.To == vertex4).From);
            Assert.IsTrue(edges.Any(x => x.To == 3));
            Assert.AreEqual(50, edges.First(x => x.To == 3).Data[0]);
            Assert.AreEqual(edgeId5, edges.First(x => x.To == 3).Id);
            Assert.AreEqual(vertex5, edges.First(x => x.To == 3).From);
        }

        /// <summary>
        /// Tests adding an edge with a shape.
        /// </summary>
        [Test]
        public void TestAddEdgeShape()
        {
            var graph = new GeometricGraph(2, 100);
            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 1, 1);
            graph.AddVertex(2, 0, 1);
            graph.AddVertex(3, 0, 2);
            graph.AddEdge(0, 1, new uint[] { 0, 10 }, new Coordinate(0.25f, 0.25f),
                    new Coordinate(0.5f, 0.5f),
                    new Coordinate(0.75f, 0.75f));
            graph.AddEdge(1, 2, new uint[] { 10, 20 }, new Coordinate(0.75f, 1),
                new Coordinate(0.5f, 1),
                new Coordinate(0.25f, 1));
            graph.AddEdge(2, 3, new uint[] { 20, 30 }, new Coordinate(0, 1.25f),
                new Coordinate(0, 1.5f),
                new Coordinate(0, 1.75f));

            // verify all edges.
            var edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().To);
            Assert.IsNotNull(edges.First().Shape);
            var shape = new List<Coordinate>(edges.First().Shape);
            Assert.AreEqual(0.25, shape[0].Latitude);
            Assert.AreEqual(0.25, shape[0].Longitude);
            Assert.AreEqual(0.5, shape[1].Latitude);
            Assert.AreEqual(0.5, shape[1].Longitude);
            Assert.AreEqual(0.75, shape[2].Latitude);
            Assert.AreEqual(0.75, shape[2].Longitude);
            shape = new List<Coordinate>(edges.First().Shape.Reverse());
            Assert.AreEqual(0.25, shape[2].Latitude);
            Assert.AreEqual(0.25, shape[2].Longitude);
            Assert.AreEqual(0.5, shape[1].Latitude);
            Assert.AreEqual(0.5, shape[1].Longitude);
            Assert.AreEqual(0.75, shape[0].Latitude);
            Assert.AreEqual(0.75, shape[0].Longitude);

            edges = graph.GetEdgeEnumerator(1);
            shape = new List<Coordinate>(edges.First(x => x.To == 0).Shape);
            Assert.AreEqual(0.25, shape[0].Latitude);
            Assert.AreEqual(0.25, shape[0].Longitude);
            Assert.AreEqual(0.5, shape[1].Latitude);
            Assert.AreEqual(0.5, shape[1].Longitude);
            Assert.AreEqual(0.75, shape[2].Latitude);
            Assert.AreEqual(0.75, shape[2].Longitude);
            shape = new List<Coordinate>(edges.First(x => x.To == 0).Shape.Reverse());
            Assert.AreEqual(0.25, shape[2].Latitude);
            Assert.AreEqual(0.25, shape[2].Longitude);
            Assert.AreEqual(0.5, shape[1].Latitude);
            Assert.AreEqual(0.5, shape[1].Longitude);
            Assert.AreEqual(0.75, shape[0].Latitude);
            Assert.AreEqual(0.75, shape[0].Longitude);
            shape = new List<Coordinate>(edges.First(x => x.To == 2).Shape);
            Assert.AreEqual(0.75, shape[0].Latitude);
            Assert.AreEqual(1, shape[0].Longitude);
            Assert.AreEqual(0.5, shape[1].Latitude);
            Assert.AreEqual(1, shape[1].Longitude);
            Assert.AreEqual(0.25, shape[2].Latitude);
            Assert.AreEqual(1, shape[2].Longitude);
            shape = new List<Coordinate>(edges.First(x => x.To == 2).Shape.Reverse());
            Assert.AreEqual(0.75, shape[2].Latitude);
            Assert.AreEqual(1, shape[2].Longitude);
            Assert.AreEqual(0.5, shape[1].Latitude);
            Assert.AreEqual(1, shape[1].Longitude);
            Assert.AreEqual(0.25, shape[0].Latitude);
            Assert.AreEqual(1, shape[0].Longitude);

            edges = graph.GetEdgeEnumerator(2);
            shape = new List<Coordinate>(edges.First(x => x.To == 1).Shape);
            Assert.AreEqual(0.75, shape[0].Latitude);
            Assert.AreEqual(1, shape[0].Longitude);
            Assert.AreEqual(0.5, shape[1].Latitude);
            Assert.AreEqual(1, shape[1].Longitude);
            Assert.AreEqual(0.25, shape[2].Latitude);
            Assert.AreEqual(1, shape[2].Longitude);
            shape = new List<Coordinate>(edges.First(x => x.To == 1).Shape.Reverse());
            Assert.AreEqual(0.75, shape[2].Latitude);
            Assert.AreEqual(1, shape[2].Longitude);
            Assert.AreEqual(0.5, shape[1].Latitude);
            Assert.AreEqual(1, shape[1].Longitude);
            Assert.AreEqual(0.25, shape[0].Latitude);
            Assert.AreEqual(1, shape[0].Longitude);
            shape = new List<Coordinate>(edges.First(x => x.To == 3).Shape);
            Assert.AreEqual(0, shape[0].Latitude);
            Assert.AreEqual(1.25, shape[0].Longitude);
            Assert.AreEqual(0, shape[1].Latitude);
            Assert.AreEqual(1.5, shape[1].Longitude);
            Assert.AreEqual(0, shape[2].Latitude);
            Assert.AreEqual(1.75, shape[2].Longitude);
            shape = new List<Coordinate>(edges.First(x => x.To == 3).Shape.Reverse());
            Assert.AreEqual(0, shape[2].Latitude);
            Assert.AreEqual(1.25, shape[2].Longitude);
            Assert.AreEqual(0, shape[1].Latitude);
            Assert.AreEqual(1.5, shape[1].Longitude);
            Assert.AreEqual(0, shape[0].Latitude);
            Assert.AreEqual(1.75, shape[0].Longitude);

            edges = graph.GetEdgeEnumerator(3);
            shape = new List<Coordinate>(edges.First().Shape);
            Assert.AreEqual(0, shape[0].Latitude);
            Assert.AreEqual(1.25, shape[0].Longitude);
            Assert.AreEqual(0, shape[1].Latitude);
            Assert.AreEqual(1.5, shape[1].Longitude);
            Assert.AreEqual(0, shape[2].Latitude);
            Assert.AreEqual(1.75, shape[2].Longitude);
            shape = new List<Coordinate>(edges.First().Shape.Reverse());
            Assert.AreEqual(0, shape[2].Latitude);
            Assert.AreEqual(1.25, shape[2].Longitude);
            Assert.AreEqual(0, shape[1].Latitude);
            Assert.AreEqual(1.5, shape[1].Longitude);
            Assert.AreEqual(0, shape[0].Latitude);
            Assert.AreEqual(1.75, shape[0].Longitude);
        }

        /// <summary>
        /// Tests getting all shape points for an edge.
        /// </summary>
        [Test]
        public void TestGetShape()
        {
            var graph = new GeometricGraph(2, 100);
            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 1, 1);
            graph.AddVertex(2, 0, 1);
            graph.AddVertex(3, 0, 2);
            var edge1 = graph.AddEdge(0, 1, new uint[] { 0, 10 }, 
                    new Coordinate(0.25f, 0.25f),
                    new Coordinate(0.50f, 0.50f),
                    new Coordinate(0.75f, 0.75f));

            var shape = graph.GetShape(graph.GetEdge(edge1));
            Assert.IsNotNull(shape);
            Assert.AreEqual(5, shape.Count);
            Assert.AreEqual(0, shape[0].Latitude);
            Assert.AreEqual(0, shape[0].Longitude);
            Assert.AreEqual(0.25, shape[1].Latitude);
            Assert.AreEqual(0.25, shape[1].Longitude);
            Assert.AreEqual(0.5, shape[2].Latitude);
            Assert.AreEqual(0.5, shape[2].Longitude);
            Assert.AreEqual(0.75, shape[3].Latitude);
            Assert.AreEqual(0.75, shape[3].Longitude);
            Assert.AreEqual(1, shape[4].Latitude);
            Assert.AreEqual(1, shape[4].Longitude);
            shape = graph.GetShape(graph.GetEdgeEnumerator(0).First());
            Assert.IsNotNull(shape);
            Assert.AreEqual(5, shape.Count);
            Assert.AreEqual(0, shape[0].Latitude);
            Assert.AreEqual(0, shape[0].Longitude);
            Assert.AreEqual(0.25, shape[1].Latitude);
            Assert.AreEqual(0.25, shape[1].Longitude);
            Assert.AreEqual(0.5, shape[2].Latitude);
            Assert.AreEqual(0.5, shape[2].Longitude);
            Assert.AreEqual(0.75, shape[3].Latitude);
            Assert.AreEqual(0.75, shape[3].Longitude);
            Assert.AreEqual(1, shape[4].Latitude);
            Assert.AreEqual(1, shape[4].Longitude);
            shape = graph.GetShape(graph.GetEdgeEnumerator(1).First(x => x.To ==0));
            Assert.IsNotNull(shape);
            Assert.AreEqual(5, shape.Count);
            Assert.AreEqual(1, shape[0].Latitude);
            Assert.AreEqual(1, shape[0].Longitude);
            Assert.AreEqual(0.75, shape[1].Latitude);
            Assert.AreEqual(0.75, shape[1].Longitude);
            Assert.AreEqual(0.5, shape[2].Latitude);
            Assert.AreEqual(0.5, shape[2].Longitude);
            Assert.AreEqual(0.25, shape[3].Latitude);
            Assert.AreEqual(0.25, shape[3].Longitude);
            Assert.AreEqual(0, shape[4].Latitude);
            Assert.AreEqual(0, shape[4].Longitude);
        }

        /// <summary>
        /// Tests setting a vertex.
        /// </summary>
        [Test]
        public void TestSetVertex()
        {
            var graph = new GeometricGraph(1, 100);

            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 1, 1);
            graph.AddVertex(2, 2, 2);

            float latitude, longitude;
            Assert.IsTrue(graph.GetVertex(0, out latitude, out longitude));
            Assert.AreEqual(0, latitude);
            Assert.AreEqual(0, longitude);
            Assert.AreEqual(0, graph.GetVertex(0).Latitude);
            Assert.AreEqual(0, graph.GetVertex(0).Longitude);
            Assert.IsTrue(graph.GetVertex(1, out latitude, out longitude));
            Assert.AreEqual(1, latitude);
            Assert.AreEqual(1, longitude);
            Assert.AreEqual(1, graph.GetVertex(1).Latitude);
            Assert.AreEqual(1, graph.GetVertex(1).Longitude);
            Assert.IsTrue(graph.GetVertex(2, out latitude, out longitude));
            Assert.AreEqual(2, latitude);
            Assert.AreEqual(2, longitude);
            Assert.AreEqual(2, graph.GetVertex(2).Latitude);
            Assert.AreEqual(2, graph.GetVertex(2).Longitude);

            Assert.IsFalse(graph.GetVertex(10000, out latitude, out longitude));
        }

        /// <summary>
        /// Tests setting a vertex.
        /// </summary>
        [Test]
        public void TestUpdateVertexWithElevation()
        {
            var graph = new GeometricGraph(1, 100);

            graph.AddVertex(0, 1, 1);
            graph.AddVertex(1, 10, 10);
            graph.AddVertex(2, 2, 2);

            graph.UpdateVertex(0, 0, 0, 10);
            graph.UpdateVertex(1, 1, 1, 20);
            graph.UpdateVertex(2, 2, 2, 30);

            float latitude, longitude;
            Assert.IsTrue(graph.GetVertex(0, out latitude, out longitude));
            Assert.AreEqual(0, latitude);
            Assert.AreEqual(0, longitude);
            Assert.AreEqual(0, graph.GetVertex(0).Latitude);
            Assert.AreEqual(0, graph.GetVertex(0).Longitude);
            Assert.AreEqual(10, graph.GetVertex(0).Elevation);
            Assert.IsTrue(graph.GetVertex(1, out latitude, out longitude));
            Assert.AreEqual(1, latitude);
            Assert.AreEqual(1, longitude);
            Assert.AreEqual(1, graph.GetVertex(1).Latitude);
            Assert.AreEqual(1, graph.GetVertex(1).Longitude);
            Assert.AreEqual(20, graph.GetVertex(1).Elevation);
            Assert.IsTrue(graph.GetVertex(2, out latitude, out longitude));
            Assert.AreEqual(2, latitude);
            Assert.AreEqual(2, longitude);
            Assert.AreEqual(2, graph.GetVertex(2).Latitude);
            Assert.AreEqual(2, graph.GetVertex(2).Longitude);
            Assert.AreEqual(30, graph.GetVertex(2).Elevation);
        }

        /// <summary>
        /// Tests get edge enumerator.
        /// </summary>
        [Test]
        public void TestGetEdgeEnumerator()
        {
            var graph = new GeometricGraph(1, 100);

            // add edges.
            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 0, 0);
            graph.AddVertex(2, 0, 0);
            graph.AddVertex(3, 0, 0);
            graph.AddVertex(4, 0, 0);
            graph.AddVertex(5, 0, 0);
            var edge1 = graph.AddEdge(0, 1, new uint[] { 10 }, null);
            var edge2 = graph.AddEdge(1, 2, new uint[] { 20 }, null);
            var edge3 = graph.AddEdge(1, 3, new uint[] { 30 }, null);
            var edge4 = graph.AddEdge(3, 4, new uint[] { 40 }, null);
            var edge5 = graph.AddEdge(4, 1, new uint[] { 50 }, null);
            var edge6 = graph.AddEdge(5, 1, new uint[] { 60 }, null);

            // get empty edge enumerator.
            var edges = graph.GetEdgeEnumerator();
            Assert.IsFalse(edges.HasData);

            // move to vertices and test result.
            Assert.IsTrue(edges.MoveTo(0));
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(10, edges.First().Data[0]);
            Assert.AreEqual(1, edges.First().To);

            Assert.IsTrue(edges.MoveTo(1));
            Assert.AreEqual(5, edges.Count());
            Assert.IsTrue(edges.Any(x => x.To == 0));
            Assert.AreEqual(true, edges.First(x => x.To == 0).DataInverted);
            Assert.AreEqual(10, edges.First(x => x.To == 0).Data[0]);
            Assert.AreEqual(edge1, edges.First(x => x.To == 0).Id);
            Assert.IsTrue(edges.Any(x => x.To == 2));
            Assert.AreEqual(20, edges.First(x => x.To == 2).Data[0]);
            Assert.AreEqual(edge2, edges.First(x => x.To == 2).Id);
            Assert.IsTrue(edges.Any(x => x.To == 3));
            Assert.AreEqual(30, edges.First(x => x.To == 3).Data[0]);
            Assert.AreEqual(edge3, edges.First(x => x.To == 3).Id);
            Assert.IsTrue(edges.Any(x => x.To == 4));
            Assert.AreEqual(true, edges.First(x => x.To == 4).DataInverted);
            Assert.AreEqual(50, edges.First(x => x.To == 4).Data[0]);
            Assert.AreEqual(edge5, edges.First(x => x.To == 4).Id);
            Assert.IsTrue(edges.Any(x => x.To == 5));
            Assert.AreEqual(true, edges.First(x => x.To == 5).DataInverted);
            Assert.AreEqual(60, edges.First(x => x.To == 5).Data[0]);
            Assert.AreEqual(edge6, edges.First(x => x.To == 5).Id);

            Assert.IsTrue(edges.MoveTo(2));
            Assert.AreEqual(1, edges.Count());
            Assert.IsTrue(edges.Any(x => x.To == 1));
            Assert.AreEqual(true, edges.First(x => x.To == 1).DataInverted);
            Assert.AreEqual(20, edges.First(x => x.To == 1).Data[0]);
            Assert.AreEqual(edge2, edges.First(x => x.To == 1).Id);

            Assert.IsTrue(edges.MoveTo(3));
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.To == 1));
            Assert.AreEqual(true, edges.First(x => x.To == 1).DataInverted);
            Assert.AreEqual(30, edges.First(x => x.To == 1).Data[0]);
            Assert.AreEqual(edge3, edges.First(x => x.To == 1).Id);
            Assert.IsTrue(edges.Any(x => x.To == 4));
            Assert.AreEqual(40, edges.First(x => x.To == 4).Data[0]);
            Assert.AreEqual(edge4, edges.First(x => x.To == 4).Id);

            Assert.IsTrue(edges.MoveTo(4));
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.To == 3));
            Assert.AreEqual(true, edges.First(x => x.To == 3).DataInverted);
            Assert.AreEqual(40, edges.First(x => x.To == 3).Data[0]);
            Assert.AreEqual(edge4, edges.First(x => x.To == 3).Id);
            Assert.IsTrue(edges.Any(x => x.To == 1));
            Assert.AreEqual(50, edges.First(x => x.To == 1).Data[0]);
            Assert.AreEqual(edge5, edges.First(x => x.To == 1).Id);

            Assert.IsTrue(edges.MoveTo(5));
            Assert.AreEqual(1, edges.Count());
            Assert.IsTrue(edges.Any(x => x.To == 1));
            Assert.AreEqual(60, edges.First(x => x.To == 1).Data[0]);
            Assert.AreEqual(edge6, edges.First(x => x.To == 1).Id);
        }

        /// <summary>
        /// Tests remove edges.
        /// </summary>
        [Test]
        public void TestRemoveEdge()
        {
            var graph = new GeometricGraph(1, 100);

            // add and remove edge.
            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 0, 0);
            graph.AddEdge(0, 1, new uint[] {1}, null);
            Assert.IsTrue(graph.RemoveEdges(0, 1) != 0);

            graph = new GeometricGraph(1, 100);

            // add and remove edge.
            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 0, 0);
            var edge = graph.AddEdge(0, 1, new uint[] { 10 }, null);
            Assert.IsTrue(graph.RemoveEdge(edge));
        }

        /// <summary>
        /// Tests remove edges.
        /// </summary>
        [Test]
        public void TestRemoveEdges()
        {
            var graph = new GeometricGraph(1, 100);

            // add and remove edge.
            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 0, 0);
            graph.AddEdge(0, 1, new uint[] { 10 }, null);
            Assert.AreEqual(1, graph.RemoveEdges(0));
            Assert.AreEqual(0, graph.RemoveEdges(1));

            // verify all edges.
            var edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(0, edges.Count());

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(0, edges.Count());

            graph = new GeometricGraph(1, 100);

            // add and remove edges.
            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 0, 0);
            graph.AddVertex(2, 0, 0);
            graph.AddEdge(0, 1, new uint[] { 10 }, null);
            graph.AddEdge(0, 2, new uint[] { 10 }, null);
            Assert.AreEqual(2, graph.RemoveEdges(0));

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(0, edges.Count());
            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(0, edges.Count());
            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(0, edges.Count());

            graph = new GeometricGraph(1, 100);

            // add and remove edges.
            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 0, 0);
            graph.AddVertex(2, 0, 0);
            graph.AddEdge(0, 1, new uint[] { 10 }, null);
            graph.AddEdge(0, 2, new uint[] { 20 }, null);
            graph.AddEdge(1, 2, new uint[] { 30 }, null);
            Assert.AreEqual(2, graph.RemoveEdges(0));

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(0, edges.Count());
            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(1, edges.Count());
            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(1, edges.Count());
        }

        /// <summary>
        /// Tests the vertex count.
        /// </summary>
        [Test]
        public void TestVertexCountAndTrim()
        {
            var graph = new GeometricGraph(1, 100);

            // add edge.
            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 0, 0);
            graph.AddEdge(0, 1, new uint[] { 10 }, null);

            // trim.
            graph.Trim();
            Assert.AreEqual(2, graph.VertexCount);
            Assert.AreEqual(1, graph.EdgeCount);

            graph = new GeometricGraph(1, 100);

            // add edge.
            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 0, 0);
            graph.AddVertex(11001, 0, 0);
            graph.AddEdge(0, 1, new uint[] { 10 }, null);
            graph.AddEdge(0, 11001, new uint[] { 10 }, null);

            // trim.
            graph.Trim();
            Assert.AreEqual(11002, graph.VertexCount);
            Assert.AreEqual(2, graph.EdgeCount);

            graph = new GeometricGraph(1, 100);

            // trim.
            graph.Trim(); // keep minimum one vertex.
            Assert.AreEqual(0, graph.VertexCount);
        }

        /// <summary>
        /// Tests the edge count.
        /// </summary>
        [Test]
        public void TestEdgeCount()
        {
            var graph = new GeometricGraph(1, 100);
            graph.Graph.MarkAsMulti();

            // add edge.
            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 0, 0);
            graph.AddEdge(0, 1, new uint[] { 10 }, null);
            Assert.AreEqual(1, graph.EdgeCount);

            graph = new GeometricGraph(1, 100);
            graph.Graph.MarkAsMulti();

            // add edge.
            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 0, 0);
            graph.AddVertex(11001, 0, 0);
            graph.AddEdge(0, 1, new uint[] { 10 }, null);
            graph.AddEdge(0, 11001, new uint[] { 10 }, null);
            Assert.AreEqual(2, graph.EdgeCount);

            graph.AddEdge(0, 11001, new uint[] { 20 }, null);
            Assert.AreEqual(3, graph.EdgeCount);

            Assert.AreEqual(2, graph.RemoveEdges(0, 11001));
            Assert.AreEqual(1, graph.EdgeCount);

            Assert.AreEqual(1, graph.RemoveEdges(0, 1));
            Assert.AreEqual(0, graph.EdgeCount);
        }

        /// <summary>
        /// Tests the compression.
        /// </summary>
        [Test]
        public void TestCompress()
        {
            var graph = new GeometricGraph(1, 100);

            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 0, 0);
            var edgeId = graph.AddEdge(0, 1, new uint[] { 10 }, new Coordinate()
                    {
                        Latitude = 1,
                        Longitude = 1
                    });
            graph.Compress();

            var edge = graph.GetEdge(edgeId);
            Assert.IsNotNull(edge);
            Assert.AreEqual(0, edge.From);
            Assert.AreEqual(1, edge.To);
            Assert.AreEqual(edgeId, edge.Id);
            Assert.AreEqual(1, edge.Shape.Count);
            Assert.AreEqual(1, edge.Shape.First().Latitude);
            Assert.AreEqual(1, edge.Shape.First().Longitude);
        }

        /// <summary>
        /// Tests the serialization.
        /// </summary>
        [Test]
        public void TestSerialize()
        {
            var graph = new GeometricGraph(1, 100);

            // add one edge.
            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 0, 0);
            graph.AddEdge(0, 1, new uint[] { 10 }, null);

            // serialize.
            graph.Compress();
            var expectedSize = graph.SizeInBytes;
            using (var stream = new System.IO.MemoryStream())
            {
                Assert.AreEqual(expectedSize, graph.Serialize(stream));
            }

            graph = new GeometricGraph(1, 100);

            // add one edge.
            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 0, 0);
            graph.AddVertex(2, 0, 0);
            graph.AddVertex(3, 0, 0);
            graph.AddVertex(4, 0, 0);
            graph.AddVertex(5, 0, 0);
            graph.AddEdge(0, 1, new uint[] { 10 }, null);
            graph.AddEdge(0, 2, new uint[] { 20 }, null);
            graph.AddEdge(0, 3, new uint[] { 30 }, null);
            graph.AddEdge(0, 4, new uint[] { 40 }, null);
            graph.AddEdge(5, 1, new uint[] { 50 }, null);
            graph.AddEdge(5, 2, new uint[] { 60 }, null);
            graph.AddEdge(5, 3, new uint[] { 70 }, null);
            graph.AddEdge(5, 4, new uint[] { 80 }, null);

            // serialize.
            graph.Compress();
            expectedSize = graph.SizeInBytes;
            using (var stream = new System.IO.MemoryStream())
            {
                Assert.AreEqual(expectedSize, graph.Serialize(stream));
            }
        }

        /// <summary>
        /// Tests the deserialization.
        /// </summary>
        [Test]
        public void TestDeserialize()
        {
            var graph = new GeometricGraph(1, 100);

            // add one edge.
            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 0, 0);
            graph.AddEdge(0, 1, new uint[] { 1 }, null);

            // serialize.
            using (var stream = new System.IO.MemoryStream())
            {
                var size = graph.Serialize(stream);
                stream.Seek(0, System.IO.SeekOrigin.Begin);

                var deserializedGraph = GeometricGraph.Deserialize(stream, GeometricGraphProfile.Default);
                Assert.AreEqual(size, stream.Position);

                Assert.AreEqual(2, deserializedGraph.VertexCount);
                Assert.AreEqual(1, deserializedGraph.EdgeCount);

                // verify all edges.
                var edges = deserializedGraph.GetEdgeEnumerator(0);
                Assert.AreEqual(1, edges.Count());
                Assert.AreEqual(1, edges.First().Data[0]);
                Assert.AreEqual(1, edges.First().To);

                edges = deserializedGraph.GetEdgeEnumerator(1);
                Assert.AreEqual(1, edges.Count());
                Assert.AreEqual(true, edges.First().DataInverted);
                Assert.AreEqual(1, edges.First().Data[0]);
                Assert.AreEqual(0, edges.First().To);
            }

            graph = new GeometricGraph(1, 100);

            // add one edge.
            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 0, 0);
            graph.AddVertex(2, 0, 0);
            graph.AddVertex(3, 0, 0);
            graph.AddVertex(4, 0, 0);
            graph.AddVertex(5, 0, 0);
            graph.AddEdge(0, 1, new uint[] { 1 }, null);
            graph.AddEdge(0, 2, new uint[] { 2 }, null);
            graph.AddEdge(0, 3, new uint[] { 3 }, null);
            graph.AddEdge(0, 4, new uint[] { 4 }, null);
            graph.AddEdge(5, 1, new uint[] { 5 }, null);
            graph.AddEdge(5, 2, new uint[] { 6 }, null);
            graph.AddEdge(5, 3, new uint[] { 7 }, null);
            graph.AddEdge(5, 4, new uint[] { 8 }, null);

            // serialize.
            using (var stream = new System.IO.MemoryStream())
            {
                var size = graph.Serialize(stream);
                stream.Seek(0, System.IO.SeekOrigin.Begin);

                var deserializedGraph = GeometricGraph.Deserialize(stream, GeometricGraphProfile.Default);
                Assert.AreEqual(size, stream.Position);

                Assert.AreEqual(6, deserializedGraph.VertexCount);
                Assert.AreEqual(8, deserializedGraph.EdgeCount);
            }
        }

        /// <summary>
        /// Tests switching two vertices.
        /// </summary>
        [Test]
        public void TestSwitch()
        {
            var graph = new GeometricGraph(1, 100);
            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 0, 0);
            graph.AddEdge(0, 1, new uint[] { 1 }, null);

            graph.Switch(0, 1);

            // verify all edges.
            var edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().To);
            Assert.AreEqual(true, edges.First().DataInverted);
            Assert.AreEqual(1, edges.First().Data[0]);
            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(0, edges.First().To);
            Assert.AreEqual(1, edges.First().Data[0]);

            graph = new GeometricGraph(1, 100);
            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 0, 0);
            graph.AddVertex(2, 0, 0);
            graph.AddVertex(3, 0, 0);
            graph.AddVertex(4, 0, 0);
            graph.AddVertex(5, 0, 0);
            graph.AddEdge(0, 1, new uint[] { 1 }, null);
            graph.AddEdge(0, 2, new uint[] { 2 }, null);
            graph.AddEdge(0, 3, new uint[] { 3 }, null);
            graph.AddEdge(0, 4, new uint[] { 4 }, null);
            graph.AddEdge(5, 1, new uint[] { 5 }, null);
            graph.AddEdge(5, 2, new uint[] { 6 }, null);
            graph.AddEdge(5, 3, new uint[] { 7 }, null);
            graph.AddEdge(5, 4, new uint[] { 8 }, null);

            graph.Switch(0, 1);

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(2, edges.Count());
            Assert.AreEqual(true, edges.First(x => x.To == 1).DataInverted);
            Assert.AreEqual(1, edges.First(x => x.To == 1).Data[0]);
            Assert.AreEqual(true, edges.First(x => x.To == 5).DataInverted);
            Assert.AreEqual(5, edges.First(x => x.To == 5).Data[0]);
            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(4, edges.Count());
            Assert.AreEqual(1, edges.First(x => x.To == 0).Data[0]);
            Assert.AreEqual(2, edges.First(x => x.To == 2).Data[0]);
            Assert.AreEqual(3, edges.First(x => x.To == 3).Data[0]);
            Assert.AreEqual(4, edges.First(x => x.To == 4).Data[0]);
            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(2, edges.Count());
            Assert.AreEqual(true, edges.First(x => x.To == 1).DataInverted);
            Assert.AreEqual(2, edges.First(x => x.To == 1).Data[0]);
            Assert.AreEqual(true, edges.First(x => x.To == 5).DataInverted);
            Assert.AreEqual(6, edges.First(x => x.To == 5).Data[0]);
            edges = graph.GetEdgeEnumerator(3);
            Assert.AreEqual(2, edges.Count());
            Assert.AreEqual(true, edges.First(x => x.To == 1).DataInverted);
            Assert.AreEqual(3, edges.First(x => x.To == 1).Data[0]);
            Assert.AreEqual(true, edges.First(x => x.To == 5).DataInverted);
            Assert.AreEqual(7, edges.First(x => x.To == 5).Data[0]);
            edges = graph.GetEdgeEnumerator(4);
            Assert.AreEqual(2, edges.Count());
            Assert.AreEqual(true, edges.First(x => x.To == 1).DataInverted);
            Assert.AreEqual(4, edges.First(x => x.To == 1).Data[0]);
            Assert.AreEqual(true, edges.First(x => x.To == 5).DataInverted);
            Assert.AreEqual(8, edges.First(x => x.To == 5).Data[0]);

            graph.Switch(0, 1);

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(4, edges.Count());
            Assert.AreEqual(1, edges.First(x => x.To == 1).Data[0]);
            Assert.AreEqual(2, edges.First(x => x.To == 2).Data[0]);
            Assert.AreEqual(3, edges.First(x => x.To == 3).Data[0]);
            Assert.AreEqual(4, edges.First(x => x.To == 4).Data[0]);
            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(2, edges.Count());
            Assert.AreEqual(true, edges.First(x => x.To == 0).DataInverted);
            Assert.AreEqual(1, edges.First(x => x.To == 0).Data[0]);
            Assert.AreEqual(true, edges.First(x => x.To == 5).DataInverted);
            Assert.AreEqual(5, edges.First(x => x.To == 5).Data[0]);
            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(2, edges.Count());
            Assert.AreEqual(true, edges.First(x => x.To == 0).DataInverted);
            Assert.AreEqual(2, edges.First(x => x.To == 0).Data[0]);
            Assert.AreEqual(true, edges.First(x => x.To == 5).DataInverted);
            Assert.AreEqual(6, edges.First(x => x.To == 5).Data[0]);
            edges = graph.GetEdgeEnumerator(3);
            Assert.AreEqual(2, edges.Count());
            Assert.AreEqual(true, edges.First(x => x.To == 0).DataInverted);
            Assert.AreEqual(3, edges.First(x => x.To == 0).Data[0]);
            Assert.AreEqual(true, edges.First(x => x.To == 5).DataInverted);
            Assert.AreEqual(7, edges.First(x => x.To == 5).Data[0]);
            edges = graph.GetEdgeEnumerator(4);
            Assert.AreEqual(2, edges.Count());
            Assert.AreEqual(true, edges.First(x => x.To == 0).DataInverted);
            Assert.AreEqual(4, edges.First(x => x.To == 0).Data[0]);
            Assert.AreEqual(true, edges.First(x => x.To == 5).DataInverted);
            Assert.AreEqual(8, edges.First(x => x.To == 5).Data[0]);
        }

        /// <summary>
        /// Tests adding a vertex with elevation.
        /// </summary>
        [Test]
        public void TestAddVertexWithElevation()
        {
            var graph = new GeometricGraph(1, 100);
            graph.AddVertex(0, 1, 10, 100);
            graph.AddVertex(1, 2, 20, 200);

            var vertex = graph.GetVertex(0);
            Assert.AreEqual(1, vertex.Latitude);
            Assert.AreEqual(10, vertex.Longitude);
            Assert.AreEqual(100, vertex.Elevation);
            vertex = graph.GetVertex(1);
            Assert.AreEqual(2, vertex.Latitude);
            Assert.AreEqual(20, vertex.Longitude);
            Assert.AreEqual(200, vertex.Elevation);
        }

        /// <summary>
        /// Tests serializing a geometric graph with elevation.
        /// </summary>
        [Test]
        public void TestSerializeVertexWithElevation()
        {
            var graph = new GeometricGraph(1, 100);
            graph.AddVertex(0, 1, 10, 100);
            graph.AddVertex(1, 2, 20, 200);

            // serialize.
            GeometricGraph deserializedGraph = null;
            using (var stream = new System.IO.MemoryStream())
            {
                var size = graph.Serialize(stream);
                stream.Seek(0, System.IO.SeekOrigin.Begin);

                deserializedGraph = GeometricGraph.Deserialize(stream, null);
                Assert.AreEqual(size, stream.Position);

                Assert.AreEqual(graph.VertexCount, deserializedGraph.VertexCount);
                Assert.AreEqual(graph.EdgeCount, deserializedGraph.EdgeCount);

                var vertex = deserializedGraph.GetVertex(0);
                Assert.AreEqual(1, vertex.Latitude);
                Assert.AreEqual(10, vertex.Longitude);
                Assert.AreEqual(100, vertex.Elevation);
                vertex = deserializedGraph.GetVertex(1);
                Assert.AreEqual(2, vertex.Latitude);
                Assert.AreEqual(20, vertex.Longitude);
                Assert.AreEqual(200, vertex.Elevation);

                stream.Seek(0, System.IO.SeekOrigin.Begin);
                deserializedGraph = GeometricGraph.Deserialize(stream, GeometricGraphProfile.NoCache);
                Assert.AreEqual(size, stream.Position);

                Assert.AreEqual(graph.VertexCount, deserializedGraph.VertexCount);
                Assert.AreEqual(graph.EdgeCount, deserializedGraph.EdgeCount);

                vertex = deserializedGraph.GetVertex(0);
                Assert.AreEqual(1, vertex.Latitude);
                Assert.AreEqual(10, vertex.Longitude);
                Assert.AreEqual(100, vertex.Elevation);
                vertex = deserializedGraph.GetVertex(1);
                Assert.AreEqual(2, vertex.Latitude);
                Assert.AreEqual(20, vertex.Longitude);
                Assert.AreEqual(200, vertex.Elevation);
            }
        }
    }
}