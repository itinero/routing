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
using Itinero.Graphs.Geometric.Shapes;
using System;
using System.Collections.Generic;
using Itinero.Algorithms;
using Itinero.Graphs.Geometric;

namespace Itinero.Data.Network
{
    /// <summary>
    /// Contains extension methods for the routing network.
    /// </summary>
    public static class RoutingNetworkExtensions
    {
        /// <summary>
        /// Gets the first point on the given edge starting a the given vertex.
        /// </summary>
        public static Coordinate GetFirstPoint(this RoutingNetwork graph, RoutingEdge edge, uint vertex)
        {
            var points = new List<Coordinate>();
            if (edge.From == vertex)
            { // start at from.
                if (edge.Shape == null)
                {
                    return graph.GetVertex(edge.To);
                }
                var shape = edge.Shape.GetEnumerator();
                shape.MoveNext();
                return shape.Current;
            }
            else if (edge.To == vertex)
            { // start at to.
                if (edge.Shape == null)
                {
                    return graph.GetVertex(edge.From);
                }
                var shape = edge.Shape.Reverse().GetEnumerator();
                shape.MoveNext();
                return shape.Current;
            }
            throw new ArgumentOutOfRangeException(string.Format("Vertex {0} is not part of edge {1}.",
                vertex, edge.Id));
        }

        /// <summary>
        /// Gets the vertex on this edge that is not the given vertex.
        /// </summary>
        public static uint GetOther(this RoutingEdge edge, uint vertex)
        {
            if(edge.From == vertex)
            {
                return edge.To;
            }
            else if(edge.To == vertex)
            {
                return edge.From;
            }
            throw new ArgumentOutOfRangeException(string.Format("Vertex {0} is not part of edge {1}.",
                vertex, edge.Id));
        }

        /// <summary>
        /// Gets the shape points including the two vertices.
        /// </summary>
        public static List<Coordinate> GetShape(this RoutingNetwork graph, RoutingEdge edge)
        {
            var points = new List<Coordinate>();
            points.Add(graph.GetVertex(edge.From));
            var shape = edge.Shape;
            if (shape != null)
            {
                if (edge.DataInverted)
                {
                    shape = shape.Reverse();
                }
                var shapeEnumerator = shape.GetEnumerator();
                shapeEnumerator.Reset();
                while (shapeEnumerator.MoveNext())
                {
                    points.Add(shapeEnumerator.Current);
                }
            }
            points.Add(graph.GetVertex(edge.To));
            return points;
        }

        /// <summary>
        /// Returns true if the routing network contains an edge between the two given vertices.
        /// </summary>
        public static bool ContainsEdge(this RoutingNetwork network, uint vertex1, uint vertex2)
        {
            var edges = network.GetEdgeEnumerator(vertex1);
            while(edges.MoveNext())
            {
                if(edges.To == vertex2)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds a new edge.
        /// </summary>
        public static uint AddEdge(this RoutingNetwork network, uint vertex1, uint vertex2, ushort profile, uint metaId, float distance,
            params Coordinate[] shape)
        {
            return network.AddEdge(vertex1, vertex2, new Edges.EdgeData()
            {
                Distance = distance,
                MetaId = metaId,
                Profile = profile
            }, new ShapeEnumerable(shape));
        }

        /// <summary>
        /// Adds a new edge.
        /// </summary>
        public static uint AddEdge(this RoutingNetwork network, uint vertex1, uint vertex2, Edges.EdgeData data,
            params Coordinate[] shape)
        {
            return network.AddEdge(vertex1, vertex2, data, new ShapeEnumerable(shape));
        }

        /// <summary>
        /// Adds a new edge.
        /// </summary>
        public static uint AddEdge(this RoutingNetwork network, uint vertex1, uint vertex2, Edges.EdgeData data,
            IEnumerable<Coordinate> shape)
        {
            if(shape == null)
            {
                return network.AddEdge(vertex1, vertex2, data, (ShapeEnumerable)null);
            }
            return network.AddEdge(vertex1, vertex2, data, new ShapeEnumerable(shape));
        }

        /// <summary>
        /// Adds a new edge.
        /// </summary>
        public static uint AddEdge(this RoutingNetwork network, uint vertex1, uint vertex2, ushort profile, uint metaId, float distance,
            IEnumerable<Coordinate> shape)
        {
            return network.AddEdge(vertex1, vertex2, new Edges.EdgeData()
            {
                Distance = distance,
                MetaId = metaId,
                Profile = profile
            }, new ShapeEnumerable(shape));
        }

        /// <summary>
        /// Merges vertex2 into vertex1.
        /// </summary>
        public static void MergeVertices(this RoutingNetwork network, uint vertex1, uint vertex2)
        {
            // get and save edge for vertex2.
            var vertex2Edges = new List<RoutingEdge>(network.GetEdgeEnumerator(vertex2));

            // remove edges.
            network.RemoveEdges(vertex2);

            // add edges.
            for(var i = 0; i < vertex2Edges.Count; i++)
            {
                if (vertex1 == vertex2Edges[i].To)
                {
                    continue;
                }

                if (!vertex2Edges[i].DataInverted)
                { // not inverted, add as vertex1 -> to.
                    network.AddEdge(vertex1, vertex2Edges[i].To, vertex2Edges[i].Data, vertex2Edges[i].Shape);
                }
                else
                { // inverted, add as to -> vertex1.
                    network.AddEdge(vertex2Edges[i].To, vertex1, vertex2Edges[i].Data, vertex2Edges[i].Shape);
                }
            }
        }
        
        /// <summary>
        /// Gets all edges starting at this edges.
        /// </summary>
        public static List<RoutingEdge> GetEdges(this RoutingNetwork network, uint vertex)
        {
            return new List<RoutingEdge>(network.GetEdgeEnumerator(vertex));
        }

        /// <summary>
        /// Returns a directed version of the edge-id. Smaller than 0 if inverted, as-is if not inverted.
        /// </summary>
        /// <remarks>
        /// The relationship between a regular edge id and a directed edge id:
        /// - 0 -> 1 forward, -1 backward.
        /// - all other id's are offset by 1 and postive when forward, negative when backward.
        /// </remarks>
        public static long IdDirected(this RoutingNetwork.EdgeEnumerator edge)
        {
            if (edge.DataInverted)
            {
                return -(edge.Id + 1);
            }
            return (edge.Id + 1);
        }

        /// <summary>
        /// Returns a directed version of the edge-id. Smaller than 0 if inverted, as-is if not inverted.
        /// </summary>
        /// <remarks>
        /// The relationship between a regular edge id and a directed edge id:
        /// - 0 -> 1 forward, -1 backward.
        /// - all other id's are offset by 1 and postive when forward, negative when backward.
        /// </remarks>
        public static long IdDirected(this RoutingEdge edge)
        {
            if (edge.DataInverted)
            {
                return -(edge.Id + 1);
            }
            return (edge.Id + 1);
        }

        /// <summary>
        /// Moves to the given directed edge-id.
        /// </summary>
        public static void MoveToEdge(this RoutingNetwork.EdgeEnumerator enumerator, long directedEdgeId)
        {
            if (directedEdgeId == 0) { throw new ArgumentOutOfRangeException("directedEdgeId"); }

            uint edgeId;
            if (directedEdgeId > 0)
            {
                edgeId = (uint)directedEdgeId - 1;
            }
            else
            {
                edgeId = (uint)((-directedEdgeId) - 1);
            }
            enumerator.MoveToEdge(edgeId);
        }

        /// <summary>
        /// Gets the edge represented by the given directed id.
        /// </summary>
        public static RoutingEdge GetEdge(this RoutingNetwork graph, long directedEdgeId)
        {
            if (directedEdgeId == 0) { throw new ArgumentOutOfRangeException("directedEdgeId"); }

            uint edgeId;
            if (directedEdgeId > 0)
            {
                edgeId = (uint)directedEdgeId - 1;
            }
            else
            {
                edgeId = (uint)((-directedEdgeId) - 1);
            }
            return graph.GetEdge(edgeId);
        }
        
        /// <summary>
        /// Returns a directed version of the edge-id. Smaller than 0 if inverted, as-is if not inverted.
        /// </summary>
        /// <remarks>
        /// The relationship between a regular edge id and a directed edge id:
        /// - 0 -> 1 forward, -1 backward.
        /// - all other id's are offset by 1 and postive when forward, negative when backward.
        /// </remarks>
        public static DirectedEdgeId DirectedEdgeId(this RoutingNetwork.EdgeEnumerator enumerator)
        {
            return new DirectedEdgeId(enumerator.Id, !enumerator.DataInverted);
        }

        /// <summary>
        /// Returns the location on the network.
        /// </summary>
        public static Coordinate LocationOnNetwork(this RoutingNetwork network, uint edgeId, ushort offset)
        {
            return network.GeometricGraph.LocationOnGraph(edgeId, offset);
        }
    }
}