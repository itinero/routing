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

using OsmSharp.Geo;
using OsmSharp.Math.Geo.Simple;
using OsmSharp.Routing.Algorithms;
using OsmSharp.Routing.Graphs.Geometric;
using OsmSharp.Routing.Network;
using OsmSharp.Routing.Profiles;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing
{
    /// <summary>
    /// Contains extension methods for the routerpoint.
    /// </summary>
    public static class RouterPointExtensions
    {
        /// <summary>
        /// Converts the router point to paths leading to the closest 2 vertices.
        /// </summary>
        /// <returns></returns>
        public static Path[] ToPaths(this RouterPoint point, RouterDb routerDb, Profile profile, bool asSource)
        {
            var graph = routerDb.Network.GeometricGraph;
            var edge = graph.GetEdge(point.EdgeId);

            float distance;
            ushort profileId;
            OsmSharp.Routing.Data.EdgeDataSerializer.Deserialize(edge.Data[0], out distance, out profileId);
            var factor = profile.Factor(routerDb.EdgeProfiles.Get(profileId));
            var length = distance;

            var offset = point.Offset / (float)ushort.MaxValue;
            if(factor.Direction == 0)
            { // bidirectional.
                if(offset == 0)
                { // the first part is just the first vertex.
                    return new Path[] {
                        new Path(edge.From),
                        new Path(edge.To, (length * (1 - offset)) * factor.Value, new Path(edge.From))
                    };
                }
                else if(offset == 1)
                { // the second path it just the second vertex.
                    return new Path[] {
                        new Path(edge.From, (length * offset) * factor.Value, new Path(edge.To)),
                        new Path(edge.To)
                    };
                }
                return new Path[] {
                    new Path(edge.From, (length * offset) * factor.Value, new Path(Constants.NO_VERTEX)),
                    new Path(edge.To, (length * (1 - offset)) * factor.Value, new Path(Constants.NO_VERTEX))
                };
            }
            else if(factor.Direction == 1)
            { // edge is forward oneway.
                if (asSource)
                {
                    if(offset == 1)
                    { // just return the to-vertex.
                        return new Path[] {
                            new Path(edge.To)
                        };
                    }
                    if (offset == 0)
                    { // return both, we are at the from-vertex.
                        return new Path[] {
                            new Path(edge.From),
                            new Path(edge.To, (length * (1 - offset)) * factor.Value, new Path(edge.From))
                        };
                    }
                    return new Path[] {
                        new Path(edge.To, (length * (1 - offset)) * factor.Value, new Path(Constants.NO_VERTEX))
                    };
                }
                if(offset == 0)
                { // just return the from vertex.
                    return new Path[] {
                        new Path(edge.From)
                    };
                }
                if(offset == 1)
                { // return both, we are at the to-vertex.
                    return new Path[] {
                        new Path(edge.To),
                        new Path(edge.From, (length * offset) * factor.Value, new Path(edge.To))
                    };
                }
                return new Path[] {
                        new Path(edge.From, (length * offset) * factor.Value, new Path(Constants.NO_VERTEX))
                    };
            }
            else
            { // edge is backward oneway.
                if (!asSource)
                {
                    if(offset == 1)
                    { // just return the to-vertex.
                        return new Path[] {
                            new Path(edge.To)
                        };
                    }
                    if(offset == 0)
                    { // return both, we are at the from-vertex.
                        return new Path[] {
                            new Path(edge.From),
                            new Path(edge.To, (length * (1 - offset)) * factor.Value, new Path(edge.From))
                        };
                    }
                    return new Path[] {
                        new Path(edge.To, (length * (1 - offset)) * factor.Value, new Path(Constants.NO_VERTEX))
                    };
                }
                if(offset == 0)
                { // just return the from-vertex.
                    return new Path[] {
                        new Path(edge.From)
                    };
                }
                if(offset == 1)
                { // return both, we are at the to-vertex.
                    return new Path[] {
                        new Path(edge.To),
                        new Path(edge.From, (length * offset) * factor.Value, new Path(edge.To))
                    };
                }
                return new Path[] {
                        new Path(edge.From, (length * offset) * factor.Value, new Path(Constants.NO_VERTEX))
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
        /// Calculates the shape points along the way from this router point to one of it's vertices.
        /// </summary>
        /// <returns></returns>
        public static List<ICoordinate> ShapePointsTo(this RouterPoint point, RouterDb routerDb, uint vertex)
        {
            List<ICoordinate> points = null;
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
        public static List<ICoordinate> ShapePointsTo(this RouterPoint point, RouterDb routerDb, RouterPoint other)
        {
            if (point.EdgeId != other.EdgeId) { throw new ArgumentException("Cannot build shape points list between router points on different edges."); }

            List<ICoordinate> points = null;

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
        public static ICoordinate Location(this RouterPoint point)
        {
            return new GeoCoordinateSimple()
            {
                Latitude = point.Latitude,
                Longitude = point.Longitude
            };
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
        /// <returns></returns>
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
        /// <returns></returns>
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
        /// Calculates the path between this router point and the given router point.
        /// </summary>
        public static Path PathTo(this RouterPoint point, RouterDb db, Profile profile, RouterPoint target)
        {
            if(point.EdgeId != target.EdgeId)
            {
                throw new ArgumentException("Target point must be part of the same edge.");
            }
            if(point.Offset == target.Offset)
            { // path is possible but it has a weight of 0.
                return new Path(point.VertexId(db));
            }
            var forward = point.Offset < target.Offset;
            var edge = db.Network.GetEdge(point.EdgeId);
            var factor = profile.Factor(db.EdgeProfiles.Get(edge.Data.Profile));
            if(factor.Value <= 0)
            { // not possible to travel here.
                return null;
            }
            if (factor.Direction == 0 ||
               (forward && factor.Direction == 1) ||
               (!forward && factor.Direction == 2))
            { // ok, directions match.
                var distance = ((float)System.Math.Abs((int)point.Offset - (int)target.Offset) / (float)ushort.MaxValue) *
                    edge.Data.Distance;
                var weight = distance * factor.Value;
                return new Path(target.VertexId(db), weight, new Path(point.VertexId(db)));
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