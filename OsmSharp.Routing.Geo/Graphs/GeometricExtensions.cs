// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
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

using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using OsmSharp.Routing.Graphs.Geometric;
using System.Collections.Generic;

namespace OsmSharp.Routing.Geo.Graphs
{
    /// <summary>
    /// Contains extensions for the geometric graph.
    /// </summary>
    public static class GeometricExtensions
    {
        /// <summary>
        /// Gets all features inside the given bounding box.
        /// </summary>
        public static FeatureCollection GetFeaturesIn(this GeometricGraph graph, float minLatitude, float minLongitude,
            float maxLatitude, float maxLongitude)
        {
            var features = new FeatureCollection();

            var vertices = OsmSharp.Routing.Algorithms.Search.Hilbert.HilbertExtensions.Search(graph, minLatitude, minLongitude,
                maxLatitude, maxLongitude);
            var edges = new HashSet<long>();

            var edgeEnumerator = graph.GetEdgeEnumerator();
            foreach (var vertex in vertices)
            {
                var attributes = new AttributesTable();
                attributes.AddAttribute("id", vertex.ToInvariantString());
                var vertexLocation = graph.GetVertex(vertex);
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
                    
                    var geometry = new LineString(graph.GetShape(edgeEnumerator.Current).ToCoordinatesArray());
                    attributes = new AttributesTable();
                    attributes.AddAttribute("id", edgeEnumerator.Id.ToInvariantString());
                    features.Add(new Feature(geometry,
                        attributes));
                }
            }

            return features;
        }
    }
}