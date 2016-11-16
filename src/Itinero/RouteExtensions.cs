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

using Itinero.Attributes;
using Itinero.LocalGeo;
using Itinero.Navigation.Directions;
using Itinero.Navigation.Instructions;
using Itinero.Navigation.Language;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Itinero
{
    /// <summary>
    /// Contains extensions for the route object.
    /// </summary>
    public static class RouteExtensions
    {        
        /// <summary>
        /// Concatenates two routes.
        /// </summary>
        public static Route Concatenate(this Route route1, Route route2)
        {
            return route1.Concatenate(route2, true);
        }

        /// <summary>
        /// Concatenates two routes.
        /// </summary>
        public static Route Concatenate(this Route route1, Route route2, bool clone)
        {
            if (route1 == null) return route2;
            if (route2 == null) return route1;
            if (route1.Shape == null || route1.Shape.Length == 0) return route2;
            if (route2.Shape == null || route2.Shape.Length == 0) return route1;

            var timeoffset = route1.TotalTime;
            var distanceoffset = route1.TotalDistance;
            var shapeoffset = route1.Shape.Length - 1;

            // merge shape.
            var shapeLength = route1.Shape.Length + route2.Shape.Length - 1;
            var shape = new Coordinate[route1.Shape.Length + route2.Shape.Length - 1];
            route1.Shape.CopyTo(shape, 0);
            route2.Shape.CopyTo(shape, route1.Shape.Length - 1);

            // merge metas.
            var metas1 = route1.ShapeMeta != null ? route1.ShapeMeta.Length : 0;
            var metas2 = route2.ShapeMeta != null ? route2.ShapeMeta.Length : 0;
            Route.Meta[] metas = null;
            if (metas1 + metas2 - 1 > 0)
            {
                metas = new Route.Meta[metas1 + metas2 - 1];
                if (route1.ShapeMeta != null)
                {
                    for (var i = 0; i < route1.ShapeMeta.Length; i++)
                    {
                        metas[i] = new Route.Meta()
                        {
                            Attributes = new AttributeCollection(route1.ShapeMeta[i].Attributes),
                            Shape = route1.ShapeMeta[i].Shape
                        };
                    }
                }
                if (route2.ShapeMeta != null)
                {
                    for (var i = 1; i < route2.ShapeMeta.Length; i++)
                    {
                        metas[metas1 + i - 1] = new Route.Meta()
                        {
                            Attributes = new AttributeCollection(route2.ShapeMeta[i].Attributes),
                            Shape = route2.ShapeMeta[i].Shape + shapeoffset,
                            Distance = route2.ShapeMeta[i].Distance + distanceoffset,
                            Time = route2.ShapeMeta[i].Time + timeoffset
                        };
                    }
                }
            }

            // merge stops.
            var stops = new List<Route.Stop>();
            if (route1.Stops != null)
            {
                for (var i = 0; i < route1.Stops.Length; i++)
                {
                    var stop = route1.Stops[i];
                    stops.Add(new Route.Stop()
                    {
                        Attributes = new AttributeCollection(stop.Attributes),
                        Coordinate = stop.Coordinate,
                        Shape = stop.Shape
                    });
                }
            }
            if (route2.Stops != null)
            {
                for (var i = 0; i < route2.Stops.Length; i++)
                {
                    var stop = route2.Stops[i];
                    if (i == 0 && stops.Count > 0)
                    { // compare with last stop to remove duplicates.
                        var existing = stops[stops.Count - 1];
                        if (existing.Shape == route1.Shape.Length - 1 &&
                            existing.Coordinate.Latitude == stop.Coordinate.Latitude &&
                            existing.Coordinate.Longitude == stop.Coordinate.Longitude &&
                            existing.Attributes.ContainsSame(stop.Attributes, "time", "distance"))
                        { // stop are identical, stop this one.
                            continue;
                        }
                    }
                    stops.Add(new Route.Stop()
                    {
                        Attributes = new AttributeCollection(stop.Attributes),
                        Coordinate = stop.Coordinate,
                        Shape = stop.Shape + shapeoffset
                    });
                    stops[stops.Count - 1].Distance = stop.Distance + distanceoffset;
                    stops[stops.Count - 1].Time = stop.Time + timeoffset;
                }
            }

            // merge branches.
            var branches1 = route1.Branches != null ? route1.Branches.Length : 0;
            var branches2 = route2.Branches != null ? route2.Branches.Length : 0;
            var branches = new Route.Branch[branches1 + branches2];
            if (branches1 + branches2 > 0)
            {
                if (route1.Branches != null)
                {
                    for (var i = 0; i < route1.Branches.Length; i++)
                    {
                        var branch = route1.Branches[i];
                        branches[i] = new Route.Branch()
                        {
                            Attributes = new AttributeCollection(branch.Attributes),
                            Coordinate = branch.Coordinate,
                            Shape = branch.Shape
                        };
                    }
                }
                if (route2.Branches != null)
                {
                    for (var i = 0; i < route2.Branches.Length; i++)
                    {
                        var branch = route2.Branches[i];
                        branches[branches1 + i] = new Route.Branch()
                        {
                            Attributes = new AttributeCollection(branch.Attributes),
                            Coordinate = branch.Coordinate,
                            Shape = branch.Shape + shapeoffset
                        };
                    }
                }
            }

            // merge attributes.
            var attributes = new AttributeCollection(route1.Attributes);
            attributes.AddOrReplace(route2.Attributes);
            var profile = route1.Profile;
            if (route2.Profile != profile)
            {
                attributes.RemoveKey("profile");
            }

            // update route.
            var route = new Route()
            {
                Attributes = attributes,
                Branches = branches,
                Shape = shape,
                ShapeMeta = metas,
                Stops = stops.ToArray()
            };
            route.TotalDistance = route1.TotalDistance + route2.TotalDistance;
            route.TotalTime = route1.TotalTime + route2.TotalTime;
            return route;
        }

        /// <summary>
        /// Calculates the position on the route after the given distance from the starting point.
        /// </summary>
        public static Coordinate? PositionAfter(this Route route, float distanceInMeter)
        {
            var distanceMeter = 0.0f;
            if (route.Shape == null)
            {
                return null;
            }

            for (int i = 0; i < route.Shape.Length - 1; i++)
            {
                var currentDistance = Coordinate.DistanceEstimateInMeter(route.Shape[i], route.Shape[i + 1]);
                if (distanceMeter + currentDistance >= distanceInMeter)
                {
                    var segmentDistance = distanceInMeter - distanceMeter;
                    var diffLat = route.Shape[i + 1].Latitude - route.Shape[i].Latitude;
                    var diffLon = route.Shape[i + 1].Longitude - route.Shape[i].Longitude;
                    var lat = route.Shape[i].Latitude + diffLat * (segmentDistance / currentDistance);
                    var lon = route.Shape[i].Longitude + diffLon * (segmentDistance / currentDistance);
                    return new Coordinate(lat, lon);
                }
                distanceMeter += currentDistance;
            }
            return null;
        }

        /// <summary>
        /// Distance and time a the given shape index.
        /// </summary>
        public static void DistanceAndTimeAt(this Route route, int shape, out float distance, out float time)
        {
            int segmentStart, segmentEnd;
            route.SegmentFor(shape, out segmentStart, out segmentEnd);

            if (shape == segmentStart)
            {
                if (shape == 0)
                {
                    distance = 0;
                    time = 0;
                    return;
                }
                else
                {
                    var shapeMeta = route.ShapeMetaFor(shape);
                    distance = shapeMeta.Distance;
                    time = shapeMeta.Time;
                    return;
                }
            }
            if (shape == segmentEnd)
            {
                if (shape == route.Shape.Length - 1)
                {
                    distance = route.TotalDistance;
                    time = route.TotalTime;
                    return;
                }
                else
                {
                    var shapeMeta = route.ShapeMetaFor(shape);
                    distance = shapeMeta.Distance;
                    time = shapeMeta.Time;
                    return;
                }
            }
            var startDistance = 0f;
            var startTime = 0f;
            if (segmentStart == 0)
            {
                startDistance = 0;
                startTime = 0;
            }
            else
            {
                var shapeMeta = route.ShapeMetaFor(segmentStart);
                startDistance = shapeMeta.Distance;
                startTime = shapeMeta.Time;
            }
            var endDistance = 0f;
            var endTime = 0f;
            if (segmentEnd == route.Shape.Length - 1)
            {
                endDistance = route.TotalDistance;
                endTime = route.TotalTime;
            }
            else
            {
                var shapeMeta = route.ShapeMetaFor(segmentEnd);
                endDistance = shapeMeta.Distance;
                endTime = shapeMeta.Time;
            }
            var distanceToShape = 0f;
            var distanceOfSegment = 0f;
            for(var i = segmentStart; i < segmentEnd; i++)
            {
                if (i == shape)
                {
                    distanceToShape = distanceOfSegment;
                }
                distanceOfSegment += Coordinate.DistanceEstimateInMeter(
                    route.Shape[i].Latitude, route.Shape[i].Longitude, 
                    route.Shape[i + 1].Latitude, route.Shape[i + 1].Longitude);
            }
            var ratio = distanceToShape / distanceOfSegment;
            distance = ((endDistance - startDistance) * ratio) + startDistance;
            time = ((endTime - startTime) * ratio) + startTime;
        }

        /// <summary>
        /// Gets the shape meta for the given shape index.
        /// </summary>
        public static Route.Meta ShapeMetaFor(this Route route, int shape)
        {
            foreach (var shapeMeta in route.ShapeMeta)
            {
                if (shapeMeta.Shape == shape)
                {
                    return shapeMeta;
                }
            }
            return null;
        }

        /// <summary>
        /// Searches the segment the given shape index exists in.
        /// </summary>
        public static void SegmentFor(this Route route, int shape, out int segmentStart, out int segmentEnd)
        {
            segmentStart = 0;
            segmentEnd = route.Shape.Length - 1;
            if (route.ShapeMeta == null)
            {
                return;
            }

            for (var i = 0; i < route.ShapeMeta.Length; i++)
            {
                if (route.ShapeMeta[i].Shape <= shape)
                {
                    if (segmentStart <= route.ShapeMeta[i].Shape &&
                        route.ShapeMeta[i].Shape < route.Shape.Length - 1)
                    {
                        segmentStart = route.ShapeMeta[i].Shape;
                    }
                }
                else if (route.ShapeMeta[i].Shape > shape)
                {
                    segmentEnd = route.ShapeMeta[i].Shape;
                    break;
                }
            }
            return;
        }

        /// <summary>
        /// Calculates the closest point on the route relative to the given coordinate.
        /// </summary>
        public static bool ProjectOn(this Route route, Coordinate coordinate, out Coordinate projected)
        {
            int segment;
            float distanceToProjectedInMeter;
            float timeToProjectedInSeconds;
            return route.ProjectOn(coordinate, out projected, out segment, out distanceToProjectedInMeter, 
                out timeToProjectedInSeconds);
        }

        /// <summary>
        /// Calculates the closest point on the route relative to the given coordinate.
        /// </summary>
        public static bool ProjectOn(this Route route, Coordinate coordinate, out Coordinate projected, 
            out float distanceToProjectedInMeter, out float timeToProjectedInSeconds)
        {
            int segment;
            return route.ProjectOn(coordinate, out projected, out segment, out distanceToProjectedInMeter, 
                out timeToProjectedInSeconds);
        }

        /// <summary>
        /// Calculates the closest point on the route relative to the given coordinate.
        /// </summary>
        public static bool ProjectOn(this Route route, Coordinate coordinate, out float distanceFromStartInMeter)
        {
            int segment;
            Coordinate projected;
            float timeFromStartInSeconds;
            return route.ProjectOn(coordinate, out projected, out segment, out distanceFromStartInMeter, out timeFromStartInSeconds);
        }

        /// <summary>
        /// Calculates the closest point on the route relative to the given coordinate.
        /// </summary>
        public static bool ProjectOn(this Route route, Coordinate coordinate, out Coordinate projected, out int segment, 
            out float distanceFromStartInMeter, out float timeFromStartInSeconds)
        {
            float distance = float.MaxValue;
            distanceFromStartInMeter = 0;
            timeFromStartInSeconds = 0;
            projected = new Coordinate();
            segment = -1;

            if (route.Shape == null)
            {
                return false;
            }
            
            Coordinate currentProjected;
            float currentDistanceFromStart = 0;
            float currentDistance;
            var shape = route.Shape;
            for (int i = 0; i < shape.Length - 1; i++)
            {
                var line = new Line(shape[i], shape[i + 1]);
                var projectedPoint = line.ProjectOn(coordinate);
                if (projectedPoint != null)
                { // there was a projected point.
                    currentProjected = new Coordinate(projectedPoint.Value.Latitude, projectedPoint.Value.Longitude);
                    currentDistance = Coordinate.DistanceEstimateInMeter(coordinate, currentProjected);
                    if (currentDistance < distance)
                    { // this point is closer.
                        projected = currentProjected;
                        segment = i;
                        distance = currentDistance;

                        // calculate distance/time.
                        var localDistance = Coordinate.DistanceEstimateInMeter(currentProjected, shape[i]);
                        distanceFromStartInMeter = currentDistanceFromStart + localDistance;
                    }
                }

                // check first point.
                currentProjected = shape[i];
                currentDistance = Coordinate.DistanceEstimateInMeter(coordinate, currentProjected);
                if (currentDistance < distance)
                { // this point is closer.
                    projected = currentProjected;
                    segment = i;
                    distance = currentDistance;
                    distanceFromStartInMeter = currentDistanceFromStart;
                }

                // update distance from start.
                currentDistanceFromStart = currentDistanceFromStart + Coordinate.DistanceEstimateInMeter(shape[i], shape[i + 1]);
            }

            // check last point.
            currentProjected = shape[shape.Length - 1];
            currentDistance = Coordinate.DistanceEstimateInMeter(coordinate, currentProjected);
            if (currentDistance < distance)
            { // this point is closer.
                projected = currentProjected;
                segment = shape.Length - 1;
                distance = currentDistance;
                distanceFromStartInMeter = currentDistanceFromStart;
            }
            return true;
        }
        
        /// <summary>
        /// Returns the turn direction for the shape point at the given index.
        /// </summary>
        public static RelativeDirection RelativeDirectionAt(this Route route, int i)
        {
            if (i < 0 || i >= route.Shape.Length) { throw new ArgumentOutOfRangeException("i"); }

            if (i == 0 || i == route.Shape.Length - 1)
            { // not possible to calculate a relative direction for the first or last segment.
                throw new ArgumentOutOfRangeException("i", "It's not possible to calculate a relative direction for the first or last segment.");
            }
            return DirectionCalculator.Calculate(
                new Coordinate(route.Shape[i - 1].Latitude, route.Shape[i - 1].Longitude),
                new Coordinate(route.Shape[i].Latitude, route.Shape[i].Longitude),
                new Coordinate(route.Shape[i + 1].Latitude, route.Shape[i + 1].Longitude));
        }

        /// <summary>
        /// Returns the direction to the next shape segment.
        /// </summary>
        public static DirectionEnum DirectionToNext(this Route route, int i)
        {
            if (i < 0 || i >= route.Shape.Length - 1) { throw new ArgumentOutOfRangeException("i"); }

            return DirectionCalculator.Calculate(
                new Coordinate(route.Shape[i].Latitude, route.Shape[i].Longitude),
                new Coordinate(route.Shape[i + 1].Latitude, route.Shape[i + 1].Longitude));
        }

        /// <summary>
        /// Returns this route as json.
        /// </summary>
        public static string ToJson(this Route route)
        {
            var stringWriter = new StringWriter();
            route.WriteJson(stringWriter);
            return stringWriter.ToInvariantString();
        }

        /// <summary>
        /// Writes the route as json.
        /// </summary>
        public static void WriteJson(this Route route, Stream stream)
        {
            route.WriteJson(new StreamWriter(stream));
        }

        /// <summary>
        /// Writes the route as json.
        /// </summary>
        public static void WriteJson(this Route route, TextWriter writer)
        {
            if (route == null) { throw new ArgumentNullException("route"); }
            if (writer == null) { throw new ArgumentNullException("writer"); }

            var jsonWriter = new IO.Json.JsonWriter(writer);
            jsonWriter.WriteOpen();
            if (route.Shape != null)
            {
                jsonWriter.WritePropertyName("Shape");
                jsonWriter.WriteArrayOpen();
                for(var i = 0; i < route.Shape.Length; i++)
                {
                    jsonWriter.WriteArrayOpen();
                    jsonWriter.WriteArrayValue(route.Shape[i].Longitude.ToInvariantString());
                    jsonWriter.WriteArrayValue(route.Shape[i].Latitude.ToInvariantString());
                    jsonWriter.WriteArrayClose();
                }
                jsonWriter.WriteArrayClose();
            }

            if (route.ShapeMeta != null)
            {
                jsonWriter.WritePropertyName("ShapeMeta");
                jsonWriter.WriteArrayOpen();
                for(var i = 0; i < route.ShapeMeta.Length; i++)
                {
                    var meta = route.ShapeMeta[i];

                    jsonWriter.WriteOpen();
                    jsonWriter.WritePropertyName("Shape");
                    jsonWriter.WritePropertyValue(meta.Shape.ToInvariantString());

                    if (meta.Attributes != null)
                    {
                        jsonWriter.WritePropertyName("Attributes");
                        jsonWriter.WriteOpen();
                        foreach(var attribute in meta.Attributes)
                        {
                            jsonWriter.WriteProperty(attribute.Key, attribute.Value, true, true);
                        }
                        jsonWriter.WriteClose();
                    }
                    jsonWriter.WriteClose();
                }
                jsonWriter.WriteArrayClose();
            }

            if (route.Stops != null)
            {
                jsonWriter.WritePropertyName("Stops");
                jsonWriter.WriteArrayOpen();
                for (var i = 0; i < route.Stops.Length; i++)
                {
                    var stop = route.Stops[i];

                    jsonWriter.WriteOpen();
                    jsonWriter.WritePropertyName("Shape");
                    jsonWriter.WritePropertyValue(stop.Shape.ToInvariantString());
                    jsonWriter.WritePropertyName("Coordinates");
                    jsonWriter.WriteArrayOpen();
                    jsonWriter.WriteArrayValue(route.Stops[i].Coordinate.Longitude.ToInvariantString());
                    jsonWriter.WriteArrayValue(route.Stops[i].Coordinate.Latitude.ToInvariantString());
                    jsonWriter.WriteArrayClose();

                    if (stop.Attributes != null)
                    {
                        jsonWriter.WritePropertyName("Attributes");
                        jsonWriter.WriteOpen();
                        foreach (var attribute in stop.Attributes)
                        {
                            jsonWriter.WriteProperty(attribute.Key, attribute.Value, true, true);
                        }
                        jsonWriter.WriteClose();
                    }
                    jsonWriter.WriteClose();
                }
                jsonWriter.WriteArrayClose();
            }

            if (route.Branches != null)
            {
                jsonWriter.WritePropertyName("Branches");
                jsonWriter.WriteArrayOpen();
                for (var i = 0; i < route.Branches.Length; i++)
                {
                    var stop = route.Branches[i];

                    jsonWriter.WriteOpen();
                    jsonWriter.WritePropertyName("Shape");
                    jsonWriter.WritePropertyValue(stop.Shape.ToInvariantString());
                    jsonWriter.WritePropertyName("Coordinates");
                    jsonWriter.WriteArrayOpen();
                    jsonWriter.WriteArrayValue(route.Branches[i].Coordinate.Longitude.ToInvariantString());
                    jsonWriter.WriteArrayValue(route.Branches[i].Coordinate.Latitude.ToInvariantString());
                    jsonWriter.WriteArrayClose();

                    if (stop.Attributes != null)
                    {
                        jsonWriter.WritePropertyName("Attributes");
                        jsonWriter.WriteOpen();
                        foreach (var attribute in stop.Attributes)
                        {
                            jsonWriter.WriteProperty(attribute.Key, attribute.Value, true, true);
                        }
                        jsonWriter.WriteClose();
                    }
                    jsonWriter.WriteClose();
                }
                jsonWriter.WriteArrayClose();
            }

            jsonWriter.WriteClose();
        }

        /// <summary>
        /// Returns this route as xml.
        /// </summary>
        public static string ToXml(this Route route)
        {
            var stringWriter = new StringWriter();
            route.WriteXml(stringWriter);
            return stringWriter.ToInvariantString();
        }

        /// <summary>
        /// Writes the route as xml.
        /// </summary>
        public static void WriteXml(this Route route, Stream stream)
        {
            route.WriteXml(new StreamWriter(stream));
        }

        /// <summary>
        /// Writes the route as xml.
        /// </summary>
        public static void WriteXml(this Route route, TextWriter writer)
        {
            var ser = new XmlSerializer(typeof(Route));
            ser.Serialize(writer, route);
            writer.Flush();
        }

        /// <summary>
        /// Reads a route in xml.
        /// </summary>
        public static Route ReadXml(Stream stream)
        {
            var ser = new XmlSerializer(typeof(Route));
            return ser.Deserialize(stream) as Route;
        }

        /// <summary>
        /// Returns this route as geojson.
        /// </summary>
        public static string ToGeoJson(this Route route, bool includeShapeMeta = true, bool includeStops = true, bool groupByShapeMeta = true)
        {
            var stringWriter = new StringWriter();
            route.WriteGeoJson(stringWriter, includeShapeMeta, includeStops, groupByShapeMeta);
            return stringWriter.ToInvariantString();
        }

        /// <summary>
        /// Writes the route as geojson.
        /// </summary>
        public static void WriteGeoJson(this Route route, Stream stream, bool includeShapeMeta = true, bool includeStops = true, bool groupByShapeMeta = true)
        {
            route.WriteGeoJson(new StreamWriter(stream), includeShapeMeta, includeStops, groupByShapeMeta);
        }

        /// <summary>
        /// Writes the route as geojson.
        /// </summary>
        public static void WriteGeoJson(this Route route, TextWriter writer, bool includeShapeMeta = true, bool includeStops = true, bool groupByShapeMeta = true)
        {
            if (route == null) { throw new ArgumentNullException("route"); }
            if (writer == null) { throw new ArgumentNullException("writer"); }

            if (groupByShapeMeta)
            { // group by shape meta.
                var jsonWriter = new IO.Json.JsonWriter(writer);
                jsonWriter.WriteOpen();
                jsonWriter.WriteProperty("type", "FeatureCollection", true, false);
                jsonWriter.WritePropertyName("features", false);
                jsonWriter.WriteArrayOpen();

                if (route.Shape != null && route.ShapeMeta != null)
                {
                    for(var i = 0; i < route.ShapeMeta.Length; i++)
                    {
                        var shapeMeta = route.ShapeMeta[i];
                        var lowerShape = -1;
                        if (i > 0)
                        {
                            lowerShape = route.ShapeMeta[i - 1].Shape;
                        }
                        var higherShape = route.ShapeMeta[i].Shape;
                        if (lowerShape >= higherShape)
                        {
                            throw new Exception(string.Format("Invalid route: {0}", route.ToJson()));
                        }

                        var coordinates = new List<Coordinate>();
                        for (var shape = lowerShape; shape <= higherShape; shape++)
                        {
                            if (shape >= 0 && shape < route.Shape.Length)
                            {
                                coordinates.Add(route.Shape[shape]);
                            }
                        }

                        if (coordinates.Count >= 2)
                        {
                            jsonWriter.WriteOpen();
                            jsonWriter.WriteProperty("type", "Feature", true, false);
                            jsonWriter.WriteProperty("name", "ShapeMeta", true, false);
                            jsonWriter.WritePropertyName("properties");
                            jsonWriter.WriteOpen();
                            jsonWriter.WriteClose();
                            jsonWriter.WritePropertyName("geometry", false);

                            jsonWriter.WriteOpen();
                            jsonWriter.WriteProperty("type", "LineString", true, false);
                            jsonWriter.WritePropertyName("coordinates", false);
                            jsonWriter.WriteArrayOpen();

                            for (var shape = 0; shape < coordinates.Count; shape++)
                            {
                                jsonWriter.WriteArrayOpen();
                                jsonWriter.WriteArrayValue(coordinates[shape].Longitude.ToInvariantString());
                                jsonWriter.WriteArrayValue(coordinates[shape].Latitude.ToInvariantString());
                                jsonWriter.WriteArrayClose();
                            }

                            jsonWriter.WriteArrayClose();
                            jsonWriter.WriteClose();

                            jsonWriter.WritePropertyName("properties");
                            jsonWriter.WriteOpen();
                            if (shapeMeta.Attributes != null)
                            {
                                foreach (var attribute in shapeMeta.Attributes)
                                {
                                    jsonWriter.WriteProperty(attribute.Key, attribute.Value, true, true);
                                }
                            }
                            jsonWriter.WriteClose();

                            jsonWriter.WriteClose();
                        }
                    }
                }

                if (route.Stops != null &&
                    includeStops)
                {
                    for (var i = 0; i < route.Stops.Length; i++)
                    {
                        var stop = route.Stops[i];

                        jsonWriter.WriteOpen();
                        jsonWriter.WriteProperty("type", "Feature", true, false);
                        jsonWriter.WriteProperty("name", "Stop", true, false);
                        jsonWriter.WriteProperty("Shape", stop.Shape.ToInvariantString(), true, false);
                        jsonWriter.WritePropertyName("geometry", false);

                        jsonWriter.WriteOpen();
                        jsonWriter.WriteProperty("type", "Point", true, false);
                        jsonWriter.WritePropertyName("coordinates", false);
                        jsonWriter.WriteArrayOpen();
                        jsonWriter.WriteArrayValue(stop.Coordinate.Longitude.ToInvariantString());
                        jsonWriter.WriteArrayValue(stop.Coordinate.Latitude.ToInvariantString());
                        jsonWriter.WriteArrayClose();
                        jsonWriter.WriteClose();

                        jsonWriter.WritePropertyName("properties");
                        jsonWriter.WriteOpen();
                        if (stop.Attributes != null)
                        {
                            foreach (var attribute in stop.Attributes)
                            {
                                jsonWriter.WriteProperty(attribute.Key, attribute.Value, true, true);
                            }
                        }
                        jsonWriter.WriteClose();

                        jsonWriter.WriteClose();
                    }
                }

                jsonWriter.WriteArrayClose();
                jsonWriter.WriteClose();
            }
            else
            { // include shape meta as points if requested.
                var jsonWriter = new IO.Json.JsonWriter(writer);
                jsonWriter.WriteOpen();
                jsonWriter.WriteProperty("type", "FeatureCollection", true, false);
                jsonWriter.WritePropertyName("features", false);
                jsonWriter.WriteArrayOpen();

                if (route.Shape != null)
                {
                    jsonWriter.WriteOpen();
                    jsonWriter.WriteProperty("type", "Feature", true, false);
                    jsonWriter.WriteProperty("name", "Shape", true, false);
                    jsonWriter.WritePropertyName("properties");
                    jsonWriter.WriteOpen();
                    jsonWriter.WriteClose();
                    jsonWriter.WritePropertyName("geometry", false);


                    jsonWriter.WriteOpen();
                    jsonWriter.WriteProperty("type", "LineString", true, false);
                    jsonWriter.WritePropertyName("coordinates", false);
                    jsonWriter.WriteArrayOpen();
                    for (var i = 0; i < route.Shape.Length; i++)
                    {
                        jsonWriter.WriteArrayOpen();
                        jsonWriter.WriteArrayValue(route.Shape[i].Longitude.ToInvariantString());
                        jsonWriter.WriteArrayValue(route.Shape[i].Latitude.ToInvariantString());
                        jsonWriter.WriteArrayClose();
                    }
                    jsonWriter.WriteArrayClose();
                    jsonWriter.WriteClose();

                    jsonWriter.WriteClose();
                }

                if (route.ShapeMeta != null &&
                    includeShapeMeta)
                {
                    for (var i = 0; i < route.ShapeMeta.Length; i++)
                    {
                        var meta = route.ShapeMeta[i];

                        jsonWriter.WriteOpen();
                        jsonWriter.WriteProperty("type", "Feature", true, false);
                        jsonWriter.WriteProperty("name", "ShapeMeta", true, false);
                        jsonWriter.WriteProperty("Shape", meta.Shape.ToInvariantString(), true, false);
                        jsonWriter.WritePropertyName("geometry", false);

                        var coordinate = route.Shape[meta.Shape];

                        jsonWriter.WriteOpen();
                        jsonWriter.WriteProperty("type", "Point", true, false);
                        jsonWriter.WritePropertyName("coordinates", false);
                        jsonWriter.WriteArrayOpen();
                        jsonWriter.WriteArrayValue(coordinate.Longitude.ToInvariantString());
                        jsonWriter.WriteArrayValue(coordinate.Latitude.ToInvariantString());
                        jsonWriter.WriteArrayClose();
                        jsonWriter.WriteClose();

                        jsonWriter.WritePropertyName("properties");
                        jsonWriter.WriteOpen();
                        if (meta.Attributes != null)
                        {
                            foreach (var attribute in meta.Attributes)
                            {
                                jsonWriter.WriteProperty(attribute.Key, attribute.Value, true, true);
                            }
                        }
                        jsonWriter.WriteClose();

                        jsonWriter.WriteClose();
                    }
                }

                if (route.Stops != null &&
                    includeStops)
                {
                    for (var i = 0; i < route.Stops.Length; i++)
                    {
                        var stop = route.Stops[i];

                        jsonWriter.WriteOpen();
                        jsonWriter.WriteProperty("type", "Feature", true, false);
                        jsonWriter.WriteProperty("name", "Stop", true, false);
                        jsonWriter.WriteProperty("Shape", stop.Shape.ToInvariantString(), true, false);
                        jsonWriter.WritePropertyName("geometry", false);

                        jsonWriter.WriteOpen();
                        jsonWriter.WriteProperty("type", "Point", true, false);
                        jsonWriter.WritePropertyName("coordinates", false);
                        jsonWriter.WriteArrayOpen();
                        jsonWriter.WriteArrayValue(stop.Coordinate.Longitude.ToInvariantString());
                        jsonWriter.WriteArrayValue(stop.Coordinate.Latitude.ToInvariantString());
                        jsonWriter.WriteArrayClose();
                        jsonWriter.WriteClose();

                        jsonWriter.WritePropertyName("properties");
                        jsonWriter.WriteOpen();
                        if (stop.Attributes != null)
                        {
                            foreach (var attribute in stop.Attributes)
                            {
                                jsonWriter.WriteProperty(attribute.Key, attribute.Value, true, true);
                            }
                        }
                        jsonWriter.WriteClose();

                        jsonWriter.WriteClose();
                    }
                }

                jsonWriter.WriteArrayClose();
                jsonWriter.WriteClose();
            }
        }
        
        /// <summary>
        /// Returns true if this route has multiple profiles.
        /// </summary>
        public static bool IsMultimodal(this Route route)
        {
            return string.IsNullOrWhiteSpace(route.Profile);
        }

        /// <summary>
        /// Generates instructions for the given route.
        /// </summary>
        public static IList<Instruction> GenerateInstruction(this Route route)
        {
            return route.GenerateInstruction(new DefaultLanguageReference());
        }

        /// <summary>
        /// Generates instructions for the given route.
        /// </summary>
        public static IList<Instruction> GenerateInstruction(this Route route, ILanguageReference languageReference)
        {
            if (route.IsMultimodal())
            {
                throw new NotSupportedException("Generate instruction for multimodal routes is not supported.");
            }
            var profile = Profiles.Profile.GetRegistered(route.Profile);
            return profile.InstructionGenerator.Generate(route, languageReference);
        }
    }
}