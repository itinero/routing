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

using Itinero.Algorithms;
using Itinero.LocalGeo;
using Itinero.Graphs.Geometric;
using Itinero.Data.Network;
using Itinero.Profiles;
using System;
using System.Collections.Generic;
using Itinero.Data.Edges;
using Itinero.Algorithms.Weights;

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
            var geometricEdge = db.Network.GeometricGraph.GetEdge(point.EdgeId);
            var shape = db.Network.GeometricGraph.GetShape(geometricEdge);
            var length = db.Network.GeometricGraph.Length(geometricEdge);
            var currentLength = 0.0;
            var targetLength = length * (point.Offset / (double)ushort.MaxValue);
            for (var i = 1; i < shape.Count; i++)
            {
                var segmentLength = Coordinate.DistanceEstimateInMeter(shape[i - 1], shape[i]);
                if(segmentLength + currentLength > targetLength)
                {
                    var segmentOffsetLength = segmentLength + currentLength - targetLength;
                    var segmentOffset = 1 - (segmentOffsetLength / segmentLength);
                    return new Coordinate()
                    {
                        Latitude = (float)(shape[i - 1].Latitude + (segmentOffset * (shape[i].Latitude - shape[i - 1].Latitude))),
                        Longitude = (float)(shape[i - 1].Longitude + (segmentOffset * (shape[i].Longitude - shape[i - 1].Longitude)))
                    };
                }
                currentLength += segmentLength;
            }
            return shape[shape.Count - 1];
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
        /// Creates a router point for the given vertex.
        /// </summary>
        public static RouterPoint CreateRouterPointForVertex(this RoutingNetwork graph, uint vertex)
        {
            return graph.GeometricGraph.CreateRouterPointForVertex(vertex);
        }

        /// <summary>
        /// Creates a router point for the given vertex.
        /// </summary>
        public static RouterPoint CreateRouterPointForVertex(this GeometricGraph graph, uint vertex)
        {
            float latitude, longitude;
            if(!graph.GetVertex(vertex, out latitude, out longitude))
            {
                throw new ArgumentException("Vertex doesn't exist, cannot create routerpoint.");
            }
            var edges = graph.GetEdgeEnumerator(vertex);
            if(!edges.MoveNext())
            {
                throw new ArgumentException("No edges associated with vertex, cannot create routerpoint.");
            }
            if(edges.DataInverted)
            {
                return new RouterPoint(latitude, longitude, edges.Id, ushort.MaxValue);
            }
            return new RouterPoint(latitude, longitude, edges.Id, 0);
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
                    return new EdgePath<T>(target.VertexId(db), weight, point.EdgeId, new EdgePath<T>(point.VertexId(db)));
                }
                return new EdgePath<T>(target.VertexId(db), weight, -point.EdgeId, new EdgePath<T>(point.VertexId(db)));
            }
            return null;
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
    }
}