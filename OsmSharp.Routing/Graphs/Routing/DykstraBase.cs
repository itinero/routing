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
using OsmSharp.Math.Geo;
using OsmSharp.Math.Primitives;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Vehicles;
using OsmSharp.Units.Distance;
using System.Collections.Generic;
using System.Linq;

namespace OsmSharp.Routing.Graph.Routing
{
    /// <summary>
    /// Contains generic fuctions common to all dykstra routers.
    /// </summary>
    public abstract class DykstraBase<TEdgeData>
        where TEdgeData : IGraphEdgeData
    {
        /// <summary>
        /// Creates a new basic dykstra router.
        /// </summary>
        protected DykstraBase()
        {

        }

        #region Search Closest

        /// <summary>
        /// Searches the data for a point on an edge closest to the given coordinate.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="vehicle"></param>
        /// <param name="coordinate"></param>
        /// <param name="delta"></param>
        /// <param name="matcher"></param>
        /// <param name="pointTags"></param>
        /// <param name="interpreter"></param>
        /// <param name="parameters"></param>
        public SearchClosestResult<TEdgeData> SearchClosest(IRoutingAlgorithmData<TEdgeData> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            GeoCoordinate coordinate, float delta, IEdgeMatcher matcher, TagsCollectionBase pointTags, Dictionary<string, object> parameters)
        {
            return this.SearchClosest(graph, interpreter, vehicle, coordinate, delta, matcher, pointTags, false, null);
        }

        /// <summary>
        /// Searches the data for a point on an edge closest to the given coordinate.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="vehicle"></param>
        /// <param name="coordinate"></param>
        /// <param name="delta"></param>
        /// <param name="matcher"></param>
        /// <param name="pointTags"></param>
        /// <param name="interpreter"></param>
        /// <param name="verticesOnly"></param>
        /// <param name="parameters"></param>
        public SearchClosestResult<TEdgeData> SearchClosest(IRoutingAlgorithmData<TEdgeData> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            GeoCoordinate coordinate, float delta, IEdgeMatcher matcher, TagsCollectionBase pointTags, bool verticesOnly, Dictionary<string, object> parameters)
        {
            Meter distanceEpsilon = .1; // 10cm is the tolerance to distinguish points.

            var closestWithMatch = new SearchClosestResult<TEdgeData>(double.MaxValue, 0);
            GeoCoordinateBox closestWithMatchBox = null;
            var closestWithoutMatch = new SearchClosestResult<TEdgeData>(double.MaxValue, 0);
            GeoCoordinateBox closestWithoutMatchBox = null;

            double searchBoxSize = delta;
            // create the search box.
            var searchBox = new GeoCoordinateBox(new GeoCoordinate(
                coordinate.Latitude - searchBoxSize, coordinate.Longitude - searchBoxSize),
                                                               new GeoCoordinate(
                coordinate.Latitude + searchBoxSize, coordinate.Longitude + searchBoxSize));

            // get the arcs from the data source.
            var edges =  graph.GetEdges(searchBox);

            if (!verticesOnly)
            { // find both closest arcs and vertices.
                // loop over all.
                while (edges.MoveNext())
                {
                    //if (!graph.TagsIndex.Contains(edges.EdgeData.Tags))
                    //{ // skip this edge, no valid tags found.
                    //    continue;
                    //}

                    // test the two points.
                    float fromLatitude, fromLongitude;
                    float toLatitude, toLongitude;
                    double distance;
                    if (graph.GetVertex(edges.Vertex1, out fromLatitude, out fromLongitude) &&
                        graph.GetVertex(edges.Vertex2, out toLatitude, out toLongitude))
                    { // return the vertex.
                        var vertex1Coordinate = new GeoCoordinate(fromLatitude, fromLongitude);
                        var vertex2Coordinate = new GeoCoordinate(toLatitude, toLongitude);

                        if (edges.EdgeData.ShapeInBox)
                        { // ok, check if it is needed to even check this edge.
                            var edgeBox = new GeoCoordinateBox(vertex1Coordinate, vertex2Coordinate);
                            var edgeBoxOverlap = false;
                            if (closestWithoutMatchBox == null ||
                                closestWithoutMatchBox.Overlaps(edgeBox))
                            { // edge box overlap.
                                edgeBoxOverlap = true;
                            }
                            else if (closestWithMatchBox == null ||
                                closestWithMatchBox.Overlaps(edgeBox))
                            { // edge box overlap.
                                edgeBoxOverlap = true;
                            }
                            if (!edgeBoxOverlap)
                            { // no overlap, impossible this edge is a candidate.
                                continue;
                            }
                        }

                        var arcTags = graph.TagsIndex.Get(edges.EdgeData.Tags);
                        var canBeTraversed = vehicle.CanTraverse(arcTags);
                        if (canBeTraversed)
                        { // the edge can be traversed.

                            distance = coordinate.DistanceEstimate(vertex1Coordinate).Value;
                            if (distance < distanceEpsilon.Value)
                            { // the distance is smaller than the tolerance value.
                                var diff = coordinate - vertex1Coordinate;
                                closestWithoutMatchBox = new GeoCoordinateBox(new GeoCoordinate(coordinate + diff),
                                    new GeoCoordinate(coordinate - diff));
                                closestWithoutMatch = new SearchClosestResult<TEdgeData>(
                                    distance, edges.Vertex1);
                                if (matcher == null ||
                                    (pointTags == null || pointTags.Count == 0) ||
                                    matcher.MatchWithEdge(vehicle, pointTags, arcTags))
                                {
                                    closestWithMatchBox = new GeoCoordinateBox(new GeoCoordinate(coordinate + diff),
                                        new GeoCoordinate(coordinate - diff));
                                    closestWithMatch = new SearchClosestResult<TEdgeData>(
                                        distance, edges.Vertex1);
                                    break;
                                }
                            }

                            if (distance < closestWithoutMatch.Distance)
                            { // the distance is smaller for the without match.
                                var diff = coordinate - vertex1Coordinate;
                                closestWithoutMatchBox = new GeoCoordinateBox(new GeoCoordinate(coordinate + diff),
                                    new GeoCoordinate(coordinate - diff));
                                closestWithoutMatch = new SearchClosestResult<TEdgeData>(
                                    distance, edges.Vertex1);
                            }
                            if (distance < closestWithMatch.Distance)
                            { // the distance is smaller for the with match.
                                if (matcher == null ||
                                    (pointTags == null || pointTags.Count == 0) ||
                                    matcher.MatchWithEdge(vehicle, pointTags, graph.TagsIndex.Get(edges.EdgeData.Tags)))
                                {
                                    var diff = coordinate - vertex1Coordinate;
                                    closestWithMatchBox = new GeoCoordinateBox(new GeoCoordinate(coordinate + diff),
                                        new GeoCoordinate(coordinate - diff));
                                    closestWithMatch = new SearchClosestResult<TEdgeData>(
                                        distance, edges.Vertex1);
                                }
                            }

                            distance = coordinate.DistanceEstimate(vertex2Coordinate).Value;
                            if (distance < closestWithoutMatch.Distance)
                            { // the distance is smaller for the without match.
                                var diff = coordinate - vertex2Coordinate;
                                closestWithoutMatchBox = new GeoCoordinateBox(new GeoCoordinate(coordinate + diff),
                                    new GeoCoordinate(coordinate - diff));
                                closestWithoutMatch = new SearchClosestResult<TEdgeData>(
                                    distance, edges.Vertex2);
                            }
                            if (distance < closestWithMatch.Distance)
                            { // the distance is smaller for the with match.
                                if (matcher == null ||
                                    (pointTags == null || pointTags.Count == 0) ||
                                    matcher.MatchWithEdge(vehicle, pointTags, arcTags))
                                {
                                    var diff = coordinate - vertex2Coordinate;
                                    closestWithMatchBox = new GeoCoordinateBox(new GeoCoordinate(coordinate + diff),
                                        new GeoCoordinate(coordinate - diff));
                                    closestWithMatch = new SearchClosestResult<TEdgeData>(
                                        distance, edges.Vertex2);
                                }
                            }
                            // search along the line.
                            var coordinatesArray = new ICoordinate[0];
                            var distanceTotal = 0.0;
                            var arcValueValueCoordinates = edges.Intermediates;
                            if (arcValueValueCoordinates != null)
                            { // calculate distance along all coordinates.
                                coordinatesArray = arcValueValueCoordinates.ToArray();
                            }

                            // loop over all edges that are represented by this arc (counting intermediate coordinates).
                            var previous = vertex1Coordinate;
                            GeoCoordinateLine line;
                            var distanceToSegment = 0.0;
                            if (arcValueValueCoordinates != null)
                            {
                                for (int idx = 0; idx < coordinatesArray.Length; idx++)
                                {
                                    var current = new GeoCoordinate(
                                        coordinatesArray[idx].Latitude, coordinatesArray[idx].Longitude);
                                    var edgeBox = new GeoCoordinateBox(previous, current);
                                    var edgeBoxOverlap = false;
                                    if (closestWithoutMatchBox == null ||
                                        closestWithoutMatchBox.Overlaps(edgeBox))
                                    { // edge box overlap.
                                        edgeBoxOverlap = true;
                                    }
                                    else if (closestWithMatchBox == null ||
                                        closestWithMatchBox.Overlaps(edgeBox))
                                    { // edge box overlap.
                                        edgeBoxOverlap = true;
                                    }
                                    if (edgeBoxOverlap)
                                    { // overlap, possible this edge is a candidate.
                                        line = new GeoCoordinateLine(previous, current, true, true);
                                        distance = line.DistanceReal(coordinate).Value;

                                        if (distance < closestWithoutMatch.Distance)
                                        { // the distance is smaller.
                                            var projectedPoint = line.ProjectOn(coordinate);

                                            // calculate the position.
                                            if (projectedPoint != null)
                                            { // calculate the distance.
                                                if (distanceTotal == 0)
                                                { // calculate total distance.
                                                    var pCoordinate = vertex1Coordinate;
                                                    for (int cIdx = 0; cIdx < coordinatesArray.Length; cIdx++)
                                                    {
                                                        var cCoordinate = new GeoCoordinate(coordinatesArray[cIdx].Latitude, coordinatesArray[cIdx].Longitude);
                                                        distanceTotal = distanceTotal + cCoordinate.DistanceReal(pCoordinate).Value;
                                                        pCoordinate = cCoordinate;
                                                    }
                                                    distanceTotal = distanceTotal + vertex2Coordinate.DistanceReal(pCoordinate).Value;
                                                }

                                                var distancePoint = previous.DistanceReal(new GeoCoordinate(projectedPoint)).Value + distanceToSegment;
                                                var position = distancePoint / distanceTotal;

                                                var diff = coordinate - new GeoCoordinate(projectedPoint);
                                                closestWithoutMatchBox = new GeoCoordinateBox(new GeoCoordinate(coordinate + diff),
                                                    new GeoCoordinate(coordinate - diff));
                                                closestWithoutMatch = new SearchClosestResult<TEdgeData>(
                                                    distance, edges.Vertex1, edges.Vertex2, position, edges.EdgeData, coordinatesArray);
                                            }
                                        }
                                        if (distance < closestWithMatch.Distance)
                                        {
                                            var projectedPoint = line.ProjectOn(coordinate);

                                            // calculate the position.
                                            if (projectedPoint != null)
                                            { // calculate the distance
                                                if (distanceTotal == 0)
                                                { // calculate total distance.
                                                    var pCoordinate = vertex1Coordinate;
                                                    for (int cIdx = 0; cIdx < coordinatesArray.Length; cIdx++)
                                                    {
                                                        var cCoordinate = new GeoCoordinate(coordinatesArray[cIdx].Latitude, coordinatesArray[cIdx].Longitude);
                                                        distanceTotal = distanceTotal + cCoordinate.DistanceReal(pCoordinate).Value;
                                                        pCoordinate = cCoordinate;
                                                    }
                                                    distanceTotal = distanceTotal + vertex2Coordinate.DistanceReal(pCoordinate).Value;
                                                }

                                                var distancePoint = previous.DistanceReal(new GeoCoordinate(projectedPoint)).Value + distanceToSegment;
                                                var position = distancePoint / distanceTotal;

                                                if (matcher == null ||
                                                    (pointTags == null || pointTags.Count == 0) ||
                                                    matcher.MatchWithEdge(vehicle, pointTags, arcTags))
                                                {
                                                    var diff = coordinate - new GeoCoordinate(projectedPoint);
                                                    closestWithMatchBox = new GeoCoordinateBox(new GeoCoordinate(coordinate + diff),
                                                        new GeoCoordinate(coordinate - diff));
                                                    closestWithMatch = new SearchClosestResult<TEdgeData>(
                                                        distance, edges.Vertex1, edges.Vertex2, position, edges.EdgeData, coordinatesArray);
                                                }
                                            }
                                        }
                                    }

                                    // add current segment distance to distanceToSegment for the next segment.
                                    distanceToSegment = distanceToSegment + previous.DistanceEstimate(current).Value;

                                    // set previous.
                                    previous = current;
                                }
                            }

                            // check the last segment.
                            line = new GeoCoordinateLine(previous, vertex2Coordinate, true, true);

                            distance = line.DistanceReal(coordinate).Value;

                            if (distance < closestWithoutMatch.Distance)
                            { // the distance is smaller.
                                var projectedPoint = line.ProjectOn(coordinate);

                                // calculate the position.
                                if (projectedPoint != null)
                                { // calculate the distance
                                    if (distanceTotal == 0)
                                    { // calculate total distance.
                                        var pCoordinate = vertex1Coordinate;
                                        for (int cIdx = 0; cIdx < coordinatesArray.Length; cIdx++)
                                        {
                                            var cCoordinate = new GeoCoordinate(coordinatesArray[cIdx].Latitude, coordinatesArray[cIdx].Longitude);
                                            distanceTotal = distanceTotal + cCoordinate.DistanceReal(pCoordinate).Value;
                                            pCoordinate = cCoordinate;
                                        }
                                        distanceTotal = distanceTotal + vertex2Coordinate.DistanceReal(pCoordinate).Value;
                                    }

                                    double distancePoint = previous.DistanceReal(new GeoCoordinate(projectedPoint)).Value + distanceToSegment;
                                    double position = distancePoint / distanceTotal;

                                    var diff = coordinate - new GeoCoordinate(projectedPoint);
                                    closestWithoutMatchBox = new GeoCoordinateBox(new GeoCoordinate(coordinate + diff),
                                        new GeoCoordinate(coordinate - diff));
                                    closestWithoutMatch = new SearchClosestResult<TEdgeData>(
                                        distance, edges.Vertex1, edges.Vertex2, position, edges.EdgeData, coordinatesArray);
                                }
                            }
                            if (distance < closestWithMatch.Distance)
                            {
                                var projectedPoint = line.ProjectOn(coordinate);

                                // calculate the position.
                                if (projectedPoint != null)
                                { // calculate the distance
                                    if (distanceTotal == 0)
                                    { // calculate total distance.
                                        var pCoordinate = vertex1Coordinate;
                                        for (int cIdx = 0; cIdx < coordinatesArray.Length; cIdx++)
                                        {
                                            var cCoordinate = new GeoCoordinate(coordinatesArray[cIdx].Latitude, coordinatesArray[cIdx].Longitude);
                                            distanceTotal = distanceTotal + cCoordinate.DistanceReal(pCoordinate).Value;
                                            pCoordinate = cCoordinate;
                                        }
                                        distanceTotal = distanceTotal + vertex2Coordinate.DistanceReal(pCoordinate).Value;
                                    }

                                    double distancePoint = previous.DistanceReal(new GeoCoordinate(projectedPoint)).Value + distanceToSegment;
                                    double position = distancePoint / distanceTotal;

                                    if (matcher == null ||
                                        (pointTags == null || pointTags.Count == 0) ||
                                        matcher.MatchWithEdge(vehicle, pointTags, arcTags))
                                    {
                                        var diff = coordinate - new GeoCoordinate(projectedPoint);
                                        closestWithMatchBox = new GeoCoordinateBox(new GeoCoordinate(coordinate + diff),
                                            new GeoCoordinate(coordinate - diff));
                                        closestWithMatch = new SearchClosestResult<TEdgeData>(
                                            distance, edges.Vertex1, edges.Vertex2, position, edges.EdgeData, coordinatesArray);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            { // only find closest vertices.
                // loop over all.
                while (edges.MoveNext())
                {
                    float fromLatitude, fromLongitude;
                    float toLatitude, toLongitude;
                    if (graph.GetVertex(edges.Vertex1, out fromLatitude, out fromLongitude) &&
                        graph.GetVertex(edges.Vertex2, out toLatitude, out toLongitude))
                    {
                        var vertexCoordinate = new GeoCoordinate(fromLatitude, fromLongitude);
                        double distance = coordinate.DistanceReal(vertexCoordinate).Value;
                        if (distance < closestWithoutMatch.Distance)
                        { // the distance found is closer.
                            var diff = coordinate - vertexCoordinate;
                            closestWithoutMatchBox = new GeoCoordinateBox(new GeoCoordinate(coordinate + diff),
                                new GeoCoordinate(coordinate - diff));
                            closestWithoutMatch = new SearchClosestResult<TEdgeData>(
                                distance, edges.Vertex1);
                        }

                        vertexCoordinate = new GeoCoordinate(toLatitude, toLongitude);
                        distance = coordinate.DistanceReal(vertexCoordinate).Value;
                        if (distance < closestWithoutMatch.Distance)
                        { // the distance found is closer.
                            var diff = coordinate - vertexCoordinate;
                            closestWithoutMatchBox = new GeoCoordinateBox(new GeoCoordinate(coordinate + diff),
                                new GeoCoordinate(coordinate - diff));
                            closestWithoutMatch = new SearchClosestResult<TEdgeData>(
                                distance, edges.Vertex2);
                        }

                        var arcValueValueCoordinates = edges.Intermediates;
                        if (arcValueValueCoordinates != null)
                        { // search over intermediate points.
                            var arcValueValueCoordinatesArray = arcValueValueCoordinates.ToArray();
                            for (int idx = 0; idx < arcValueValueCoordinatesArray.Length; idx++)
                            {
                                vertexCoordinate = new GeoCoordinate(
                                    arcValueValueCoordinatesArray[idx].Latitude,
                                    arcValueValueCoordinatesArray[idx].Longitude);
                                distance = coordinate.DistanceReal(vertexCoordinate).Value;
                                if (distance < closestWithoutMatch.Distance)
                                { // the distance found is closer.
                                    var diff = coordinate - vertexCoordinate;
                                    closestWithoutMatchBox = new GeoCoordinateBox(new GeoCoordinate(coordinate + diff),
                                        new GeoCoordinate(coordinate - diff));
                                    closestWithoutMatch = new SearchClosestResult<TEdgeData>(
                                        distance, edges.Vertex1, edges.Vertex2, idx, edges.EdgeData, arcValueValueCoordinatesArray);
                                }
                            }
                        }
                    }
                }
            }

            // return the best result.
            if (closestWithMatch.Distance < double.MaxValue)
            {
                return closestWithMatch;
            }
            return closestWithoutMatch;
        }

        #endregion
    }
}