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

using OsmSharp.Math.Geo;
using OsmSharp.Routing.Data;
using OsmSharp.Routing.Graph.Geometric;
using OsmSharp.Routing.Network;
using OsmSharp.Routing.Vehicles;
using OsmSharp.Units.Distance;
using System;

namespace OsmSharp.Routing.Algorithms.Resolver
{
    /// <summary>
    /// A resolver for a path segment visit list.
    /// </summary>
    public class PathSegmentVisitListResolver : IResolver<PathSegmentVisitList, RouterPoint>
    {
        private readonly GeometricGraph<EdgeData> _graph;
        private readonly double _delta = .0075f;
        private readonly Vehicle _vehicle;

        /// <summary>
        /// Creates a path segment visit list.
        /// </summary>
        public PathSegmentVisitListResolver(GeometricGraph<EdgeData> graph, Vehicle vehicle)
        {
            _graph = graph;
            _vehicle = vehicle;
        }

        /// <summary>
        /// Resolves a location to a hook that hooks onto a routing graph and that is routable.
        /// </summary>
        /// <exception cref="System.Exception">The resolving operation failed.</exception>
        /// <returns>A routing hook. This method should quarantee a non-null return.</returns>
        public RouterPoint Resolve(float latitude, float longitude)
        {
            GeoCoordinate closestLocation;
            var vertex = this.SearchClosest(new GeoCoordinate(latitude, longitude), out closestLocation);
            return new RouterPoint(vertex, _vehicle, closestLocation);
        }

        /// <summary>
        /// Converts a resolved point to a routing hook that can serve as the start of a routing algorithm run.
        /// </summary>
        /// <param name="routingPoint"></param>
        /// <returns></returns>
        public PathSegmentVisitList GetHook(RouterPoint routingPoint)
        {
            return new PathSegmentVisitList(routingPoint.Id);
        }

        #region Search Closest

        /// <summary>
        /// Searches the data for a point on an edge closest to the given coordinate.
        /// </summary>
        private uint SearchClosest(GeoCoordinate location, out GeoCoordinate closestLocation)
        {
            Meter distanceEpsilon = .1; // 10cm is the tolerance to distinguish points.

            // create the search box.
            var searchBox = new GeoCoordinateBox(location, location);
            searchBox = searchBox.Resize(_delta);

            // get the arcs from the data source.
            var arcs = _graph.GetEdges(searchBox);

            var bestVertex = uint.MaxValue;
            var bestDistance = double.MaxValue;
            closestLocation = null;

            // loop over all.
            while (arcs.MoveNext())
            {
                float fromLatitude, fromLongitude;
                float toLatitude, toLongitude;
                if (_graph.GetVertex(arcs.Vertex1, out fromLatitude, out fromLongitude) &&
                    _graph.GetVertex(arcs.Vertex2, out toLatitude, out toLongitude))
                {
                    var vertexCoordinate = new GeoCoordinate(fromLatitude, fromLongitude);
                    var distance = location.DistanceEstimate(vertexCoordinate).Value;
                    if (distance < bestDistance)
                    { // the distance found is closer.
                        bestDistance = distance;
                        bestVertex = arcs.Vertex1;
                        closestLocation = vertexCoordinate;
                    }

                    vertexCoordinate = new GeoCoordinate(toLatitude, toLongitude);
                    distance = location.DistanceEstimate(vertexCoordinate).Value;
                    if (distance < bestDistance)
                    { // the distance found is closer.
                        bestDistance = distance;
                        bestVertex = arcs.Vertex2;
                        closestLocation = vertexCoordinate;
                    }
                }
            }

            if (bestDistance == uint.MaxValue)
            {
                throw new Exception("Resolve failed: Could not find a vertex close enough.");
            }
            return bestVertex;
        }

        #endregion
    }
}