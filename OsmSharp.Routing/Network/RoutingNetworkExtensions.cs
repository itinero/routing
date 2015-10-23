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

using OsmSharp.Collections.Coordinates.Collections;
using OsmSharp.Collections.Tags;
using OsmSharp.Geo.Attributes;
using OsmSharp.Geo.Features;
using OsmSharp.Geo.Geometries;
using OsmSharp.Math.Geo;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Network
{
    /// <summary>
    /// Contains extension methods for the routing network.
    /// </summary>
    public static class RoutingNetworkExtensions
    {
        /// <summary>
        /// Gets the first point on the given edge starting a the given vertex.
        /// </summary>
        /// <returns></returns>
        public static ICoordinate GetFirstPoint(this RoutingNetwork graph, RoutingEdge edge, uint vertex)
        {
            var points = new List<ICoordinate>();
            if (edge.From == vertex)
            { // start at from.
                if (edge.Shape == null)
                {
                    return graph.GetVertex(edge.To);
                }
                var shape = edge.Shape;
                shape.MoveNext();
                return shape.Current;
            }
            else if (edge.To == vertex)
            { // start at to.
                if (edge.Shape == null)
                {
                    return graph.GetVertex(edge.From);
                }
                var shape = edge.Shape.Reverse();
                shape.MoveNext();
                return shape.Current;
            }
            throw new ArgumentOutOfRangeException(string.Format("Vertex {0} is not part of edge {1}.",
                vertex, edge.Id));
        }

        /// <summary>
        /// Gets the vertex on this edge that is not the given vertex.
        /// </summary>
        /// <returns></returns>
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
        /// <returns></returns>
        public static List<ICoordinate> GetShape(this RoutingNetwork graph, RoutingEdge edge)
        {
            var points = new List<ICoordinate>();
            points.Add(graph.GetVertex(edge.From));
            var shape = edge.Shape;
            if (shape != null)
            {
                if (edge.DataInverted)
                {
                    shape = shape.Reverse();
                }
                shape.Reset();
                while (shape.MoveNext())
                {
                    points.Add(shape.Current);
                }
            }
            points.Add(graph.GetVertex(edge.To));
            return points;
        }

        /// <summary>
        /// Gets all features.
        /// </summary>
        /// <returns></returns>
        public static FeatureCollection GetFeatures(this RoutingNetwork network)
        {
            return network.GetFeaturesIn(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue);
        }

        /// <summary>
        /// Gets all features inside the given bounding box.
        /// </summary>
        /// <returns></returns>
        public static FeatureCollection GetFeaturesIn(this RoutingNetwork network, float minLatitude, float minLongitude,
            float maxLatitude, float maxLongitude)
        {
            var features = new FeatureCollection();

            var vertices = OsmSharp.Routing.Algorithms.Search.Hilbert.Search(network.GeometricGraph, minLatitude, minLongitude,
                maxLatitude, maxLongitude);
            var edges = new HashSet<long>();

            var edgeEnumerator = network.GetEdgeEnumerator();
            foreach (var vertex in vertices)
            {
                var vertexLocation = network.GeometricGraph.GetVertex(vertex);
                features.Add(new Feature(new Point(new GeoCoordinate(vertexLocation.Latitude, vertexLocation.Longitude)),
                    new SimpleGeometryAttributeCollection(new Tag[] { Tag.Create("id", vertex.ToInvariantString()) })));
                edgeEnumerator.MoveTo(vertex);
                edgeEnumerator.Reset();
                while (edgeEnumerator.MoveNext())
                {
                    if (edges.Contains(edgeEnumerator.Id))
                    {
                        continue;
                    }
                    edges.Add(edgeEnumerator.Id);

                    var shape = network.GetShape(edgeEnumerator.Current);
                    var coordinates = new List<GeoCoordinate>();
                    foreach (var shapePoint in shape)
                    {
                        coordinates.Add(new GeoCoordinate(shapePoint.Latitude, shapePoint.Longitude));
                    }
                    var geometry = new LineString(coordinates);

                    features.Add(new Feature(geometry,
                        new SimpleGeometryAttributeCollection(new Tag[] { Tag.Create("id", edgeEnumerator.Id.ToInvariantString()) })));
                }
            }

            return features;
        }

        /// <summary>
        /// Gets features for all the given vertices.
        /// </summary>
        /// <returns></returns>
        public static FeatureCollection GetFeaturesFor(this RoutingNetwork network, List<uint> vertices)
        {
            var features = new FeatureCollection();

            foreach (var vertex in vertices)
            {
                float latitude1, longitude1;
                if(network.GeometricGraph.GetVertex(vertex, out latitude1, out longitude1))
                {
                    var vertexLocation = new GeoCoordinate(latitude1, longitude1);
                    features.Add(new Feature(new Point(new GeoCoordinate(vertexLocation.Latitude, vertexLocation.Longitude)),
                        new SimpleGeometryAttributeCollection(new Tag[] { Tag.Create("id", vertex.ToInvariantString()) })));
                }
            }

            return features;
        }

        /// <summary>
        /// Returns true if the routing network contains an edge between the two given vertices.
        /// </summary>
        /// <returns></returns>
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
        public static uint AddEdge(this RoutingNetwork network, uint vertex1, uint vertex2, ushort profile, uint metaId, float distance, ICoordinateCollection shape)
        {
            return network.AddEdge(vertex1, vertex2, new Data.EdgeData()
                {
                    Distance = distance,
                    MetaId = metaId,
                    Profile = profile
                }, shape);
        }

        /// <summary>
        /// Adds a new edge.
        /// </summary>
        public static uint AddEdge(this RoutingNetwork network, uint vertex1, uint vertex2, ushort profile, uint metaId, float distance,
            params ICoordinate[] shape)
        {
            return network.AddEdge(vertex1, vertex2, new Data.EdgeData()
            {
                Distance = distance,
                MetaId = metaId,
                Profile = profile
            }, new CoordinateArrayCollection<ICoordinate>(shape));
        }
    }
}
