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

using Itinero.Algorithms;
using Itinero.LocalGeo;
using Itinero.Graphs.Geometric;
using Itinero.Data.Network;
using Itinero.Profiles;
using System;
using System.Collections.Generic;
using Itinero.Data.Edges;
using Itinero.Algorithms.Weights;
using Itinero.Algorithms.Search.Hilbert;
using System.IO;
using Itinero.IO.Json;
using Itinero.Navigation.Directions;

namespace Itinero
{
    /// <summary>
    /// Contains extension methods for the routerpoint.
    /// </summary>
    public static class RouterPointExtensions
    {
        /// <summary>
        /// Converts the router point to paths leading to the closest 2 vertices.
        /// </summary>
        public static EdgePath<T>[] ToEdgePaths<T>(this RouterPoint point, RouterDb routerDb, WeightHandler<T> weightHandler, bool asSource, bool? forward)
            where T : struct
        {
            if (forward == null)
            { // don't-care direction, use default implementation.
                return point.ToEdgePaths(routerDb, weightHandler, asSource);
            }

            var graph = routerDb.Network.GeometricGraph;
            var edge = graph.GetEdge(point.EdgeId);

            float distance;
            ushort profileId;
            EdgeDataSerializer.Deserialize(edge.Data[0], out distance, out profileId);
            Factor factor;
            var edgeWeight = weightHandler.Calculate(profileId, distance, out factor);

            var offset = point.Offset / (float)ushort.MaxValue;
            if (factor.Direction == 0)
            { // bidirectional.
                if (forward.Value)
                {
                    return new EdgePath<T>[] {
                        new EdgePath<T>(edge.To, weightHandler.Calculate(profileId, distance * (1 - offset)), edge.IdDirected(), new EdgePath<T>())
                    };
                }
                return new EdgePath<T>[] {
                    new EdgePath<T>(edge.From, weightHandler.Calculate(profileId, distance * offset), -edge.IdDirected(), new EdgePath<T>())
                };
            }
            else if (factor.Direction == 1)
            { // edge is forward oneway.
                if (asSource)
                {
                    if (forward.Value)
                    {
                        return new EdgePath<T>[] {
                            new EdgePath<T>(edge.To, weightHandler.Calculate(profileId, distance * (1 - offset)), edge.IdDirected(), new EdgePath<T>())
                        };
                    }
                    return new EdgePath<T>[0];
                }
                if (forward.Value)
                {
                    return new EdgePath<T>[0];
                }
                return new EdgePath<T>[] {
                    new EdgePath<T>(edge.From, weightHandler.Calculate(profileId, distance * offset), -edge.IdDirected(), new EdgePath<T>())
                };
            }
            else
            { // edge is backward oneway.
                if (!asSource)
                {
                    if (forward.Value)
                    {
                        return new EdgePath<T>[] {
                            new EdgePath<T>(edge.To, weightHandler.Calculate(profileId, distance * (1 - offset)), edge.IdDirected(), new EdgePath<T>())
                        };
                    }
                    return new EdgePath<T>[0];
                }
                if (forward.Value)
                {
                    return new EdgePath<T>[0];
                }
                return new EdgePath<T>[] {
                    new EdgePath<T>(edge.From, weightHandler.Calculate(profileId, distance * offset), -edge.IdDirected(), new EdgePath<T>())
                };
            }
        }

        /// <summary>
        /// Converts the router point to paths leading to the closest 2 vertices.
        /// </summary>
        public static EdgePath<T>[] ToEdgePaths<T>(this RouterPoint point, RouterDb routerDb, WeightHandler<T> weightHandler, bool asSource)
            where T : struct
        {
            var graph = routerDb.Network.GeometricGraph;
            var edge = graph.GetEdge(point.EdgeId);

            float distance;
            ushort profileId;
            EdgeDataSerializer.Deserialize(edge.Data[0], out distance, out profileId);
            Factor factor;
            var edgeWeight = weightHandler.Calculate(profileId, distance, out factor);

            var offset = point.Offset / (float)ushort.MaxValue;
            if (factor.Direction == 0)
            { // bidirectional.
                if (offset == 0)
                { // the first part is just the first vertex.
                    return new EdgePath<T>[] {
                        new EdgePath<T>(edge.From),
                        new EdgePath<T>(edge.To, weightHandler.Calculate(profileId, distance * (1 - offset)), edge.IdDirected(), new EdgePath<T>(edge.From))
                    };
                }
                else if (offset == 1)
                { // the second path it just the second vertex.
                    return new EdgePath<T>[] {
                        new EdgePath<T>(edge.From, weightHandler.Calculate(profileId, distance * offset), -edge.IdDirected(), new EdgePath<T>(edge.To)),
                        new EdgePath<T>(edge.To)
                    };
                }
                return new EdgePath<T>[] {
                    new EdgePath<T>(edge.From, weightHandler.Calculate(profileId, distance * offset), -edge.IdDirected(), new EdgePath<T>()),
                    new EdgePath<T>(edge.To, weightHandler.Calculate(profileId, distance * (1 - offset)), edge.IdDirected(), new EdgePath<T>())
                };
            }
            else if (factor.Direction == 1)
            { // edge is forward oneway.
                if (asSource)
                {
                    if (offset == 1)
                    { // just return the to-vertex.
                        return new EdgePath<T>[] {
                            new EdgePath<T>(edge.To)
                        };
                    }
                    if (offset == 0)
                    { // return both, we are at the from-vertex.
                        return new EdgePath<T>[] {
                            new EdgePath<T>(edge.From),
                            new EdgePath<T>(edge.To, weightHandler.Calculate(profileId, distance * (1 - offset)), edge.IdDirected(), new EdgePath<T>(edge.From))
                        };
                    }
                    return new EdgePath<T>[] {
                        new EdgePath<T>(edge.To, weightHandler.Calculate(profileId, distance * (1 - offset)), edge.IdDirected(), new EdgePath<T>())
                    };
                }
                if (offset == 0)
                { // just return the from vertex.
                    return new EdgePath<T>[] {
                        new EdgePath<T>(edge.From)
                    };
                }
                if (offset == 1)
                { // return both, we are at the to-vertex.
                    return new EdgePath<T>[] {
                        new EdgePath<T>(edge.To),
                        new EdgePath<T>(edge.From, weightHandler.Calculate(profileId, distance * offset), -edge.IdDirected(), new EdgePath<T>(edge.To))
                    };
                }
                return new EdgePath<T>[] {
                        new EdgePath<T>(edge.From, weightHandler.Calculate(profileId, distance * offset), -edge.IdDirected(), new EdgePath<T>())
                    };
            }
            else
            { // edge is backward oneway.
                if (!asSource)
                {
                    if (offset == 1)
                    { // just return the to-vertex.
                        return new EdgePath<T>[] {
                            new EdgePath<T>(edge.To)
                        };
                    }
                    if (offset == 0)
                    { // return both, we are at the from-vertex.
                        return new EdgePath<T>[] {
                            new EdgePath<T>(edge.From),
                            new EdgePath<T>(edge.To, weightHandler.Calculate(profileId, distance * (1 - offset)), edge.IdDirected(), new EdgePath<T>(edge.From))
                        };
                    }
                    return new EdgePath<T>[] {
                        new EdgePath<T>(edge.To, weightHandler.Calculate(profileId, distance * (1 - offset)), edge.IdDirected(), new EdgePath<T>())
                    };
                }
                if (offset == 0)
                { // just return the from-vertex.
                    return new EdgePath<T>[] {
                        new EdgePath<T>(edge.From)
                    };
                }
                if (offset == 1)
                { // return both, we are at the to-vertex.
                    return new EdgePath<T>[] {
                        new EdgePath<T>(edge.To),
                        new EdgePath<T>(edge.From, weightHandler.Calculate(profileId, distance * offset), -edge.IdDirected(), new EdgePath<T>(edge.To))
                    };
                }
                return new EdgePath<T>[] {
                        new EdgePath<T>(edge.From, weightHandler.Calculate(profileId, distance * offset), -edge.IdDirected(), new EdgePath<T>())
                    };
            }
        }
        
        /// <summary>
        /// Calculates the distance to one of the vertices on the edge this router point is on.
        /// </summary>
        /// <returns></returns>
        public static float DistanceTo(this RouterPoint point, RouterDb routerDb, uint vertex)
        {
            var geometricEdge = routerDb.Network.GeometricGraph.GetEdge(point.EdgeId);
            var edgeDistance = routerDb.Network.GeometricGraph.Length(geometricEdge);
            var offsetDistance = edgeDistance * ((float)point.Offset / (float)ushort.MaxValue);
            if(geometricEdge.From == vertex)
            { // offset = distance.
                return offsetDistance;
            }
            else if(geometricEdge.To == vertex)
            { // offset = 100 - distance.
                return edgeDistance - offsetDistance;
            }
            throw new ArgumentOutOfRangeException(string.Format("Vertex {0} is not part of edge {1}.",
                vertex, point.EdgeId));
        }

        /// <summary>
        /// Calculates the distance to one of the vertices on the edge this router point is on.
        /// </summary>
        /// <returns></returns>
        public static float DistanceTo(this RouterPoint point, RouterDb routerDb, RouterPoint target)
        {
            if (point.EdgeId != target.EdgeId)
            {
                throw new ArgumentException("Target point must be part of the same edge.");
            }
            var edge = routerDb.Network.GetEdge(point.EdgeId);
            var diff = (float)System.Math.Abs(point.Offset - target.Offset);
            return (diff / ushort.MaxValue) * edge.Data.Distance;
        }

        /// <summary>
        /// Calculates the shape points along the way from this router point to one of it's vertices.
        /// </summary>
        public static List<Coordinate> ShapePointsTo(this RouterPoint point, RouterDb routerDb, uint vertex)
        {
            List<Coordinate> points = null;
            var geometricEdge = routerDb.Network.GeometricGraph.GetEdge(point.EdgeId);
            var edgeDistance = routerDb.Network.GeometricGraph.Length(geometricEdge);
            var offsetDistance = edgeDistance * ((float)point.Offset / (float)ushort.MaxValue);
            if (geometricEdge.From == vertex)
            { // offset = distance.
                points = routerDb.Network.GeometricGraph.GetShape(geometricEdge, vertex, offsetDistance);
                points.Reverse();
                return points;
            }
            else if (geometricEdge.To == vertex)
            { // offset = 100 - distance.
                points = routerDb.Network.GeometricGraph.GetShape(geometricEdge, vertex, edgeDistance - offsetDistance);
                points.Reverse();
                return points;
            }
            throw new ArgumentOutOfRangeException(string.Format("Vertex {0} is not part of edge {1}.",
                vertex, point.EdgeId));
        }

        /// <summary>
        /// Calculates the shape points along the way from this router point to another routerpoint on the same edge.
        /// </summary>
        public static List<Coordinate> ShapePointsTo(this RouterPoint point, RouterDb routerDb, RouterPoint other)
        {
            if (point.EdgeId != other.EdgeId) { throw new ArgumentException("Cannot build shape points list between router points on different edges."); }

            List<Coordinate> points = null;

            var geometricEdge = routerDb.Network.GeometricGraph.GetEdge(point.EdgeId);
            var edgeDistance = routerDb.Network.GeometricGraph.Length(geometricEdge);
            var pointOffset = edgeDistance * ((float)point.Offset / (float)ushort.MaxValue);
            var otherOffset = edgeDistance * ((float)other.Offset / (float)ushort.MaxValue);

            if(pointOffset > otherOffset)
            { // oeps, shapes are opposite to edge.
                points = routerDb.Network.GeometricGraph.GetShape(geometricEdge, otherOffset, pointOffset);
                points.Reverse();
            }
            else
            { // with edge direction.
                points = routerDb.Network.GeometricGraph.GetShape(geometricEdge, pointOffset, otherOffset);
            }
            return points;
        }

        /// <summary>
        /// Returns the location.
        /// </summary>
        public static Coordinate Location(this RouterPoint point)
        {
            return new Coordinate()
            {
                Latitude = point.Latitude,
                Longitude = point.Longitude
            };
        }

        /// <summary>
        /// Returns the location on the network.
        /// </summary>
        public static Coordinate LocationOnNetwork(this RouterPoint point, RouterDb db)
        {
            return db.LocationOnNetwork(point.EdgeId, point.Offset);
        }

        /// <summary>
        /// Returns true if the router point matches exactly with the given vertex.
        /// </summary>
        public static bool IsVertex(this RouterPoint point)
        {
            if (point.Offset == 0)
            { // offset is zero.
                return true;
            }
            else if (point.Offset == ushort.MaxValue)
            { // offset is max.
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the router point matches exactly with the given vertex.
        /// </summary>
        public static bool IsVertex(this RouterPoint point, RouterDb router, uint vertex)
        {
            if(point.Offset == 0)
            { // offset is zero, maybe there is a match.
                var edge = router.Network.GetEdge(point.EdgeId);
                return edge.From == vertex;
            }
            else if(point.Offset == ushort.MaxValue)
            { // offset is max, maybe there is a match.
                var edge = router.Network.GetEdge(point.EdgeId);
                return edge.To == vertex;
            }
            return false;
        }

        /// <summary>
        /// Creates a new router point.
        /// </summary>
        public static RouterPoint CreateRouterPoint(this RouterDb routerDb, uint edgeId, ushort offset)
        {
            var location = routerDb.LocationOnNetwork(edgeId, offset);
            return new RouterPoint(location.Latitude, location.Longitude, edgeId, offset);
        }

        /// <summary>
        /// Creates a router point for the given edge.
        /// </summary>
        public static RouterPoint CreateRouterPointForEdge(this RouterDb routerDb, long directedEdgeId, bool atStart)
        {
            var edge = routerDb.Network.GetEdge(directedEdgeId);

            return routerDb.CreateRouterPointForEdge(edge, directedEdgeId > 0, atStart);
        }

        /// <summary>
        /// Creates a router point for the given edge.
        /// </summary>
        public static RouterPoint CreateRouterPointForEdge(this RouterDb routerDb, RoutingEdge edge, bool edgeIsForward, bool atStart)
        {
            Coordinate location;
            if (atStart)
            {
                if (!edgeIsForward)
                {
                    location = routerDb.Network.GetVertex(edge.To);
                    return new RouterPoint(location.Latitude, location.Longitude, edge.Id, ushort.MaxValue);
                }
                location = routerDb.Network.GetVertex(edge.From);
                return new RouterPoint(location.Latitude, location.Longitude, edge.Id, 0);
            }
            else
            {
                if (!edgeIsForward)
                {
                    location = routerDb.Network.GetVertex(edge.From);
                    return new RouterPoint(location.Latitude, location.Longitude, edge.Id, 0);
                }
                location = routerDb.Network.GetVertex(edge.To);
                return new RouterPoint(location.Latitude, location.Longitude, edge.Id, ushort.MaxValue);
            }
        }

        /// <summary>
        /// Creates a router point for the given vertex.
        /// </summary>
        public static RouterPoint CreateRouterPointForVertex(this RouterDb routerDb, uint vertex, params Profile[] profile)
        {
            float latitude, longitude;
            if (!routerDb.Network.GeometricGraph.GetVertex(vertex, out latitude, out longitude))
            {
                throw new ArgumentException("Vertex doesn't exist, cannot create routerpoint.");
            }
            var edges = routerDb.Network.GetEdgeEnumerator(vertex);
            while (edges.MoveNext())
            {
                var allowed = true;
                for (var i = 0; i < profile.Length; i++)
                {
                    if (!profile[i].CanStopOn(
                        routerDb.EdgeProfiles.Get(edges.Data.Profile)))
                    {
                        allowed = false;
                        break;
                    }
                }

                if (allowed)
                {
                    if (edges.DataInverted)
                    {
                        return new RouterPoint(latitude, longitude, edges.Id, ushort.MaxValue);
                    }
                    return new RouterPoint(latitude, longitude, edges.Id, 0);
                }
            }
            throw new ArgumentException("No edges associated with vertex can be used for all of the given profiles, cannot create routerpoint.");
        }
        
        /// <summary>
        /// Creates a router point for the given vertex.
        /// </summary>
        public static RouterPoint CreateRouterPointForVertex(this GeometricGraph graph, uint vertex, Func<GeometricEdge, bool> isAcceptable)
        {
            float latitude, longitude;
            if (!graph.GetVertex(vertex, out latitude, out longitude))
            {
                throw new ArgumentException("Vertex doesn't exist, cannot create routerpoint.");
            }
            var edges = graph.GetEdgeEnumerator(vertex);
            while (edges.MoveNext())
            {
                if (isAcceptable(edges.Current))
                {
                    if (edges.DataInverted)
                    {
                        return new RouterPoint(latitude, longitude, edges.Id, ushort.MaxValue);
                    }
                    return new RouterPoint(latitude, longitude, edges.Id, 0);
                }
            }
            throw new ArgumentException("No edges associated with vertex can be used for all of the given profiles, cannot create routerpoint.");
        }

        /// <summary>
        /// Creates a router point for the given vertex.
        /// </summary>
        public static RouterPoint CreateRouterPointForVertex(this RoutingNetwork graph, uint vertex)
        {
            float latitude, longitude;
            if (!graph.GetVertex(vertex, out latitude, out longitude))
            {
                throw new ArgumentException("Vertex doesn't exist, cannot create routerpoint.");
            }
            var edges = graph.GetEdgeEnumerator(vertex);
            while (edges.MoveNext())
            {
                if (edges.DataInverted)
                {
                    return new RouterPoint(latitude, longitude, edges.Id, ushort.MaxValue);
                }
                return new RouterPoint(latitude, longitude, edges.Id, 0);
            }
            throw new ArgumentException("No edges associated with vertex can be used for all of the given profiles, cannot create routerpoint.");
        }

        /// <summary>
        /// Creates a router point for the given vertex.
        /// </summary>
        public static RouterPoint CreateRouterPointForVertex(this RoutingNetwork graph, uint vertex, uint neighbour)
        {
            return graph.GeometricGraph.CreateRouterPointForVertex(vertex, neighbour);
        }

        /// <summary>
        /// Creates a router point for the given vertex.
        /// </summary>
        public static RouterPoint CreateRouterPointForVertex(this GeometricGraph graph, uint vertex, uint neighbour)
        {
            float latitude, longitude;
            if (!graph.GetVertex(vertex, out latitude, out longitude))
            {
                throw new ArgumentException("Vertex doesn't exist, cannot create routerpoint.");
            }
            var edges = graph.GetEdgeEnumerator(vertex);
            while(true)
            {
                if(!edges.MoveNext())
                {
                    throw new ArgumentException("No edges associated with vertex and it's neigbour, cannot create routerpoint.");
                }
                if(edges.To == neighbour)
                {
                    break;
                }
            }
            if (edges.DataInverted)
            {
                return new RouterPoint(latitude, longitude, edges.Id, ushort.MaxValue);
            }
            return new RouterPoint(latitude, longitude, edges.Id, 0);
        }

        /// <summary>
        /// Calculates the edge path between this router point and the given router point.
        /// </summary>
        public static EdgePath<T> EdgePathTo<T>(this RouterPoint point, RouterDb db, WeightHandler<T> weightHandler, RouterPoint target)
            where T : struct
        {
            if (point.EdgeId != target.EdgeId)
            {
                throw new ArgumentException("Target point must be part of the same edge.");
            }
            if (point.Offset == target.Offset)
            { // path is possible but it has a weight of 0.
                return new EdgePath<T>(point.VertexId(db));
            }
            var forward = point.Offset < target.Offset;
            var edge = db.Network.GetEdge(point.EdgeId);
            var distance = ((float)System.Math.Abs((int)point.Offset - (int)target.Offset) / (float)ushort.MaxValue) *
                edge.Data.Distance;
            Factor factor;
            var weight = weightHandler.Calculate(edge.Data.Profile, distance, out factor);
            if (factor.Value <= 0)
            { // not possible to travel here.
                return null;
            }
            if (factor.Direction == 0 ||
               (forward && factor.Direction == 1) ||
               (!forward && factor.Direction == 2))
            { // ok, directions match.
                if (forward)
                {
                    return new EdgePath<T>(target.VertexId(db), weight, point.EdgeId + 1, new EdgePath<T>(point.VertexId(db)));
                }
                return new EdgePath<T>(target.VertexId(db), weight, -point.EdgeId - 1, new EdgePath<T>(point.VertexId(db)));
            }
            return null;
        }
        
        /// <summary>
        /// Calculates the edge path between this router point and the given router point.
        /// </summary>
        /// <param name="point">The source point.</param>
        /// <param name="db">The router db.</param>
        /// <param name="weightHandler">The weight handler.</param>
        /// <param name="target">The target point.</param>
        /// <param name="backward">Forces the direction on the edge to travel in.</param>
        public static EdgePath<T> EdgePathTo<T>(this RouterPoint point, RouterDb db, WeightHandler<T> weightHandler, RouterPoint target, bool backward = false)
            where T : struct
        {
            if (point.EdgeId != target.EdgeId)
            {
                throw new ArgumentException("Target point must be part of the same edge.");
            }

            if (point.Offset == target.Offset)
            { // two points are identical, path of length '0'.
                return new EdgePath<T>(point.VertexId(db));
            }

            var forward = point.Offset < target.Offset;
            if (forward && backward)
            { // forward travel is required but backward travel is requested.
                return null;
            }

            var edge = db.Network.GetEdge(point.EdgeId);
            var distance = ((float)System.Math.Abs((int)point.Offset - (int)target.Offset) / (float)ushort.MaxValue) *
                edge.Data.Distance;
            Factor factor;
            var weight = weightHandler.Calculate(edge.Data.Profile, distance, out factor);
            if (factor.Value <= 0)
            { // not possible to travel here.
                return null;
            }
            if (factor.Direction == 0 ||
               (forward && factor.Direction == 1) ||
               (!forward && factor.Direction == 2))
            { // ok, directions match.
                if (forward)
                {
                    return new EdgePath<T>(target.VertexId(db), weight, point.EdgeId + 1, new EdgePath<T>(point.VertexId(db)));
                }
                return new EdgePath<T>(target.VertexId(db), weight, -point.EdgeId - 1, new EdgePath<T>(point.VertexId(db)));
            }
            return null;
        }
        
        /// <summary>
        /// Calculates the edge path between this router point and the given router point.
        /// </summary>
        /// <param name="point">The source point.</param>
        /// <param name="db">The router db.</param>
        /// <param name="weightHandler">The weight handler.</param>
        /// <param name="sourceForward">The source forward flag, true if forward, false if backward, null if don't care.</param>
        /// <param name="target">The target point.</param>
        /// <param name="targetForward">The target forward flag, true if forward, false if backward, null if don't care.</param>
        public static EdgePath<T> EdgePathTo<T>(this RouterPoint point, RouterDb db, WeightHandler<T> weightHandler, bool? sourceForward, RouterPoint target, bool? targetForward)
            where T : struct
        {
            if (!sourceForward.HasValue && !targetForward.HasValue)
            {
                return point.EdgePathTo(db, weightHandler, target);
            }
            if (sourceForward.HasValue && targetForward.HasValue)
            {
                if (sourceForward.Value != targetForward.Value)
                { // impossible route inside one edge.
                    return null;
                }
            }
            if(sourceForward.HasValue)
            {
                return point.EdgePathTo(db, weightHandler, target, !sourceForward.Value);
            }
            return point.EdgePathTo(db, weightHandler, target, !targetForward.Value);
        }

        /// <summary>
        /// Calculates the edge path between this router point and one of it's neighbours.
        /// </summary>
        public static EdgePath<T> EdgePathTo<T>(this RouterPoint point, RouterDb db, WeightHandler<T> weightHandler, uint neighbour, bool backward = false)
            where T : struct
        {
            Factor factor;

            var edge = db.Network.GetEdge(point.EdgeId);
            if (edge.From == neighbour)
            {
                var distance = ((float)System.Math.Abs((int)point.Offset) / (float)ushort.MaxValue) *  edge.Data.Distance;
                var weight = weightHandler.Calculate(edge.Data.Profile, distance, out factor);
                if (backward)
                {
                    return new EdgePath<T>(Constants.NO_VERTEX, weight, edge.IdDirected(), new EdgePath<T>(neighbour));
                }
                return new EdgePath<T>(neighbour, weight, edge.IdDirected(), new EdgePath<T>());
            }
            else if(edge.To == neighbour)
            {
                var distance = (1 - ((float)System.Math.Abs((int)point.Offset) / (float)ushort.MaxValue)) * edge.Data.Distance;
                var weight = weightHandler.Calculate(edge.Data.Profile, distance, out factor);
                if (backward)
                {
                    return new EdgePath<T>(Constants.NO_VERTEX, weight, edge.IdDirected(), new EdgePath<T>(neighbour));
                }
                return new EdgePath<T>(neighbour, weight, edge.IdDirected(), new EdgePath<T>());
            }
            else
            {
                throw new ArgumentException(string.Format("Cannot route to neighbour: {0} is not a vertex on edge {1}.", neighbour, point.EdgeId));
            }
        }
        
        /// <summary>
        /// Returns the vertex id if any.
        /// </summary>
        public static uint VertexId(this RouterPoint point, RouterDb db)
        {
            var edge = db.Network.GetEdge(point.EdgeId);
            if (point.Offset == 0)
            { // offset is zero.
                return edge.From;
            }
            else if (point.Offset == ushort.MaxValue)
            { // offset is max, maybe there is a match.
                return edge.To;
            }
            return Constants.NO_VERTEX;
        }

        /// <summary>
        /// Returns true if the given point is identical.
        /// </summary>
        public static bool IsIdenticalTo(this RouterPoint point, RouterPoint other)
        {
            return other.EdgeId == point.EdgeId &&
                other.Offset == point.Offset;
        }

        /// <summary>
        /// Gets the directed edge id for the routerpoint.
        /// </summary>
        public static long EdgeIdDirected(this RouterPoint point, bool forward = true)
        {
            if (forward)
            {
                return point.EdgeId + 1;
            }
            return -(point.EdgeId + 1);
        }

        /// <summary>
        /// Calculates the angle in degress at the given routerpoint over a given distance. 
        /// </summary>
        /// <param name="point">The router point.</param>
        /// <param name="network">The routing network.</param>
        /// <param name="distance">The distance to average over.</param>
        /// <returns>The angle relative to the meridians.</returns>
        public static float? Angle(this RouterPoint point, RoutingNetwork network, float distance = 100)
        {
            var edge = network.GetEdge(point.EdgeId);
            var edgeLength = edge.Data.Distance;
            var distanceOffset = (distance / edgeLength) * ushort.MaxValue;
            ushort offset1 = 0;
            ushort offset2 = ushort.MaxValue;
            if (distanceOffset <= ushort.MaxValue)
            { // not the entire edge.
                offset1 = (ushort)System.Math.Max(0,
                    point.Offset - distanceOffset);
                offset2 = (ushort)System.Math.Min(ushort.MaxValue,
                    point.Offset + distanceOffset);

                if (offset2 - offset1 == 0)
                {
                    if (offset1 > 0)
                    {
                        offset1--;
                    }
                    if (offset2 < ushort.MaxValue)
                    {
                        offset2++;
                    }
                }
            }

            // calculate the locations.
            var location1 = network.LocationOnNetwork(point.EdgeId, offset1);
            var location2 = network.LocationOnNetwork(point.EdgeId, offset2);

            if (Coordinate.DistanceEstimateInMeter(location1, location2) < .1)
            { // distance too small, edge to short.
                return null;
            }

            // calculate and return angle.
            var toNorth =  new Coordinate(location1.Latitude + 0.001f, location1.Longitude);
            var angleRadians = DirectionCalculator.Angle(location2, location1,  toNorth);
            return (float)angleRadians.ToDegrees().NormalizeDegrees();
        }

        
        /// <summary>
        /// Converts the router point to paths leading to the closest 2 vertices.
        /// </summary>
        public static EdgePath<T>[] ToEdgePathsDirected<T>(this RouterPoint point, RouterDb routerDb, WeightHandler<T> weightHandler, bool asSource)
            where T : struct
        {
            var graph = routerDb.Network.GeometricGraph;
            var edge = graph.GetEdge(point.EdgeId);

            float distance;
            ushort profileId;
            EdgeDataSerializer.Deserialize(edge.Data[0], out distance, out profileId);
            Factor factor;
            var edgeWeight = weightHandler.Calculate(profileId, distance, out factor);

            var offset = point.Offset / (float)ushort.MaxValue;
            if (factor.Direction == 0)
            { // bidirectional.
                if (offset == 0)
                { // the first part is just the first vertex.
                    return new EdgePath<T>[] {
                        new EdgePath<T>(edge.From, weightHandler.Zero, -edge.IdDirected(), new EdgePath<T>(edge.To)),
                        new EdgePath<T>(edge.To, weightHandler.Calculate(profileId, distance * (1 - offset)), edge.IdDirected(), new EdgePath<T>(edge.From))
                    };
                }
                else if (offset == 1)
                { // the second path it just the second vertex.
                    return new EdgePath<T>[] {
                        new EdgePath<T>(edge.From, weightHandler.Calculate(profileId, distance * offset), -edge.IdDirected(), new EdgePath<T>(edge.To)),
                        new EdgePath<T>(edge.To, weightHandler.Zero, edge.IdDirected(), new EdgePath<T>(edge.From))
                    };
                }
                return new EdgePath<T>[] {
                    new EdgePath<T>(edge.From, weightHandler.Calculate(profileId, distance * offset), -edge.IdDirected(), new EdgePath<T>(edge.To)),
                    new EdgePath<T>(edge.To, weightHandler.Calculate(profileId, distance * (1 - offset)), edge.IdDirected(), new EdgePath<T>(edge.From))
                };
            }
            else if (factor.Direction == 1)
            { // edge is forward oneway.
                if (asSource)
                {
                    if (offset == 1)
                    { // just return the to-vertex.
                        return new EdgePath<T>[] {
                            new EdgePath<T>(edge.To, weightHandler.Zero, edge.IdDirected(), new EdgePath<T>(edge.From))
                        };
                    }
                    return new EdgePath<T>[] {
                        new EdgePath<T>(edge.To, weightHandler.Calculate(profileId, distance * (1 - offset)), edge.IdDirected(), new EdgePath<T>(edge.From))
                    };
                }
                if (offset == 0)
                { // just return the from vertex.
                    return new EdgePath<T>[] {
                        new EdgePath<T>(edge.From, weightHandler.Zero, -edge.IdDirected(), new EdgePath<T>(edge.To))
                    };
                }
                return new EdgePath<T>[] {
                        new EdgePath<T>(edge.From, weightHandler.Calculate(profileId, distance * offset), -edge.IdDirected(), new EdgePath<T>(edge.To))
                    };
            }
            else
            { // edge is backward oneway.
                if (!asSource)
                {
                    if (offset == 1)
                    { // just return the to-vertex.
                        return new EdgePath<T>[] {
                            new EdgePath<T>(edge.To, weightHandler.Zero, edge.IdDirected(), new EdgePath<T>(edge.From))
                        };
                    }
                    return new EdgePath<T>[] {
                        new EdgePath<T>(edge.To, weightHandler.Calculate(profileId, distance * (1 - offset)), edge.IdDirected(), new EdgePath<T>(edge.From))
                    };
                }
                if (offset == 0)
                { // just return the from-vertex.
                    return new EdgePath<T>[] {
                        new EdgePath<T>(edge.From, weightHandler.Zero, new EdgePath<T>(edge.To))
                    };
                }
                return new EdgePath<T>[] {
                        new EdgePath<T>(edge.From, weightHandler.Calculate(profileId, distance * offset), -edge.IdDirected(), new EdgePath<T>(edge.To))
                    };
            }
        }

        /// <summary>
        /// Determines the direction from the given angle, true meaning forward relative to the edge in the routerpoint, false backwards.
        /// </summary>
        /// <param name="routerPoint">The router point.</param>
        /// <param name="routerDb">The router db.</param>
        /// <param name="angle">The angle in degrees relative to the longitudinal lines.</param>
        /// <param name="diffLimit">The diff limit when the angle is smaller than this we consider it the same direction.</param>
        /// <returns>True when forward, false when backward, null when direction couldn't be determined (outside of difflimit for example).</returns>
        public static bool? DirectionFromAngle(this RouterPoint routerPoint, RouterDb routerDb, float? angle, float diffLimit = 90)
        {
            if (diffLimit <= 0 || diffLimit > 90) { throw new ArgumentOutOfRangeException(nameof(diffLimit), "Expected to be in range ]0, 90]."); }
            if (routerDb == null) { throw new ArgumentNullException(nameof(routerDb)); }
            if (routerPoint == null) { throw new ArgumentNullException(nameof(routerPoint)); }
            if (angle == null) return null;

            angle = (float)Tools.NormalizeDegrees(angle.Value);
            var edgeAngle = routerPoint.Angle(routerDb.Network);
            if (edgeAngle == null) return null; // no angle could be determined, for example on extremely short edges.
            
            var diff = System.Math.Abs(Itinero.LocalGeo.Tools.SmallestDiffDegrees(angle.Value, edgeAngle.Value));
            if (diff < diffLimit)
            { // forward, angle is close to that of the edge.
                return true;
            }
            if (180 - diff < diffLimit)
            { // backward, angle is close to the anti-direction of the edge.
                return false;
            }
            return null;
        }

        /// <summary>
        /// Returns a geojson description for 
        /// </summary>
        public static string ToGeoJson(this RouterPoint routerPoint, RouterDb routerDb)
        {
            var stringWriter = new StringWriter();
            routerPoint.WriteGeoJson(stringWriter, routerDb);
            return stringWriter.ToInvariantString();
        }

        /// <summary>
        /// Writes geojson describing the given routerpoint.
        /// </summary>
        internal static void WriteGeoJson(this RouterPoint routerPoint, TextWriter writer, RouterDb db)
        {
            if (db == null) { throw new ArgumentNullException("db"); }
            if (writer == null) { throw new ArgumentNullException("writer"); }

            var jsonWriter = new JsonWriter(writer);
            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("type", "FeatureCollection", true, false);
            jsonWriter.WritePropertyName("features", false);
            jsonWriter.WriteArrayOpen();

            var edgeEnumerator = db.Network.GetEdgeEnumerator();
            edgeEnumerator.MoveToEdge(routerPoint.EdgeId);

            db.WriteEdge(jsonWriter, edgeEnumerator);

            db.WriteVertex(jsonWriter, edgeEnumerator.From);
            db.WriteVertex(jsonWriter, edgeEnumerator.To);

            // write location on network.
            var coordinate = routerPoint.LocationOnNetwork(db);
            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("type", "Feature", true, false);
            jsonWriter.WritePropertyName("geometry", false);

            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("type", "Point", true, false);
            jsonWriter.WritePropertyName("coordinates", false);
            jsonWriter.WriteArrayOpen();
            jsonWriter.WriteArrayValue(coordinate.Longitude.ToInvariantString());
            jsonWriter.WriteArrayValue(coordinate.Latitude.ToInvariantString());
            jsonWriter.WriteArrayClose();
            jsonWriter.WriteClose();

            jsonWriter.WritePropertyName("properties");
            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("type", "location_on_network", true);
            jsonWriter.WriteProperty("offset", routerPoint.Offset.ToInvariantString());
            jsonWriter.WriteClose();

            jsonWriter.WriteClose();

            // write original location.
            coordinate = routerPoint.Location();
            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("type", "Feature", true, false);
            jsonWriter.WritePropertyName("geometry", false);

            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("type", "Point", true, false);
            jsonWriter.WritePropertyName("coordinates", false);
            jsonWriter.WriteArrayOpen();
            jsonWriter.WriteArrayValue(coordinate.Longitude.ToInvariantString());
            jsonWriter.WriteArrayValue(coordinate.Latitude.ToInvariantString());
            jsonWriter.WriteArrayClose();
            jsonWriter.WriteClose();

            jsonWriter.WritePropertyName("properties");
            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("type", "original_location", true);
            jsonWriter.WriteClose();

            jsonWriter.WriteClose();


            jsonWriter.WriteArrayClose();
            jsonWriter.WriteClose();
        }
    }
}