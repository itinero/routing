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

using OsmSharp.Collections.Tags;
using OsmSharp.Geo.Attributes;
using OsmSharp.Geo.Features;
using OsmSharp.Geo.Geometries;
using OsmSharp.Math.Geo;
using OsmSharp.Routing.Algorithms.Contracted;
using OsmSharp.Routing.Algorithms.Contracted.Witness;
using OsmSharp.Routing.Data.Contracted;
using OsmSharp.Routing.Graphs.Directed;
using OsmSharp.Routing.Network;
using System.Collections.Generic;

namespace OsmSharp.Routing
{
    /// <summary>
    /// Contains extension methods for the router db.
    /// </summary>
    public static class RouterDbExtensions
    {
        /// <summary>
        /// Gets all features inside the given bounding box.
        /// </summary>
        /// <returns></returns>
        public static FeatureCollection GetFeatures(this RouterDb db)
        {
            var network = db.Network;
            var features = new FeatureCollection();

            var edges = new HashSet<long>();

            var edgeEnumerator = network.GetEdgeEnumerator();
            for (uint vertex = 0; vertex < network.VertexCount; vertex++)
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

                    var tags = new TagsCollection(db.Profiles.Get(edgeEnumerator.Data.Profile));
                    tags.AddOrReplace(db.Meta.Get(edgeEnumerator.Data.MetaId));
                    tags.AddOrReplace(Tag.Create("id", edgeEnumerator.Id.ToInvariantString()));
                    features.Add(new Feature(geometry,
                        new SimpleGeometryAttributeCollection(tags)));
                }
            }

            return features;
        }

        /// <summary>
        /// Gets all features inside the given bounding box.
        /// </summary>
        /// <returns></returns>
        public static FeatureCollection GetFeaturesIn(this RouterDb db, float minLatitude, float minLongitude,
            float maxLatitude, float maxLongitude)
        {
            var network = db.Network;
            var features = new FeatureCollection();

            var vertices = OsmSharp.Routing.Algorithms.Search.Hilbert.Search(network.GeometricGraph, minLatitude, minLongitude,
                maxLatitude, maxLongitude);
            var edges = new HashSet<long>();

            var edgeEnumerator = network.GetEdgeEnumerator();
            var extraVertices = new HashSet<uint>();
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

                    var tags = new TagsCollection(db.Profiles.Get(edgeEnumerator.Data.Profile));
                    tags.AddOrReplace(db.Meta.Get(edgeEnumerator.Data.MetaId));
                    tags.AddOrReplace(Tag.Create("id", edgeEnumerator.Id.ToInvariantString()));
                    features.Add(new Feature(geometry,
                        new SimpleGeometryAttributeCollection(tags)));

                    if(!vertices.Contains(edgeEnumerator.To))
                    {
                        extraVertices.Add(edgeEnumerator.To);
                    }
                }
            }
            foreach (var vertex in extraVertices)
            {
                var vertexLocation = network.GeometricGraph.GetVertex(vertex);
                features.Add(new Feature(new Point(new GeoCoordinate(vertexLocation.Latitude, vertexLocation.Longitude)),
                    new SimpleGeometryAttributeCollection(new Tag[] { Tag.Create("id", vertex.ToInvariantString()) })));
            }

            return features;
        }

        /// <summary>
        /// Creates a new contracted graph and adds it to the router db for the given profile.
        /// </summary>
        public static void AddContracted(this RouterDb db, Profiles.Profile profile)
        {
            // create the raw directed graph.
            var contracted = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size, ContractedEdgeDataSerializer.MetaSize);
            var directedGraphBuilder = new DirectedGraphBuilder(db.Network.GeometricGraph.Graph, contracted, (p) =>
                {
                    var tags = db.Profiles.Get(p);
                    return profile.Factor(tags);
                });
            directedGraphBuilder.Run();

            // contract the graph.
            var priorityCalculator = new EdgeDifferencePriorityCalculator(contracted,
                new DykstraWitnessCalculator(int.MaxValue));
            priorityCalculator.DifferenceFactor = 5;
            priorityCalculator.DepthFactor = 5;
            priorityCalculator.ContractedFactor = 8;
            var hierarchyBuilder = new HierarchyBuilder(contracted, priorityCalculator,
                    new DykstraWitnessCalculator(int.MaxValue));
            hierarchyBuilder.Run();

            // add the graph.
            db.AddContracted(profile, contracted);
        }

        /// <summary>
        /// Returns true if all of the given profiles are supported.
        /// </summary>
        /// <returns></returns>
        public static bool SupportsAll(this RouterDb db, Profiles.Profile[] profiles)
        {
            for (var i = 0; i < profiles.Length; i++)
            {
                if (!db.Supports(profiles[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
