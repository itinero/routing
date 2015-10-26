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
using OsmSharp.Math.Geo.Meta;
using OsmSharp.Math.Geo.Simple;
using OsmSharp.Routing.Network;
using OsmSharp.Routing.Profiles;
using OsmSharp.Units.Distance;
using OsmSharp.Units.Time;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace OsmSharp.Routing
{
    /// <summary>
    /// Contains extensions for the route object.
    /// </summary>
    public static class RouteExtensions
    {
        #region Save / Load

        /// <summary>
        /// Saves a serialized version to a stream.
        /// </summary>
        public static void Save(this Route route, Stream stream)
        {
            var ser = new XmlSerializer(typeof(Route));
            ser.Serialize(stream, route);
            stream.Flush();
        }

        /// <summary>
        /// Saves the route as a byte stream.
        /// </summary>
        /// <returns></returns>
        public static byte[] SaveToByteArray(this Route route)
        {
            using (var memoryStream = new MemoryStream())
            {
                route.Save(memoryStream);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Reads a route from a data stream.
        /// </summary>
        /// <returns></returns>
        public static Route Load(Stream stream)
        {
            var ser = new XmlSerializer(typeof(Route));
            return ser.Deserialize(stream) as Route;
        }

        /// <summary>
        /// Parses a route from a byte array.
        /// </summary>
        /// <returns></returns>
        public static Route Load(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream(bytes))
            {
                var serializer = new XmlSerializer(typeof(Route));
                return (serializer.Deserialize(memoryStream) as Route);
            }
        }

        /// <summary>
        /// Save the route as GeoJson.
        /// </summary>
        public static void SaveAsGeoJson(this Route route, Stream stream)
        {
            var streamWriter = new StreamWriter(stream);
            streamWriter.Write(route.ToGeoJson());
            streamWriter.Flush();
        }

        /// <summary>
        /// Returns this route in GeoJson.
        /// </summary>
        /// <returns></returns>
        public static string ToGeoJson(this Route route)
        {
            return OsmSharp.Geo.Streams.GeoJson.GeoJsonConverter.ToGeoJson(route.ToFeatureCollection());
        }

        /// <summary>
        /// Converts this route to a feature collection.
        /// </summary>
        /// <returns></returns>
        public static FeatureCollection ToFeatureCollection(this Route route)
        {
            var featureCollection = new FeatureCollection();
            if (route.Segments == null)
            {
                return featureCollection;
            }
            for (int i = 0; i < route.Segments.Count; i++)
            {
                // create a line string for the current segment.
                if (i > 0)
                { // but only do so when there is a previous point available.
                    var segmentLineString = new LineString(
                        new GeoCoordinate(route.Segments[i - 1].Latitude, route.Segments[i - 1].Longitude),
                        new GeoCoordinate(route.Segments[i].Latitude, route.Segments[i].Longitude));

                    var segmentTags = route.Segments[i].Tags;
                    var attributesTable = new SimpleGeometryAttributeCollection();
                    if (segmentTags != null)
                    { // there are tags.
                        foreach (var tag in segmentTags)
                        {
                            attributesTable.Add(tag.Key, tag.Value);
                        }
                    }
                    attributesTable.Add("time", route.Segments[i].Time);
                    attributesTable.Add("distance", route.Segments[i].Distance);
                    //if (route.Segments[i].Vehicle != null)
                    //{
                    //    attributesTable.Add("vehicle", route.Segments[i].Vehicle);
                    //}
                    featureCollection.Add(new Feature(segmentLineString, attributesTable));
                }

                // create points.
                if (route.Segments[i].Points != null)
                {
                    foreach (var point in route.Segments[i].Points)
                    {
                        // build attributes.
                        var currentPointTags = point.Tags;
                        var attributesTable = new SimpleGeometryAttributeCollection();
                        if (currentPointTags != null)
                        { // there are tags.
                            foreach (var tag in currentPointTags)
                            {
                                attributesTable.Add(tag.Key, tag.Value);
                            }
                        }

                        // build feature.
                        var pointGeometry = new Point(new GeoCoordinate(point.Latitude, point.Longitude));
                        featureCollection.Add(new Feature(pointGeometry, attributesTable));
                    }
                }
            }
            return featureCollection;
        }

        #endregion

        /// <summary>
        /// Concatenates two routes.
        /// </summary>
        /// <returns></returns>
        public static Route Concatenate(this Route route1, Route route2)
        {
            return route1.Concatenate(route2, true);
        }

        /// <summary>
        /// Concatenates two routes.
        /// </summary>
        /// <returns></returns>
        public static Route Concatenate(this Route route1, Route route2, bool clone)
        {
            if (route1 == null) return route2;
            if (route2 == null) return route1;
            if (route1.Segments.Count == 0) return route2;
            if (route2.Segments.Count == 0) return route1;

            // get the end/start point.
            var end = route1.Segments[route1.Segments.Count - 1];
            var endTime = end.Time;
            var endDistance = end.Distance;
            var start = route2.Segments[0];

            // only do all this if the routes are 'concatenable'.
            if (end.Latitude == start.Latitude &&
                end.Longitude == start.Longitude)
            {
                // construct the new route.
                var route = new Route();

                // concatenate points.
                var entries = new List<RouteSegment>();
                for (var idx = 0; idx < route1.Segments.Count - 1; idx++)
                {
                    if (clone)
                    {
                        entries.Add(route1.Segments[idx].Clone() as RouteSegment);
                    }
                    else
                    {
                        entries.Add(route1.Segments[idx]);
                    }
                }

                // merge last and first entry.
                var mergedEntry = route1.Segments[route1.Segments.Count - 1].Clone() as RouteSegment;
                if (route2.Segments[0].Points != null && route2.Segments[0].Points.Length > 0)
                { // merge in important points from the second route too but do not keep duplicates.
                    var points = new List<RouteStop>();
                    if (mergedEntry.Points != null)
                    { // keep originals.
                        points.AddRange(mergedEntry.Points);
                    }
                    for (int otherIdx = 0; otherIdx < route2.Segments[0].Points.Length; otherIdx++)
                    { // remove duplicates.
                        bool found = false;
                        for (int idx = 0; idx < points.Count; idx++)
                        {
                            if (points[idx].RepresentsSame(
                                route2.Segments[0].Points[otherIdx]))
                            { // the points represent the same info!
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        { // the point was not in there yet!
                            points.Add(route2.Segments[0].Points[otherIdx]);
                        }
                    }
                    mergedEntry.Points = points.ToArray();
                }
                entries.Add(mergedEntry);

                // add points of the next route.
                for (var idx = 1; idx < route2.Segments.Count; idx++)
                {
                    if (clone)
                    {
                        entries.Add(route2.Segments[idx].Clone() as RouteSegment);
                    }
                    else
                    {
                        entries.Add(route2.Segments[idx]);
                    }
                    entries[entries.Count - 1].Distance = entries[entries.Count - 1].Distance + endDistance;
                    entries[entries.Count - 1].Time = entries[entries.Count - 1].Time + endTime;
                }
                route.Segments = entries;

                // concatenate tags.
                var tags = new List<RouteTags>((route1.Tags == null ? 0 : route1.Tags.Count) +
                    (route2.Tags == null ? 0 : route2.Tags.Count));
                if (route1.Tags != null) { tags.AddRange(route1.Tags); }
                if (route2.Tags != null) { tags.AddRange(route2.Tags); }
                route.Tags = tags;

                // set the vehicle.
                return route;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Contatenation routes can only be done when the end point of the first route equals the start of the second.");
            }
        }

        /// <summary>
        /// Returns the bounding box around this route.
        /// </summary>
        /// <returns></returns>
        public static GeoCoordinateBox GetBox(this Route route)
        {
            return new GeoCoordinateBox(route.GetPoints().ToArray());
        }

        /// <summary>
        /// Returns the points along the route for the entire route in the correct order.
        /// </summary>
        /// <returns></returns>
        public static List<GeoCoordinate> GetPoints(this Route route)
        {
            var coordinates = new List<GeoCoordinate>(route.Segments.Count);
            for (int p = 0; p < route.Segments.Count; p++)
            {
                coordinates.Add(new GeoCoordinate(route.Segments[p].Latitude, route.Segments[p].Longitude));
            }
            return coordinates;
        }

        /// <summary>
        /// Calculates the position on the route after the given distance from the starting point.
        /// </summary>
        /// <returns></returns>
        public static GeoCoordinate PositionAfter(this Route route, Meter m)
        {
            var distanceMeter = 0.0;
            var points = route.GetPoints();
            for (int idx = 0; idx < points.Count - 1; idx++)
            {
                var currentDistance = points[idx].DistanceReal(points[idx + 1]).Value;
                if (distanceMeter + currentDistance >= m.Value)
                { // the current distance should be in this segment.
                    var segmentDistance = m.Value - distanceMeter;
                    var direction = points[idx + 1] - points[idx];
                    direction = direction * (segmentDistance / currentDistance);
                    var position = points[idx] + direction;
                    return new GeoCoordinate(position[1], position[0]);
                }
                distanceMeter += currentDistance;
            }
            return null;
        }

        /// <summary>
        /// Calculates the closest point on the route relative to the given coordinate.
        /// </summary>
        /// <returns></returns>
        public static bool ProjectOn(this Route route, GeoCoordinate coordinates, out GeoCoordinate projectedCoordinates)
        {
            int entryIdx;
            Meter distanceToProjected;
            Second timeToProjected;
            return route.ProjectOn(coordinates, out projectedCoordinates, out entryIdx, out distanceToProjected, out timeToProjected);
        }

        /// <summary>
        /// Calculates the closest point on the route relative to the given coordinate.
        /// </summary>
        /// <returns></returns>
        public static bool ProjectOn(this Route route, GeoCoordinate coordinates, out GeoCoordinate projectedCoordinates, out Meter distanceToProjected, out Second timeFromStart)
        {
            int entryIdx;
            return route.ProjectOn(coordinates, out projectedCoordinates, out entryIdx, out distanceToProjected, out timeFromStart);
        }

        /// <summary>
        /// Calculates the closest point on the route relative to the given coordinate.
        /// </summary>
        /// <returns></returns>
        public static bool ProjectOn(this Route route, GeoCoordinate coordinates, out Meter distanceFromStart)
        {
            int entryIdx;
            GeoCoordinate projectedCoordinates;
            Second timeFromStart;
            return route.ProjectOn(coordinates, out projectedCoordinates, out entryIdx, out distanceFromStart, out timeFromStart);
        }

        /// <summary>
        /// Calculates the closest point on the route relative to the given coordinate.
        /// </summary>
        /// <returns></returns>
        public static bool ProjectOn(this Route route, GeoCoordinate coordinates, out GeoCoordinate projectedCoordinates, out int entryIndex, out Meter distanceFromStart, out Second timeFromStart)
        {
            double distance = double.MaxValue;
            distanceFromStart = 0;
            timeFromStart = 0;
            double currentDistanceFromStart = 0;
            projectedCoordinates = null;
            entryIndex = -1;

            // loop over all points and try to project onto the line segments.
            GeoCoordinate projected;
            double currentDistance;
            var points = route.GetPoints();
            for (int idx = 0; idx < points.Count - 1; idx++)
            {
                var line = new GeoCoordinateLine(points[idx], points[idx + 1], true, true);
                var projectedPoint = line.ProjectOn(coordinates);
                if (projectedPoint != null)
                { // there was a projected point.
                    projected = new GeoCoordinate(projectedPoint[1], projectedPoint[0]);
                    currentDistance = coordinates.Distance(projected);
                    if (currentDistance < distance)
                    { // this point is closer.
                        projectedCoordinates = projected;
                        entryIndex = idx;
                        distance = currentDistance;

                        // calculate distance/time.
                        var localDistance = projected.DistanceReal(points[idx]).Value;
                        distanceFromStart = currentDistanceFromStart + localDistance;
                        //if (route.HasTimes && idx > 0)
                        //{ // there should be proper timing information.
                        //    var timeToSegment = route.Segments[idx].Time;
                        //    var timeToNextSegment = route.Segments[idx + 1].Time;
                        //    timeFromStart = timeToSegment + ((timeToNextSegment - timeToSegment) * (localDistance / line.LengthReal.Value));
                        //}
                    }
                }

                // check first point.
                projected = points[idx];
                currentDistance = coordinates.Distance(projected);
                if (currentDistance < distance)
                { // this point is closer.
                    projectedCoordinates = projected;
                    entryIndex = idx;
                    distance = currentDistance;
                    distanceFromStart = currentDistanceFromStart;
                    //if (route.HasTimes)
                    //{ // there should be proper timing information.
                    //    timeFromStart = route.Segments[idx].Time;
                    //}
                }

                // update distance from start.
                currentDistanceFromStart = currentDistanceFromStart + points[idx].DistanceReal(points[idx + 1]).Value;
            }

            // check last point.
            projected = points[points.Count - 1];
            currentDistance = coordinates.Distance(projected);
            if (currentDistance < distance)
            { // this point is closer.
                projectedCoordinates = projected;
                entryIndex = points.Count - 1;
                distance = currentDistance;
                distanceFromStart = currentDistanceFromStart;
                //if (route.HasTimes)
                //{ // there should be proper timing information.
                //    timeFromStart = route.Segments[points.Count - 1].Time;
                //}
            }
            return true;
        }

        /// <summary>
        /// Sets a stop on the given segment.
        /// </summary>
        public static void SetStop(this RouteSegment segment, ICoordinate coordinate)
        {
            segment.Points = new RouteStop[]
            {
                new RouteStop()
                {
                    Latitude = coordinate.Latitude,
                    Longitude = coordinate.Longitude,
                    Metrics = null,
                    Tags = null
                }
            };
        }

        /// <summary>
        /// Sets a stop on the given segment.
        /// </summary>
        public static void SetStop(this RouteSegment segment, ICoordinate coordinate, OsmSharp.Collections.Tags.TagsCollectionBase tags)
        {
            segment.Points = new RouteStop[]
            {
                new RouteStop()
                {
                    Latitude = coordinate.Latitude,
                    Longitude = coordinate.Longitude,
                    Metrics = null,
                    Tags = tags.ConvertFrom()
                }
            };
        }

        /// <summary>
        /// Sets a stop on the given segment.
        /// </summary>
        public static void SetStop(this RouteSegment segment, ICoordinate[] coordinates, 
            OsmSharp.Collections.Tags.TagsCollectionBase[] tags)
        {
            if (coordinates.Length != tags.Length) { throw new ArgumentException("Coordinates and tags arrays must have the same dimensions."); }

            segment.Points = new RouteStop[coordinates.Length];
            for (var i = 0; i < coordinates.Length;i++)
            {
                segment.Points[i] = new RouteStop()
                {
                    Latitude = coordinates[i].Latitude,
                    Longitude = coordinates[i].Longitude,
                    Metrics = null,
                    Tags = tags[i] == null ? null : tags[i].ConvertFrom()
                };
            }
        }

        /// <summary>
        /// Sets the distance/time.
        /// </summary>
        public static void SetDistanceAndTime(this RouteSegment segment, RouteSegment previous, 
            OsmSharp.Routing.Profiles.Speed speed)
        {
            var distance = GeoCoordinate.DistanceEstimateInMeter(
                new GeoCoordinateSimple()
                {
                    Latitude = previous.Latitude,
                    Longitude = previous.Longitude
                }, new GeoCoordinateSimple()
                {
                    Latitude = segment.Latitude,
                    Longitude = segment.Longitude
                });
            segment.Distance = previous.Distance + distance;
            segment.Time = previous.Time + (distance / speed.Value);
        }

        /// <summary>
        /// Sets the details of the segment.
        /// </summary>
        public static void Set(this RouteSegment segment, RouteSegment previous, Profile profile, TagsCollectionBase tags, OsmSharp.Routing.Profiles.Speed speed)
        {
            segment.SetDistanceAndTime(previous, speed);
            segment.Tags = tags.ConvertFrom();
            segment.Profile = profile.Name;
        }

        /// <summary>
        /// Sets the side streets.
        /// </summary>
        public static void SetSideStreets(this RouteSegment segment, RouterDb routerDb, uint vertex, uint previousEdge, uint nextVertex)
        {
            var sideStreets = new List<RouteSegmentBranch>();

            var edges = routerDb.Network.GetEdgeEnumerator(vertex);
            while(edges.MoveNext())
            {
                if(edges.Id != previousEdge &&
                    edges.To != nextVertex)
                {
                    var edge = edges.Current;
                    var profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
                    var meta = routerDb.EdgeMeta.Get(edge.Data.MetaId);

                    var tags = new TagsCollection(profile);
                    tags.AddOrReplace(meta);

                    var point = routerDb.Network.GetFirstPoint(edge, edges.From);
                    sideStreets.Add(new RouteSegmentBranch()
                        {
                            Latitude = point.Latitude,
                            Longitude = point.Longitude,
                            Tags = tags.ConvertFrom()
                        });
                }
            }

            if(sideStreets.Count > 0)
            {
                segment.SideStreets = sideStreets.ToArray();
            }
        }

        /// <summary>
        /// Returns the turn direction for the segment at the given index.
        /// </summary>
        public static RelativeDirection RelativeDirectionAt(this Route route, int i)
        {
            if (i < 0 || i >= route.Segments.Count) { throw new ArgumentOutOfRangeException("i"); }

            if (i == 0 || i == route.Segments.Count - 1)
            { // not possible to calculate a relative direction for the first or last segment.
                return null;
            }
            return RelativeDirectionCalculator.Calculate(
                new GeoCoordinate(route.Segments[i - 1].Latitude, route.Segments[i - 1].Longitude),
                new GeoCoordinate(route.Segments[i].Latitude, route.Segments[i].Longitude),
                new GeoCoordinate(route.Segments[i + 1].Latitude, route.Segments[i + 1].Longitude));
        }

        /// <summary>
        /// Returns the direction to the next segment.
        /// </summary>
        /// <returns></returns>
        public static DirectionEnum DirectionToNext(this Route route, int i)
        {
            if (i < 0 || i >= route.Segments.Count - 1) { throw new ArgumentOutOfRangeException("i"); }

            return DirectionCalculator.Calculate(
                new GeoCoordinate(route.Segments[i].Latitude, route.Segments[i].Longitude),
                new GeoCoordinate(route.Segments[i + 1].Latitude, route.Segments[i + 1].Longitude));
        }
    }
}