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

using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Itinero.Data.Network;
using System.Collections.Generic;

namespace Itinero.Geo.Network
{
    /// <summary>
    /// Contains extensions for the routing network.
    /// </summary>
    public static class RoutingNetworkExtensions
    {
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

            var vertices = Itinero.Algorithms.Search.Hilbert.HilbertExtensions.Search(network.GeometricGraph, minLatitude, minLongitude,
                maxLatitude, maxLongitude);
            var edges = new HashSet<long>();

            var edgeEnumerator = network.GetEdgeEnumerator();
            foreach (var vertex in vertices)
            {
                var attributes = new AttributesTable();
                attributes.Add("id", vertex.ToInvariantString());
                var vertexLocation = network.GetVertex(vertex);
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

                    var geometry = new LineString(network.GetShape(edgeEnumerator.Current).ToCoordinatesArray());
                    attributes = new AttributesTable();
                    attributes.Add("id", edgeEnumerator.Id.ToInvariantString());
                    features.Add(new Feature(geometry,
                        attributes));
                }
            }

            return features;
        }

        /// <summary>
        /// Gets features for all the given vertices.
        /// </summary>
        public static FeatureCollection GetFeaturesFor(this RoutingNetwork network, List<uint> vertices)
        {
            var features = new FeatureCollection();

            foreach (var vertex in vertices)
            {
                float latitude1, longitude1;
                if (network.GeometricGraph.GetVertex(vertex, out latitude1, out longitude1))
                {
                    var vertexLocation = new LocalGeo.Coordinate(latitude1, longitude1);
                    var attributes = new AttributesTable();
                    attributes.Add("id", vertex.ToInvariantString());
                    features.Add(new Feature(new Point(vertexLocation.ToCoordinate()),
                        attributes));
                }
            }

            return features;
        }
    }
}