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
                attributes.AddAttribute("id", vertex.ToInvariantString());
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
                    attributes.AddAttribute("id", edgeEnumerator.Id.ToInvariantString());
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
                    attributes.AddAttribute("id", vertex.ToInvariantString());
                    features.Add(new Feature(new Point(vertexLocation.ToCoordinate()),
                        attributes));
                }
            }

            return features;
        }
    }
}