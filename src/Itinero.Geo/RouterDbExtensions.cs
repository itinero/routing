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

 using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Itinero.Data.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Itinero.Attributes;
using Itinero.Geo.Attributes;

namespace Itinero.Geo
{
    /// <summary>
    /// Contains extensions for the router db.
    /// </summary>
    public static class RouterDbExtensions
    {
        /// <summary>
        /// Gets all features.
        /// </summary>
        public static FeatureCollection GetFeatures(this RouterDb db)
        {
            var network = db.Network;
            var features = new FeatureCollection();

            var edges = new HashSet<long>();

            var edgeEnumerator = network.GetEdgeEnumerator();
            for (uint vertex = 0; vertex < network.VertexCount; vertex++)
            {
                var vertexLocation = network.GeometricGraph.GetVertex(vertex);
                var attributes = new AttributesTable();
                attributes.Add("id", vertex.ToInvariantString());
                features.Add(new Feature(new Point(vertexLocation.ToCoordinate()),
                    attributes));
                edgeEnumerator.MoveTo(vertex);
                edgeEnumerator.Reset();
                while (edgeEnumerator.MoveNext())
                {
                    if (edges.Contains(edgeEnumerator.Id))
                    {
                        continue;
                    }
                    edges.Add(edgeEnumerator.Id);

                    var edgeAttributes = new Itinero.Attributes.AttributeCollection(db.EdgeMeta.Get(edgeEnumerator.Data.MetaId));
                    edgeAttributes.AddOrReplace(db.EdgeProfiles.Get(edgeEnumerator.Data.Profile));

                    var geometry = new LineString(network.GetShape(edgeEnumerator.Current).ToCoordinatesArray());
                    attributes = edgeAttributes.ToAttributesTable();
                    attributes.Add("id", edgeEnumerator.Id.ToInvariantString());
                    attributes.Add("distance", edgeEnumerator.Data.Distance.ToInvariantString());
                    var tags = db.GetProfileAndMeta(edgeEnumerator.Data.Profile, edgeEnumerator.Data.MetaId);
                    features.Add(new Feature(geometry,
                        attributes));
                }
            }

            return features;
        }

        /// <summary>
        /// Gets all features inside the given bounding box.
        /// </summary>
        public static FeatureCollection GetFeaturesIn(this RouterDb db, LocalGeo.Coordinate coord1,
            LocalGeo.Coordinate coord2, bool includeEdges = true, bool includeVertices = true)
        {
            return db.GetFeaturesIn(System.Math.Min(coord1.Latitude, coord2.Latitude), System.Math.Min(coord1.Longitude, coord2.Longitude),
                System.Math.Max(coord1.Latitude, coord2.Latitude), System.Math.Max(coord1.Longitude, coord2.Longitude), includeEdges, includeVertices);
        }

        /// <summary>
        /// Gets all features inside the given bounding box.
        /// </summary>
        public static FeatureCollection GetFeaturesIn(this RouterDb db, float minLatitude, float minLongitude,
            float maxLatitude, float maxLongitude, bool includeEdges = true, bool includeVertices = true)
        {
            var features = new FeatureCollection();

            var vertices = Itinero.Algorithms.Search.Hilbert.HilbertExtensions.Search(db.Network.GeometricGraph, minLatitude, minLongitude,
                maxLatitude, maxLongitude);
            var edges = new HashSet<long>();

            var edgeEnumerator = db.Network.GetEdgeEnumerator();
            foreach (var vertex in vertices)
            {
                if (includeVertices)
                {
                    features.Add(db.GetFeatureForVertex(vertex));
                }

                if (includeEdges)
                {
                    edgeEnumerator.MoveTo(vertex);
                    edgeEnumerator.Reset();
                    while (edgeEnumerator.MoveNext())
                    {
                        if (edges.Contains(edgeEnumerator.Id))
                        {
                            continue;
                        }
                        edges.Add(edgeEnumerator.Id);

                        var edgeAttributes = new Itinero.Attributes.AttributeCollection(db.EdgeMeta.Get(edgeEnumerator.Data.MetaId));
                        edgeAttributes.AddOrReplace(db.EdgeProfiles.Get(edgeEnumerator.Data.Profile));

                        var geometry = new LineString(db.Network.GetShape(edgeEnumerator.Current).ToCoordinatesArray());
                        var attributes = edgeAttributes.ToAttributesTable();
                        attributes.Add("id", edgeEnumerator.Id.ToInvariantString());
                        attributes.Add("distance", edgeEnumerator.Data.Distance.ToInvariantString());
                        features.Add(new Feature(geometry,
                            attributes));
                    }
                }
            }

            return features;
        }

        /// <summary>
        /// Gets a feature representing the edge with the given id.
        /// </summary>
        public static Feature GetFeatureForEdge(this RouterDb routerDb, uint edgeId)
        {
            var edge = routerDb.Network.GetEdge(edgeId);

            var edgeAttributes = new Itinero.Attributes.AttributeCollection(routerDb.EdgeMeta.Get(edge.Data.MetaId));
            edgeAttributes.AddOrReplace(routerDb.EdgeProfiles.Get(edge.Data.Profile));

            var geometry = new LineString(routerDb.Network.GetShape(edge).ToCoordinatesArray());
            var attributes = edgeAttributes.ToAttributesTable();
            attributes.Add("id", edge.Id.ToInvariantString());
            attributes.Add("distance", edge.Data.Distance.ToInvariantString());
            return new Feature(geometry, attributes);
        }

        /// <summary>
        /// Gets a features representing the vertex with the given id.
        /// </summary>
        public static Feature GetFeatureForVertex(this RouterDb routerDb, uint vertex)
        {
            var coordinate = routerDb.Network.GetVertex(vertex).ToCoordinate();

            var attributes = new AttributesTable();
            attributes.Add("id", vertex);
            return new Feature(new Point(coordinate), attributes);
        }
    }
}