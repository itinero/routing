using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Itinero.Data.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                attributes.AddAttribute("id", vertex.ToInvariantString());
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
        public static FeatureCollection GetFeaturesIn(this RouterDb db, float minLatitude, float minLongitude,
            float maxLatitude, float maxLongitude, bool includeEdges = true)
        {
            var features = new FeatureCollection();

            var vertices = Itinero.Algorithms.Search.Hilbert.HilbertExtensions.Search(db.Network.GeometricGraph, minLatitude, minLongitude,
                maxLatitude, maxLongitude);
            var edges = new HashSet<long>();

            var edgeEnumerator = db.Network.GetEdgeEnumerator();
            foreach (var vertex in vertices)
            {
                var attributes = new AttributesTable();
                attributes.AddAttribute("id", vertex.ToInvariantString());
                var vertexLocation = db.Network.GetVertex(vertex);
                features.Add(new Feature(new Point(vertexLocation.ToCoordinate()),
                    attributes));

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

                        var geometry = new LineString(db.Network.GetShape(edgeEnumerator.Current).ToCoordinatesArray());
                        attributes = new AttributesTable();
                        attributes.AddAttribute("id", edgeEnumerator.Id.ToInvariantString());
                        features.Add(new Feature(geometry,
                            attributes));
                    }
                }
            }

            return features;
        }
    }
}