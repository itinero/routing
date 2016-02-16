// Itinero - OpenStreetMap (OSM) SDK
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
using Itinero.Geo.Attributes;
using System;
using System.Collections.Generic;

namespace Itinero.Geo
{
    /// <summary>
    /// Contains extensions for the Route type.
    /// </summary>
    public static class RouteExtensions
    {
        /// <summary>
        /// Returns the bounding box around this route.
        /// </summary>
        public static GeoAPI.Geometries.Envelope GetBox(this Route route)
        {
            if (route == null) { throw new ArgumentNullException("route"); }
            if (route.Shape  == null)
            {
                return null;
            }

            if (route.Shape.Length == 1)
            {
                return new GeoAPI.Geometries.Envelope(
                    route.Shape[0].ToCoordinate(), route.Shape[0].ToCoordinate());
            }

            var envelope = new GeoAPI.Geometries.Envelope(
                    route.Shape[0].ToCoordinate(), route.Shape[1].ToCoordinate());
            for(var i = 2; i < route.Shape.Length; i++)
            {
                envelope.ExpandToInclude(route.Shape[i].ToCoordinate());
            }
            return envelope;
        }

        /// <summary>
        /// Converts this route to a linestring.
        /// </summary>
        public static LineString ToLineString(this Route route)
        {
            if (route == null) { throw new ArgumentNullException("route"); }
            if (route.Shape == null)
            {
                return null;
            }
            return new LineString(route.Shape.ToCoordinatesArray());
        }

        /// <summary>
        /// Converts this route to a feature collection.
        /// </summary>
        public static FeatureCollection ToFeatureCollection(this Route route)
        {
            var featureCollection = new FeatureCollection();
            if (route.Shape == null)
            {
                return featureCollection;
            }

            if (route.ShapeMeta == null)
            { // if there is no meta data, just use one linestring.
                var linestring = route.ToLineString();
                var attributes = route.Attributes.ToAttributesTable();
                featureCollection.Add(new Feature(linestring, attributes));
            }
            else
            { // one linestring per meta-object.
                var previous = route.ShapeMeta[0];
                for (var i = 1; i < route.ShapeMeta.Length; i++)
                {
                    var current = route.ShapeMeta[i];
                    if (current.Shape < previous.Shape)
                    {
                        Logging.Logger.Log("RouteExtensions", Logging.TraceEventType.Warning, 
                            "Invalid meta-data detected on route. One of the meta-description has a shape index smaller than the previous one.");
                    }
                    var shape = new GeoAPI.Geometries.Coordinate[current.Shape - previous.Shape + 1];
                    for(var s = previous.Shape; s <= current.Shape; s++)
                    {
                        if (s < 0 || s >= route.Shape.Length)
                        {
                            throw new Exception("Invalid meta-data object: shape-index outside of the range of shapepoints.");
                        }
                        shape[s - previous.Shape] = route.Shape[s].ToCoordinate();
                    }
                    var attributes = current.Attributes.ToAttributesTable();
                    featureCollection.Add(new Feature(new LineString(shape), attributes));
                    previous = current;
                }
            }

            if (route.Stops != null)
            {
                for (var i = 0; i < route.Stops.Length; i++)
                {
                    featureCollection.Add(new Feature(
                        new Point(new GeoAPI.Geometries.Coordinate(
                            route.Stops[i].Coordinate.ToCoordinate())), 
                        route.Stops[i].Attributes.ToAttributesTable()));
                }
            }
            return featureCollection;
        }
    }
}